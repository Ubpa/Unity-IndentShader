using UnityEngine;
using System.Collections;


namespace Wacki.IndentSurface
{
    /// <summary>
    /// Simple control script for our sphere that leaves a track in the snow.
    /// </summary>
    public class IndentActor : MonoBehaviour
    {
        [Range(0.0f, 0.2f)]
        public float drawDelta = 0.1f;
        private Vector3 _prevDrawPos;
        [Range(0.0f,100.0f)]
        public float speed = 1.0f;

        public GameObject snowGround;

        private void Awake()
        {
            _prevDrawPos = transform.position;
        }

        void Update()
        {
            float v = speed * Input.GetAxis("Vertical");
            float h = speed * Input.GetAxis("Horizontal");
            var F = transform.forward * v +transform.right * h;

            GetComponent<Rigidbody>().AddForce(F);
        }
    }

}