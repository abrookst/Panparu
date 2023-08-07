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
    private DateTime lastTimeAttention;
    private DateTime lastTimePlay;
    public static Panparu Instance { get; private set; }

    TimeSpan attentionCooldown = new(0, 0, 12);
    TimeSpan playCooldown = new(0, 0, 12);

    void Start()
    {
        Instance = this;
        birthTime = DateTime.Now;
        lastTimeHungry = DateTime.Now;
        lastTimeAttention = DateTime.Now;
        lastTimePlay = DateTime.Now;
    }
    void Update()
    {
        TimeSpan hungerCooldown = new(0, 0, 6);
        DateTime timeNow = DateTime.Now;
        //Get difference between lastTimeHungry and timeNow
        TimeSpan timeSinceHungry = timeNow - lastTimeHungry;
        //If timeSinceHungry is greater than 6 seconds, reduce food by 1
        while (timeSinceHungry.CompareTo(hungerCooldown) > 0) {
            food -= 1;
            lastTimeHungry = lastTimeHungry.Add(hungerCooldown);
            timeSinceHungry = timeNow - lastTimeHungry;
        }

        TimeSpan timeSinceAttention = timeNow - lastTimeAttention;
        if (timeSinceAttention.CompareTo(attentionCooldown) > 0)
        {
            if (attention > 0)
                attention -= 1;
            lastTimeAttention = lastTimeAttention.Add(attentionCooldown);
        }

        TimeSpan timeSincePlay = timeNow - lastTimePlay;
        if (timeSincePlay.CompareTo(playCooldown) > 0)
        {
            if (play > 0)
                play -= 1;
            lastTimePlay = lastTimePlay.Add(playCooldown);
        }
    }

    public void Feed()
    {
        if (food < 4) {
            food += 1;
            lastTimeHungry = DateTime.Now;
        } else
            Debug.Log("I'm full!");
    }
    public void Pet()
    {
        if (attention < 1) {
            attention += 1;
            lastTimeAttention = DateTime.Now;
        }
        else
            Debug.Log("I'm already happy!");
    }
    public void Play()
    {
        if (play < 1) {
            play += 1;
            lastTimePlay = DateTime.Now;
        }
        else
            Debug.Log("I'm tired!");
    }
}
