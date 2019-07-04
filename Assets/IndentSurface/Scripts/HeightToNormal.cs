using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightToNormal : MonoBehaviour
{
    public RenderTexture heightMap;
    public RenderTexture normalMap;

    //public int rtWidth = 512;
    //public int rtHeight = 512;

    public Material heightToNormal;

    // Update is called once per frame
    void Update()
    {
        Graphics.Blit(heightMap, normalMap, heightToNormal);
    }
}
