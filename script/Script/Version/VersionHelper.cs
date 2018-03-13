using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class VersionHelper : SimleManagerTemplate<VersionHelper>
{
    private const string M_NEED_UPDATE_APK = "update_apk";
    public VersionConfig version { get { return m_version; } }
    private VersionConfig m_version;

    private VersionBundleConfig m_localConfig;
    private VersionBundleConfig m_loadedConfig;
    private VersionBundleConfig m_httpConfig;
    private VersionBundleConfig m_localHttpConfig;

    private Dictionary<string, VersionBundle> m_httpBundles = new Dictionary<string, VersionBundle>();
    private Dictionary<string, VersionBundle> m_bundles = new Dictionary<string,VersionBundle>();
    public Dictionary<string, VersionBundle> needUpdateBundles { get { return m_needUpdateBundles; } }
    private Dictionary<string, VersionBundle> m_needUpdateBundles = new Dictionary<string, VersionBundle>();

    /// <summary>
    /// 加载版本文件
    /// </summary>
    /// <param name="onComplete"></param>
    /// <param name="onError"></param>
    public void LoadVersion(Action onComplete, Action onError)
    {
        string url = GameConfig.HOST_RES() + GameConfig.LOCAL_HTTP_CONFIG_FILE + "?t=" + TimeUtils.CurLocalTimeMilliSecond();
        BestHTTP.HTTPRequest request = new BestHTTP.HTTPRequest(new Uri(url), (req, resp) => {
            if (resp != null)
            {
                Loom.RunAsync(() =>
                {
                    m_httpConfig = JsonFx.Json.JsonReader.Deserialize<VersionBundleConfig>(resp.DataAsText);
                    M_CacheHttpBundles();

                    string infoFilePath = PathUtils.MakeFilePath(GameConfig.LOCAL_DOWNLOAD_INFO_FILE, PathUtils.PathType.MobileDiskWrite);
                    if (File.Exists(infoFilePath))
                    {
                        using (FileStream infoStream = File.OpenRead(infoFilePath))
                        {
                            if (infoStream != null)
                            {
                                byte[] index = new byte[infoStream.Length];
                                infoStream.Read(index, 0, index.Length);
                                string content = System.Text.Encoding.Default.GetString(index);
                                DownloadFileInfo downloadFileInfo = JsonFx.Json.JsonReader.Deserialize<DownloadFileInfo>(content);
                                ResHelper.Instance().lastZipIndex = downloadFileInfo.totalSize;
                                for (int i = 0; i < downloadFileInfo.ids.Length; i++)
                                {
                                    ResHelper.Instance().downloadedFiles.Add(downloadFileInfo.ids[i], 1);
                                }
                            }
                        }
                    }

                    if (GameConfig.useLocalRes)
                    {
                        m_version = new VersionConfig();
                        m_version.version = "0.0.0";
                        Loom.QueueOnMainThread(() => {
                            if (onComplete != null) onComplete();
                        });
                    }
                    else
                    {
                        url = GameConfig.HOST_RES_ZIP() + "zip/" + GameConfig.LOCAL_VERSION_FILE + "?t=" + TimeUtils.CurLocalTimeMilliSecond();
                        BestHTTP.HTTPRequest zipRequest = new BestHTTP.HTTPRequest(new Uri(url), (zipReq, zipResp) => {
                            if (zipResp != null)
                            {
                                m_version = JsonFx.Json.JsonReader.Deserialize<VersionConfig>(zipResp.DataAsText);
                                if (null != onComplete) onComplete();
                            }
                            else
                            {
                                if (null != onError) onError();
                            }
                        });
                        zipRequest.Send();
                    }
                });
            }
            else
            {
                if (null != onError) onError();
            }
        });
        request.DisableCache = true;
        request.Send();
    }

    /// <summary>
    /// 加载版本配置文件
    /// </summary>
    /// <param name="onComplete"></param>
    /// <param name="onError"></param>
    public void LoadConfig(Action onComplete, Action onError)
    {
        GameWorld.instance.StartCoroutine(ReadLocalConfig(
            () =>
            {
                if (null != m_localConfig && m_localConfig.versionValue >= m_version.versionValue)
                {
                    //缓存中有配置且配置与服务器同步，使用缓存构建需要加载的版本信息
                    M_CacheBundles();
                    onComplete();
                }
                else
                {
                    ThreadManager.instance.AddEvent(new NotiData() { id = MessageId.DOWNLOAD_CONFIG, evParams = m_localConfig.versionValue + "_" +  m_version.versionValue }, null);
                }
            },
            () =>
            {
                onError();
            }));
    }

    IEnumerator ReadLocalConfig(Action onComplete, Action onError)
    {
        string path = PathUtils.MakeFilePath(GameConfig.LOCAL_CONFIG_FILE, PathUtils.PathType.MobileDiskWrite);
        try
        {
            if (File.Exists(path))
            {
                Loom.RunAsync(() => {
                    StreamReader sr = null;
                    sr = File.OpenText(path);
                    string content = sr.ReadToEnd();
                    sr.Close();
                    m_localConfig = JsonFx.Json.JsonReader.Deserialize<VersionBundleConfig>(content);
                    if (null != onComplete)
                    {
                        Loom.QueueOnMainThread(()=>{
                            onComplete();
                        });
                    }
                });
                yield break;
            }
        }
        catch (Exception error)
        {
            JZLog.LogError("VersionManager:Read - " + "Error: " + error.Message);
            if (null != onError) onError();
        }

        path = "file://" + PathUtils.MakeFilePath(GameConfig.LOCAL_CONFIG_FILE, PathUtils.PathType.MobileDiskStreamAssert);
        if (Application.platform == RuntimePlatform.Android)
        {
            path = "jar:" + path;
        }
        WWW www = new WWW(path);
        yield return www;
        if (null != www && null == www.error)
        {
            JZLog.Log("load config success");
            string content = System.Text.Encoding.Default.GetString(www.bytes);
            Loom.RunAsync(() => {
                m_localConfig = JsonFx.Json.JsonReader.Deserialize<VersionBundleConfig>(content);
                if (null != onComplete)
                {
                    Loom.QueueOnMainThread(() =>
                    {
                        onComplete();
                    });
                }
            });
        }
        else
        {
            JZLog.LogError("VersionManager:Read - " + "Error: " + www.error);
            if (null != onError) onError();
        }
    }

    IEnumerator M_LoadConfig(Action onComplete, Action onError)
    {
        string url = GameConfig.HOST_RES() + "ab/" + GameConfig.LOCAL_CONFIG_FILE + "?t=" + TimeUtils.CurLocalTimeMilliSecond();
        WWW loader = new WWW(url);
        yield return loader;

        if (string.IsNullOrEmpty(loader.error))
        {
            VersionBundleConfig config = JsonFx.Json.JsonReader.Deserialize<VersionBundleConfig>(loader.text);
            m_loadedConfig = config;
            M_CacheBundles();
            if (onComplete != null) onComplete();
        }
        else
        {
            JZLog.LogError(loader.error);
            if (onError != null) onError();
        }
    }

    public List<string> GetAllLuaBundle()
    {
        List<string> ids = new List<string>();
        if (!GameConfig.useLocalRes)
        {
            foreach (string id in m_bundles.Keys)
            {
                if (id.StartsWith("lua"))
                {
                    ids.Add(id);
                }
            }
        }
        
        return ids;
    }

    public bool HasBundleHttp(string id)
    {
        return m_httpBundles.ContainsKey(id);
    }

    public void GetBundleHttp(string id, out VersionBundle vb)
    {
        m_httpBundles.TryGetValue(id, out vb);
    }

    public void GetLocalBundle(string id, out VersionBundle vb)
    {
        m_bundles.TryGetValue(id, out vb);
    }

    public bool HasBundle(string id)
    {
        if (!m_bundles.ContainsKey(id))
        {
            return m_httpBundles.ContainsKey(id);
        }
        else
        {
            return true;
        }
    }

    public VersionBundle GetBundle(string id)
    {
        VersionBundle vb = null;

        if (m_bundles.TryGetValue(id, out vb))
        {
            return vb;
        }

        if (m_httpBundles.TryGetValue(id, out vb))
        {
            return vb;
        }

        return vb;
    }

    void M_CacheHttpBundles()
    {
        m_httpBundles.Clear();
        string path = "file://" + PathUtils.MakeFilePath(GameConfig.LOCAL_HTTP_CONFIG_FILE, PathUtils.PathType.MobileDiskStreamAssert);
        if (Application.platform == RuntimePlatform.Android)
        {
            path = "jar:" + path;
        }

        try
        {
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            if (stream != null && stream.Length > 0)
            {
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                stream.Close();
                string content = System.Text.Encoding.Default.GetString(data);
                m_localHttpConfig = JsonFx.Json.JsonReader.Deserialize<VersionBundleConfig>(content);
            }
            else
            {
                m_localHttpConfig = null;
            }
        }
        catch(Exception e)
        {
            m_loadedConfig = null;
        }
        

        VersionBundle vb = null;
        Dictionary<string, VersionBundle> temp = new Dictionary<string, VersionBundle>();

        if (null != m_localHttpConfig && null != m_localHttpConfig)
        {
             for (int i = 0; i < m_localHttpConfig.bundles.Length; i++)
            {
                vb = m_localHttpConfig.bundles[i];
                temp.Add(vb.id, vb);
            }
        }

        if (null != m_httpConfig && null != m_httpConfig.bundles)
        {
            for (int i = 0; i < m_httpConfig.bundles.Length; i++)
            {
                vb = m_httpConfig.bundles[i];
                if (temp.ContainsKey(vb.id))
                {
                    if (temp[vb.id].versionValue < vb.versionValue)
                    {
                        m_httpBundles.Add(vb.id, vb);
                    }
                }
                else
                {
                    m_httpBundles.Add(vb.id, vb);
                }
            }
        }
    }

    /// <summary>
    /// 缓存bundle包并确定需要更新的资源包
    /// !!!: crc > 0 表示为http资源，使用WWW.LoadFromCacheOrDownload加载
    /// !!!: crc <= 0 并且本地版本号低于服务器版本号的需要更新，暂时为子bundle更新，后续可能更改为zip包更新解压方式
    /// </summary>
    void M_CacheBundles()
    {
        VersionBundle vb = null;
        m_bundles.Clear();
        m_needUpdateBundles.Clear();
        if (null != m_localConfig && null != m_localConfig.bundles)
        {
            for (int i = 0; i < m_localConfig.bundles.Length; i++)
            {
                vb = m_localConfig.bundles[i];
                if (m_bundles.ContainsKey(vb.id))
                {
                    Debug.LogError(m_localConfig.bundles[i].id);
                }

                m_bundles.Add(vb.id, vb);
            }
        }

        if (null != m_loadedConfig && null != m_loadedConfig.bundles)
        {
            for (int i = 0; i < m_loadedConfig.bundles.Length; i++)
            {
                vb = m_loadedConfig.bundles[i];
                if (m_bundles.ContainsKey(vb.id))
                {
                    if (m_bundles[vb.id].versionValue < vb.versionValue)
                    {
                        m_bundles[vb.id] = vb;
                        AddBundleToDic(m_needUpdateBundles, vb);
                    }
                }
                else
                {
                    m_bundles.Add(vb.id, vb);
                    AddBundleToDic(m_needUpdateBundles, vb);
                }
            }
        }
    }

    /// <summary>
    /// 添加bundle到指定字典中
    /// </summary>
    /// <param name="dic"></param>
    /// <param name="bundle"></param>
    void AddBundleToDic(Dictionary<string, VersionBundle> dic, VersionBundle bundle)
    {
        if (!dic.ContainsKey(bundle.id))
        {
            dic.Add(bundle.id, bundle);
        }
        else
        {
            dic[bundle.id] = bundle;
        }
    }

    /// <summary>
    /// 写入版本配置文件
    /// !!!: 现在为下载完所有写入方式，后续可能改为下载几个就写入一次
    /// </summary>
    public void WriteConfig()
    {
        if (null != m_loadedConfig)
        {
            M_Write(GameConfig.LOCAL_CONFIG_FILE, m_loadedConfig);
        }
    }

    /// <summary>
    /// 写入版本配置文件
    /// </summary>
    /// <param name="url"></param>
    /// <param name="config"></param>
    private void M_Write(string name, VersionBundleConfig config)
    {
#if !UNITY_WEBPLAYER
        string path = PathUtils.MakeFilePath(name, PathUtils.PathType.MobileDiskWrite);
        Debug.LogWarning("write path: " + path);
        try
        {
            StreamWriter sw;
            FileInfo fInfo = new FileInfo(path);
            if (!fInfo.Exists)
            {
                string directoryPath = path.Substring(0, path.LastIndexOf('/'));
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                sw = fInfo.CreateText();
            }
            else
            {
                fInfo.Delete();
                fInfo = new FileInfo(path);
                sw = fInfo.CreateText();
            }
            string content = JsonFx.Json.JsonWriter.Serialize(config);
            sw.Write(content);
            sw.Close();
            sw.Dispose();
        }
        catch (Exception error)
        {
            JZLog.LogError("VersionManager:Write - " + "Error: " + error.Message);
        }
#endif
    }

    /// <summary>
    /// 检测是否需要更新整包
    /// </summary>
    /// <returns></returns>
    public bool NeedUpdateAPK()
    {
        return null != version && null != version.other &&
            Array.Exists<OtherData>(version.other, it => it.key.Equals(M_NEED_UPDATE_APK));
    }
}
