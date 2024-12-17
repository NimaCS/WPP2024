using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera p1Cam, p2Cam, sharedCam;
    [SerializeField] private bool close, dynamicSplitSides, alwaysSplit;
    [SerializeField] private SpriteRenderer bg;
    [SerializeField] private Image splitBorder;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!alwaysSplit)
            CheckIfClose();

        if (close && !alwaysSplit)
            SharedCam();
        else
            SplitScreen();
    }

    private void CheckIfClose()
    {
        if (bg.gameObject.GetComponent<ObjectCheckScript>().allObjectsInArea)
            close = true;
        else
            close = false;
    }

    private void SplitScreen()
    {
        p1Cam.enabled = true;
        p2Cam.enabled = true;
        sharedCam.enabled = false;
        splitBorder.enabled = true;

        if (!dynamicSplitSides)
        {
            p1Cam.rect = new Rect(0f, 0f, 0.5f, 1f);
            p2Cam.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }

        if (p1Cam.transform.position.x < p2Cam.transform.position.x && dynamicSplitSides)
        {
            p1Cam.rect = new Rect(0f, 0f, 0.5f, 1f);
            p2Cam.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        } else if (p1Cam.transform.position.x > p2Cam.transform.position.x && dynamicSplitSides) {
            p1Cam.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            p2Cam.rect = new Rect(0f, 0f, 0.5f, 1f);
        }
    }

    private void SharedCam()
    {
        sharedCam.enabled = true;
        p1Cam.enabled = false;
        p2Cam.enabled = false;
        splitBorder.enabled = false;

        float orthoSize = bg.bounds.size.y / 2;
        sharedCam.orthographicSize = orthoSize;
    }
}
