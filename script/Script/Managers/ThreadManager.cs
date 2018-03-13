using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Net;
using System;
using UnityEngine;

public class NotiData 
{
    public int id;
    public object evParams = null;
}

public class DownloadTips
{
    public string tips;
    public string speed;
    public float progress;
}

    /// <summary>
    /// 当前线程管理器，同时只能做一个任务
    /// </summary>
public class ThreadManager : MonoBehaviour
{
    public static ThreadManager instance
    {
        get
        {
            if (null == m_instance)
            {
                if (GameWorld.instance == null) return null;
                m_instance = GameWorld.instance.gameObject.AddComponent<ThreadManager>();
            }
            return m_instance;
        }
    }
    private static ThreadManager m_instance = null;

    private Thread thread;
    private string currDownFile = string.Empty;

    static readonly object m_lockObj = new object();
    static Queue<NotiData> events = new Queue<NotiData>();

    delegate void ThreadSyncEvent(NotiData data);
    private ThreadSyncEvent _syncEvent = null;

    private Action<NotiData> _callback = null;

    void Awake() 
    {
        _syncEvent = OnSyncEvent;
        thread = new Thread(OnUpdate);
    }

    // Use this for initialization
    void Start() 
    {
        thread.IsBackground = true;
        thread.Start();
    }

    /// <summary>
    /// 应用程序退出
    /// </summary>
    void OnDestroy()
    {
        thread.Abort();
    }

    /// <summary>
    /// 添加到事件队列
    /// </summary>
    public void AddEvent(NotiData ev, Action<NotiData> callback)
    {
        lock (m_lockObj)
        {
            _callback = callback;
            events.Enqueue(ev);
        }
    }

    /// <summary>
    /// 通知事件
    /// </summary>
    /// <param name="state"></param>
    private void OnSyncEvent(NotiData data)
    {
        if (null != this._callback) _callback(data);
        //通知View层
    }

    // Update is called once per frame
    void OnUpdate() 
    {
        while (true) 
        {
            lock (m_lockObj) 
            {
                if (events.Count > 0) 
                {
                    NotiData e = events.Dequeue();
                    try {
                        switch (e.id) 
                        {
                            case MessageId.DOWNLOAD_CONFIG: 
                            {     //解压文件
                                OnDownloadConfig((string)e.evParams);
                            }
                            break;
                        }
                    } 
                    catch (System.Exception ex) 
                    {
                        UnityEngine.Debug.LogError(ex.Message);
                    }
                }
            }
            Thread.Sleep(1);
        }
    }

    void OnDownloadConfig(string versionValue)
    {
        string fileName = versionValue + ".zip";
        string url = GameConfig.HOST_RES_ZIP() + "zip/" + fileName + "?t=" + TimeUtils.CurLocalTimeMilliSecond();
        string filePath = PathUtils.MakeFilePath(fileName, PathUtils.PathType.MobileDiskWrite);

        float progress = 0f;

        //使用流操作文件
        FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        //获取文件现在的长度
        long fileLength = fs.Length;
        //获取下载文件的总长度
        //long totalLength = GetLength(url);
        HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
        request.Timeout = 60000;
        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
        long totalLength = response.ContentLength;
        string size = (totalLength / 1048576f).ToString("0.00") + "MB";
        string tips = "下载更新包：0/" + size;
        DownloadTips dt = new DownloadTips() { tips = tips };
        MessageSystem.Instance().PostMessage(MessageId.DOWNLOAD_CONFIG_PROGRESS, dt, 0);
        DateTime startTime = DateTime.Now;
        //如果没下载完
        if (fileLength < totalLength)
        {
            //断点续传核心，设置本地文件流的起始位置
            fs.Seek(fileLength, SeekOrigin.Begin);
            //断点续传核心，设置远程访问文件流的起始位置
            request.AddRange((int)fileLength);
            Stream stream = response.GetResponseStream();

            byte[] buffer = new byte[1024];
            //使用流读取内容到buffer中
            //注意方法返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
            int length = stream.Read(buffer, 0, buffer.Length);
            while (length > 0)
            {
                //将内容再写入本地文件中
                fs.Write(buffer, 0, length);
                //计算进度
                fileLength += length;
                progress = (float)fileLength / (float)totalLength;

                dt.tips = "下载更新包：" + (fileLength / 1048576f).ToString("0.00") + "/" + size;
                dt.progress = progress;
                dt.speed = "速度：" + (int)(fileLength / 1024f / (DateTime.Now - startTime).TotalSeconds) + "KB/s";

                MessageSystem.Instance().PostMessage(MessageId.DOWNLOAD_CONFIG_PROGRESS, dt, 0);
                //类似尾递归
                length = stream.Read(buffer, 0, buffer.Length);
            }
            stream.Close();
            stream.Dispose();
        }
        else
        {
            progress = 1;
        }
        fs.Close();
        fs.Dispose();
        //如果下载完毕，执行回调
        if (progress == 1)
        {
            OnDecompressConfig(versionValue);
        }
    }

    long GetLength(string url)
    {
        HttpWebRequest requet = HttpWebRequest.Create(url) as HttpWebRequest;
        requet.Method = "HEAD";
        HttpWebResponse response = requet.GetResponse() as HttpWebResponse;
        return response.ContentLength;
    }

    string zipFilePath = "";
    void OnDecompressConfig(string versionValue)
    {
        string dir = PathUtils.MakeFilePath("", PathUtils.PathType.MobileDiskWrite);
        string filePath = dir + versionValue + ".zip";
        zipFilePath = filePath;
        ZipCodeProgress progress = new ZipCodeProgress(DecompressProgress);
        VersionBuilderUtil.UnPack(filePath, dir, progress, null);
    }

    void DecompressProgress(Int64 fileSize, Int64 processSize)
    {
        float v = (float)processSize / fileSize;
        if (v >= 1)
        {
            //File.Delete(zipFilePath);
        }
        MessageSystem.Instance().PostMessage(MessageId.DECOMPRESS_CONFIG_PGOGRESS, v, 0f);
    }
}

