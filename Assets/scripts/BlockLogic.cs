using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLogic : MonoBehaviour
{
    Canvas canvas;
    float moveTimer;
    float dropTimer;
    float fallTimer;
    bool moveable = true;
    [SerializeField] RectTransform rig;
    // Start is called before the first frame update
    void Start()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        canvas = canvasObj.GetComponent<Canvas>();
    }
    bool CheckValid() {
        foreach (Transform subBlock in rig) {
            Vector3 pos = subBlock.position;
            //Get absolute position
            pos = canvas.transform.InverseTransformPoint(pos);
            //Check if out of bounds
            if (pos.x < MinigameManager.minX || pos.x > MinigameManager.maxX || pos.y < MinigameManager.minY || pos.y > MinigameManager.maxY) {
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
                while (!CheckValid()) {
                    gameObject.transform.position += new Vector3(1, 0, 0) * canvas.scaleFactor;
                }
            } else if (right) {
                gameObject.transform.position += new Vector3(2, 0, 0) * canvas.scaleFactor;
                while (!CheckValid()) {
                    gameObject.transform.position += new Vector3(-1, 0, 0) * canvas.scaleFactor;
                }
            }
            moveTimer = 0;
        }
        
        //Harddrop
        if (Input.GetMouseButton(1)) {
            if (fallTimer > MinigameManager.fallTime) {
                gameObject.transform.position += new Vector3(0, -2, 0) * canvas.scaleFactor;
                if (!CheckValid()) {
                    while (!CheckValid()) {
                        gameObject.transform.position += new Vector3(0, 1, 0) * canvas.scaleFactor;
                    }
                    moveable = false;
                    // foreach (Transform subBlock in transform) {
                    //     Vector3 pos = subBlock.position;
                    //     MinigameManager.grid[(int)pos.x, (int)pos.y] = subBlock;
                    // }
                    // MinigameManager.CheckLines();
                    // MinigameManager.SpawnBlock();
                }
                fallTimer = 0;
            }
        } else {
            if (dropTimer > MinigameManager.dropTime) {
                gameObject.transform.position += new Vector3(0, -2, 0) * canvas.scaleFactor;
                if (!CheckValid()) {
                    while (!CheckValid()) {
                        gameObject.transform.position += new Vector3(0, 1, 0) * canvas.scaleFactor;
                    }
                    moveable = false;
                    // foreach (Transform subBlock in transform) {
                    //     Vector3 pos = subBlock.position;
                    //     MinigameManager.grid[(int)pos.x, (int)pos.y] = subBlock;
                    // }
                    // MinigameManager.CheckLines();
                    // MinigameManager.SpawnBlock();
                }
                dropTimer = 0;
            }
        }

        //Rotate
        if (Input.GetMouseButtonDown(0)) {
            rig.eulerAngles -= new Vector3(0, 0, 90);
            if (!CheckValid()) {
                rig.eulerAngles += new Vector3(0, 0, 90);
            }
        }


        moveTimer += Time.deltaTime;
        dropTimer += Time.deltaTime;
        fallTimer += Time.deltaTime;
    }
}
