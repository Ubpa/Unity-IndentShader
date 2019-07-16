using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace IndentSurface
{
    public class SnowTraceCtrl : MonoBehaviour
    {
        public float stampSize = 0.1f;

        public Texture2D stampTexture;
        public RenderTexture rtWorldPos;
        public RenderTexture rtWorldNormal;

        public Material SSP2N; // screnn space postion to normal
        public Material SSP; // screen space position

        public GameObject[] actors;

        // mesh
        struct TraceInfo
        {
            public int preIdx;
            public Vector3 prePos;
        }
        private Dictionary<GameObject, TraceInfo> infoMap;
        private List<Vector3> verts;
        private List<Vector3> normals;
        private List<Vector2> uvs;
        private List<int> idxs;
        private Mesh batchedMesh;

        // camera
        private Camera posCamera;
        private Camera posToNormalCamera;

        void Awake()
        {
            // init camera
            posCamera = GenCamera("Position Camera");
            posToNormalCamera = GenCamera("Position To Normal Cameraa");

            posCamera.depth = Camera.main.depth - 2; // first
            posCamera.targetTexture = rtWorldPos;

            posToNormalCamera.depth = Camera.main.depth - 1; // second
            posToNormalCamera.targetTexture = rtWorldNormal;

            // init mesh
            batchedMesh = new Mesh();
            infoMap = new Dictionary<GameObject, TraceInfo>();
            verts = new List<Vector3>();
            normals = new List<Vector3>();
            uvs = new List<Vector2>();
            idxs = new List<int>();

            for (int i = 0; i < actors.Length; i++)
            {
                TraceInfo info = new TraceInfo();
                info.preIdx = 2 * i;

                Vector3 pos = actors[i].transform.position;
                info.prePos = pos;
                
                infoMap.Add(actors[i], info);

                Vector3 rightDir = new Vector3(1, 0, 0);
                verts.Add(pos - stampSize / 2 * rightDir);
                verts.Add(pos + stampSize / 2 * rightDir);
                normals.Add(new Vector3(0, 1, 0));
                normals.Add(new Vector3(0, 1, 0));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
            }

            // init material
            SSP2N.SetTexture("_SSP", rtWorldPos);
            SSP.SetTexture("_StampTex", stampTexture);
        }

        void Update()
        {
            UpdateCamera(posCamera);
            UpdateCamera(posToNormalCamera);

            UpdateMesh();
            
            Graphics.DrawMesh(batchedMesh, Matrix4x4.identity, SSP, LayerMask.NameToLayer("snowMesh"), posCamera);
            Graphics.DrawMesh(batchedMesh, Matrix4x4.identity, SSP2N, LayerMask.NameToLayer("snowMesh"), posToNormalCamera);
        }

        private void UpdateCamera(Camera camera)
        {
            camera.transform.position = Camera.main.transform.position;
            camera.transform.rotation = Camera.main.transform.rotation;
            camera.fieldOfView = Camera.main.fieldOfView;
            camera.nearClipPlane = Camera.main.nearClipPlane;
            camera.farClipPlane = Camera.main.farClipPlane;
            camera.aspect = Camera.main.aspect;
        }

        private Camera GenCamera(string name)
        {
            var obj = new GameObject(name);
            var camera = obj.AddComponent(typeof(Camera)) as Camera;

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(1, 1, 1, 0);

            camera.cullingMask = LayerMask.GetMask("snowMesh");

            return camera;
        }
        
        private void UpdateMesh()
        {
            foreach (GameObject actor in actors)
            {
                var info = infoMap[actor];
                Vector3 p0 = info.prePos;
                Vector3 p1 = actor.transform.position;

                if (Vector3.Distance(p0, p1) < 0.01)
                    continue;

                Vector3 front = (p1 - p0).normalized;
                Vector3 normal = new Vector3(0, 1, 0); // default
                Vector3 right = Vector3.Cross(normal,front);
                
                int curIdx = verts.Count;

                verts.Add(p1 - stampSize / 2 * right);
                verts.Add(p1 + stampSize / 2 * right);

                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));

                normals.Add(normal);
                normals.Add(normal);

                // 2  3
                // 0  1
                int[] newIdxs = {
                    info.preIdx, info.preIdx + 1, curIdx,
                    curIdx + 1, curIdx, info.preIdx + 1
                };
                idxs.AddRange(newIdxs);

                info.preIdx = curIdx;
                info.prePos = p1;

                infoMap[actor] = info;
            }

            batchedMesh.SetVertices(verts);
            batchedMesh.SetNormals(normals);
            batchedMesh.SetUVs(0, uvs);
            batchedMesh.SetIndices(idxs.ToArray(), MeshTopology.Triangles, 0);
        }
    }

}