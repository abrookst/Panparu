using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class swappable : MonoBehaviour
{
    public Sprite daySprite;
    public Sprite nightSprite;
    

    public void swap(bool day)
    {
        if (day)
        {
            gameObject.GetComponent<Image>().sprite = daySprite;
        }
        else
        {
            gameObject.GetComponent<Image>().sprite = nightSprite;
        }
    }
    
}
