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

    enum CareType { Good, Okay, Bad, Dead};
    enum Age { Egg, Child, Adult };

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

    [SerializeField] private GameObject feeling;
    [SerializeField] private Sprite goodSprite;
    [SerializeField] private Sprite okaySprite;
    [SerializeField] private Sprite badSprite;
    [SerializeField] private Sprite deadSprite;
    [SerializeField] private Sprite[] childSprites;
    [SerializeField] private Sprite[] adultSpritesFromGood;
    [SerializeField] private Sprite[] adultSpritesFromOkay;
    [SerializeField] private Sprite[] adultSpritesFromBad;

    private Image sprRdr;

    private Animator m_Animator;

    bool isChecking = false;

    public Sprite[] eggSprites;

    public Sprite tombstone;

    public int GetFood(){
        return food;
    }

    void Start()
    {
        Instance = this;
        //NOTE these need to be saved and reloaded accurately on close
        birthTime = DateTime.Now;
        currentAge = Age.Egg;
        //end note
        lastTimeHungry = DateTime.Now;
        lastTimeCheckCare = DateTime.Now;
        lastTimeAttention = DateTime.Now;
        lastTimePlay = DateTime.Now;

        sprRdr = gameObject.GetComponent<Image>();
        m_Animator = gameObject.GetComponent<Animator>();

        Instance.GetComponent<Image>().sprite = SetEgg();

        Instance.GetComponent<Button>().onClick.AddListener(CheckEmotion);
    }

    void Update() 
    {
        #if UNITY_EDITOR //Fix bug with reloading scripts in editor causing variables to reset, which causes Panparu to loose tons of hunger
        if (Instance == null)
            Instance = this;
        if (lastTimeHungry == default)
            lastTimeHungry = DateTime.Now;
        if (lastTimeCheckCare == default)
            lastTimeCheckCare = DateTime.Now;
        if (lastTimeAttention == default)
            lastTimeAttention = DateTime.Now;
        if (lastTimePlay == default)
            lastTimePlay = DateTime.Now;
        #endif

        DateTime timeNow = DateTime.Now;
        //Get difference cooldown times
        TimeSpan timeSinceCheckCare = timeNow - lastTimeCheckCare;

        while (timeSinceCheckCare.CompareTo(checkCareCooldown) > 0)
        {
            TimeSpan timeSinceHungry = timeNow - lastTimeHungry;
            TimeSpan timeSinceAttention = timeNow - lastTimeAttention;
            TimeSpan timeSincePlay = timeNow - lastTimePlay;
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
            
            lastTimeCheckCare = lastTimeCheckCare.Add(checkCareCooldown);
            TimeSpan timeSinceBirth = lastTimeCheckCare - birthTime;
            averageCare = (averageCare*timeSinceBirth.Seconds + CalcCare()) / (timeSinceBirth.Seconds+1);
            timeSinceCheckCare = timeNow - lastTimeCheckCare;
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
                Dead();
            }
        }

        /*
        SETTING ANIMATION SPEED DEPENDING ON HEALTH:
        Borked bc this slows down stuff besides panparu_shift. ill figure it out tomorrow *yawn*
        */
        if (averageCare > .8)
        {
            currentCare = CareType.Good;
            //m_Animator.speed = 1f;
        }
        else if (averageCare > .6)
        {
            currentCare = CareType.Okay;
            //m_Animator.speed = .75f;
        }
        else if (averageCare > .4)
        {
            currentCare = CareType.Bad;
            //m_Animator.speed = .5f;
        }
        else
        {
            currentCare = CareType.Dead;
            //m_Animator.speed = .25f;
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
        } else
            ShowFeeling(CareType.Okay);
            Debug.Log("I'm full!");
    }
    public void Pet()
    {
        lastTimeAttention = DateTime.Now;
        ShowFeeling(CareType.Good);
        if (attention < 1) {
            attention += 1;
        }
        else
            Debug.Log("I'm already happy!");
    }
    public void Play()
    {
        lastTimePlay = DateTime.Now;
        ShowFeeling(CareType.Good);
        if (play < 1) {
            play += 1;
        }
        else
            Debug.Log("I'm tired!");
    }

    void ShowFeeling(CareType emotion)
    {
        feeling.SetActive(true);
        if(emotion == CareType.Good){
            feeling.GetComponent<Image>().sprite = goodSprite;
        }
        else if(emotion == CareType.Okay){
            feeling.GetComponent<Image>().sprite = okaySprite;
        }
        else if(emotion == CareType.Bad){
            feeling.GetComponent<Image>().sprite = badSprite;
        }
        else if(emotion == CareType.Dead){
            feeling.GetComponent<Image>().sprite = deadSprite;
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

    Sprite SetEgg()
    {
        int randEgg = Random.Range(0, 6);
        print("Rand:" + randEgg);
        return eggSprites[randEgg];
    }

    void EvolveFromEggToChild(){
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

    void reset(){
        birthTime = DateTime.Now;
        averageCare = 1f;
        childCare = default;
        eggCare = default;
        lastTimeHungry = default;
        lastTimeCheckCare = default;
        lastTimeAttention = default;
        lastTimePlay = default;
        food = 4;
        attention = 1;
        play = 1;
        m_Animator.enabled = true;
        Start();
        Instance.GetComponent<Button>().onClick.RemoveListener(reset);
        Instance.GetComponent<Button>().onClick.AddListener(CheckEmotion);
    }

    void Dead()
    {
        Instance.GetComponent<Image>().sprite = tombstone;
        m_Animator.enabled = false;
        Button_Functions.Instance.ToggleButtons(false);
        Instance.GetComponent<Button>().onClick.RemoveListener(CheckEmotion);
        //add a thing where, on click everything is reset
        Instance.GetComponent<Button>().onClick.AddListener(reset);
        
    }
}
