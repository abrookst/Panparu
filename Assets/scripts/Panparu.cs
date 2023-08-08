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
    private float averageCare = 1f;
    public static Panparu Instance { get; private set; }
    readonly TimeSpan checkCareCooldown = new(0, 0, 1);
    readonly TimeSpan hungerCooldown = new(0, 0, 6);
    readonly TimeSpan attentionCooldown = new(0, 0, 12);
    readonly TimeSpan playCooldown = new(0, 0, 12);

    private Age currentAge;
    private CareType childCare;
    private CareType currentCare; 

    public GameObject feeling;
    public Sprite goodSprite;
    public Sprite okaySprite;
    public Sprite badSprite;
    public Sprite deadSprite;

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
        currentAge = Age.Child;
        //end note
        lastTimeHungry = DateTime.Now;
        lastTimeCheckCare = DateTime.Now;
        lastTimeAttention = DateTime.Now;
        lastTimePlay = DateTime.Now;

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
        TimeSpan timeSinceBirth = lastTimeCheckCare - birthTime;

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
            averageCare = (averageCare*timeSinceBirth.Seconds + CalcCare()) / (timeSinceBirth.Seconds+1);
            print(averageCare);
            timeSinceCheckCare = timeNow - lastTimeCheckCare;
        }

        /*
        SETTING ANIMATION SPEED DEPENDING ON HEALTH:
        Borked bc this slows down stuff besides panparu_shift. ill figure it out tomorrow *yawn*
        if(averageCare > .8){
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
        if ((timeSinceBirth.Seconds > 24 && currentAge == Age.Egg) || (timeSinceBirth.Seconds > 60 && currentAge == Age.Child))
        {
            Evolve();
        }
        if(averageCare < 0)
        {
            Dead();
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

    void Evolve()
    {
        birthTime = DateTime.Now;
        if (currentAge == Age.Egg)
        {
            currentAge = Age.Child;
            if (currentCare == CareType.Good)
            {
                //set to CG
                childCare = CareType.Good;

            }
            else if (currentCare == CareType.Okay)
            {
                //set to CO
                childCare = CareType.Okay;
            }
            else
            {
                //set to CB
                childCare = CareType.Bad;
            }
        }
        else
        {
            currentAge = Age.Adult;
            if (currentCare == CareType.Good)
            {
                if (childCare == CareType.Good)
                {
                    //set to AGG
                }
                else if (childCare == CareType.Okay)
                {
                    //set to AGO
                }
                else
                {
                    //set to AGB
                }
            }
            else if (currentCare == CareType.Okay)
            {
                if (childCare == CareType.Good)
                {
                    //set to AOG
                }
                else if (childCare == CareType.Okay)
                {
                    //set to AOO
                }
                else
                {
                    //set to AOB
                }
            }
            else
            {
                if (childCare == CareType.Good)
                {
                    //set to ABG
                }
                else if (childCare == CareType.Okay)
                {
                    //set to ABO
                }
                else
                {
                    //set to ABB
                }
            }
        }
    }

    void Dead()
    {
        Instance.GetComponent<Image>().sprite = tombstone;
        m_Animator.enabled = false;
        Button_Functions.Instance.ToggleButtons(false);
        Instance.GetComponent<Button>().onClick.RemoveListener(CheckEmotion);
        //add a thing where, on click everything is reset
    }
}
