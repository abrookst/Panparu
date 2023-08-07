using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_Functions : MonoBehaviour
{

    [SerializeField] private GameObject clock;
    [SerializeField] private GameObject food;
    [SerializeField] private GameObject petClick;
    [SerializeField] private GameObject playClick;

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
    public void Pet() {
        StartCoroutine(PetCoroutine());
    }
    IEnumerator PetCoroutine() {
        FeedButton.interactable = false;
        PetButton.interactable = false;
        PlayButton.interactable = false;
        TimeButton.interactable = false;

        petClick.SetActive(true);

        //Play rotate animation on Panparu
        int timesToPet = 3;
        bool petting = false;
        Button panparuButton = Panparu.Instance.GetComponent<Button>();
        void getPet() {
            petting = true;
        }
        panparuButton.onClick.AddListener(getPet);
        while (timesToPet > 0) {
            yield return new WaitUntil(() => petting);
            panparuButton.interactable = false;
            petting = false;
            yield return StartCoroutine(PettingCoroutine());
            timesToPet--;
            panparuButton.interactable = true;
        }
        panparuButton.onClick.RemoveListener(getPet);

        Panparu.Instance.Pet();

        petClick.SetActive(false);

        FeedButton.interactable = true;
        PetButton.interactable = true;
        PlayButton.interactable = true;
        TimeButton.interactable = true;
    }
    IEnumerator PettingCoroutine()
    {
        //Play pet animation on Panparu
        Panparu.Instance.GetComponent<Animator>().SetTrigger("Pet");
        yield return new WaitForSeconds(1);
    }
    public void Play() {
        StartCoroutine(PlayCoroutine());
    }
    IEnumerator PlayCoroutine() {
        FeedButton.interactable = false;
        PetButton.interactable = false;
        PlayButton.interactable = false;
        TimeButton.interactable = false;

        playClick.SetActive(true);

        //Play rotate animation on Panparu
        Panparu.Instance.GetComponent<Animator>().SetTrigger("Play");
        yield return new WaitForSeconds(2);

        Panparu.Instance.Play();

        playClick.SetActive(false);

        FeedButton.interactable = true;
        PetButton.interactable = true;
        PlayButton.interactable = true;
        TimeButton.interactable = true;
    }
}
