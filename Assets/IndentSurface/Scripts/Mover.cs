using UnityEngine;
using System.Collections;


namespace IndentSurface
{
    public class Mover : MonoBehaviour
    {
        [Range(0.0f,100.0f)]
        public float speed = 1.0f;

        void Update()
        {
            float v = speed * Input.GetAxis("Vertical");
            float h = speed * Input.GetAxis("Horizontal");
            GetComponent<Rigidbody>().AddForce(h, 0, v);
        }
    }

}