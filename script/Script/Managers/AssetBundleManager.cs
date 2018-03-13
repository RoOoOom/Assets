using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
In this demo, we demonstrate:
1.	Automatic asset bundle dependency resolving & loading.
	It shows how to use the manifest assetbundle like how to get the dependencies etc.
2.	Automatic unloading of asset bundles (When an asset bundle or a dependency thereof is no longer needed, the asset bundle is unloaded)
3.	Editor simulation. A bool defines if we load asset bundles from the project or are actually using asset bundles(doesn't work with assetbundle variants for now.)
	With this, you can player in editor mode without actually building the assetBundles.
4.	Optional setup where to download all asset bundles
5.	Build pipeline build postprocessor, integration so that building a player builds the asset bundles and puts them into the player data (Default implmenetation for loading assetbundles from disk on any platform)
6.	Use WWW.LoadFromCacheOrDownload and feed 128 bit hash to it when downloading via web
	You can get the hash from the manifest assetbundle.
7.	AssetBundle variants. A prioritized list of variants that should be used if the asset bundle with that variant exists, first variant in the list is the most preferred etc.
*/

// Loaded assetBundle contains the references count which can be used to unload dependent assetBundles automatically.
public class LoadedAssetBundle
{
	public AssetBundle m_AssetBundle;
	public int m_ReferencedCount;
    public bool isOk = false;
    internal event Action unload;

    internal void OnUnload(bool unloadTrue = false)
    {
        m_AssetBundle.Unload(unloadTrue);
        if (unload != null)
            unload();
    }

    public LoadedAssetBundle(AssetBundle assetBundle)
    {
        m_AssetBundle = assetBundle;
        m_ReferencedCount = 1;
    }
}
	
// Class takes care of loading assetBundle and its dependencies automatically, loading variants automatically.
public class AssetBundleManager : MonoBehaviour
{
    static AssetBundleManager m_AssetBundleManager = null;

    static public AssetBundleManager instance
    {
        get
        {
            if (m_AssetBundleManager == null)
            {
                if (GameWorld.instance == null) return null;

                m_AssetBundleManager = GameWorld.instance.gameObject.AddComponent<AssetBundleManager>();
            }

            return m_AssetBundleManager;
        }
    }

    string m_BundleFileName = ".ab";

	Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle> ();
	Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string> ();
    static List<string> m_DownloadingBundles = new List<string>();
	List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation> ();

    /// <summary>
    /// Returns true if certain asset bundle has been downloaded without checking
    /// whether the dependencies have been loaded.
    /// </summary>
    public bool IsAssetBundleDownloaded(string assetBundleName)
    {
        return m_LoadedAssetBundles.ContainsKey(assetBundleName);
    }

    string[] GetDependencies(string assetBundleName)
    {
        VersionBundle bundle = VersionHelper.instance.GetBundle(assetBundleName);
        if (bundle == null || bundle.dependency == null || bundle.dependency.Length < 1)
        {
            return null;
        }

        return bundle.dependency;
    }
		
	// Get loaded AssetBundle, only return vaild object when all the dependencies are downloaded successfully.
	public LoadedAssetBundle GetLoadedAssetBundle (string assetBundleName, out string error)
	{
		if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
			return null;
		
		LoadedAssetBundle bundle = null;
		m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
		if (bundle == null)
			return null;

        if (bundle.isOk)
            return bundle;
			
		// No dependencies are recorded, only the bundle itself is required.
		string[] dependencies = GetDependencies(assetBundleName);
        if (dependencies == null) return bundle;

        LoadedAssetBundle dependentBundle;
        for (int i = 0; i < dependencies.Length; i++)
        {
            if (m_DownloadingErrors.TryGetValue(dependencies[i], out error))
                return null;

            if (!m_LoadedAssetBundles.TryGetValue(dependencies[i], out dependentBundle))
            {
                return null;
            }
        }

        bundle.isOk = true;
		return bundle;
	}

    // Where we get all the dependencies and load them all.
    protected void LoadDependencies(string assetBundleName)
    {
        // Get dependecies from the AssetBundleManifest object..
        string[] dependencies = GetDependencies(assetBundleName);
        if (dependencies == null) return;

        for (int i = 0; i < dependencies.Length; i++)
        {
            LoadAssetBundleInternal(dependencies[i]);
        }
    }

	// Load AssetBundle and its dependencies.
	protected void LoadAssetBundle(string assetBundleName, int level = 0)
	{
      //  Debug.Log("1111=>  " + assetBundleName);
		// Check if the assetBundle has already been processed.
		bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, level);
	
		// Load dependencies.
		if (!isAlreadyProcessed)
			LoadDependencies(assetBundleName);
	}
		
	// Where we actuall call WWW to download the assetBundle.
	protected bool LoadAssetBundleInternal (string assetBundleName, int level = 0)
	{
		// Already loaded.
		LoadedAssetBundle bundle = null;
		m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
		if (bundle != null)
		{
			bundle.m_ReferencedCount++;
			return true;
		}
	
		// @TODO: Do we need to consider the referenced count of WWWs?
		// In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
		// But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
        if (m_DownloadingBundles.Contains(assetBundleName))
            return true;

        VersionBundle vb = null;
        VersionHelper.instance.GetBundleHttp(assetBundleName, out vb);
        if (vb != null)
        {
            string id = vb.versionValue + "_" + vb.id;
            if (ResHelper.Instance().downloadedFiles.ContainsKey(id))
            {
                string url = PathUtils.MakeFilePath(id, PathUtils.PathType.MobileDiskWrite);
                AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(url);
                m_InProgressOperations.Add(new AssetBundleDownloadFromFile(assetBundleName, abcr, url));
            }
            else
            {
                string url = GameConfig.HOST_RES() + vb.id + "?v=" + vb.versionValue;
                m_InProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleName, url, vb.versionValue, vb.crc, level));
            }
        }
        else
        {
            PathUtils.PathType eType = PathUtils.PathType.None;
            string url = PathUtils.GetReadablePath(assetBundleName, ref eType, false);
            AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(url);
            m_InProgressOperations.Add(new AssetBundleDownloadFromFile(assetBundleName, abcr, url));
        }
        m_DownloadingBundles.Add(assetBundleName);

		return false;
	}
	
	// Unload assetbundle and its dependencies.
	public void UnloadAssetBundle(string assetBundleName, bool unload)
	{
        if (!assetBundleName.EndsWith(m_BundleFileName))
        {
            assetBundleName += m_BundleFileName;
        }

		UnloadAssetBundleInternal(assetBundleName, unload);
		UnloadDependencies(assetBundleName, unload);
	}

    protected void UnloadDependencies(string assetBundleName, bool unload)
	{
		string[] dependencies = GetDependencies(assetBundleName);
		if (dependencies == null) return;

        for (int i = 0; i < dependencies.Length; i++)
        {
            UnloadAssetBundleInternal(dependencies[i], unload);
        }
	}

    protected void UnloadAssetBundleInternal(string assetBundleName, bool unload)
	{
		string error;
		LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
		if (bundle == null) return;
	
		if (--bundle.m_ReferencedCount == 0)
		{
            bundle.OnUnload(unload);
			m_LoadedAssetBundles.Remove(assetBundleName);
#if UNITY_EDITOR
            //Debug.Log(assetBundleName + " has been unloaded successfully ");
#endif
		}
	}

    public void DelayUnload(string assetBundleName, bool unload)
    {

    }

	void LateUpdate()
	{
        for (int i = 0; i < m_InProgressOperations.Count; )
        {
            var operation = m_InProgressOperations[i];
            if (operation.Update())
            {
                i++;
            }
            else
            {
                m_InProgressOperations.RemoveAt(i);
                ProcessFinishedOperation(operation);
            }
        }
	}

    static string[] editorShaders = { 
                                        "shader_greymaterialalpha.ab", 
                                        "shader_greymaterial.ab", 
                                        "shader_hudtextmat.ab", 
                                        "shader_hudfrontmat.ab", 
                                        "shader_hudbackmat.ab"};
    static string[] editorMats = { 
                                     "GreyMaterialAlpha", 
                                     "GreyMaterial", 
                                     "HudTextMat", 
                                     "HudFrontMat", 
                                     "HudBackMat"};
    static int[] editorRenderQueues = {
                                    3000, 
                                    3000, 
                                    3000, 
                                    3103, 
                                    3101};

    void ProcessFinishedOperation(AssetBundleLoadOperation operation)
    {
        AssetBundleDownloadOperation download = operation as AssetBundleDownloadOperation;
        if (download == null)
            return;

        if (string.IsNullOrEmpty(download.error))
        {
#if UNITY_EDITOR
            for (int i = 0; i < editorShaders.Length; i++ )
            {
                if (editorShaders[i].Equals(download.assetBundleName))
                {
                    Material mat = download.assetBundle.m_AssetBundle.LoadAsset<Material>(editorMats[i]);
                    mat.shader = Shader.Find(mat.shader.name);
                    mat.renderQueue = editorRenderQueues[i];
                }
            }
#endif
            m_LoadedAssetBundles.Add(download.assetBundleName, download.assetBundle);
        }
        else
        {
            string msg = string.Format("Failed downloading bundle {0} from {1}: {2}",
                    download.assetBundleName, download.GetSourceURL(), download.error);
            m_DownloadingErrors.Add(download.assetBundleName, msg);
        }

        m_DownloadingBundles.Remove(download.assetBundleName);
    }

    public void GetBundleAsync(string assetBundleName, System.Action<string, AssetBundle> callback)
    {
        if (!assetBundleName.EndsWith(m_BundleFileName))
        {
            assetBundleName += m_BundleFileName;
        }

        StartCoroutine(LoadBundleAsync(assetBundleName, callback));
    }

    IEnumerator LoadBundleAsync(string assetBundleName, System.Action<string, AssetBundle> callback)
    {
        LoadAssetBundle(assetBundleName);
        AssetBundleDownloaded request = new AssetBundleDownloaded(assetBundleName);
        m_InProgressOperations.Add(request);
        yield return StartCoroutine(request);
        if (callback == null || callback.Target == null || "null".Equals(callback.Target.ToString()))
            yield break;
        AssetBundle bundle = request.GetAssetBundle();
        callback.Invoke(assetBundleName, bundle);
        callback = null;
    }
	
    /// <summary>
    /// 获取下载等级
    /// </summary>
    /// <returns>0：最高等级， 无论什么情况，必须下载； 1：类似地图碎片这种小图； 2：类似模型这种大图</returns>
    int GetLevel(string assetBundleName, string assetName)
    {
        if (assetName.EndsWith("_animation"))
        {
            return 2;
        }

        return 1;
    }

	// Load asset from the given assetBundle.
    public void GetResourceAsync<T>(string assetBundleName, string assetName, System.Action<T, bool> callback) where T : UnityEngine.Object
	{
		//Log(LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");
        if (!assetBundleName.EndsWith(m_BundleFileName))
        {
            assetBundleName += m_BundleFileName;
        }

        StartCoroutine(LoadAssetAsync(assetBundleName, assetName, callback));
	}

    IEnumerator LoadAssetAsync<T>(string assetBundleName, string assetName, System.Action<T, bool> callback) where T : UnityEngine.Object
    {
        LoadAssetBundle(assetBundleName, GetLevel(assetBundleName, assetName));
        AssetBundleLoadAssetOperation request = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, typeof(T));
        m_InProgressOperations.Add(request);
        yield return StartCoroutine(request);
        T obj = (T)request.GetAsset<UnityEngine.Object>();
        InitHideFlag(obj);
        if (callback == null || callback.Target == null || "null".Equals(callback.Target.ToString()))
        yield break;
        callback.Invoke(obj, IsValid(obj));
        callback = null;
    }

    bool IsValid(UnityEngine.Object obj)
    {
        return obj != null;
    }

    void InitHideFlag(UnityEngine.Object obj)
    {
        if (IsValid(obj))
        {
            obj.hideFlags = HideFlags.None;
        }
    }

    public Dictionary<string, List<string>> GetAllLoadedResInfo()
    {
        Dictionary<string, List<string>> list = new Dictionary<string, List<string>>();
        foreach (string key in m_LoadedAssetBundles.Keys)
        {
            List<string> res = new List<string>();
            LoadedAssetBundle data = m_LoadedAssetBundles[key];
            res.Add(data.m_ReferencedCount.ToString());
            list.Add(key, res);
        }
        return list;
    }

    public Dictionary<string, Dictionary<string, byte[]>> luaBundleBytes = new Dictionary<string, Dictionary<string, byte[]>>();
    public void AddLuaAssetBundle(Dictionary<string, byte[]> dic, string name, byte[] bytes)
    {
        if (dic.ContainsKey(name))
        {
            dic[name] = bytes;
        }
        else
        {
            dic.Add(name, bytes);
        }
    }
}