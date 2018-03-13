using UnityEngine;
using UnityEngine.UI;

namespace JZWL
{
    public class GreyColor : MonoBehaviour
    {
        public Graphic m_graphic = null;
        private Color m_originalColor = Color.white;
        private bool m_inited = false;

        private void M_Init()
        {
            if (!m_inited)
            {
                m_inited = true;
                m_originalColor = m_graphic.color;
            }
        }

        public void SetGrey()
        {
            M_Init();
            if (m_graphic is Text)
            {
                if (m_graphic.material != null && m_graphic.material.shader.name == "UI/GreyShader")
                {
                    m_graphic.color = Color.black;
                }
                else
                {
                    m_graphic.color = new Color32(153, 153, 153, 255);
                }
            }
            else
            {
                m_graphic.color = Color.black;
            }
        }

        public void Reset()
        {
            if (m_graphic != null)
            {
                m_graphic.color = m_originalColor;
            }
        }
    }
}