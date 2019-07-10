using UnityEngine;
using System.Collections;

namespace Wacki.IndentSurface
{

    public class IndentDraw : MonoBehaviour
    {
        public Texture2D stampTexture;
        public int rtWidth = 1024;
        public int rtHeight = 1024;
        public float width = 1;
        public float height = 1;
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
                indentCam.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-width / 2, width / 2, -height / 2, height / 2, 0.1f, 30.0f);
            }
        }

        void Update()
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

        private void LateUpdate()
        {
            Graphics.Blit(heightMap, normalMap, heightToNormal);
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
    }

}