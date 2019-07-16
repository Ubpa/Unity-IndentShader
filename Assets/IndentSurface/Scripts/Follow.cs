using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject followee;
    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - followee.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = followee.transform.position + offset;
    }
}
