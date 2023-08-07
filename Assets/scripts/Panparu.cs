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
    private float averageCare = 1f;
    public static Panparu Instance { get; private set; }
    TimeSpan hungerCooldown = new(0, 0, 6);
    TimeSpan checkCareCooldown = new(0, 0, 1);

    void Start()
    {
        Instance = this;
        birthTime = DateTime.Now;
        lastTimeHungry = DateTime.Now;
        lastTimeCheckCare = DateTime.Now;
    }
    void Update()
    {
        
        DateTime timeNow = DateTime.Now;
        //Get difference cooldown times
        TimeSpan timeSinceHungry = timeNow - lastTimeHungry;
        TimeSpan timeSinceCheckCare = timeNow - lastTimeCheckCare;
        //Check to see if the time since the last cooldown is greater than our cooldowns
        if (timeSinceHungry.CompareTo(hungerCooldown) > 0) {
            food -= 1;
            lastTimeHungry = lastTimeHungry.Add(hungerCooldown);
            //print("-1 Hunger!");
        }
        if (timeSinceCheckCare.CompareTo(checkCareCooldown) > 0)
        {
            TimeSpan timeSinceBirth = timeNow - birthTime;
            averageCare = (averageCare*timeSinceBirth.Seconds + CalcCare()) / (timeSinceBirth.Seconds+1);
            //print(averageCare);
            lastTimeCheckCare = lastTimeCheckCare.Add(checkCareCooldown);
        }

    }

    float CalcCare()
    {
        return ((food / 4f) + attention/1f + play/1f) / 3f;
    }

    public void Feed()
    {
        if (food < 4)
            food += 1;
        else
            Debug.Log("I'm full!");
    }
}
