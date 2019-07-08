using UnityEngine;
using System.Collections;

namespace Wacki.IndentSurface
{

    public class IndentDraw : MonoBehaviour
    {
        public Texture2D initTexture;
        public Texture2D stampTexture;
        public int rtWidth = 1024;
        public int rtHeight = 1024;
        public float width = 1;
        public float height = 1;
        public float stampWidth = 2.0f;
        public float stampHeight = 2.0f;

        public RenderTexture RT0;
        private RenderTexture auxTexture;

        public Material moveHeight;
        public Material drawIndent;

        // mouse debug draw
        private Vector3 _prevMousePosition;
        private bool _mouseDrag = false;

        private Vector2 xzPos; 

        void Awake()
        {
            // temporarily use a given render texture to be able to see how it looks
            auxTexture = new RenderTexture(rtWidth, rtHeight, 32);

            GetComponent<Renderer>().material.SetTexture("_Indentmap", RT0);
            Graphics.Blit(initTexture, RT0);

            xzPos.Set(99999, 99999);
        }

        // add an indentation at a raycast hit position
        public void IndentAt(RaycastHit hit)
        {
            if (!hit.collider || hit.collider.gameObject != this.gameObject)
                return;

            DrawAt(hit.point.x, hit.point.z, 1.0f);
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
                IndentAt(hit);
            }
        }

        /// <summary>
        /// todo:   it would probably be a bit more straight forward if we didn't use Graphics.DrawTexture
        ///         and instead handle everything ourselves. This way we could directly provide multiple 
        ///         texture coordinates to each vertex.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="alpha"></param>
        void DrawAt(float x, float z, float alpha)
        {
            Graphics.Blit(RT0, auxTexture);

            // activate our render texture
            RenderTexture.active = RT0;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, RT0.width, RT0.height, 0);

            float u = (x - xzPos[0]) / width + 0.5f;
            float v = (z - xzPos[1]) / height + 0.5f;

            // setup rect for our indent texture stamp to draw into
            Rect screenRect = new Rect();
            // put the center of the stamp at the actual draw position
            float drawWidth = stampWidth / width * RT0.width;
            float drawHeight = stampHeight / height * RT0.height;

            screenRect.x = u * RT0.width - drawWidth * 0.5f;
            screenRect.y = (RT0.height - v * RT0.height) - drawWidth * 0.5f;
            //screenRect.y = v * RT0.height - stampTexture.height * 0.5f;
            screenRect.width = drawWidth;
            screenRect.height = drawWidth;

            var tempVec = new Vector4();

            tempVec.x = screenRect.x / ((float)RT0.width);
            tempVec.y = 1 - (screenRect.y / (float)RT0.height);
            tempVec.z = screenRect.width / RT0.width;
            tempVec.w = screenRect.height / RT0.height;
            tempVec.y -= tempVec.w;

            // Graphics.DrawTexture 会设置 _MainTex，以下冗余
            // mat.SetTexture("_MainTex", stampTexture);

            // 用于将 stamp 纹理坐标映射成 surface texture 纹理坐标
            drawIndent.SetVector("_SourceTexCoords", tempVec);

            drawIndent.SetTexture("_SurfaceTex", auxTexture);

            // Draw the texture
            Graphics.DrawTexture(screenRect, stampTexture, drawIndent);

            GL.PopMatrix();
            RenderTexture.active = null;
        }
        
        public void MoveHeight(float x, float z)
        {
            Vector2 cur_xzPos = new Vector2(x, z);
            Vector2 delta = cur_xzPos - xzPos;
            if (delta.sqrMagnitude < 0.2 * width * height)
                return;

            //if (!GetComponent<MeshRenderer>().material.HasProperty("_IndentNormalMapOffset"))
            //    Debug.Log("not have _IndentNormalMapOffset");
            GetComponent<MeshRenderer>().material.SetVector("_IndentNormalMapOffset", new Vector4(cur_xzPos[0], cur_xzPos[1], width, height));

            //Debug.Log("delta:" + delta);

            xzPos = cur_xzPos;
            Vector2 uvOffset = new Vector2(delta.x / width, delta.y / height);
            moveHeight.SetFloat("_uOffset", uvOffset.x);
            moveHeight.SetFloat("_vOffset", uvOffset.y);
            //Debug.Log("uvOffset:" + uvOffset);
            Graphics.Blit(RT0, auxTexture);
            Graphics.Blit(auxTexture, RT0, moveHeight);
        }
    }

}