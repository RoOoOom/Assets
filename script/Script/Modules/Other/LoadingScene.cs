using System;
using UnityEngine;
using LuaInterface;
using UnityEngine.UI;
using DG.Tweening;
namespace JZWL
{

    public class LoadingScene : MonoBehaviour
    {
        public static LoadingScene instance { get { return m_Instance; } }
        private static LoadingScene m_Instance;

        private Image m_imgBG;
        private Image m_imgAnim;
        private Text m_txtLoadingHint;
        private Text m_txtProgress;

        public float endValue { get { return m_endValue; } set { m_endValue = value; } }
        private float m_endValue;
        private float m_curValue;
        private float m_speed = 0.001f;
        private float m_rotateSpeed = 360f;
        private bool m_startRotation = false;

        private static Action m_onLoaded;
        private Action m_onProgressEnd;

        void Awake()
        {
            m_Instance = this;
            m_imgBG = Util.GetChildByName<Image>(gameObject, "imgBG", true);
            m_imgAnim = Util.GetChildByName<Image>(gameObject, "imgAnim", true);
            m_txtLoadingHint = Util.GetChildByName<Text>(gameObject, "txtLoadingHint", true);
            m_txtProgress = Util.GetChildByName<Text>(gameObject, "txtProgress", true);
        }

        void FixedUpdate()
        {
            if (m_startRotation) m_imgAnim.transform.Rotate(Vector3.forward, -Time.fixedDeltaTime * m_rotateSpeed);

            if (m_curValue < m_endValue)
            {
                m_curValue += m_speed;
                P_SetProgress(m_curValue);
            }

            if (m_curValue >= 1f && null != m_onProgressEnd)
            {
                m_startRotation = false;
                P_SetProgress(1f);
                m_onProgressEnd();
                m_onProgressEnd = null;
            }
        }

        void OnDestroy()
        {
            Scene.onSceneLoaded -= M_OnSceneLoaded;
        }

        void P_SetProgress(float value)
        {
            m_txtProgress.text = (int)(value * 100) + "%";
        }

        public static void Init(Action onLoaded)
        {
            m_onLoaded = onLoaded;
            Scene.onSceneLoaded += M_OnSceneLoaded;
            SceneHelper.instance.LoadLevel(ResConfig.SCENE_LOADING);
        }

        public static void InitLua(LuaFunction onLoaded)
        {
            Init(() => { if (null != onLoaded) onLoaded.Call(); });
        }

        private static void M_OnSceneLoaded()
        {
            Scene.onSceneLoaded -= M_OnSceneLoaded;
            if (null != m_onLoaded)
            {
                m_onLoaded();
                m_onLoaded = null;
            }
        }

        public void StartProgress(int career, float endValue, string loadingHint)
        {
#if UNITY_EDITOR
            JZLog.Log(" ===== 开始加载进度条到 ====>> " + endValue);
#endif

            m_endValue = endValue;
            m_txtLoadingHint.text = loadingHint;
            P_SetProgress(0f);
            m_startRotation = true;
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
                m_txtLoadingHint.text = loadingHint;
            }

            if (endValue >= 1f)
            {
                if (null != onProgressEnd)
                {
                    m_onProgressEnd = onProgressEnd;
                }
            }
        }

        public void SetValue(float value, string hint = null)
        {
            m_curValue = value;
            P_SetProgress(m_curValue);
            if (null != hint)
            {
                m_txtLoadingHint.text = hint;
            }
        }
    }
}
