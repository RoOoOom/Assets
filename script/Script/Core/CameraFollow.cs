using UnityEngine;
using DG.Tweening;

namespace JZWL
{ 
    public class CameraFollow : MonoBehaviour
    {
        
        public bool active
        {
            get
            {
                return m_active;
            }
            set
            {
                m_active = value;
                if (!m_active && null != m_cacheTweener)
                {
                    m_cacheTweener.Kill(false);
                    m_cacheTweener = null;
                }
            }
        }
        public bool m_active = false;

        private bool m_inited = false;
        private float m_minX, m_minY, m_maxX, m_maxY;
        private float m_gridSize;
        private Vector3 m_lastPos = Vector3.zero;
        private Tweener m_cacheTweener = null;

        private float timeCount = 0.2f;

        void Start()
        {
            m_lastPos = transform.position;
            Camera.main.transform.position = m_lastPos;
        }

        void LateUpdate()
        {
            if (!m_inited || !m_active) return;

            Vector3 toPos = transform.position;
            toPos.x = Mathf.Clamp(toPos.x, m_minX, m_maxX);
            toPos.y = Mathf.Clamp(toPos.y + 0.2f, m_minY, m_maxY);

            Camera.main.transform.position = toPos;
            //if (Mathf.Abs(m_lastPos.x - toPos.x) > 0.1 || Mathf.Abs(m_lastPos.y - toPos.y) > 0.1)
            //{
            //    m_lastPos = toPos;
            //    TweenTo(toPos);
            //}
        }

        public void Init(float minX, float minY, float maxX, float maxY, float gridSize)
        {
            m_minX = minX;
            m_minY = minY;
            m_maxX = maxX;
            m_maxY = maxY;
            m_gridSize = gridSize;
            m_lastPos = transform.position;
            m_inited = true;
            m_active = true;
        }

        public void ResetPos()
        {
            active = false;
            m_lastPos = transform.position;
            Camera.main.transform.position = m_lastPos;
            active = true;
        }

        void TweenTo(Vector3 toPos)
        {
            if (null != m_cacheTweener)
            {
                m_cacheTweener.ChangeEndValue(toPos, true).Play();
            }
            else
            {
                m_cacheTweener = Camera.main.transform.DOMove(toPos, 0.6f, false).SetAutoKill(false);
            }
        }
    }
}
