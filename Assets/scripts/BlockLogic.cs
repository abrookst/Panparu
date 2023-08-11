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
                Vector3 pos = subBlock.position;
                Vector2 gridPos = MinigameManager.Instance.ConvertPosToGridCoordinates(pos);
                //Check if out of bounds
                if (gridPos.y >= MinigameManager.grid.GetLength(1)) {
                    atTop = true;
                    break;
                }
            }
            if (!atTop) 
                gameObject.transform.position += new Vector3(0, 2, 0) * canvas.scaleFactor;
        }
        gameObject.transform.position += new Vector3(0, -2, 0) * canvas.scaleFactor;
        if (!CheckValid()) {
            moveable = false;
            foreach (Transform subBlock in rig) {
                Vector2 gridPos = MinigameManager.Instance.ConvertPosToGridCoordinates(subBlock.position);
                if (MinigameManager.grid[(int)gridPos.x, (int)gridPos.y] != null)
                    Destroy(MinigameManager.grid[(int)gridPos.x, (int)gridPos.y].gameObject);
                MinigameManager.grid[(int)gridPos.x, (int)gridPos.y] = subBlock;
            }
            MinigameManager.Instance.GameOver();
            return;
        }

        ghost = Instantiate(gameObject, transform.position, transform.rotation, transform.parent).GetComponent<BlockLogic>();
        ghost.enabled = false;
        foreach (Transform subBlock in ghost.rig) {
            subBlock.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        }
        
    }
    public bool CheckValid() {
        foreach (Transform subBlock in rig) {
            Vector3 pos = subBlock.position;
            Vector2 gridPos = MinigameManager.Instance.ConvertPosToGridCoordinates(pos);
            //Check if out of bounds
            if (gridPos.x < 0 || gridPos.x >= MinigameManager.grid.GetLength(0) || gridPos.y < 0 || gridPos.y >= MinigameManager.grid.GetLength(1)) {
                return false;
            }
        }
        foreach (Transform subBlock in rig) {
            //Check if there is a block in the way
            Vector3 pos = subBlock.position;
            Vector2 gridPos = MinigameManager.Instance.ConvertPosToGridCoordinates(pos);
            if (MinigameManager.grid[(int)gridPos.x, (int)gridPos.y] != null) {
                return false;
            }
        }
        return true;
    }

    // Update is called once per frame
    void Update() {
        if (!moveable) return;
        if (moveTimer > MinigameManager.moveTime) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out Vector2 pos);
            bool left = transform.position.x > canvas.transform.TransformPoint(pos).x + (2 * canvas.scaleFactor);
            bool right = transform.position.x < canvas.transform.TransformPoint(pos).x - (2 * canvas.scaleFactor);

            if (left) {
                gameObject.transform.position += new Vector3(-2, 0, 0) * canvas.scaleFactor;
                if (!CheckValid()) {
                    gameObject.transform.position += new Vector3(2, 0, 0) * canvas.scaleFactor;
                }
            } else if (right) {
                gameObject.transform.position += new Vector3(2, 0, 0) * canvas.scaleFactor;
                if (!CheckValid()) {
                    gameObject.transform.position += new Vector3(-2, 0, 0) * canvas.scaleFactor;
                }
            }
            moveTimer = 0;
        }
        
        //Harddrop
        if (Input.GetMouseButton(0)) {
            if (fallTimer > MinigameManager.fallTime) {
                gameObject.transform.position += new Vector3(0, -2, 0) * canvas.scaleFactor;
                if (!CheckValid()) {
                    gameObject.transform.position += new Vector3(0, 2, 0) * canvas.scaleFactor;
                    BlockLanded();
                }
                fallTimer = 0;
            }
        } else {
            if (dropTimer > MinigameManager.dropTime) {
                gameObject.transform.position += new Vector3(0, -2, 0) * canvas.scaleFactor;
                if (!CheckValid()) {
                    gameObject.transform.position += new Vector3(0, 2, 0) * canvas.scaleFactor;
                    BlockLanded();
                }
                dropTimer = 0;
            }
        }
        
        //Rotate
        if (Input.GetMouseButtonDown(1)) {
            MinigameManager.Instance.GetComponent<AudioSource>().PlayOneShot(MinigameManager.Instance.pieceRotate);
            rig.eulerAngles -= new Vector3(0, 0, 90);
            if (!CheckValid()) {
                rig.eulerAngles += new Vector3(0, 0, 90);
            }
        }


        moveTimer += Time.deltaTime;
        dropTimer += Time.deltaTime;
        fallTimer += Time.deltaTime;

        if (ghost == null) return;
        //Ghost
        ghost.transform.position = transform.position;
        ghost.rig.eulerAngles = rig.eulerAngles;
        while (ghost.CheckValid()) {
            ghost.transform.position += new Vector3(0, -2, 0) * canvas.scaleFactor;
        }
        ghost.transform.position += new Vector3(0, 2, 0) * canvas.scaleFactor;
    }

    void BlockLanded() {
        moveable = false;
        MinigameManager.Instance.GetComponent<AudioSource>().PlayOneShot(MinigameManager.Instance.pieceLand);
        foreach (Transform subBlock in rig) {
            Vector2 gridPos = MinigameManager.Instance.ConvertPosToGridCoordinates(subBlock.position);
            MinigameManager.grid[(int)gridPos.x, (int)gridPos.y] = subBlock;
        }
        MinigameManager.Instance.CheckLines();
        MinigameManager.Instance.SpawnBlock();
        Destroy(ghost.gameObject);
        ghost = null;
    }

}
