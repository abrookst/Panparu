using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Panparu : MonoBehaviour
{
    [SerializeField] private int food = 4;
    [SerializeField] private int attention = 1;
    [SerializeField] private int play = 1;

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

    public GameObject feeling;
    public Sprite goodSprite;
    public Sprite okaySprite;
    public Sprite badSprite;
    public Sprite deadSprite;

    private Animator m_Animator;


    public int GetFood(){
        return food;
    }

    void Start()
    {
        Instance = this;
        birthTime = DateTime.Now;
        lastTimeHungry = DateTime.Now;
        lastTimeCheckCare = DateTime.Now;
        lastTimeAttention = DateTime.Now;
        lastTimePlay = DateTime.Now;

        m_Animator = gameObject.GetComponent<Animator>();

        enableChecking();
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
            print(averageCare);
            timeSinceCheckCare = timeNow - lastTimeCheckCare;
        }

        /*
        SETTING ANIMATION SPEED DEPENDING ON HEALTH:
        Borked bc this slows down stuff besides panparu_shift. ill figure it out tomorrow *yawn*
        if(averageCare > .8){
            m_Animator.speed = 1f;
        }
        else if(averageCare > .6){
            m_Animator.speed = .75f;
        }
        else if(averageCare > .4){
            m_Animator.speed = .5f;
        }
        else{
            m_Animator.speed = .25f;
        }
        */


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
            ShowFeeling("good");
        } else
            ShowFeeling("okay");
            Debug.Log("I'm full!");
    }
    public void Pet()
    {
        lastTimeAttention = DateTime.Now;
        ShowFeeling("good");
        if (attention < 1) {
            attention += 1;
        }
        else
            Debug.Log("I'm already happy!");
    }
    public void Play()
    {
        lastTimePlay = DateTime.Now;
        ShowFeeling("good");
        if (play < 1) {
            play += 1;
        }
        else
            Debug.Log("I'm tired!");
    }

    void ShowFeeling(string emotion)
    {
        feeling.SetActive(true);
        if(emotion == "good"){
            feeling.GetComponent<Image>().sprite = goodSprite;
        }
        else if(emotion == "okay"){
            feeling.GetComponent<Image>().sprite = okaySprite;
        }
        else if(emotion == "bad"){
            feeling.GetComponent<Image>().sprite = badSprite;
        }
        else if(emotion == "dead"){
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
        yield return new WaitForSeconds(2);
        feeling.SetActive(false);
    }

    void checkEmotion(){
        if(averageCare > .8){
            ShowFeeling("good");
        }
        else if(averageCare > .6){
            ShowFeeling("okay");
        }
        else if(averageCare > .4){
            ShowFeeling("bad");
        }
        else{
            ShowFeeling("dead");
        }
    }

    public void enableChecking(){
        Instance.GetComponent<Button>().onClick.AddListener(checkEmotion);
    }
    public void disableChecking(){
        Instance.GetComponent<Button>().onClick.RemoveListener(checkEmotion);
    }
}
