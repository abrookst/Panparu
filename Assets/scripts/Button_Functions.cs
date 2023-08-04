using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_Functions : MonoBehaviour
{

    public GameObject clock;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void ToggleTime()
    {
        clock.SetActive(!clock.activeInHierarchy);
    }
}
