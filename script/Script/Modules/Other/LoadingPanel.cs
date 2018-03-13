using System;
using UnityEngine;
using LuaInterface;
using UnityEngine.UI;
using DG.Tweening;
namespace JZWL
{
    public class LoadingPanel : MonoBehaviour
    {
        public static LoadingPanel instance
        {
            get
            {
                if (null == m_instance)
                {
                    GameObject go = GameObject.Instantiate(ResourceManager.instance.AsyncGetResource<GameObject>("prefab_loadingpanel", "LoadingPanel", "", null));
                    Util.BindParent(GameObject.Find("LoadingCanvas").transform, go.transform);
                    m_instance = go.GetComponent<LoadingPanel>();
                }
                return m_instance;
            }
        }
        private static LoadingPanel m_instance = null;

        public Image imgBG;
        public Text txtLoadingHint;
        public Text txtProgress;
        public Slider barProgress;

        public float endValue { get { return m_endValue; } set { m_endValue = value; } }
        private float m_endValue;
        private float m_curValue;
        private float m_speed = 0.001f;

        private static Action m_onLoaded;
        private Action m_onProgressEnd;

        void FixedUpdate()
        {
            if (m_curValue < m_endValue)
            {
                m_curValue += m_speed;
                P_SetProgress(m_curValue);
            }

            if (m_curValue >= 1f && null != m_onProgressEnd)
            {
                P_SetProgress(1f);
                m_onProgressEnd();
                m_onProgressEnd = null;
            }
        }

        void P_SetProgress(float value)
        {
            barProgress.value = value;
            txtProgress.text = (int)(value * 100) + "%";
        }

        public void StartProgress(float endValue, string loadingHint, string imgBG)
        {
#if UNITY_EDITOR
            JZLog.Log(" ===== 开始加载进度条到 ====>> " + endValue);
#endif

            m_endValue = endValue;
            txtLoadingHint.text = loadingHint;
            P_SetProgress(0f);
        }

        public void SetEndValue(float endValue, string loadingHint = null, Action onProgressEnd = null)
        {
#if UNITY_EDITOR
            if (endValue < 1f)
            {
                JZLog.Log(" ===== 加载进度条到 ====>> " + endValue);
            }
            else
            {
                JZLog.Log(" ===== 结束加载进度条 ====>> ");
            }
#endif

            m_endValue = endValue;
            if (m_endValue - m_curValue > 0.2f)
            {
                m_speed = (m_endValue - m_curValue) / 12;
            }

            if (null != loadingHint)
            {
                txtLoadingHint.text = loadingHint;
            }

            if (endValue >= 1f)
            {
                if (null != onProgressEnd)
                {
                    m_onProgressEnd = onProgressEnd;
                }
            }
        }
    }
}
