using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LuaInterface;
using UnityEngine.UI;
using BestHTTP;
using ICSharpCode.SharpZipLib.Zip;


public class ResHelper
{
    private static ResHelper m_instance = null;
    public static ResHelper Instance()
    {
        if (null == m_instance)
        {
            m_instance = new ResHelper();
        }
        return m_instance;
    }

    public bool downloadNotice = true;

    private void CallLuaFunction(LuaFunction func, UnityEngine.Object obj, Type t)
    {
        if (null == obj)
        {
            func.Call();
        }
        else
        {
            func.BeginPCall();
            func.Push(obj);
            func.PCall();
            func.EndPCall();
            func.Dispose();
            func = null;
        }
    }

    public void LoadImageAsyn(string bundleName, string resName, LuaFunction func)
    {
        AtlasHelper.instance.GetSpriteAsync(bundleName, resName, (go) => {
            if (null != go && null != func)
            {
                CallLuaFunction(func, go, typeof(Sprite));
            }
        });

        //AssetBundleManager.instance.GetResourceAsync<Sprite>(bundleName, resName, (Sprite go, bool result) =>
        //{
        //    if (result && null != go && null != func)
        //    {
        //        //func.Call(go);
        //        CallLuaFunction(func, go, typeof(Sprite));
        //    }
        //});
    }

    public void GetGameObjectAsync(string pkgName, string resName, LuaFunction func)
    {
        AssetBundleManager.instance.GetResourceAsync<GameObject>(pkgName, resName, (GameObject go, bool result) =>
        {
            if (null != func)
            {
                CallLuaFunction(func, go, typeof(GameObject));
            }
        });
    }

    public void GetTK2dAsync(string pkgName, string resName, LuaFunction func)
    {
        AtlasHelper.instance.GetTK2dAnimationAsync(pkgName, resName, (GameObject go) => 
        {
            if (null != func)
            {
                CallLuaFunction(func, go, typeof(GameObject));
            }
        });
    }

    public void GetTexture(string pkgName, string resName, LuaFunction func)
    {
        AssetBundleManager.instance.GetResourceAsync<Texture>(pkgName, resName, (Texture go, bool result) =>
        {
            if (null != func)
            {
                CallLuaFunction(func, go, typeof(Texture));
            }
        });
    }

    public void AddReference(string pkgName)
    {
        //ResourceManager.instance.AddReference(pkgName);
    }

    public void SubReference(string pkgName)
    {
        //ResourceManager.instance.DelayUnload(pkgName);
    }

    public void DestroyAssetBundle(string pkgName, bool unload)
    {
        AssetBundleManager.instance.UnloadAssetBundle(pkgName, unload);
    }

    private System.Diagnostics.Stopwatch _stopWatch = new System.Diagnostics.Stopwatch();
    public void StartSW(string name = "")
    {
        Debug.Log("开始========>> stopwatch: " + name);
        _stopWatch.Reset();
        _stopWatch.Start();
    }

    public void StopSW()
    {
        _stopWatch.Stop();
        Debug.Log("历时========>> " + _stopWatch.ElapsedMilliseconds);
    }

    public void GC()
    {
        //Debug.Log(1111111);
        //System.GC.Collect();
        Resources.UnloadUnusedAssets();
        LuaManager.instance.LuaGC();
    }

    public void AddDontUnloadAB(string abName)
    {
        //ResourceManager.instance.AddDontUnloadAB(abName);
    }

    public GameObject NewGO(GameObject prefab)
    {
        if (null == prefab) return null;

        GameObject go = GameObject.Instantiate(prefab);
        //ResourceManager.Instance.AfterInstantiate(go);
        return go;
    }

    public void DestroyGO(GameObject go)
    {
        if (null == go) return;

        //ResourceManager.Instance.BeforeDestroy(go);
        GameObject.Destroy(go);
    }

    long GetLength(HTTPResponse resp)
    {
        List<string> contentLengthHeaders = resp.GetHeaderValues("content-length");
        var contentRangeHeaders = resp.GetHeaderValues("content-range");
        if (contentLengthHeaders != null && contentRangeHeaders == null)
            return long.Parse(contentLengthHeaders[0]);

        return 0;
    }

    void SendToLua(long cur, long total)
    {
        LuaManager.instance.CallFunction("Event.Broadcast", 14000001, (int)(cur / 1048576), (int)(total / 1048576));
    }

    HTTPRequest m_request = null;
    FileStream m_stream = null;
    long m_localLength = 0;
    long m_remoteLength = 0;
    bool m_zipIsOk = false;
    bool m_startDecompress = false;
    bool m_downloadComplated = false;
    public long lastZipIndex = 0;
    public Dictionary<string, int> downloadedFiles = new Dictionary<string, int>();

    public void InitDownloadHttp()
    {
        try
        {
            if (m_remoteLength > 0)
            {
                SendToLua(m_localLength, m_remoteLength);
                return;
            }

            string path = PathUtils.MakeFilePath("http_zip.zip", PathUtils.PathType.MobileDiskWrite);
            string url = GameConfig.HOST_RES() + "http_zip.zip";
            FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            HTTPRequest http = new BestHTTP.HTTPRequest(new Uri(url), HTTPMethods.Head, (req, resp) =>
            {
                if (resp == null)
                {
                    stream.Dispose();
                    return;
                }

                if (resp.StatusCode == 416)
                {
                    stream.Dispose();
                    return;
                }

                if (resp.HasHeaderWithValue("accept-ranges", "none"))
                {
                    stream.Dispose();
                    return;
                }

                m_remoteLength = GetLength(resp);
                if (m_remoteLength <= 0)
                {
                    stream.Dispose();
                    return;
                }

                m_localLength = stream.Length;

                SendToLua(m_localLength, m_remoteLength);

                if (m_localLength == m_remoteLength)
                {
                    m_zipIsOk = true;
                    m_downloadComplated = true;
                    StartDecompress();
                    return;
                }
                else
                {
                    stream.Close();

                    if (NetSpeed.GetCurrentNetType() != JzwlNetworkInfo.TYPE_MOBILE)
                    {
                        StartDownloadHttp();
                    }
                }
            });
            http.DisableCache = true;
            http.Send();
        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message + "\n" + ex.StackTrace);
        }
    }

    void StartDecompress()
    {
        if (m_startDecompress)
            return;

        m_startDecompress = true;

        string endFilePath = PathUtils.MakeFilePath("http_end.txt", PathUtils.PathType.MobileDiskWrite);
        if (File.Exists(endFilePath))
            return;

        Loom.RunAsync(() => {
            FileStream fs = null;
            ZipInputStream zis = null;
            try
            {
                string path = PathUtils.MakeFilePath(GameConfig.HTTP_ZIP_FILE, PathUtils.PathType.MobileDiskWrite);
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                zis = new ZipInputStream(fs);
                ZipEntry ze = null;
                byte[] data = new byte[2048];
                //long lastZipIndex = 0;

                //string infoFilePath = PathUtils.MakeFilePath(GameConfig.LOCAL_DOWNLOAD_INFO_FILE, PathUtils.PathType.MobileDiskWrite);
                //if (File.Exists(infoFilePath))
                //{
                //    using (FileStream infoStream = File.OpenRead(infoFilePath))
                //    {
                //        if (infoStream != null)
                //        {
                //            byte[] index = new byte[infoStream.Length];
                //            infoStream.Read(index, 0, index.Length);
                //            string content = System.Text.Encoding.Default.GetString(index);
                //            DownloadFileInfo downloadFileInfo = JsonFx.Json.JsonReader.Deserialize<DownloadFileInfo>(content);
                //            lastZipIndex = downloadFileInfo.totalSize;
                //            for (int i = 0; i < downloadFileInfo.ids.Length; i++)
                //            {
                //                downloadedFiles.Add(downloadFileInfo.ids[i], 1);
                //            }
                //            fs.Seek(lastZipIndex, SeekOrigin.Begin);
                //        }
                //    }
                //}

                fs.Seek(lastZipIndex, SeekOrigin.Begin);
                while (!GameWorld.isQuit)
                {
                    try
                    {
                        ze = zis.GetNextEntry();
                        if (null != ze)
                        {
                            using (FileStream stream = File.Create(PathUtils.MakeFilePath(ze.Name, PathUtils.PathType.MobileDiskWrite)))
                            {
                                long totalSize = ze.Size;
                                while (!GameWorld.isQuit && totalSize > 0)
                                {
                                    int size = zis.Read(data, 0, data.Length);
                                    totalSize -= size;
                                    if (size > 0)
                                    {
                                        stream.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        System.Threading.Thread.Sleep(10);
                                    }
                                }

                                if (totalSize == 0)
                                {
                                    lastZipIndex += ze.headSize;
                                    lastZipIndex += zis.GetTotalIn();
                                    GameUtils.AddItemToDic(downloadedFiles, ze.Name, 1);
                                }
                            }
                        }
                        else if (m_downloadComplated)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        if (m_downloadComplated)
                        {
                            break;
                        }
                        zis.Reset();
                        fs.Seek(lastZipIndex, SeekOrigin.Begin);
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
            catch(Exception e)
            {
                Loom.QueueOnMainThread(() => {
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
                });
            }
            finally
            {
                if (null != zis)
                    zis.Close();

                if (null != fs)
                    fs.Close();

                using (FileStream infoStream = new FileStream(PathUtils.MakeFilePath(GameConfig.LOCAL_DOWNLOAD_INFO_FILE, PathUtils.PathType.MobileDiskWrite), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    DownloadFileInfo info = new DownloadFileInfo() { totalSize = lastZipIndex, ids = downloadedFiles.Keys.ToArray<string>() };
                    string content = JsonFx.Json.JsonWriter.Serialize(info);
                    byte[] index = Encoding.Default.GetBytes(content);
                    infoStream.Write(index, 0, index.Length);
                }
            }
        });
    }

    public void StartDownloadHttp()
    {
        if (m_zipIsOk)
        {
            return;
        }

        string path = PathUtils.MakeFilePath(GameConfig.HTTP_ZIP_FILE, PathUtils.PathType.MobileDiskWrite);
        string url = GameConfig.HOST_RES() + GameConfig.HTTP_ZIP_FILE;
        m_stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        m_request = new HTTPRequest(new Uri(url), HTTPMethods.Get, true, (downloadReq, downloadResp) =>
        {
            m_stream.Seek(m_localLength, SeekOrigin.Begin);
            if (downloadResp == null)
                return;

            System.Collections.Generic.List<byte[]> fragments = downloadResp.GetStreamedFragments();
            foreach (byte[] data in fragments)
            {
                m_stream.Write(data, 0, data.Length);
                m_localLength += data.Length;
            }
            m_stream.Flush();
            if (downloadNotice || m_localLength > m_remoteLength)
            {
                SendToLua(m_localLength, m_remoteLength);
            }
            //SendToLua(m_localLength, m_remoteLength);
            StartDecompress();
            
            if (downloadResp.IsStreamingFinished)
            {
                m_downloadComplated = true;
                m_stream.Close();
            }
        });

        m_stream.Seek(m_localLength, SeekOrigin.Begin);
        m_request.SetRangeHeader((int)m_localLength);
        m_request.UseStreaming = true;
        m_request.StreamFragmentSize = 1 * 1024 * 1024;
        m_request.DisableCache = true;
        m_request.Send();
    }

    public void StopDownloadHttp()
    {
        if (null != m_request)
        {
            m_request.Abort();
        }

        if (null != m_stream)
        {
            m_stream.Close();
        }

        m_request = null;
        m_stream = null;
        m_localLength = 0;
        m_remoteLength = 0;
        m_zipIsOk = false;
        m_startDecompress = false;
        m_downloadComplated = false;
    }
}
