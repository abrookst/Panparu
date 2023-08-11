using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Panparu : MonoBehaviour
{
    [SerializeField] private int food = 4;
    [SerializeField] private int attention = 1;
    [SerializeField] private int play = 1;

    public enum CareType { Good, Okay, Bad, Dead};
    public enum Age { Egg, Child, Adult };

    private DateTime birthTime;
    private DateTime lastTimeHungry;
    private DateTime lastTimeCheckCare;
    private DateTime lastTimeAttention;
    private DateTime lastTimePlay;
    [SerializeField] private float averageCare = 1f;
    public static Panparu Instance { get; private set; }
    readonly TimeSpan checkCareCooldown = new(0, 0, 1);
    readonly TimeSpan hungerCooldown = new(0, 0, 6);
    readonly TimeSpan attentionCooldown = new(0, 0, 12);
    readonly TimeSpan playCooldown = new(0, 0, 12);

    readonly TimeSpan eggToChild = new(0, 0, 24);
    readonly TimeSpan childToAdult = new(0, 0, 60);

    [SerializeField] private CareType currentCare; 
    [SerializeField] private CareType eggCare;
    [SerializeField] private CareType childCare;
    [SerializeField] private Age currentAge;
    private int eggType;

    [SerializeField] private GameObject feeling;
    [SerializeField] private Sprite goodSprite;
    [SerializeField] private Sprite okaySprite;
    [SerializeField] private Sprite badSprite;
    [SerializeField] private Sprite deadSprite;
    [SerializeField] private Sprite[] childSprites;
    [SerializeField] private Sprite[] adultSpritesFromGood;
    [SerializeField] private Sprite[] adultSpritesFromOkay;
    [SerializeField] private Sprite[] adultSpritesFromBad;

    public AudioClip happy;
    public AudioClip reject;
    public AudioSource panparuAudio;

    private Image sprRdr;

    private Animator m_Animator;

    bool isChecking = false;

    public Sprite[] eggSprites;

    public Sprite tombstone;

    public bool initialized = false;

    public int GetFood(){
        return food;
    }

    void Awake()
    {
        Instance = this;
    }
    public void Initialize(PanparuData data){
        //NOTE these need to be saved and reloaded accurately on close
        food = data.food;
        attention = data.attention;
        play = data.play;
        averageCare = data.averageCare;
        eggCare = data.eggCare;
        childCare = data.childCare;
        currentAge = data.currentAge;
        birthTime = new DateTime(data.birthTime);
        lastTimeHungry = new DateTime(data.lastTimeHungry);
        lastTimeCheckCare = new DateTime(data.lastTimeCheckCare);
        lastTimeAttention = new DateTime(data.lastTimeAttention);
        lastTimePlay = new DateTime(data.lastTimePlay);
        eggType = data.eggType;

        sprRdr = gameObject.GetComponent<Image>();
        m_Animator = gameObject.GetComponent<Animator>();
        m_Animator.enabled = true;

        UpdateCurrentCare();

        if (currentCare == CareType.Dead) {
            initialized = true;
            return;
        }
        else if (currentAge == Age.Egg)
        {
            if (eggType == -1)
                eggType = Random.Range(0, eggSprites.Length);
            sprRdr.sprite = eggSprites[eggType];
        }
        else if (currentAge == Age.Child)
        {
            //Calculate what sprite it should have based on how it was treated in the egg
            if (eggCare == CareType.Good)
            {
                sprRdr.sprite = childSprites[0];
            }
            else if (eggCare == CareType.Okay)
            {
                sprRdr.sprite = childSprites[1];
            }
            else if (eggCare == CareType.Bad)
            {
                sprRdr.sprite = childSprites[2];
            }
        }
        else
        {
            //Calculate what sprite it should have based on how it was treated in the egg and as a child
            if (eggCare == CareType.Good)
            {
                if (childCare == CareType.Good)
                {
                    sprRdr.sprite = adultSpritesFromGood[0];
                }
                else if (childCare == CareType.Okay)
                {
                    sprRdr.sprite = adultSpritesFromGood[1];
                }
                else if (childCare == CareType.Bad)
                {
                    sprRdr.sprite = adultSpritesFromGood[2];
                }
            }
            else if (eggCare == CareType.Okay)
            {
                if (childCare == CareType.Good)
                {
                    sprRdr.sprite = adultSpritesFromOkay[0];
                }
                else if (childCare == CareType.Okay)
                {
                    sprRdr.sprite = adultSpritesFromOkay[1];
                }
                else if (childCare == CareType.Bad)
                {
                    sprRdr.sprite = adultSpritesFromOkay[2];
                }
            }
            else if (eggCare == CareType.Bad)
            {
                if (childCare == CareType.Good)
                {
                    sprRdr.sprite = adultSpritesFromBad[0];
                }
                else if (childCare == CareType.Okay)
                {
                    sprRdr.sprite = adultSpritesFromBad[1];
                }
                else if (childCare == CareType.Bad)
                {
                    sprRdr.sprite = adultSpritesFromBad[2];
                }
            }
        }

        Instance.GetComponent<Button>().onClick.AddListener(CheckEmotion);
        
        initialized = true;
    }
    public PanparuData GetPanparuData(){
        PanparuData data = new()
        {
            food = food,
            attention = attention,
            play = play,
            averageCare = averageCare,
            eggCare = eggCare,
            childCare = childCare,
            currentAge = currentAge,
            birthTime = birthTime.Ticks,
            lastTimeHungry = lastTimeHungry.Ticks,
            lastTimeCheckCare = lastTimeCheckCare.Ticks,
            lastTimeAttention = lastTimeAttention.Ticks,
            lastTimePlay = lastTimePlay.Ticks,
            eggType = eggType
        };
        return data;
    }

    void Update() 
    {
        if (!initialized)
            return;

        DateTime timeNow = DateTime.Now;
        //Get difference cooldown times
        TimeSpan timeSinceCheckCare = timeNow - lastTimeCheckCare;

        while (timeSinceCheckCare.CompareTo(checkCareCooldown) > 0)
        {
            if (currentCare == CareType.Dead) {
                return;
            }
            lastTimeCheckCare = lastTimeCheckCare.Add(checkCareCooldown);

            TimeSpan timeSinceHungry = lastTimeCheckCare - lastTimeHungry;
            TimeSpan timeSinceAttention = lastTimeCheckCare - lastTimeAttention;
            TimeSpan timeSincePlay = lastTimeCheckCare - lastTimePlay;
            if (timeSinceHungry.CompareTo(hungerCooldown) > 0) {
                food -= 1;
                lastTimeHungry = lastTimeHungry.Add(hungerCooldown);
                //print("-1 Hunger!");
            }
            
            if (timeSinceAttention.CompareTo(attentionCooldown) > 0)
            {
                if (attention > 0)
                    attention -= 1;
                lastTimeAttention = lastTimeAttention.Add(attentionCooldown);
            }

            if (timeSincePlay.CompareTo(playCooldown) > 0)
            {
                if (play > 0)
                    play -= 1;
                lastTimePlay = lastTimePlay.Add(playCooldown);
            }
            
            TimeSpan timeSinceBirth = lastTimeCheckCare - birthTime;
            //print("TimeSPAN since birth: " + timeSinceBirth.Seconds + " seconds");
            averageCare = (averageCare * (float)timeSinceBirth.TotalSeconds + CalcCare()) / ((float)timeSinceBirth.TotalSeconds + 1);
            timeSinceCheckCare = timeNow - lastTimeCheckCare;
            
            UpdateCurrentCare();

            if (timeSinceBirth.CompareTo(eggToChild) > 0 && currentAge == Age.Egg) {
                EvolveFromEggToChild();
                averageCare = 1f;
            }
            if (timeSinceBirth.CompareTo(childToAdult) > 0 && currentAge == Age.Child) {
                EvolveFromChildToAdult();
                averageCare = 1f;
            }
            if(food < 0)
            {
                currentCare = CareType.Dead;
                Dead();
            }
        }

        /*
        SETTING ANIMATION SPEED DEPENDING ON HEALTH:
        Borked bc this slows down stuff besides panparu_shift. ill figure it out tomorrow *yawn*
        */
    }
    private void UpdateCurrentCare() {
        float tempSpeed;
        if (averageCare > .8)
        {
            currentCare = CareType.Good;
            tempSpeed = 1f;
        }
        else if (averageCare > .6)
        {
            currentCare = CareType.Okay;
            tempSpeed = .75f;
        }
        else if (averageCare > .4)
        {
            currentCare = CareType.Bad;
            tempSpeed = .5f;
        }
        else
        {
            currentCare = CareType.Dead;
            tempSpeed = .25f;
            Dead();
        }
        foreach(AnimationState state in GetComponent<Animation>())
        {
            state.speed = tempSpeed;
        }
    }

    float CalcCare()
    {
        return ((food / 4f) + attention/1f + play/1f) / 3f;
    }

    public void Feed()
    {
        if (food < 4) {
            food += 1;
            lastTimeHungry = DateTime.Now;
            ShowFeeling(CareType.Good);
            panparuAudio.PlayOneShot(happy, 1f);
            StartCoroutine(ShowHappy());
        }
        else
        {
            ShowFeeling(CareType.Okay);
            Debug.Log("I'm full!");
            panparuAudio.PlayOneShot(reject, 1f);
        }
            
    }
    public void Pet()
    {
        lastTimeAttention = DateTime.Now;
        ShowFeeling(CareType.Good);
        if (attention < 1) {
            attention += 1;
        }
        else
        {
            Debug.Log("I'm already happy!");
        }
        panparuAudio.PlayOneShot(happy, 1f);
        StartCoroutine(ShowHappy());

    }
    public void Play()
    {
        lastTimePlay = DateTime.Now;
        ShowFeeling(CareType.Good);
        if (play < 1) {
            play += 1;
        }
        else
        {
            Debug.Log("I'm tired!");
        }
        panparuAudio.PlayOneShot(happy, 1f);
        StartCoroutine(ShowHappy());
    }

    void ShowFeeling(CareType emotion)
    {
        feeling.SetActive(true);
        if(emotion == CareType.Good){
            feeling.GetComponent<Image>().sprite = goodSprite;
            panparuAudio.PlayOneShot(happy, 1f);
            StartCoroutine(ShowHappy());
        }
        else if(emotion == CareType.Okay){
            feeling.GetComponent<Image>().sprite = okaySprite;
        }
        else if(emotion == CareType.Bad){
            feeling.GetComponent<Image>().sprite = badSprite;
        }
        else if(emotion == CareType.Dead){
            feeling.GetComponent<Image>().sprite = deadSprite;
            panparuAudio.PlayOneShot(reject, 1f);
        }
        else{
            Debug.LogError("Invalid Emotion");
            feeling.GetComponent<Image>().sprite = deadSprite;
        }
        StartCoroutine(ShowFeelingCoroutine());
    }

    IEnumerator ShowFeelingCoroutine()
    {
        isChecking = true;
        yield return new WaitForSeconds(2);
        feeling.SetActive(false);
        isChecking = false;
    }

    void CheckEmotion(){  
        if (Button_Functions.Instance.isBusy || isChecking)
            return;
        ShowFeeling(currentCare);
    }

    void EvolveFromEggToChild(){
        if (currentCare == CareType.Dead)
        {
            return;
        }
        currentAge = Age.Child;
        eggCare = currentCare;
        switch (eggCare)
        {
            case CareType.Good://CG
                sprRdr.sprite = childSprites[0];
                print("CG");
                break;
            case CareType.Okay://CO
                sprRdr.sprite = childSprites[1];
                print("CO");
                break;
            case CareType.Bad://CB
                sprRdr.sprite = childSprites[2];
                print("CB");
                break;
        }
    }
    void EvolveFromChildToAdult(){
        if (currentCare == CareType.Dead)
        {
            return;
        }
        currentAge = Age.Adult;
        childCare = currentCare;
        switch (eggCare)
        {
            case CareType.Good://CG
                EvolveFromGoodChildToAdult();
                break;
            case CareType.Okay://CO
                EvolveFromOkayChildToAdult();
                break;
            case CareType.Bad://CB
                EvolveFromBadChildToAdult();
                break;             
        }
    }
    void EvolveFromGoodChildToAdult(){
        switch (childCare)
        {
            case CareType.Good://AGG
                sprRdr.sprite = adultSpritesFromGood[0];
                print("AGG");
                break;
            case CareType.Okay://AGO
                sprRdr.sprite = adultSpritesFromGood[1];
                print("AGO");
                break;
            case CareType.Bad://AGB
                sprRdr.sprite = adultSpritesFromGood[2];
                print("AGB");
                break;
        }
    }
    void EvolveFromOkayChildToAdult(){
        switch (childCare)
        {
            case CareType.Good://AOG
                sprRdr.sprite = adultSpritesFromOkay[0];
                print("AOG");
                break;
            case CareType.Okay://AOO
                sprRdr.sprite = adultSpritesFromOkay[1];
                print("AOO");
                break;
            case CareType.Bad://AOB
                sprRdr.sprite = adultSpritesFromOkay[2];
                print("AOB");
                break;
        }
    }
    void EvolveFromBadChildToAdult(){
        switch (childCare)
        {
            case CareType.Good://ABG
                sprRdr.sprite = adultSpritesFromBad[0];
                print("ABG");
                break;
            case CareType.Okay://ABO
                sprRdr.sprite = adultSpritesFromBad[1];
                print("ABO");
                break;
            case CareType.Bad://ABB
                sprRdr.sprite = adultSpritesFromBad[2];
                print("ABB");
                break;
        }
    }

    IEnumerator ShowHappy()
    {
        Sprite curSprite = Instance.GetComponent<Image>().sprite;
        var sp = Resources.Load(Instance.GetComponent<Image>().sprite.name + "(H)") as Sprite;
        print(Instance.GetComponent<Image>().sprite.name + "(H)");
        Instance.GetComponent<Image>().sprite = sp;
        yield return new WaitForSeconds(1);
        Instance.GetComponent<Image>().sprite = curSprite;
    }

    void Reset(){
        Button_Functions.Instance.ToggleButtons(true);
        DataManager.Instance.NewGame();
        m_Animator.enabled = true;
        Instance.GetComponent<Button>().onClick.RemoveListener(Reset);
        Instance.GetComponent<Button>().onClick.AddListener(CheckEmotion);
    }

    void Dead()
    {
        //Print current age in seconds
        Debug.Log("I died at " + (DateTime.Now - birthTime).TotalSeconds + " seconds old");

        Instance.GetComponent<Image>().sprite = tombstone;
        m_Animator.enabled = false;
        Button_Functions.Instance.ToggleButtons(false);
        Instance.GetComponent<Button>().onClick.RemoveListener(CheckEmotion);
        //add a thing where, on click everything is reset
        Instance.GetComponent<Button>().onClick.AddListener(Reset);
        
    }
}
