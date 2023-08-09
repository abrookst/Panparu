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

    public static Button_Functions Instance { get; private set; }

    //Get button references
    [SerializeField] private Button FeedButton;
    [SerializeField] private Button PetButton;
    [SerializeField] private Button PlayButton;
    [SerializeField] private Button TimeButton;

    public AudioClip pet;
    public AudioSource buttonAudio;

    public GameObject feeling;
    public bool isBusy = false;

    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    public void ToggleTime()
    {
        clock.SetActive(!clock.activeInHierarchy);
    }

    public void Feed() {
        feeling.SetActive(false);
        if(Panparu.Instance.GetFood() < 4){
            StartCoroutine(FeedCoroutine());
            
        }
        else{
            Panparu.Instance.Feed();
        }
    }
    IEnumerator FeedCoroutine() {
        ToggleButtons(false);
        isBusy = true;

        food.SetActive(true);
        
        //Play rotate animation on Panparu
        Panparu.Instance.GetComponent<Animator>().SetTrigger("Feed");
        yield return new WaitForSeconds(.5f);
        buttonAudio.PlayOneShot(pet, 1f);
        yield return new WaitForSeconds(1f);
        buttonAudio.PlayOneShot(pet, 1f);
        yield return new WaitForSeconds(.5f);


        food.SetActive(false);

        Panparu.Instance.Feed();

        ToggleButtons(true);
        isBusy = false;
        
    }
    public void Pet() {
        feeling.SetActive(false);
        StartCoroutine(PetCoroutine());
    }
    IEnumerator PetCoroutine() {
        ToggleButtons(false);
        isBusy = true;

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
            buttonAudio.PlayOneShot(pet, 1f);
            petting = false;
            Panparu.Instance.GetComponent<Animator>().SetTrigger("Pet");
            yield return new WaitForSeconds(0.5f);
            timesToPet--;
            panparuButton.interactable = true;
        }
        panparuButton.onClick.RemoveListener(getPet);

        Panparu.Instance.Pet();

        petClick.SetActive(false);

        ToggleButtons(true);
        isBusy = false;
    }
    public void Play() {
        feeling.SetActive(false);
        StartCoroutine(PlayCoroutine());
    }
    IEnumerator PlayCoroutine() {
        ToggleButtons(false);
        isBusy = true;

        playClick.SetActive(true);

        //Play rotate animation on Panparu
        Panparu.Instance.GetComponent<Animator>().SetTrigger("Play");
        yield return new WaitForSeconds(2);

        Panparu.Instance.Play();

        playClick.SetActive(false);

        ToggleButtons(true);
        isBusy = false;
    }

    public void ToggleButtons(bool on)
    {
        FeedButton.interactable = on;
        PetButton.interactable = on;
        PlayButton.interactable = on;
        TimeButton.interactable = on;
    }

}
