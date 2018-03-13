// ***************************************************************
//  FileName: AudioPlay2D.cs
//  Version : 1.0
//  Date    : 2016/6/27
//  Author  : cjzhanying 
//  Copyright (C) 2012 - Digital Technology Co.,Ltd. All rights reserved.	版权申明
//  --------------------------------------------------------------
//  Description: 2D声音播放
//  -------------------------------------------------------------
//  History:
//  -------------------------------------------------------------
// ***************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#region 数据类
public class Audio2DData
{
    public enum PlayState
    {
        Stop,
        eWaitPlay,
        Play,
        Pause,
    }
    public PlayState ePlayState = PlayState.Stop;
    public AudioPlay2D.AudioType eType = AudioPlay2D.AudioType.Audio;
    public AudioSource audioSource = null;
    public Audio2DData(AudioPlay2D.AudioType type)
    {
        eType = type;
    }
    public bool IsPlaying()
    {
        if (audioSource != null)
        {
            return audioSource.isPlaying;
        }
        return false;
    }
    public float AudioTime()
    {
        if (audioSource != null)
        {
            return audioSource.time;
        }
        return 0f;
    }
}
#endregion

public class AudioPlay2D : MonoBehaviour
{
    #region 声音类型
    /** @brief: 声音类型 */
    public enum AudioType
    {
        BGM,    //背景音乐
        Audio,  //音效
        Max,
    }
    private static bool[] gAudioMute = new bool[(int)AudioType.Max]
    {
        false,
        false,
    };
    #endregion

    #region 内部数据
    private static AudioPlay2D gInstance = null;
    private Dictionary<int, Audio2DData> mDicOpenAudioList = new Dictionary<int, Audio2DData>(); /** @brief:  播放记录 */
    private GameObject mPlayerRoot = null; /** @brief: 挂载音乐的东东 */
    #endregion

    /* ============================================================================================================================*/
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /* ============================================================================================================================*/

    #region 销毁所有音乐
    /************************************
     * 函数说明: 销毁所有音乐
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    public static void CleanAllAudio()
    {
        if (gInstance == null)
        {
            return;
        }
        gInstance.CleanAllAudioInterface();
    }
    #endregion

    #region 播放声音
    /************************************
     * 函数说明: 播放声音
     * 返 回 值: void
     * 参数说明: ePkg
     * 参数说明: name
     * 参数说明: loop
     * 注意事项: 
    ************************************/
    public static void PlayOther(string ePkg, string name, bool loop)
    {
        Play(ePkg, AudioPlay2D.AudioType.Audio, name, 1.0f, loop);
    }
    public static void PlayOfType(string ePkg, AudioPlay2D.AudioType type, string name, bool loop)
    {
        Play(ePkg, type, name, 1.0f, loop);
    }
    public static void Play(string ePkg, AudioPlay2D.AudioType type, string name, float volume, bool loop)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        InitInstance();
        gInstance.PlayAudio(ePkg, type, name, volume, loop);
    }
    #endregion

    #region 停止播放声音
    /************************************
     * 函数说明: 停止播放声音
     * 返 回 值: void
     * 参数说明: ePkg
     * 参数说明: name
     * 注意事项: 
    ************************************/
    public static void Stop(string ePkg, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        InitInstance();
        gInstance.StopAudio(ePkg, name);
    }
    #endregion

    #region 暂停播放声音
    /************************************
     * 函数说明: 暂停声音
     * 返 回 值: void
     * 参数说明: ePkg
     * 参数说明: name
     * 注意事项: 
    ************************************/
    public static void Pause(string ePkg, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        InitInstance();
        gInstance.PauseAudio(ePkg, name);
    }
    #endregion

    #region 恢复播放声音
    /************************************
     * 函数说明: 恢复播放声音
     * 返 回 值: void
     * 参数说明: ePkg
     * 参数说明: name
    ************************************/
    public static void Resume(string ePkg, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        InitInstance();
        gInstance.ResumeAudio(ePkg, name);
    }
    #endregion

    #region 设置某种类型声音是否静音
    /************************************
     * 函数说明: 设置某种类型声音是否静音
     * 返 回 值: void
     * 参数说明: type
     * 参数说明: bMute
     * 注意事项: true 表示静音 , false 表示非静音
                 根据需求自行添加音效类型 目前只区分 背景音效 和 其他音效
    ************************************/
    public static void SetMute(AudioPlay2D.AudioType type, bool bMute)
    {
        if (gAudioMute[(int)type] == bMute)
        {
            return;
        }
        InitInstance();
        gAudioMute[(int)type] = bMute;
        /** @brief: 查询所有的声音 设置是否静音 */
        foreach (KeyValuePair<int, Audio2DData> mianKey in gInstance.mDicOpenAudioList)
        {
            if (mianKey.Value.eType == type)
            {
                if (mianKey.Value.audioSource != null)
                {
                    mianKey.Value.audioSource.mute = bMute;
                }
            }
        }
    }
    #endregion

    #region 设置某种类型声音音量
    /************************************
     * 函数说明: 设置某种类型声音是否静音
     * 返 回 值: void
     * 参数说明: type
     * 参数说明: volume 音量,百分比
    ************************************/
    public static void SetVolume(AudioPlay2D.AudioType type, int volume)
    {
        InitInstance();
        /** @brief: 查询所有的声音 设置是否静音 */
        foreach (KeyValuePair<int, Audio2DData> mianKey in gInstance.mDicOpenAudioList)
        {
            if (mianKey.Value.eType == type)
            {
                if (mianKey.Value.audioSource != null)
                {
                    mianKey.Value.audioSource.volume = volume / 100f;
                }
            }
        }
    }
    #endregion

    #region 获取音频片段
    /************************************
     * 函数说明: 获取音频片段
     * 返 回 值: void
     * 参数说明: ePkg
     * 参数说明: name
     * 参数说明: callBack
     * 注意事项: 
    ************************************/
    public static void GetAudioClip(string ePkg, string name, System.Action<AudioClip, bool> callBack)
    {
        AssetBundleManager.instance.GetResourceAsync<AudioClip>(ePkg, name, (clip, bRet) =>
        {
            if (callBack != null)
            {
                callBack(clip, bRet);
                callBack = null;
            }
        });
    }
    #endregion

    /* ============================================================================================================================*/
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /* ============================================================================================================================*/

    #region 创建根挂载点
    /************************************
     * 函数说明: 创建根挂载点
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    void CreatPlayerRoot()
    {
        mPlayerRoot = new GameObject("Audio2DPlayRoot");
        mPlayerRoot.transform.parent = gInstance.gameObject.transform;
    }
    #endregion

    #region 内部销毁所有音乐
    void CleanAllAudioInterface()
    {
        if (mPlayerRoot != null)
        {
            Destroy(mPlayerRoot);
        }
        CreatPlayerRoot();
        mDicOpenAudioList.Clear();
    }
    #endregion

    #region 获取Hash值
    /************************************
     * 函数说明: 获取Hash值
     * 返 回 值: int
     * 参数说明: ePkg
     * 参数说明: szName
     * 注意事项: 由包名和资源名组成的字符串而来
    ************************************/
    static int GetAudioHashCode(string ePkg, string szName)
    {
        if (string.IsNullOrEmpty(szName))
        {
            return 0;
        }
        return (ePkg + szName).GetHashCode();
    }
    #endregion

    #region 内部播放声音
    /************************************
     * 函数说明: 播放声音
     * 返 回 值: void
     * 参数说明: ePkg
     * 参数说明: name
     * 参数说明: volume
     * 参数说明: loop
     * 注意事项: 
    ************************************/
    void PlayAudio(string ePkg, AudioPlay2D.AudioType type, string name, float volume, bool loop)
    {
        int iKey = GetAudioHashCode(ePkg, name);
        if (iKey == 0)
        {
            return;
        }
        /** @brief: 如果当前类型为静音 且不重复 那么跳过 */
        if (gAudioMute[(int)type] == true && loop == false)
        {
            return;
        }

        /** @brief: 如果播放还未停止或者已经加载过 */
        if (mDicOpenAudioList.ContainsKey(iKey))
        {
            AudioSource audio = mDicOpenAudioList[iKey].audioSource;
            /** @brief: 资源已经加载过 直接播放 */
            if (audio.clip != null)
            {
                audio.volume = volume;
                audio.loop = loop;
                audio.pitch = 1.0f;
                audio.mute = gAudioMute[(int)type];
                audio.Play();
                mDicOpenAudioList[iKey].ePlayState = Audio2DData.PlayState.Play;
                return;
            }
            else
            {
                mDicOpenAudioList[iKey].ePlayState = Audio2DData.PlayState.Stop;
            }
        }
        else
        {
            /** @brief: 初次次播放 */
            mDicOpenAudioList[iKey] = new Audio2DData(type);
            /** @brief: 创建播放者 */
            GameObject audioPlayer = new GameObject(iKey.ToString());
            audioPlayer.transform.parent = mPlayerRoot.transform;
            /** @brief: 添加记录 */
            mDicOpenAudioList[iKey].audioSource = audioPlayer.AddComponent<AudioSource>();
            /** @brief: 设置为2D音效 */
            mDicOpenAudioList[iKey].audioSource.spatialBlend = 0;
        }

        /** @brief: 加载音频 */
        mDicOpenAudioList[iKey].ePlayState = Audio2DData.PlayState.eWaitPlay;
        GetAudioClip(ePkg, name, (clip, bRet) =>
        {
            if (mDicOpenAudioList.ContainsKey(iKey))
            {
                if (mDicOpenAudioList[iKey].ePlayState != Audio2DData.PlayState.eWaitPlay)
                {
                    Debug.LogWarning("You have Stop or Pause this audio first!");
                    return;
                }
                AudioSource audio = mDicOpenAudioList[iKey].audioSource;
                if (audio.clip == null)
                {
                    #region 第一次加载
                    if (bRet == true && clip != null)
                    {
                        audio.clip = clip;
                        audio.volume = volume;
                        audio.loop = loop;
                        audio.pitch = 1.0f;
                        audio.mute = gAudioMute[(int)type];
                        audio.Play();
                        mDicOpenAudioList[iKey].ePlayState = Audio2DData.PlayState.Play;
                    }
                    else
                    {
                        Destroy(audio.gameObject);
                        mDicOpenAudioList.Remove(iKey);
                    }
                    #endregion
                }
                else
                {
                    #region 已经加载过
                    audio.volume = volume;
                    audio.loop = loop;
                    audio.pitch = 1.0f;
                    audio.mute = gAudioMute[(int)type];
                    if (audio.isPlaying == false)
                    {
                        audio.Play();
                    }
                    mDicOpenAudioList[iKey].ePlayState = Audio2DData.PlayState.Play;
                    #endregion
                }
            }

        });
    }
    #endregion

    #region 内部停止播放声音
    /************************************
     * 函数说明: 停止播放声音
     * 返 回 值: void
     * 参数说明: ePkg
     * 参数说明: name
     * 注意事项: 删除资源
    ************************************/
    void StopAudio(string ePkg, string name)
    {
        int iKey = GetAudioHashCode(ePkg, name);
        if (mDicOpenAudioList.ContainsKey(iKey))
        {
            if (mDicOpenAudioList[iKey].IsPlaying())
            {
                mDicOpenAudioList[iKey].audioSource.Stop();
            }
            mDicOpenAudioList[iKey].ePlayState = Audio2DData.PlayState.Stop;
        }
    }
    #endregion

    #region 内部暂停播放声音
    /************************************
     * 函数说明: 暂停播放声音
     * 返 回 值: void
     * 参数说明: ePkg
     * 参数说明: name
     * 注意事项: 如果声音正在播放,那么播放声音,
    ************************************/
    void PauseAudio(string ePkg, string name)
    {
        int iKey = GetAudioHashCode(ePkg, name);
        if (mDicOpenAudioList.ContainsKey(iKey))
        {
            if (mDicOpenAudioList[iKey].audioSource != null)
            {
                mDicOpenAudioList[iKey].audioSource.Pause();
            }
            mDicOpenAudioList[iKey].ePlayState = Audio2DData.PlayState.Pause;
        }
    }
    #endregion

    #region 恢复播放声音
    /************************************
     * 函数说明: 播放声音
     * 返 回 值: void
     * 参数说明: ePkg
     * 参数说明: name
    ************************************/
    void ResumeAudio(string ePkg, string name)
    {
        int iKey = GetAudioHashCode(ePkg, name);
        if (mDicOpenAudioList.ContainsKey(iKey))
        {
            if (mDicOpenAudioList[iKey].audioSource != null)
            {
                mDicOpenAudioList[iKey].audioSource.Play();
            }
            mDicOpenAudioList[iKey].ePlayState = Audio2DData.PlayState.Play;
        }
    }
    #endregion

    #region 初始化管理类实例
    /************************************
     * 函数说明: 添加实例
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    static void InitInstance()
    {
        if (gInstance == null)
        {
            GameWorld gw = GameWorld.instance;
            if (null != gw)
            {
                gInstance = gw.gameObject.AddComponent<AudioPlay2D>();
                gInstance.CreatPlayerRoot();
            }
        }
    }
    #endregion
}