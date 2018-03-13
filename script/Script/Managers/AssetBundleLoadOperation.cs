using BestHTTP;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public abstract class AssetBundleLoadOperation : IEnumerator
{
	public object Current
	{
		get
		{
			return null;
		}
	}
	public bool MoveNext()
	{
		return !IsDone();
	}
		
	public void Reset()
	{
	}
		
	abstract public bool Update ();
		
	abstract public bool IsDone ();
}

public abstract class AssetBundleDownloadOperation : AssetBundleLoadOperation
{
    public static int[] maxLevels = { -1, 10, 1};//{-1, 10, 4};
    public static int[] levels = new int[3];

    bool done;

    public string assetBundleName { get; private set; }
    public LoadedAssetBundle assetBundle { get; protected set; }
    public string error { get; protected set; }

    protected abstract bool downloadIsDone { get; }
    protected abstract void FinishDownload();

    public override bool Update()
    {
        if (!done && downloadIsDone)
        {
            FinishDownload();
            done = true;
        }

        return !done;
    }

    public override bool IsDone()
    {
        return done;
    }

    public abstract string GetSourceURL();

    public AssetBundleDownloadOperation(string assetBundleName)
    {
        this.assetBundleName = assetBundleName;
    }
}

public class AssetBundleDownloadFromHttp : AssetBundleDownloadOperation
{
    bool m_isDownloaded;
    byte[] m_data;
    AssetBundleCreateRequest m_abcr;
    string m_url;

    long GetLength(HTTPResponse resp)
    {
        List<string> contentLengthHeaders = resp.GetHeaderValues("content-length");
        var contentRangeHeaders = resp.GetHeaderValues("content-range");
        if (contentLengthHeaders != null && contentRangeHeaders == null)
            return long.Parse(contentLengthHeaders[0]);

        return 0;
    }

    public AssetBundleDownloadFromHttp(string assetBundleName, string url, int version, int crc, int level)
        : base(assetBundleName)
    {
        m_url = url;
        string path = PathUtils.MakeFilePath(assetBundleName, PathUtils.PathType.MobileDiskWrite);
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
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

            long remoteLength = GetLength(resp);
            if (remoteLength <= 0)
            {
                stream.Dispose();
                return;
            }

            long localLength = stream.Length;

            if (localLength == remoteLength)
            {
                Loom.RunAsync(() => {
                    m_data = new byte[localLength];
                    stream.Read(m_data, 0, (int)localLength);
                    stream.Dispose();
                    Loom.QueueOnMainThread(() =>
                    {
                        if (m_data != null && m_data.Length > 0)
                        {
                            m_abcr = AssetBundle.LoadFromMemoryAsync(m_data);
                        }
                        m_isDownloaded = true;
                    });
                });
                return;
            }


            var downloadRequest = new HTTPRequest(req.Uri, HTTPMethods.Get, true, (downloadReq, downloadResp) => {
                Loom.RunAsync(() =>
                {
                    stream.Write(downloadResp.Data, 0, downloadResp.Data.Length);
                    stream.Flush();
                    long length = stream.Length;
                    m_data = new byte[length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(m_data, 0, m_data.Length);
                    stream.Dispose();
                    Loom.QueueOnMainThread(() =>
                    {
                        if (m_data != null && m_data.Length > 0)
                        {
                            m_abcr = AssetBundle.LoadFromMemoryAsync(m_data);
                        }
                        m_isDownloaded = true;
                    });
                });
            });

            //Debug.Log(req.Uri + "\n" + "length: " + localLength);
            downloadRequest.SetRangeHeader((int)localLength);
            downloadRequest.DisableCache = true;
            downloadRequest.Send();
            //m_data = resp.Data;
            //m_isDownloaded = true;
            //if (m_data != null && m_data.Length > 0)
            //{
            //    m_abcr = AssetBundle.LoadFromMemoryAsync(m_data);
            //}
        });
        http.DisableCache = true;
        http.Send();
    }

    protected override bool downloadIsDone
    {
        get 
        {
            if (m_abcr != null)
                return m_abcr.isDone;

            return m_isDownloaded;
        }
    }

    protected override void FinishDownload()
    {
        if (m_abcr == null || m_abcr.assetBundle == null)
        {
            error = "url is error";
            return;
        }

        assetBundle = new LoadedAssetBundle(m_abcr.assetBundle);
        m_data = null;
        m_abcr = null;
    }

    public override string GetSourceURL()
    {
        return m_url;
    }
}

public class AssetBundleDownloadFromWebOperation : AssetBundleDownloadOperation
{
    WWW m_www;
    string m_url;
    int m_version;
    uint m_crc;
    int m_level;

    public AssetBundleDownloadFromWebOperation(string assetBundleName, string url, int v, uint crc, int level)
        : base(assetBundleName)
    {
        m_url = url;
        m_crc = crc;
        m_version = v;
        m_level = level;
        if (m_level < 1)
        {
            m_www = WWW.LoadFromCacheOrDownload(m_url, m_version, m_crc);
        }
    }

    protected override bool downloadIsDone
    {
        get 
        { 
            bool ok = m_www != null && m_www.isDone;
            if (ok)
            {
                levels[m_level]--;
            }
            return ok;
        }
    }

    public override bool Update()
    {
        if (m_www == null)
        {
            if (levels[m_level] < maxLevels[m_level])
            {
                if (NetJZWL.NetClient.SendCount <= GameConfig.MustSendCount)
                {
                    levels[m_level]++;
                    m_www = WWW.LoadFromCacheOrDownload(m_url, m_version, m_crc);
                }
                //else
                //{
                //    Debug.LogError("pkg send " + NetJZWL.NetClient.SendCount);
                //}
            }
        }
        return base.Update();
    }

    protected override void FinishDownload()
    {
        error = m_www.error;
        if (!string.IsNullOrEmpty(error))
            return;

        AssetBundle bundle = m_www.assetBundle;
        if (bundle == null)
            error = string.Format("{0} is not a valid www asset bundle.", assetBundleName);
        else
            assetBundle = new LoadedAssetBundle(m_www.assetBundle);

        m_www.Dispose();
        m_www = null;
    }

    public override string GetSourceURL()
    {
        return m_url;
    }
}

public class AssetBundleDownloadFromFile : AssetBundleDownloadOperation
{
    private AssetBundleCreateRequest m_request;
    private string m_url;

    public AssetBundleDownloadFromFile(string assetBundleName, AssetBundleCreateRequest request, string url)
        : base(assetBundleName)
    {
        m_request = request;
        m_url = url;
    }

    protected override bool downloadIsDone { get { return (m_request == null) || m_request.isDone; } }

    protected override void FinishDownload()
    {
        AssetBundle bundle = m_request.assetBundle;
        if (bundle == null)
            error = string.Format("{0} is not a valid local asset bundle.", assetBundleName);
        else
            assetBundle = new LoadedAssetBundle(m_request.assetBundle);
    }
    public override string GetSourceURL()
    {
        return m_url;
    }
}

public class AssetBundleDownloadFromFileExt : AssetBundleDownloadOperation
{
    private AssetBundleCreateRequest m_request;
    byte[] m_data;
    private string m_url;
    private bool m_loaded = false;

    public AssetBundleDownloadFromFileExt(string assetBundleName, string url)
        : base(assetBundleName)
    {
        m_url = url;
        Loom.RunAsync(() => {
            using (Stream stream = new FileStream(m_url, FileMode.Open, FileAccess.Read))
            {
                if (stream != null)
                {
                    m_data = new byte[stream.Length];
                    stream.Read(m_data, 0, m_data.Length);
                    Loom.QueueOnMainThread(() => {
                        if (m_data != null && m_data.Length > 0)
                        {
                            m_request = AssetBundle.LoadFromMemoryAsync(m_data);
                        }
                        m_loaded = true;
                    });
                }
                else
                {
                    m_loaded = true;
                }
            }
        });
    }

    protected override bool downloadIsDone 
    { 
        get
        {
            if (m_request != null)
                return m_request.isDone;

            return m_loaded;
        }
    }

    protected override void FinishDownload()
    {
        if (m_request != null && m_request.assetBundle != null)
        {
            assetBundle = new LoadedAssetBundle(m_request.assetBundle);
        }
        else
        {
            error = string.Format("{0} is not a valid local asset bundle.", assetBundleName);
        }
        AssetBundle bundle = m_request.assetBundle;
    }
    public override string GetSourceURL()
    {
        return m_url;
    }
}

public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
{
	protected string 				m_AssetBundleName;
	protected string 				m_LevelName;
	protected bool 					m_IsAdditive;
	protected string 				m_DownloadingError;
	protected AsyncOperation		m_Request;
	
	public AssetBundleLoadLevelOperation (string assetbundleName, string levelName, bool isAdditive)
	{
		m_AssetBundleName = assetbundleName;
		m_LevelName = levelName;
		m_IsAdditive = isAdditive;
	}
	
	public override bool Update ()
	{
		if (m_Request != null)
			return false;
			
		LoadedAssetBundle bundle = AssetBundleManager.instance.GetLoadedAssetBundle (m_AssetBundleName, out m_DownloadingError);
		if (bundle != null)
		{
			if (m_IsAdditive)
				m_Request = Application.LoadLevelAdditiveAsync (m_LevelName);
			else
				m_Request = Application.LoadLevelAsync (m_LevelName);
			return false;
		}
		else
			return true;
	}
		
	public override bool IsDone ()
	{
		// Return if meeting downloading error.
		// m_DownloadingError might come from the dependency downloading.
		if (m_Request == null && m_DownloadingError != null)
		{
			Debug.LogError(m_DownloadingError);
			return true;
		}
			
		return m_Request != null && m_Request.isDone;
	}
}
	
public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
{
	public abstract T GetAsset<T>() where T : UnityEngine.Object;
}

public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
{
	protected string 				m_AssetBundleName;
	protected string 				m_AssetName;
	protected string 				m_DownloadingError;
	protected System.Type 			m_Type;
	protected AssetBundleRequest	m_Request = null;
	
	public AssetBundleLoadAssetOperationFull (string bundleName, string assetName, System.Type type)
	{
		m_AssetBundleName = bundleName;
		m_AssetName = assetName;
		m_Type = type;
	}
		
	public override T GetAsset<T>()
	{
		if (m_Request != null && m_Request.isDone)
			return m_Request.asset as T;
		else
			return null;
	}
		
	// Returns true if more Update calls are required.
	public override bool Update ()
	{
		if (m_Request != null)
			return false;
	
		LoadedAssetBundle bundle = AssetBundleManager.instance.GetLoadedAssetBundle (m_AssetBundleName, out m_DownloadingError);
		if (bundle != null)
		{
            //Debug.Log(m_AssetBundleName);
			///@TODO: When asset bundle download fails this throws an exception...
			m_Request = bundle.m_AssetBundle.LoadAssetAsync (m_AssetName, m_Type);
			return false;
		}
		else
		{
			return true;
		}
	}
		
	public override bool IsDone ()
	{
		// Return if meeting downloading error.
		// m_DownloadingError might come from the dependency downloading.
		if (m_Request == null && m_DownloadingError != null)
		{
			Debug.LogError(m_DownloadingError);
			return true;
		}
	
		return m_Request != null && m_Request.isDone;
	}
}

public class AssetBundleDownloaded : AssetBundleLoadOperation
{
    private string m_AssetBundleName;
    private string m_DownloadingError;
    private LoadedAssetBundle m_bundle;

    public AssetBundleDownloaded(string assetBundleName)
    {
        m_AssetBundleName = assetBundleName;
    }

    public override bool Update()
    {
        m_bundle = AssetBundleManager.instance.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
        if (m_bundle != null)
        {
            return false;
        }

        if (m_DownloadingError != null)
        {
            return false;
        }

        return true;
    }

    public override bool IsDone()
    {
        return m_bundle != null || m_DownloadingError != null;
    }

    public AssetBundle GetAssetBundle()
    {
        if (m_bundle == null)
            return null;

        return m_bundle.m_AssetBundle;
    }
}
