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
    public static Panparu Instance { get; private set; }

    void Start()
    {
        Instance = this;
        birthTime = DateTime.Now;
        lastTimeHungry = DateTime.Now;
    }
    void Update()
    {
        TimeSpan hungerCooldown = new(0, 0, 6);
        DateTime timeNow = DateTime.Now;
        //Get difference between lastTimeHungry and timeNow
        TimeSpan timeSinceHungry = timeNow - lastTimeHungry;
        //If timeSinceHungry is greater than 6 seconds, reduce food by 1
        //Repeat until timeSinceHungry is less than 6 seconds
        while (timeSinceHungry.CompareTo(hungerCooldown) > 0) {
            food -= 1;
            lastTimeHungry = lastTimeHungry.Add(hungerCooldown);
            timeSinceHungry = timeNow - lastTimeHungry;
        }
        print(food);
    }

    public void Feed()
    {
        if (food < 4)
            food += 1;
        else
            Debug.Log("I'm full!");
    }
    public void Pet()
    {
        if (attention < 1)
            attention += 1;
        else
            Debug.Log("I'm already happy!");
    }
    public void Play()
    {
        if (play < 1)
            play += 1;
        else
            Debug.Log("I'm tired!");
    }
}
