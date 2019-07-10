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

            GetComponent<Rigidbody>().AddForce(h, 0, v);

            snowGround.GetComponent<IndentDraw>().MoveHeightMap(transform.position);

            if ((transform.position - _prevDrawPos).magnitude < drawDelta)
                return;

            //snowGround.GetComponent<IndentDraw>().DrawLine(_prevDrawPos, transform.position);
            snowGround.GetComponent<IndentDraw>().DrawRectMesh(_prevDrawPos, transform.position, new Vector3(0,1,0));
            _prevDrawPos = transform.position;
        }
    }

}