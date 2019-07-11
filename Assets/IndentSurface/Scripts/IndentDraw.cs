using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Wacki.IndentSurface
{

    public class IndentDraw : MonoBehaviour
    {
        public Texture2D stampTexture;
        public int rtWidth = 512;
        public int rtHeight = 512;
        public float width = 20;
        public float height = 20;
        public float stampSize = 2.0f;

        public RenderTexture heightMap;
        public RenderTexture normalMap;
        public Material heightToNormal;
        private RenderTexture auxTexture;

        public Material moveHeight;
        public Material drawIndent;
        public Material drawLineIndent;
        public Material drawMeshIndent;

        // mouse debug draw
        private Vector3 _prevMousePosition;
        private bool _mouseDrag = false;

        private Vector2 xzPos;

        public GameObject indentCam;

        public GameObject[] actors;
        struct TraceInfo
        {
            public int preIdx;
            public Vector3 prePos;
        }
        private Dictionary<GameObject, TraceInfo> infoMap;
        private List<Vector3> verts;
        private List<Vector2> uvs;
        private List<int> idxs;
        private Mesh batchedMesh;

        void Awake()
        {
            // temporarily use a given render texture to be able to see how it looks
            auxTexture = new RenderTexture(rtWidth, rtHeight, 32);

            GetComponent<Renderer>().material.SetTexture("_Indentmap", heightMap);

            xzPos.Set(float.MaxValue, float.MaxValue);

            if(indentCam != null)
            {
                indentCam.GetComponent<Camera>().targetTexture = heightMap;
                indentCam.transform.rotation = Quaternion.LookRotation(new Vector3(0, -1, 0));
            }

            batchedMesh = new Mesh();
            infoMap = new Dictionary<GameObject, TraceInfo>();
            verts = new List<Vector3>();
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
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
            }

            indentCam.GetComponent<CamPostRender>().AddTask(() =>
            {
                Graphics.Blit(heightMap, normalMap, heightToNormal);
            });
        }

        void Update()
        {
            indentCam.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-width / 2, width / 2, -height / 2, height / 2, 0.1f, 30.0f);
            GetComponent<MeshRenderer>().material.SetVector("_IndentNormalMapOffset",
                new Vector4(indentCam.transform.position.x, indentCam.transform.position.z, width, height));
            heightToNormal.SetFloat("_Width", rtWidth);
            heightToNormal.SetFloat("_Height", rtHeight);

            UpdateCameraPos();
            UpdateMesh();

            Graphics.DrawMesh(batchedMesh, Matrix4x4.identity, drawMeshIndent, LayerMask.NameToLayer("snowMesh"), indentCam.GetComponent<Camera>());
        } 

        private void DrawByMouse()
        {
            if (Camera.main == null)
                return;

            bool draw = false;
            float drawThreshold = 0.01f;

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.collider.gameObject != gameObject)
                    return;
            }

            // force a draw on mouse down
            draw = Input.GetMouseButtonDown(0);
            // set dragging state
            _mouseDrag = Input.GetMouseButton(0);


            if (_mouseDrag && (draw || Vector3.Distance(hit.point, _prevMousePosition) > drawThreshold))
            {
                _prevMousePosition = hit.point;
                DrawAt(hit.point);
            }
        }

        public void DrawAt(Vector3 pos)
        {
            Graphics.Blit(heightMap, auxTexture);

            // activate our render texture
            RenderTexture.active = heightMap;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, heightMap.width, heightMap.height, 0);

            float u = (pos.x - xzPos[0]) / width + 0.5f;
            float v = (pos.z - xzPos[1]) / height + 0.5f;

            // setup rect for our indent texture stamp to draw into
            Rect screenRect = new Rect();
            float drawWidth = stampSize / width * heightMap.width;
            float drawHeight = stampSize / height * heightMap.height;

            // put the center of the stamp at the actual draw position
            screenRect.x = u * heightMap.width - drawWidth * 0.5f;
            screenRect.y = (heightMap.height - v * heightMap.height) - drawWidth * 0.5f;
            screenRect.width = drawWidth;
            screenRect.height = drawHeight;

            var tempVec = new Vector4();

            tempVec.x = screenRect.x / ((float)heightMap.width);
            tempVec.y = 1 - (screenRect.y / (float)heightMap.height);
            tempVec.z = screenRect.width / heightMap.width;
            tempVec.w = screenRect.height / heightMap.height;
            tempVec.y -= tempVec.w;

            // 用于将 stamp 纹理坐标映射成 surface texture 纹理坐标
            drawIndent.SetVector("_SourceTexCoords", tempVec);

            drawIndent.SetTexture("_SurfaceTex", auxTexture);

            // Draw the texture
            Graphics.DrawTexture(screenRect, stampTexture, drawIndent);

            GL.PopMatrix();
            RenderTexture.active = null;
        }
        
        public void DrawLine(Vector3 p0, Vector3 p1)
        {
            Graphics.Blit(heightMap, auxTexture);

            // activate our render texture
            RenderTexture.active = heightMap;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, heightMap.width, heightMap.height, 0);

            float u0 = (p0.x - xzPos[0]) / width + 0.5f;
            float v0 = (p0.z - xzPos[1]) / height + 0.5f;

            float u1 = (p1.x - xzPos[0]) / width + 0.5f;
            float v1 = (p1.z - xzPos[1]) / height + 0.5f;

            float minU = Mathf.Min(u0, u1);
            float maxU = Mathf.Max(u0, u1);
            float minV = Mathf.Min(v0, v1);
            float maxV = Mathf.Max(v0, v1);

            // setup rect for our indent texture stamp to draw into
            Rect screenRect = new Rect();
            float drawWidth = stampSize / width * heightMap.width;
            float drawHeight = stampSize / height * heightMap.height;

            // put the center of the stamp at the actual draw position
            screenRect.x = minU * heightMap.width - drawWidth * 0.5f;
            screenRect.y = (heightMap.height - maxV * heightMap.height) - drawWidth * 0.5f;
            screenRect.width = drawWidth + (maxU - minU)* heightMap.width;
            screenRect.height = drawHeight + (maxV - minV) * heightMap.height;

            var tempVec = new Vector4();

            tempVec.x = screenRect.x / ((float)heightMap.width);
            tempVec.y = 1 - (screenRect.y / (float)heightMap.height);
            tempVec.z = screenRect.width / heightMap.width;
            tempVec.w = screenRect.height / heightMap.height;
            tempVec.y -= tempVec.w;

            // 用于将 tex 纹理坐标映射成 surface texture 纹理坐标
            drawLineIndent.SetVector("_SourceTexCoords", tempVec);
            Debug.Log("_SourceTexCoords: "+tempVec);

            drawLineIndent.SetTexture("_SurfaceTex", auxTexture);

            drawLineIndent.SetFloat("_TexR", stampSize / width / 2.0f);

            drawLineIndent.SetVector("_UV01", new Vector4(u0,v0,u1,v1));

            // Draw the texture
            Graphics.DrawTexture(screenRect, stampTexture, drawLineIndent);

            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public void DrawRectMesh(Vector3 p0, Vector3 p1, Vector3 norm)
        {
            float xLen = Vector3.Distance(p0, p1);
            float yLen = stampSize;
            Vector3 xDir = (p1 - p0).normalized;
            Vector3 yDir = Vector3.Cross(norm, xDir);

            // [↑x][←y]
            // 3  p1  2
            // 
            // 
            // 
            // 0  p0  1

            var mesh = new Mesh();

            Vector3[] verts = {
                p0 + yDir * yLen / 2, // 0
                p0 - yDir * yLen / 2, // 1
                p0 - yDir * yLen / 2 + xLen * xDir, // 2
                p0 + yDir * yLen / 2 + xLen * xDir, // 3
            };

            int[] indice = { 0, 1, 3, 2, 3, 1 };
            Vector2[] uvs = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };

            mesh.vertices = verts;
            mesh.triangles = indice;
            mesh.uv = uvs;

            Graphics.Blit(heightMap, auxTexture);
            drawLineIndent.SetTexture("_MainTex", stampTexture);
            drawLineIndent.SetTexture("_SurfaceTex", auxTexture);
            Debug.Log(LayerMask.NameToLayer("snowMesh"));
            Graphics.DrawMesh(mesh, Matrix4x4.identity, drawMeshIndent, LayerMask.NameToLayer("snowMesh"), indentCam.GetComponent<Camera>());
        }

        public void MoveHeightMap(Vector3 pos)
        {
            Vector2 cur_xzPos = new Vector2(pos.x, pos.z);
            Vector2 delta = cur_xzPos - xzPos;
            if (delta.sqrMagnitude < 0.2 * width * height)
                return;

            GetComponent<MeshRenderer>().material.SetVector("_IndentNormalMapOffset", new Vector4(cur_xzPos[0], cur_xzPos[1], width, height));

            xzPos = cur_xzPos;
            if(indentCam != null)
                indentCam.transform.position = pos + new Vector3(0, 10, 0);

            Vector2 uvOffset = new Vector2(delta.x / width, delta.y / height);
            moveHeight.SetFloat("_uOffset", uvOffset.x);
            moveHeight.SetFloat("_vOffset", uvOffset.y);
            Graphics.Blit(heightMap, auxTexture);
            Graphics.Blit(auxTexture, heightMap, moveHeight);
        }

        private void UpdateCameraPos()
        {
            if (actors.Length == 0)
                return;

            Vector3 pos = new Vector3(0,0,0);
            float maxY = float.MinValue;
            foreach (GameObject actor in actors)
            {
                pos += actor.transform.position;
                maxY = Mathf.Max(maxY, actor.transform.position.y);
            }

            pos /= actors.Length;
            Vector3 newCamPos = new Vector3(pos.x, maxY + 1,pos.z);
            if (Vector3.Distance(indentCam.transform.position, newCamPos) < 0.1 * Mathf.Sqrt(width * height))
                return;
            //UnityEditor.EditorApplication.isPaused = true;
            indentCam.transform.position = newCamPos;

            GetComponent<MeshRenderer>().material.SetVector("_IndentNormalMapOffset",
                new Vector4(newCamPos.x, newCamPos.z, width, height));
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
            batchedMesh.SetUVs(0, uvs);
            batchedMesh.SetIndices(idxs.ToArray(), MeshTopology.Triangles, 0);
        }
    }

}