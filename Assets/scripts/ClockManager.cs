using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using TMPro;
public class ClockManager : MonoBehaviour
{
    public TMP_Text textClock;
    public TMP_Text ageDisplay;
    private bool day;
    public swappable[] spriteSwapList;
    public Image[] backgrounds;

    void Awake()
    {
        DateTime time = DateTime.Now;
        if (time.Hour < 8 | time.Hour >= 20)
        {
            day = false;
        }
        else
        {
            day = true;
        }
        UpdateSprites(day);
    }


    void Update()
    {
        DateTime time = DateTime.Now;
        string hour = LeadingZero(time.Hour);
        string minute = LeadingZero(time.Minute);
        textClock.text = hour + ":" + minute;
        if(time.Hour < 8 | time.Hour >= 20)
        {
            if (day)
            {
                UpdateSprites(false);
            }
            day = false;
        }
        else
        {
            if (!day)
            {
                UpdateSprites(true);
            }
            day = true;
        }
        ageDisplay.text = ((int)(Panparu.Instance.lastTimeCheckCare - Panparu.Instance.birthTime).TotalDays).ToString() + " days\n" + (Panparu.Instance.lastTimeCheckCare - Panparu.Instance.birthTime).TotalHours.ToString("00") + " hours";
    }
    //night bgr 6461c2, day bgr f0dab1
    
    void UpdateSprites(bool day)
    {
        foreach(swappable s in spriteSwapList)
        {
            s.swap(day);
        }
        if (day)
        {
            foreach(Image i in backgrounds)
            {
                i.color = new Color32(240, 218, 177,255);
            }
            //day txt 634B7D
        }
        else
        {
            foreach(Image i in backgrounds)
            {
                i.color = new Color32(100, 97, 194,255);
            }
            //night txt f0f6e8
        }
    }
    string LeadingZero(int n)
    {
        return n.ToString().PadLeft(2, '0');
    }

}