﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraScalar : MonoBehaviour
{
    private Board mBoard;
    public float cameraOffset = -20;
    public float aspectRatio = 0.625f;
    public float padding = 2;
    public float yOffset = 1;

    // Start is called before the first frame update
    void Start()
    {
        mBoard = FindObjectOfType<Board>();    

        if(mBoard != null)
        {
            RepositionCamera(mBoard.width - 1, mBoard.height - 1);
        }
    }

    void RepositionCamera(float x, float y)
    {
        Vector3 tmpPos = new Vector3(x / 2, y / 2 + yOffset, cameraOffset);
        transform.position = tmpPos;
        if(mBoard.width >= mBoard.height)
        {
            Camera.main.orthographicSize = (mBoard.width / 2 + padding) / aspectRatio;
        }
        else
        {
            Camera.main.orthographicSize = mBoard.height / 2 + padding;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
