using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPostRender : MonoBehaviour
{
    public Camera mainCam;
    private List<System.Action<RenderTexture>> useRstActions = new List<System.Action<RenderTexture>>();

    private void Awake()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
    }

    private void Update()
    {
        transform.position = mainCam.transform.position;
        transform.rotation = mainCam.transform.rotation;
        GetComponent<Camera>().fieldOfView = mainCam.fieldOfView;
        GetComponent<Camera>().nearClipPlane = mainCam.nearClipPlane;
        GetComponent<Camera>().farClipPlane = mainCam.farClipPlane;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        foreach (var action in useRstActions)
        {
            action(source);
        }
        Graphics.Blit(source, destination);
    }

    public void AddTask(System.Action<RenderTexture> action)
    {
        useRstActions.Add(action);
    }
}
