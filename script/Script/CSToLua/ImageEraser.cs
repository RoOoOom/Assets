using UnityEngine;
using UnityEngine.UI;
using System;
namespace JZWL
{
    public class ImageEraser : MonoBehaviour
    {
        private RawImage m_image;
        private Texture2D m_texture;
        private Action m_callback;
        private Color[] m_pixels = null;
        private int brushSize = 25;
        private float minSupportOpenRate = 0.6f; //提交的最小刮开率

        // -----贴图属性的存档------
        private RawImage imageBack;
        private Texture2D textureBack;
        private Color[] colsBack;
        private int imgHight = 175;
        private int imgWidth = 175;
        private Canvas m_canvas;

        void Awake()
        {
            m_image = GetComponent<RawImage>();
            m_texture = (Texture2D)m_image.mainTexture;
            m_pixels = m_texture.GetPixels();

            imageBack = transform.parent.parent.Find("imgForeBack").GetComponent<RawImage>();
            textureBack = (Texture2D)imageBack.mainTexture;
            colsBack = textureBack.GetPixels();
            m_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        }

        void Start()
        {
            imageBack.gameObject.SetActive(false);
        }

        void Update()
        {
            if (Input.GetMouseButton(0) && gameObject.activeInHierarchy)
            {
                CheckPoint(Input.mousePosition);
            }
            // -----------------------------------test
            //if (Input.GetMouseButtonDown(1))
            //{
            //    ResumeImage();
            //}
        }

        void OnDisable()
        {
            ResumeImage();
        }

        void CheckPoint(Vector2 pos)
        {
            // 鼠标位置--> 图片上位置
            Vector2 localPos = pos;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), pos, m_canvas.worldCamera, out localPos))
            {
                localPos = Vector2.zero;
            }

            if (localPos.x > -imgWidth / 2 && localPos.x < imgWidth / 2
                && localPos.y > -imgHight / 2 && localPos.y < imgHight / 2)
            {
                for (int i = (int)(localPos.x) - brushSize; i < (int)(localPos.x) + brushSize; i++)
                {
                    for (int j = (int)(localPos.y) - brushSize; j < (int)(localPos.y) + brushSize; j++)
                    {
                        if (Mathf.Pow(i - localPos.x, 2) + Mathf.Pow(j - localPos.y, 2) > Mathf.Pow(brushSize, 2))
                            continue;
                        Color col = m_texture.GetPixel(i + imgWidth / 2, j + imgHight / 2);
                        col.a = 0.0f;
                        m_texture.SetPixel(i + imgWidth / 2, j + imgHight / 2, col);
                    }
                }
                m_texture.Apply();
                CheckFinish();
            }
        }

        void CheckFinish()
        {
            m_pixels = m_texture.GetPixels();
            int openedCount = m_pixels.Length;
            for (int i = 0; i < m_pixels.Length; i++)
            {
                if (m_pixels[i].a == 1.0f)
                {
                    openedCount--;
                }
            }
            float openedRate = openedCount / (float)m_pixels.Length;
            if (openedRate > minSupportOpenRate)
            {
                gameObject.SetActive(false);
                if (null != m_callback) m_callback();
            }
        }

        public void ResumeImage()
        {
            if (m_texture)
            {
                m_texture.SetPixels(colsBack);
                m_texture.Apply();
            }
        }

        public void SetCallBack(Action func)
        {
            m_callback = func;
        }
    }
}