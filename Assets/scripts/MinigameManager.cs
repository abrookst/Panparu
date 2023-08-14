using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinigameManager : MonoBehaviour
{
    public enum ControlMode { mouse, keyboard, mobile }
    public RectTransform m_parent;
    public Camera m_camera;
    // private static Canvas canvas;
    [SerializeField] private GameObject MiniGame;
    [SerializeField] private GameObject PanparuPlayer;
    [SerializeField] public GameObject PantrisBorder;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject PauseIcon;
    [SerializeField] private GameObject MouseModeInstructions;
    [SerializeField] private GameObject KeyboardModeInstructions;
    public static float moveTime = 0.04f;
    public static float dropTime = 0.6f;
    public static float fallTime = 0.02f;
    public static int minX = 1, maxX = 22, minY = 1, maxY = 41;
    [SerializeField] GameObject[] blocks;
    public static Transform[,] grid;
    int score = 0;
    public static MinigameManager Instance{get; private set;}
    public AudioClip pieceLand;
    public AudioClip lineClear;
    public AudioClip pieceRotate;
    public static ControlMode curMode;
    public static bool Paused = false;
    public static bool IsGameOver = false;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        Instance = this;
        if (Application.platform == RuntimePlatform.Android)
        {
            curMode = ControlMode.mobile;
        }
        // canvas = PantrisBorder.transform.parent.parent.GetComponent<Canvas>();
    }
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        grid = new Transform[(maxX - minX) / 2, (maxY - minY) / 2];
        dropTime = 0.6f;
        Button_Functions.Instance.isBusy=true;
        score = 0;
        scoreText.text = score.ToString();
        Panparu.Instance.RecalcSprite();
        PanparuPlayer.GetComponent<Image>().sprite = Panparu.Instance.GetComponent<Image>().sprite;
        Paused = false;
        PauseIcon.SetActive(false);
        IsGameOver = false;

        SpawnBlock();
    }
    void Update() {
        if (IsGameOver) return;
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Paused = !Paused;
            PauseIcon.SetActive(Paused);
        }
        if (Application.platform == RuntimePlatform.Android)
        {

        }
        // if (KeyboardMode) {
        //     KeyboardModeInstructions.SetActive(true);
        //     MouseModeInstructions.SetActive(false);
        // } else {
        //     KeyboardModeInstructions.SetActive(false);
        //     MouseModeInstructions.SetActive(true);
        // }
    }
    public Vector2Int ConvertToGridCoordinates(Transform t) {
        Vector3 pos = PantrisBorder.transform.InverseTransformPoint(t.position);
        return new Vector2Int(Mathf.FloorToInt((pos.x - minX)/2), Mathf.FloorToInt((pos.y - minY)/2));
    }

    public void SpawnBlock()
    {
        if (dropTime > 0.35f)
            dropTime *= 0.95f;
        float guess = Random.Range(0f, 1f);
        guess *= blocks.Length;
        int index = Mathf.FloorToInt(guess);
        GameObject newBlock = Instantiate(blocks[index], PantrisBorder.transform, false);
        newBlock.transform.localPosition = new Vector3(10, 36, 0);
    }
    public void CheckLines() {
        int linesCleared = 0;
        for (int y = 0; y < grid.GetLength(1); y++) {
            bool line = true;
            for (int x = 0; x < grid.GetLength(0); x++) {
                if (grid[x, y] == null) {
                    line = false;
                    break;
                }
            }
            if (line) {
                DestroyLine(y);
                MoveLines(y);
                y--;
                linesCleared += 1;
            }
        }
        if (linesCleared > 0) {
            Instance.GetComponent<AudioSource>().PlayOneShot(Instance.lineClear);
            StartCoroutine(ShowHappy());
            PanparuPlayer.GetComponent<Animator>().SetTrigger("LinesCleared");
            if (linesCleared == 1) score += 100;
            else if (linesCleared == 2) score += 300;
            else if (linesCleared == 3) score += 500;
            else if (linesCleared == 4) score += 800;
            scoreText.text = score.ToString();
            if (score > Panparu.Instance.highscore)
                Panparu.Instance.highscore = score;
        }
    }
    IEnumerator ShowHappy()
    {
        var image = PanparuPlayer.GetComponent<Image>();
        var oldSprite = image.sprite;
        Sprite sp = Resources.Load<Sprite>(PanparuPlayer.GetComponent<Image>().sprite.name + "(H)") as Sprite;
        if (sp != null) {
            image.sprite = sp;
            yield return new WaitForSeconds(1);
            image.sprite = oldSprite;
        }
    }

    private static void MoveLines(int y)
    {
        for (int i = y; i < grid.GetLength(1) - 1; i++)
        // The array goes out of bounds if you don't set -1,
        // since you check for the grid above in the second for loop
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                if (grid[x, i + 1] != null)
                // In the tutors code, the code only checks for the row above, now it checks every row
                {
                    grid[x, i] = grid[x, i + 1];
                    if (grid[x, i] != null)
                        grid[x, i].gameObject.transform.localPosition += new Vector3(0, -2, 0);
                    grid[x, i + 1] = null;
                }
            }
        }
    }

    private static void DestroyLine(int y)
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            if (grid[x, y] != null) {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
            // Setting the cleared grid to null, then MoveLines will correct this if it got blocks above
        }
    }
    public void GameOver() {
        IsGameOver = true;
        // PanparuPlayer.GetComponent<Animator>().SetTrigger("GameOver");
        StartCoroutine(DestroyAllLinesAndEndGame());
    }
    IEnumerator DestroyAllLinesAndEndGame() {
        for (int y = 0; y < grid.GetLength(1); y++) {
            yield return new WaitForSeconds(0.1f);
            Instance.GetComponent<AudioSource>().PlayOneShot(Instance.pieceLand);
            DestroyLine(y);
        }
        MiniGame.SetActive(false);
        Panparu.Instance.Play();
        Button_Functions.Instance.isBusy=false;
        foreach (Transform child in PantrisBorder.transform) {
            Destroy(child.gameObject);
        }
    }
}
