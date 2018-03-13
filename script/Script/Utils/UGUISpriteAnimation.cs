using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace JZWL
{
    public class UGUISpriteAnimation : MonoBehaviour
    {
        [SerializeField]
        protected int framesPerSecond = 20;
        public string[] frameResNames;
        public int frameCount { get { return frameResNames.Length; } }
        public string atlasBundleName;
        public bool ignoreTimeScale = true;
        public bool isPlaying { get { return enabled; } }
        public bool loop = true;
        public bool setNativeSieze = false;
        public Action callback;

        private Image m_imageRoot;
        private int curIndex = 0;
        private float timeCounter = 0;

        void Start()
        {
            GetImage().enabled = false;
        }

        public void Play()
        {
            if (frameResNames != null && frameResNames.Length > 0)
            {
                if (!enabled && !loop)
                {
                    int newIndex = framesPerSecond > 0 ? curIndex + 1 : curIndex - 1;
                    if (newIndex < 0 || newIndex >= frameResNames.Length)
                    {
                        curIndex = framesPerSecond < 0 ? frameResNames.Length - 1 : 0;
                    }
                }

                enabled = true;
                UpdateSprite();
            }
        }

        Image GetImage()
        {
            if (m_imageRoot == null)
            {
                m_imageRoot = GetComponent<Image>();
            }

            return m_imageRoot;
        }

        public void Pause()
        {
            enabled = false;
        }

        public void ResetToBeginning()
        {
            curIndex = framesPerSecond < 0 ? frameResNames.Length - 1 : 0;
            UpdateSprite();
        }

        void Update()
        {
            if (frameResNames == null || frameResNames.Length == 0)
            {
                enabled = false;
                return;
            }
            
            if (framesPerSecond != 0)
            {
                float time = ignoreTimeScale ? Time.unscaledTime : Time.time;
                if (timeCounter < time)
                {
                    timeCounter = time;
                    int newIndex = framesPerSecond > 0 ? curIndex + 1 : curIndex - 1;
                    if (!loop && (newIndex < 0 || newIndex >= frameResNames.Length))
                    {
                        enabled = false;
                        return;
                    }
                    curIndex = RepeatIndex(newIndex, frameResNames.Length);
                    UpdateSprite();
                }
            }
        }

        /// <summary>
        /// 获取循环后的下一帧
        /// </summary>
        /// <param name="val"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private int RepeatIndex(int val, int max)
        {
            if (max < 1) return 0;
            while (val < 0) val += max;
            while (val >= max) val -= max;
            return val;
        }

        /// <summary>
        /// 切换图片
        /// </summary>
        private void UpdateSprite()
        {
            if (GetImage() == null)
            {
                enabled = false;
                return;
            }

            float time = ignoreTimeScale ? Time.unscaledTime : Time.time;
            if (framesPerSecond != 0)
            {
                timeCounter = time + Mathf.Abs(1f / framesPerSecond);
            }

            AtlasHelper.instance.GetSpriteAsync(atlasBundleName, frameResNames[curIndex], (sprite) => {
                if (null != sprite)
                {
                    if (!GetImage().enabled)
                    {
                        GetImage().enabled = true;
                    }

                    GetImage().sprite = sprite;

                    if (setNativeSieze)
                    {
                        GetImage().SetNativeSize();
                    }

                    if (null != this.callback)
                    {
                        this.callback();
                        this.callback = null;
                    }
                }
            });
            //LoadImageAsyn(atlasBundleName, frameResNames[curIndex]);
        }

        /// <summary>
        /// 通过ab加载图片
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="resName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private void LoadImageAsyn(string bundleName, string resName)
        {
            AssetBundleManager.instance.GetResourceAsync<Sprite>(bundleName, resName, (Sprite go, bool result) =>
            {
                if (GetImage() == null)
                    return;

                if (!GetImage().enabled)
                {
                    GetImage().enabled = true;
                }

                GetImage().sprite = go;

                if (setNativeSieze)
                {
                    GetImage().SetNativeSize();
                }

                if (result && null != go && null != this.callback)
                {
                    this.callback();
                    this.callback = null;
                }
            });
        }
    }
}
