using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPostRender : MonoBehaviour
{
    private List<System.Action<RenderTexture>> useRstActions;
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
        if (useRstActions == null)
            useRstActions = new List<System.Action<RenderTexture>>();

        useRstActions.Add(action);
    }
}
