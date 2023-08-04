using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using TMPro;
public class ClockManager : MonoBehaviour
{
    private TMP_Text textClock;

    private void Awake()
    {
        textClock = GetComponent<TMP_Text>();
    }
    void Update()
    {
        DateTime time = DateTime.Now;
        string hour = LeadingZero(time.Hour);
        string minute = LeadingZero(time.Minute);
        textClock.text = hour + ":" + minute;
    }
    string LeadingZero(int n)
    {
        return n.ToString().PadLeft(2, '0');
    }

}