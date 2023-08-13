using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockLogic : MonoBehaviour
{
    Canvas canvas;
    float moveTimer;
    float dropTimer;
    float fallTimer;
    bool moveable = true;
    public RectTransform rig;
    BlockLogic ghost;
    // Start is called before the first frame update
    void Start()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        canvas = canvasObj.GetComponent<Canvas>();
        bool atTop = false;
        while (!atTop) {
            foreach (Transform subBlock in rig) {
                Vector2Int gridPos = MinigameManager.Instance.ConvertToGridCoordinates(subBlock);
                //Check if out of bounds
                if (gridPos.y >= MinigameManager.grid.GetLength(1)) {
                    atTop = true;
                    break;
                }
            }
            if (!atTop) 
                gameObject.transform.localPosition += new Vector3(0, 2, 0);
        }
        gameObject.transform.localPosition += new Vector3(0, -2, 0);
        if (!CheckValid()) {
            moveable = false;
            foreach (Transform subBlock in rig) {
                Vector2Int gridPos = MinigameManager.Instance.ConvertToGridCoordinates(subBlock);
                // print("possible overlap" + gridPos + " " + MinigameManager.grid.GetLength(0) + " " + MinigameManager.grid.GetLength(1));
                if (MinigameManager.grid[gridPos.x, gridPos.y] != null)
                    Destroy(MinigameManager.grid[gridPos.x, gridPos.y].gameObject);
                MinigameManager.grid[gridPos.x, gridPos.y] = subBlock;
            }
            MinigameManager.Instance.GameOver();
            return;
        }

        ghost = Instantiate(gameObject, transform.localPosition, transform.rotation, transform.parent).GetComponent<BlockLogic>();
        ghost.enabled = false;
        foreach (Transform subBlock in ghost.rig) {
            subBlock.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        }
        
    }
    public bool CheckValid() {
        foreach (Transform subBlock in rig) {
            Vector2Int gridPos = MinigameManager.Instance.ConvertToGridCoordinates(subBlock);
            //Check if out of bounds
            if (gridPos.x < 0 || gridPos.x >= MinigameManager.grid.GetLength(0) || gridPos.y < 0 || gridPos.y >= MinigameManager.grid.GetLength(1)) {
                return false;
            } else {
            }
        }
        foreach (Transform subBlock in rig) {
            //Check if there is a block in the way
            Vector2Int gridPos = MinigameManager.Instance.ConvertToGridCoordinates(subBlock);
            // print("possible overlap" + gridPos + " " + MinigameManager.grid.GetLength(0) + " " + MinigameManager.grid.GetLength(1));
            if (MinigameManager.grid[gridPos.x, gridPos.y] != null) {
                return false;
            }
        }
        return true;
    }

    // Update is called once per frame
    void Update() {
        if (!moveable || MinigameManager.Paused) return;
        if (MinigameManager.KeyboardMode) {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
                MinigameManager.KeyboardMode = false;
            }
        } else {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                MinigameManager.KeyboardMode = true;
            }
        }
        if (moveTimer > MinigameManager.moveTime) {
            bool left;
            bool right;
            if (!MinigameManager.KeyboardMode) {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out Vector2 pos);
                left = transform.localPosition.x > canvas.transform.TransformPoint(pos).x + (1 * canvas.scaleFactor);
                right = transform.localPosition.x < canvas.transform.TransformPoint(pos).x - (1 * canvas.scaleFactor);
            } else {
                left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
                right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            }
            if (left) {
                gameObject.transform.localPosition += new Vector3(-2, 0, 0);
                if (!CheckValid()) {
                    gameObject.transform.localPosition += new Vector3(2, 0, 0);
                }
            } else if (right) {
                gameObject.transform.localPosition += new Vector3(2, 0, 0);
                if (!CheckValid()) {
                    gameObject.transform.localPosition += new Vector3(-2, 0, 0);
                }
            }
            moveTimer = 0;
        }
        
        //Harddrop
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            if (fallTimer > MinigameManager.fallTime) {
                gameObject.transform.localPosition += new Vector3(0, -2, 0);
                if (!CheckValid()) {
                    gameObject.transform.localPosition += new Vector3(0, 2, 0);
                    BlockLanded();
                }
                fallTimer = 0;
            }
        } else {
            if (dropTimer > MinigameManager.dropTime) {
                gameObject.transform.localPosition += new Vector3(0, -2, 0);
                if (!CheckValid()) {
                    gameObject.transform.localPosition += new Vector3(0, 2, 0);
                    BlockLanded();
                }
                dropTimer = 0;
            }
        }
        
        //Rotate
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.R)) {
            MinigameManager.Instance.GetComponent<AudioSource>().PlayOneShot(MinigameManager.Instance.pieceRotate);
            rig.eulerAngles += new Vector3(0, 0, -90);
            if (!CheckValid()) {
                rig.eulerAngles += new Vector3(0, 0, 90);
            }
        }


        moveTimer += Time.deltaTime;
        dropTimer += Time.deltaTime;
        fallTimer += Time.deltaTime;

        if (ghost == null) return;
        //Ghost
        ghost.transform.localPosition = transform.localPosition;
        ghost.rig.eulerAngles = rig.eulerAngles;
        while (ghost.CheckValid()) {
            ghost.transform.localPosition += new Vector3(0, -2, 0);
        }
        ghost.transform.localPosition += new Vector3(0, 2, 0);
    }

    void BlockLanded() {
        moveable = false;
        MinigameManager.Instance.GetComponent<AudioSource>().PlayOneShot(MinigameManager.Instance.pieceLand);
        while (rig.childCount > 0) {
            Transform subBlock = rig.GetChild(0);
            Vector2Int gridPos = MinigameManager.Instance.ConvertToGridCoordinates(subBlock);
            MinigameManager.grid[gridPos.x, gridPos.y] = subBlock;
            subBlock.SetParent(MinigameManager.Instance.PantrisBorder.transform);
        }
        MinigameManager.Instance.CheckLines();
        MinigameManager.Instance.SpawnBlock();
        Destroy(ghost.gameObject);
        ghost = null;
        Destroy(gameObject);
    }

}
