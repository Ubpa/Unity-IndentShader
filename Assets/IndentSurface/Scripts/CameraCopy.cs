using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCopy : MonoBehaviour
{
    public Camera mainCam;

    private void Update()
    {
        transform.position = mainCam.transform.position;
        transform.rotation = mainCam.transform.rotation;
        GetComponent<Camera>().fieldOfView = mainCam.fieldOfView;
        GetComponent<Camera>().nearClipPlane = mainCam.nearClipPlane;
        GetComponent<Camera>().farClipPlane = mainCam.farClipPlane;
        GetComponent<Camera>().aspect = mainCam.aspect;
    }
}
