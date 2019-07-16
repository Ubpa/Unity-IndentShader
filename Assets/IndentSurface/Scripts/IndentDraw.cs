using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace IndentSurface
{

    public class IndentDraw : MonoBehaviour
    {
        public float stampSize = 0.1f;

        public Texture2D stampTexture;
        public RenderTexture heightMap;
        public RenderTexture normalMap;

        public Material SSH2N;
        public Material SSH;

        public Camera heightCamera;
        public Camera heightToNormalCamera;

        public GameObject[] actors;
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

        void Awake()
        {
            heightCamera.targetTexture = heightMap;
            heightToNormalCamera.targetTexture = normalMap;

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

            SSH2N.SetTexture("_MainTex", heightMap);
            SSH.SetTexture("_MainTex", stampTexture);
        }

        void Update()
        {
            SSH2N.SetMatrix("_Clip2World",
                (heightCamera.projectionMatrix
                * heightCamera.worldToCameraMatrix).inverse);
            
            UpdateMesh();
            
            Graphics.DrawMesh(batchedMesh, Matrix4x4.identity, SSH, LayerMask.NameToLayer("snowMesh"), heightCamera);
            Graphics.DrawMesh(batchedMesh, Matrix4x4.identity, SSH2N, LayerMask.NameToLayer("snowMesh"), heightToNormalCamera);
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