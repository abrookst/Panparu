using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockLogic : MonoBehaviour
{
    Canvas canvas;
    CanvasScaler canvasScaler;
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
        canvasScaler = canvasObj.GetComponent<CanvasScaler>();
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
    Vector2 UnscalePosition(Vector2 vec)
    {
        Vector2 referenceResolution = canvasScaler.referenceResolution;
        Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
       
        float widthRatio = currentResolution.x / referenceResolution.x;
        float heightRatio = currentResolution.y / referenceResolution.y;
 
        float ratio = Mathf.Lerp(heightRatio, widthRatio, canvasScaler.matchWidthOrHeight);
 
        return vec / ratio;
    }

    // Update is called once per frame
    void Update() {
        if (!moveable || MinigameManager.Paused) return;
        if (MinigameManager.curMode == MinigameManager.ControlMode.keyboard)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                MinigameManager.curMode = MinigameManager.ControlMode.mouse;
            }
        }
        else if (MinigameManager.curMode == MinigameManager.ControlMode.mouse)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                MinigameManager.curMode = MinigameManager.ControlMode.keyboard;
            }
        }
        

        if (moveTimer > MinigameManager.moveTime) {
            bool left = false;
            bool right = false;
            if (MinigameManager.curMode == MinigameManager.ControlMode.mouse) {
                float xPos = canvas.transform.InverseTransformPoint(transform.position).x;
                Vector2 mousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(MinigameManager.Instance.m_parent,Input.mousePosition,MinigameManager.Instance.m_camera,out mousePos);
                left = mousePos.x < xPos - 1;
                right = mousePos.x > xPos + 1;
            } 
            else if(MinigameManager.curMode == MinigameManager.ControlMode.mobile)
            {
                try {
                    Touch touch = Input.GetTouch(0);
                    var touchPos = touch.position;
                    left = touchPos.x < (Screen.width / 2);
                    right = touchPos.x > (Screen.width / 2);
                } catch (System.Exception e) {
                    Debug.Log(e);
                }
            }
            else {
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
        if (MinigameManager.curMode == MinigameManager.ControlMode.mobile) {
            if (dropTimer > MinigameManager.dropTime) {
                gameObject.transform.localPosition += new Vector3(0, -2, 0);
                if (!CheckValid()) {
                    gameObject.transform.localPosition += new Vector3(0, 2, 0);
                    BlockLanded();
                }
                dropTimer = 0;
            }
        } else {
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
