using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    private static Canvas canvas;
    [SerializeField] private GameObject MiniGame;
    [SerializeField] private GameObject PanparuPlayer;
    [SerializeField] private GameObject PantrisBorder;
    public static float moveTime = 0.04f;
    public static float dropTime = 1f;
    public static float fallTime = 0.02f;
    public static int minX = -28, maxX = -8, minY = -20, maxY = 20;
    [SerializeField] GameObject[] blocks;
    public static Transform[,] grid;
    public static MinigameManager Instance{get; private set;}
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        Instance = this;
        canvas = PantrisBorder.transform.parent.parent.GetComponent<Canvas>();
    }
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        grid = new Transform[(maxX - minX) / 2, (maxY - minY) / 2];
        print(grid.GetLength(0) + "x" + grid.GetLength(1));
        SpawnBlock();
        Button_Functions.Instance.isBusy=true;
    }
    public Vector2 ConvertPosToGridCoordinates(Vector3 pos) {
        pos = PantrisBorder.transform.parent.InverseTransformPoint(pos);
        return new Vector2(Mathf.FloorToInt((pos.x - minX)/2), Mathf.FloorToInt((pos.y - minY)/2));
    }

    public void SpawnBlock()
    {
        dropTime *= 0.9f;
        float guess = Random.Range(0f, 1f);
        guess *= blocks.Length;
        int index = Mathf.FloorToInt(guess);
        GameObject newBlock = Instantiate(blocks[index], PantrisBorder.transform, false);
        newBlock.transform.localPosition = new Vector3(12, 36, 0);
    }
    public void CheckLines() {
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
            }
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
                        grid[x, i].gameObject.transform.position -= new Vector3(0, 2, 0) * canvas.scaleFactor;;
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
        // PanparuPlayer.GetComponent<Animator>().SetTrigger("GameOver");
        StartCoroutine(DestroyAllLinesAndEndGame());
    }
    IEnumerator DestroyAllLinesAndEndGame() {
        for (int y = 0; y < grid.GetLength(1); y++) {
            yield return new WaitForSeconds(0.1f);
            DestroyLine(y);
        }
        MiniGame.SetActive(false);
        Panparu.Instance.Play();
        Button_Functions.Instance.isBusy=false;
    }
}
