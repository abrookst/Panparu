using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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

    void Start()
    {
        Instance = this;
        birthTime = DateTime.Now;
        lastTimeHungry = DateTime.Now;
        lastTimeCheckCare = DateTime.Now;
        lastTimeAttention = DateTime.Now;
        lastTimePlay = DateTime.Now;
    }
    void Update() {
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
        TimeSpan timeSinceHungry = timeNow - lastTimeHungry;
        TimeSpan timeSinceCheckCare = timeNow - lastTimeCheckCare;
        TimeSpan timeSinceAttention = timeNow - lastTimeAttention;
        TimeSpan timeSincePlay = timeNow - lastTimePlay;
        while (timeSinceHungry.CompareTo(hungerCooldown) > 0) {
            food -= 1;
            lastTimeHungry = lastTimeHungry.Add(hungerCooldown);
            timeSinceHungry = timeNow - lastTimeHungry;
            //print("-1 Hunger!");
        }
        
        if (timeSinceCheckCare.CompareTo(checkCareCooldown) > 0)
        {
            TimeSpan timeSinceBirth = timeNow - birthTime;
            averageCare = (averageCare*timeSinceBirth.Seconds + CalcCare()) / (timeSinceBirth.Seconds+1);
            print(averageCare);
            lastTimeCheckCare = lastTimeCheckCare.Add(checkCareCooldown);
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

    }

    float CalcCare()
    {
        return ((food / 4f) + attention/1f + play/1f) / 3f;
    }

    public void Feed()
    {
        if (food < 4) {
            food += 1;
            lastTimeHungry.Add(new TimeSpan(0, 0, 1));
        } else
            Debug.Log("I'm full!");
    }
    public void Pet()
    {
        if (attention < 1) {
            attention += 1;
            lastTimeAttention.Add(new TimeSpan(0, 0, 1));
        }
        else
            Debug.Log("I'm already happy!");
    }
    public void Play()
    {
        if (play < 1) {
            play += 1;
            lastTimePlay.Add(new TimeSpan(0, 0, 1));
        }
        else
            Debug.Log("I'm tired!");
    }
}
