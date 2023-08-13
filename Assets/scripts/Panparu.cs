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
    public enum Age { Egg, Baby, Child, Adult };

    public DateTime birthTime;
    private DateTime lastTimeEvolve;
    private DateTime lastTimeHungry;
    public DateTime lastTimeCheckCare;
    private DateTime lastTimeAttention;
    private DateTime lastTimePlay;
    [SerializeField] private float averageCare = 1f;
    [SerializeField] public int highscore = 0;
    public static Panparu Instance { get; private set; }
    readonly TimeSpan checkCareCooldown = new(1, 0, 0);
    readonly TimeSpan hungerCooldown = new(6, 0, 0);
    readonly TimeSpan attentionCooldown = new(12, 0, 0);
    readonly TimeSpan playCooldown = new(12, 0, 0);

    readonly TimeSpan babyToChild = new(24, 0, 0);
    readonly TimeSpan childToAdult = new(60, 0, 0);

    [SerializeField] private CareType currentCare; 
    [SerializeField] private CareType babyCare;
    [SerializeField] private CareType childCare;
    [SerializeField] private Age currentAge;
    private int eggType;

    [SerializeField] private GameObject feeling;
    [SerializeField] private Sprite goodSprite;
    [SerializeField] private Sprite okaySprite;
    [SerializeField] private Sprite badSprite;
    [SerializeField] private Sprite deadSprite;
    [SerializeField] private Sprite babySprite;
    [SerializeField] private Sprite[] childSprites;
    [SerializeField] private Sprite[] adultSpritesFromGood;
    [SerializeField] private Sprite[] adultSpritesFromOkay;
    [SerializeField] private Sprite[] adultSpritesFromBad;

    public AudioClip happy;
    public AudioClip neutral;
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
        babyCare = data.babyCare;
        childCare = data.childCare;
        currentAge = data.currentAge;
        birthTime = new DateTime(data.birthTime);
        lastTimeEvolve = new DateTime(data.lastTimeEvolve);
        lastTimeHungry = new DateTime(data.lastTimeHungry);
        lastTimeCheckCare = new DateTime(data.lastTimeCheckCare);
        lastTimeAttention = new DateTime(data.lastTimeAttention);
        lastTimePlay = new DateTime(data.lastTimePlay);
        highscore = data.highscore;
        eggType = data.eggType;

        sprRdr = gameObject.GetComponent<Image>();
        m_Animator = gameObject.GetComponent<Animator>();
        m_Animator.enabled = true;

        UpdateCurrentCare();

        RecalcSprite();

        Instance.GetComponent<Button>().onClick.AddListener(CheckEmotion);
        
        initialized = true;
        print("Age: " + (DateTime.Now - birthTime).TotalHours);
        if (currentAge == Age.Egg)
        {
           Button_Functions.Instance.Pet();
        }
    }

    public void RecalcSprite()
    {
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
        else if (currentAge == Age.Baby) {
            sprRdr.sprite = babySprite;
        }
        else if (currentAge == Age.Child)
        {
            //Calculate what sprite it should have based on how it was treated in the egg
            if (babyCare == CareType.Good)
            {
                sprRdr.sprite = childSprites[0];
            }
            else if (babyCare == CareType.Okay)
            {
                sprRdr.sprite = childSprites[1];
            }
            else if (babyCare == CareType.Bad)
            {
                sprRdr.sprite = childSprites[2];
            }
        }
        else
        {
            if (highscore >= 10000) {
                sprRdr.sprite = adultSpritesFromGood[3];
            }
            //Calculate what sprite it should have based on how it was treated in the egg and as a child
            else if (babyCare == CareType.Good)
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
            else if (babyCare == CareType.Okay)
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
            else if (babyCare == CareType.Bad)
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
    }

    public PanparuData GetPanparuData(){
        PanparuData data = new()
        {
            food = food,
            attention = attention,
            play = play,
            averageCare = averageCare,
            babyCare = babyCare,
            childCare = childCare,
            currentAge = currentAge,
            birthTime = birthTime.Ticks,
            lastTimeEvolve = lastTimeEvolve.Ticks,
            lastTimeHungry = lastTimeHungry.Ticks,
            lastTimeCheckCare = lastTimeCheckCare.Ticks,
            lastTimeAttention = lastTimeAttention.Ticks,
            lastTimePlay = lastTimePlay.Ticks,
            highscore = highscore,
            eggType = eggType
        };
        return data;
    }

    void Update() 
    {
        if (!initialized || currentCare == CareType.Dead || currentAge == Age.Egg) 
            return;

        DateTime timeNow = DateTime.Now;
        //Get difference cooldown times
        TimeSpan timeSinceCheckCare = timeNow - lastTimeCheckCare;

        while (timeSinceCheckCare.CompareTo(checkCareCooldown) >= 0)
        {
            if (currentCare == CareType.Dead) {
                return;
            }
            lastTimeCheckCare = lastTimeCheckCare.Add(checkCareCooldown);

            TimeSpan timeSinceHungry = lastTimeCheckCare - lastTimeHungry;
            TimeSpan timeSinceAttention = lastTimeCheckCare - lastTimeAttention;
            TimeSpan timeSincePlay = lastTimeCheckCare - lastTimePlay;
            if (timeSinceHungry.CompareTo(hungerCooldown) >= 0) {
                food -= 1;
                lastTimeHungry = lastTimeHungry.Add(hungerCooldown);
                //print("-1 Hunger!");
            }
            
            if (timeSinceAttention.CompareTo(attentionCooldown) >= 0)
            {
                if (attention > 0)
                    attention -= 1;
                lastTimeAttention = lastTimeAttention.Add(attentionCooldown);
            }

            if (timeSincePlay.CompareTo(playCooldown) >= 0)
            {
                if (play > 0)
                    play -= 1;
                lastTimePlay = lastTimePlay.Add(playCooldown);
            }
            
            TimeSpan timeSinceBirth = lastTimeCheckCare - birthTime;
            TimeSpan timeSinceEvolve = lastTimeCheckCare - lastTimeEvolve;
            TimeSpan avgTime = (timeSinceBirth + timeSinceEvolve) / 2;
            //print("TimeSPAN since birth: " + timeSinceBirth.Hours + " Hours");
            averageCare = (averageCare * (float)avgTime.TotalHours + CalcCare()) / ((float)avgTime.TotalHours + 1);
            timeSinceCheckCare = timeNow - lastTimeCheckCare;
            
            UpdateCurrentCare();
            if (currentCare == CareType.Dead) {
                return;
            }
            if (timeSinceBirth.CompareTo(babyToChild) >= 0 && currentAge == Age.Baby) {
                EvolveFromBabyToChild();
                averageCare = 1f;
                UpdateCurrentCare();
            }
            if (timeSinceBirth.CompareTo(childToAdult) >= 0 && currentAge == Age.Child) {
                EvolveFromChildToAdult();
                averageCare = 1f;
                UpdateCurrentCare();
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
        if (currentCare == CareType.Dead) {
            return;
        }
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
        DataManager.Instance.SaveGame();
    }
    public void Pet()
    {
        if (currentCare == CareType.Dead) {
            return;
        }
        if (currentAge == Age.Egg) {
            EvolveFromEggToBaby();
            averageCare = 1f;
            DataManager.Instance.SaveGame();
            panparuAudio.PlayOneShot(happy, 1f);
            StartCoroutine(ShowHappy());
            return;
        }
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
        DataManager.Instance.SaveGame();
    }
    public void Play()
    {
        if (currentCare == CareType.Dead) {
            return;
        }
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
        DataManager.Instance.SaveGame();
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
            panparuAudio.PlayOneShot(neutral, 0.5f);
        }
        else if(emotion == CareType.Bad){
            feeling.GetComponent<Image>().sprite = badSprite;
            panparuAudio.PlayOneShot(reject, 1f);
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
    void EvolveFromEggToBaby(){
        if (currentCare == CareType.Dead) {
            return;
        }
        currentAge = Age.Baby;
        lastTimeEvolve = DateTime.Now;
        sprRdr.sprite = babySprite;
        birthTime = DateTime.Now;
        lastTimeHungry = DateTime.Now;
        lastTimeAttention = DateTime.Now;
        lastTimePlay = DateTime.Now;
        lastTimeCheckCare = DateTime.Now;
        averageCare = 1f;
    }

    void EvolveFromBabyToChild(){
        if (currentCare == CareType.Dead) {
            return;
        }
        currentAge = Age.Child;
        babyCare = currentCare;
        lastTimeEvolve = lastTimeCheckCare;
        switch (babyCare)
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
        lastTimeEvolve = lastTimeCheckCare;
        if (highscore >= 10000) {
            sprRdr.sprite = adultSpritesFromGood[3];
            return;
        }
        switch (babyCare)
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
        if (currentCare == CareType.Dead)
        {
            yield break;
        }
        if(currentAge == Age.Egg)
        {
            yield break;
        }
        Sprite sp = Resources.Load<Sprite>(Instance.GetComponent<Image>().sprite.name + "(H)") as Sprite;
        if (currentCare == CareType.Dead || sp == null)
        {
            print(Instance.GetComponent<Image>().sprite.name + "(H)" + " not found");
            yield break;
        }
        print("sp: "+sp.name);
        sprRdr.sprite = sp;
        yield return new WaitForSeconds(1);
        RecalcSprite();
    }

    void Reset(){
        DataManager.Instance.NewGame();
        m_Animator.enabled = true;
        Instance.GetComponent<Button>().onClick.RemoveListener(Reset);
        Instance.GetComponent<Button>().onClick.AddListener(CheckEmotion);
    }

    void Dead()
    {
        //Print current age in Hours
        Debug.Log("I died at " + lastTimeCheckCare.ToString("yyyy-MM-dd\\THH:mm:ss\\Z"));
        Debug.Log("I lived for " + (lastTimeCheckCare - birthTime).TotalHours + " Hours");

        Instance.GetComponent<Image>().sprite = tombstone;
        m_Animator.enabled = false;
        Button_Functions.Instance.ToggleButtons(false);
        Instance.GetComponent<Button>().onClick.RemoveListener(CheckEmotion);
        Instance.GetComponent<Button>().onClick.AddListener(Reset);
        
    }
}
