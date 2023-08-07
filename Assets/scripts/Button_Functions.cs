using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_Functions : MonoBehaviour
{

    [SerializeField] private GameObject clock;
    [SerializeField] private GameObject food;

    //Get button references
    [SerializeField] private Button FeedButton;
    [SerializeField] private Button PetButton;
    [SerializeField] private Button PlayButton;
    [SerializeField] private Button TimeButton;

    // Update is called once per frame
    public void ToggleTime()
    {
        clock.SetActive(!clock.activeInHierarchy);
    }

    public void Feed() {
        StartCoroutine(FeedCoroutine());
    }
    IEnumerator FeedCoroutine() {
        FeedButton.interactable = false;
        PetButton.interactable = false;
        PlayButton.interactable = false;
        TimeButton.interactable = false;

        food.SetActive(true);
        
        //Play rotate animation on Panparu
        Panparu.Instance.GetComponent<Animator>().SetTrigger("Feed");
        yield return new WaitForSeconds(2);

        Panparu.Instance.Feed();

        food.SetActive(false);

        FeedButton.interactable = true;
        PetButton.interactable = true;
        PlayButton.interactable = true;
        TimeButton.interactable = true;
        
    }
}
