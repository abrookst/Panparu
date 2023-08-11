using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    [SerializeField] private GameObject PanparuPlayer;
    [SerializeField] private GameObject PantrisBorder;
    public static float moveTime = 0.04f;
    public static float dropTime = 1f;
    public static float fallTime = 0.02f;
    public static int minX = -28, maxX = -10, minY = -19, maxY = 20;
    [SerializeField] GameObject[] blocks;
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        SpawnBlock();
    }

    void SpawnBlock()
    {
        float guess = Random.Range(0f, 1f);
        guess *= blocks.Length;
        int index = Mathf.FloorToInt(guess);
        GameObject newBlock = Instantiate(blocks[index], PantrisBorder.transform, false);
        newBlock.transform.localPosition = new Vector3(12, 37, 0);
    }
}
