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

    void Start()
    {
        birthTime = DateTime.Now;
        lastTimeHungry = DateTime.Now;
    }
    void Update()
    {
        DateTime timeNow = DateTime.Now;
        //Get difference between lastTimeHungry and timeNow
        TimeSpan timeSinceHungry = timeNow - lastTimeHungry;
        //If timeSinceHungry is greater than 6 seconds, reduce food by 1
        if (timeSinceHungry.Seconds > 6) {
            food -= 1;
            lastTimeHungry = timeNow;
        }
    }

    public void Feed()
    {
        if (food < 4)
            food += 1;
        else
            Debug.Log("I'm full!");
    }
}
