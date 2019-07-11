using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPostRender : MonoBehaviour
{
    private List<System.Action> postRenderTasks;

    private void OnPostRender()
    {
        foreach (System.Action action in postRenderTasks)
        {
            action();
        }
    }

    public void AddTask(System.Action action)
    {
        if (postRenderTasks == null)
            postRenderTasks = new List<System.Action>();

        postRenderTasks.Add(action);
    }
}
