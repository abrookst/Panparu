using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
[Serializable]
public class PanparuData {
    [SerializeField] public int food = 3;
    [SerializeField] public int attention = 1;
    [SerializeField] public int play = 1;

    [SerializeField] public long birthTime;
    [SerializeField] public long lastTimeEvolve;
    [SerializeField] public long lastTimeHungry;
    [SerializeField] public long lastTimeCheckCare;
    [SerializeField] public long lastTimeAttention;
    [SerializeField] public long lastTimePlay;
    [SerializeField] public float averageCare = 1f;
    [SerializeField] public int eggType = -1;
    [SerializeField] public bool secret = false;

    [SerializeField] public Panparu.CareType babyCare;
    [SerializeField] public Panparu.CareType childCare;
    [SerializeField] public Panparu.Age currentAge;
    public PanparuData() {
        this.food = 3;
        this.attention = 1;
        this.play = 1;
        this.birthTime = DateTime.Now.Ticks;
        this.lastTimeEvolve = DateTime.Now.Ticks;
        this.lastTimeHungry = DateTime.Now.Ticks;
        this.lastTimeCheckCare = DateTime.Now.Ticks;
        this.lastTimeAttention = DateTime.Now.Ticks;
        this.lastTimePlay = DateTime.Now.Ticks;
        this.babyCare = Panparu.CareType.Good;
        this.childCare = Panparu.CareType.Good;
        this.currentAge = Panparu.Age.Egg;
        this.eggType = -1;
        this.secret = false;
        this.averageCare = 1f;
    }
}

public class DataManager : MonoBehaviour
{
    [SerializeField] private string fileName;
    private FileDataHandler fileDataHandler;
    public static DataManager Instance {get; private set;}
    private PanparuData panparuData;
    // Start is called before the first frame update
    void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
            print("Duplicate game object destroyed");
            return;
        }
        print(Application.persistentDataPath);
    }
    void Start()
    {
        fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        LoadGame();
    }
    public void NewGame(){
        panparuData = new PanparuData();
        Panparu.Instance.Initialize(panparuData);
        fileDataHandler.SaveGame(panparuData);
    }
    public void SaveGame(){
        panparuData = Panparu.Instance.GetPanparuData();
        fileDataHandler.SaveGame(panparuData);
    }
    public void LoadGame(){
        panparuData = fileDataHandler.LoadGame();
        if (panparuData == null) {
            print("No save data");
            NewGame();
            return;
        }
        Panparu.Instance.Initialize(panparuData);
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}
