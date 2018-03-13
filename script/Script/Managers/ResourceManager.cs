// ***************************************************************
//  FileName: ResourceMgr.cs
//  Version : 1.0
//  Date    : 2016/06/27
//  Author  : cjzhanying 
//  Copyright (C) 2012 - Digital Technology Co.,Ltd. All rights reserved. 版权申明
//  --------------------------------------------------------------
//  Description: 资源加载管理 
//               使用队列方式加载资源(一个一个加载 不会同时加载)
//               修改GetBundle函数的加载方式可以满足是网络加载还是本地加载的需求 自行修改(添加宏定义)
//               PrivateAutoDestroyAssetBundle函数可以指定一部分资源加载后实例化 然后就删除
//  -------------------------------------------------------------
//  History:
//  -------------------------------------------------------------
// ***************************************************************
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class ResourceManager : SimleManagerTemplate<ResourceManager>
{
    private class ResourceMgrData
    {
        public enum State
        {
            None,
            Loading,
            LoadFinish,
        }
        public State eState = State.None;
        public AssetBundle assetBundle = null;
        public Dictionary<string, UnityEngine.Object> dicResource = new Dictionary<string, UnityEngine.Object>();
        public int referenceCount = 0;
    }

    private class RemoveAB
    {
        public float time;
        public List<string> removeABs;
    }

    private static Dictionary<string, ResourceMgrData> gDicPkgResource = new Dictionary<string, ResourceMgrData>();
    private static string gSysResourcePkg = "Resources"; /** @brief: 系统资源包 */
    private static bool bIsRunUnloadUnusedAssets = false; /** @brief: 是否正在允许卸载无用资源 */
    private static bool bIsDestroyAllAssetBundle = false; /** @brief: 是否正在销毁所有资源 */
    private static string gBundleFileName = ".ab"; /** @brief: 文件后缀名 */
    private static List<string> _dontUnloadABs = new List<string>();
    private static List<RemoveAB> m_toBeRmovedABs = new List<RemoveAB>();

    /* ============================================================================================================================*/
    /** @brief: -------------------------------------------加载资源或者资源包----------------------------------------------------- */
    /** @brief: -------------------------------------------加载资源或者资源包----------------------------------------------------- */
    /** @brief: -------------------------------------------加载资源或者资源包----------------------------------------------------- */
    /** @brief: -------------------------------------------加载资源或者资源包----------------------------------------------------- */
    /** @brief: -------------------------------------------加载资源或者资源包----------------------------------------------------- */
    /* ============================================================================================================================*/

    /************************************
     * 函数说明: 异步加载资源,资源加载完成后调用回调函数
     * 返 回 值: T泛型
     * 参数说明: szPkgName AssetBundle包名称,如果为空表示为Resource目录下的默认资源
     * 参数说明: resName 要加载的szPKgName中的资源的名称,不能为空
     * 参数说明: replaceRes 如果没有找到resName 使用Resource目录的replaceRes资源替换
     * 参数说明: System.Action<T, bool> callack 回调函数
     * 参数说明: req 输出进度回调
     * 注意事项: 异步加载
    ************************************/
    public T AsyncGetResource<T>(string szPkgName, string resName, string replaceRes, System.Action<T, bool> callack) where T : UnityEngine.Object
    {
        ////JZLog.LogWarning("will load " + szPkgName + " -> " + resName + " -> " + replaceRes);
        if (szPkgName.EndsWith(gBundleFileName) == false)
        {
            szPkgName = szPkgName + gBundleFileName;
        }

        T resRet = default(T); /** @brief: 指定要找到资源 */
        do
        {
            if (string.IsNullOrEmpty(resName))
            {
                if (callack != null)
                {
                    InitHideFlag(resRet as UnityEngine.Object);
                    callack.Invoke(resRet, false);
                    callack = null;
                }
                break;
            }

            /** @brief: 已经加载过的资源直接返回 */
            ResourceMgrData rmd;
            gDicPkgResource.TryGetValue(szPkgName, out rmd);
            if (null != rmd)
            {
                UnityEngine.Object obj;
                rmd.dicResource.TryGetValue(resName, out obj);
                if (null != obj)
                {
                    resRet = (T)obj;
                    if (null != callack)
                    {
                        InitHideFlag(resRet as UnityEngine.Object);
                        callack.Invoke(resRet, !IsInvalid(resRet));
                        callack = null;
                    }
                    break;
                }
            }

            /** @brief: 等待包加载成功 */
            LoadBundleAsync(szPkgName, (name, assetBundle) =>
            {
                rmd = gDicPkgResource[szPkgName];
                /** @brief: 包已经加载过 */
                if (rmd.dicResource.ContainsKey(resName))
                {
                    resRet = (T)(object)rmd.dicResource[resName];
                    if (callack != null)
                    {
                        InitHideFlag(resRet as UnityEngine.Object);
                        callack.Invoke(resRet, !IsInvalid(resRet));
                        callack = null;
                    }
                }
                else
                {
                    if (rmd.assetBundle != null)
                    {
                        if (!VersionHelper.instance.HasBundleHttp(szPkgName))
                        {
                            resRet = (T)(object)rmd.assetBundle.LoadAsset(resName, typeof(T));
                            rmd.dicResource[resName] = (UnityEngine.Object)(object)resRet;
                            if (callack != null)
                            {
                                InitHideFlag(resRet as UnityEngine.Object);
                                callack.Invoke(resRet, !IsInvalid(resRet));
                                callack = null;
                            }
                        }
                        else if (GameWorld.instance != null)
                        {
                            GameWorld.instance.StartCoroutine(LoadBundleAssetAsync<T>(rmd.assetBundle, resName, (asset, ret) => 
                            {
                                UnityEngine.Object obj = (UnityEngine.Object)(object)asset;
                                rmd.dicResource[resName] = obj;
                                if (null != callack)
                                {
                                    InitHideFlag(obj);
                                    callack((T)obj, !IsInvalid(obj));
                                    callack = null;
                                }
                            }));
                        }
                    }
                }
            });
        } while (false);
        return resRet;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bundle"></param>
    /// <param name="name"></param>
    /// <param name="callack"></param>
    /// <returns></returns>
    IEnumerator LoadBundleAssetAsync<T>(AssetBundle bundle, string name, System.Action<T, bool> callack) where T : UnityEngine.Object
    {
        if (null == bundle)
        {
            if (null != callack)
            {
                callack.Invoke(null, false);
                callack = null;
            }
            yield break;
        }

        AssetBundleRequest abr = bundle.LoadAssetAsync(name, typeof(T));
        yield return abr;
        if (null != callack)
        {
            callack.Invoke((T)abr.asset, true);
            callack = null;
        }
    }

    /************************************
     * 函数说明: 异步加载资源bundle
     * 返 回 值: WonderGame.ResourceRequest
     * 参数说明: szPkgName
     * 参数说明: callback
     * 注意事项: 仅仅是加入下载队列,真正的下载是在Update中执行
    ************************************/
    public void LoadBundleAsync(string szPkgName, System.Action<string, AssetBundle> callback)
    {
        /** @brief: 加载资源 */
        if (GameWorld.instance != null)
        {
            GameWorld.instance.StartCoroutine(LoadResources(szPkgName, callback));
        }
    }

    public void AddReference(string szPkgName)
    {
        if (!szPkgName.EndsWith(gBundleFileName))
        {
            szPkgName += gBundleFileName;
        }
        ResourceMgrData resoureData;
        if (gDicPkgResource.TryGetValue(szPkgName, out resoureData))
        {
            resoureData.referenceCount++;
            VersionBundle bundle = VersionHelper.instance.GetBundle(szPkgName);
            if (bundle != null && bundle.dependency != null)
            {
                for( int i = 0; i < bundle.dependency.Length; i++ )
                {
                    AddReference(bundle.dependency[i]);
                }
            }
        }
    }

    public void SubReference(string szPkgName, ref List<string> removeABs)
    {
        if (!szPkgName.EndsWith(gBundleFileName))
        {
            szPkgName += gBundleFileName;
        }
        ResourceMgrData resoureData;
        if (gDicPkgResource.TryGetValue(szPkgName, out resoureData))
        {
            if (--resoureData.referenceCount <= 0)
            {
                removeABs.Add(szPkgName);
            }

            VersionBundle bundle = VersionHelper.instance.GetBundle(szPkgName);
            if (bundle != null && bundle.dependency != null)
            {
                for (int i = 0; i < bundle.dependency.Length; i++)
                {
                    SubReference(bundle.dependency[i], ref removeABs);
                }
            }
        }
    }

    public void DelayUnload(string szPkgName)
    {
        List<string> removeABs = new List<string>();
        SubReference(szPkgName, ref removeABs);
        m_toBeRmovedABs.Add(new RemoveAB() 
        {
            time = 2, // 延时两秒清除，防止突然加载ab
            removeABs = removeABs
        });
    }

    /* ============================================================================================================================*/
    /** @brief: -------------------------------------------销毁指定包中所有资源--------------------------------------------------- */
    /** @brief: -------------------------------------------销毁指定包中所有资源--------------------------------------------------- */
    /** @brief: -------------------------------------------销毁指定包中所有资源--------------------------------------------------- */
    /** @brief: -------------------------------------------销毁指定包中所有资源--------------------------------------------------- */
    /** @brief: -------------------------------------------销毁指定包中所有资源--------------------------------------------------- */
    /* ============================================================================================================================*/

    #region 卸载指定的AssetBundle包
    /************************************
     * 函数说明: 卸载指定的AssetBundle包
     * 返 回 值: void
     * 参数说明: szPkgName 指定的AssetBundle名字 当为空的时候表示卸载Resource         
     * 参数说明: unloadAll == true 卸载全部克隆出来的Obj以及自身的文件内存镜像. unloadAll == false 仅仅卸载自身的内存文件镜像(克隆出来的Obj还存在)
     * 注意事项: unloadAll == true 小心使用 , unloadAll 对于Resources包无效
    ************************************/
    public void DestroyAssetBundle(string szPkgName, bool unloadAll)
    {
        if (!szPkgName.EndsWith(gBundleFileName))
        {
            szPkgName = szPkgName + gBundleFileName;
        }

        if (_dontUnloadABs.Contains(szPkgName))
        {
            return;
        }

        /** @brief: 卸载其他资源 */
        if (gDicPkgResource.ContainsKey(szPkgName) == false)
        {
            return;
        }

        ResourceMgrData resourceData = gDicPkgResource[szPkgName];

        /** @brief: 正在加载中 */
        if (resourceData.eState == ResourceMgrData.State.Loading)
        {
            //JZLog.LogError("DestroyAssetBundle : Pkg == " + szPkgName + " is Loading , Destroy bundle may be error !");
            return;
        }

        resourceData.eState = ResourceMgrData.State.None;

        Dictionary<string, UnityEngine.Object>.Enumerator etor = resourceData.dicResource.GetEnumerator();
        while(etor.MoveNext())
        {
            ResourceUnLoad(etor.Current.Value);
        }
        resourceData.dicResource.Clear();

        if (IsInvalid(resourceData.assetBundle) == false)
        {
            resourceData.assetBundle.Unload(unloadAll);
            resourceData.assetBundle = null;
        }
    }
    #endregion

    #region 卸载所有包和里面的所有资源(除去忽略类表)
    /************************************
     * 函数说明: 卸载所有包和里面的所有资源
     * 返 回 值: void
     * 参数说明: mListIgnoreBundle 忽略卸载的包名
     * 注意事项: 忽略列表可以是具体的某一个资源eg Player/Modle_Man 也可以是多个省略的固定包的资源Player/Modle_  名字开头是"@"的表示系统资源 
    ************************************/
    public void DestroyAllAssetBundle(List<string> mListIgnoreBundle)
    {
        /** @brief: 卸载全部资源 */
        bIsDestroyAllAssetBundle = true;

        #region 删除StreanAssets目录的资源
        {
            bool bIgnore = false;
            foreach (KeyValuePair<string, ResourceMgrData> mainKey in gDicPkgResource)
            {
                #region 查询是否忽略卸载
                bIgnore = false;
                if (mListIgnoreBundle != null && mListIgnoreBundle.Count > 0)
                {
                    for (int i = 0; i < mListIgnoreBundle.Count; i++)
                    {
                        if (mainKey.Key.StartsWith(mListIgnoreBundle[i]))
                        {
                            bIgnore = true;
                            ////JZLog.LogWarning("DestroyAllAssetBundle : Ignore Bundle [" + mainKey.Key + "]");
                            break;
                        }
                    }
                }

                if (bIgnore == true)
                {
                    continue;
                }
                #endregion

                Dictionary<string, UnityEngine.Object>.Enumerator etor = mainKey.Value.dicResource.GetEnumerator();
                while (etor.MoveNext())
                {
                    ResourceUnLoad(etor.Current.Value);
                }

                mainKey.Value.dicResource.Clear();

                #region 卸载资源
                if (IsInvalid(mainKey.Value.assetBundle) == false)
                {
                    mainKey.Value.assetBundle.Unload(true);
                    mainKey.Value.assetBundle = null;
                }

                mainKey.Value.eState = ResourceMgrData.State.None;
                #endregion
            }
        }
        #endregion

        GC.Collect();

        bIsDestroyAllAssetBundle = false;
    }
    public void DestroyAllAssetBundle()
    {
        DestroyAllAssetBundle(_dontUnloadABs);
    }
    #endregion

    /* ============================================================================================================================*/
    /** @brief: -------------------------------------------其他函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------其他函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------其他函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------其他函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------其他函数--------------------------------------------------------------- */
    /* ============================================================================================================================*/

    #region 是否正在后台卸载资源
    /************************************
     * 函数说明: 是否正在后台卸载资源
     * 返 回 值: bool
     * 注意事项: 
    ************************************/
    public bool IsUnloadUnusedAssets()
    {
        return bIsRunUnloadUnusedAssets;
    }
    #endregion

    /* ============================================================================================================================*/
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /* ============================================================================================================================*/

    #region 初始化数据
    /************************************
     * 函数说明: 初始化数据
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    void Awake()
    {

    }
    #endregion

    #region 内部加载资源
    /************************************
     * 函数说明: 内部加载资源
     * 返 回 值: System.Collections.IEnumerator
     * 参数说明: resRequest
     * 注意事项: 
    ************************************/
    private IEnumerator LoadResources(string szPkgName, System.Action<string, AssetBundle> callback)
    {
        do
        {
            /** @brief: 检查参数 */
            if (string.IsNullOrEmpty(szPkgName))
            {
                //JZLog.LogError("AsyncLoadRes : Error szPkgName == null");
                yield break;
            }

            ResourceMgrData resourceData = null;
            /** @brief: 检查是否加载过此包 或者正在加载此包 */
            if (gDicPkgResource.ContainsKey(szPkgName) == false)
            {
                resourceData = new ResourceMgrData()
                {
                    eState = ResourceMgrData.State.None,
                    assetBundle = null
                };
                gDicPkgResource[szPkgName] = resourceData;
            }
            else
            {
                resourceData = gDicPkgResource[szPkgName];
                if (resourceData.eState == ResourceMgrData.State.LoadFinish)
                {
                    if (null != callback)
                    {
                        callback(szPkgName, resourceData.assetBundle);
                        callback = null;
                    }
                    yield break;
                }
            }

            /** @brief: 避免一帧时间过长 */
            /** @brief: Non matching Profiler.EndSample (BeginSample and EndSample count must match) */
            /** @brief: One is catching an exception from another object, the other is long frame times. If it's either of those two, it can only be fixed by the Unity team. */
            // yield return new WaitForEndOfFrame();
            if (gDicPkgResource.ContainsKey(szPkgName) == false)
            {
                //JZLog.LogError("LoadResources : Warning Pkg == " + szPkgName + " Have no this Pkg , MayBe Remove it!");
                yield break;
            }

            if (resourceData.eState == ResourceMgrData.State.None)
            {
                resourceData.eState = ResourceMgrData.State.Loading;
            }
            else
            {
                break;
            }

            VersionBundle bundle = VersionHelper.instance.GetBundle(szPkgName);
            if (null != bundle && null != bundle.dependency && bundle.dependency.Length > 0)
            {
                for (int i = 0; i < bundle.dependency.Length; i++)
                {
                    string depName = bundle.dependency[i];
                    if (gDicPkgResource.ContainsKey(depName) == false || gDicPkgResource[depName].assetBundle == null)
                    {
                        yield return GameWorld.instance.StartCoroutine(LoadResources(depName, null));
                    }
                }
            }

            AssetBundle asset = null;
            if (VersionHelper.instance.HasBundleHttp(szPkgName))
            {
                VersionBundle httpAB = VersionHelper.instance.GetBundle(szPkgName);
                string szUrl = GameConfig.HOST_RES() + httpAB.id + "?v=" + httpAB.versionValue;
                WWW www = WWW.LoadFromCacheOrDownload(szUrl, httpAB.versionValue, httpAB.crc);
                yield return www;

                if (null != www && null == www.error)
                {
                    asset = www.assetBundle;
                    www.Dispose();
                    www = null;
                }
                else
                {
                    JZLog.LogError("LoadResources : Load from www Error: " + szUrl + "\n" + www.error);
                }
            }
            else
            {
                PathUtils.PathType eType = PathUtils.PathType.None;
                string szUrl = PathUtils.GetReadablePath(szPkgName, ref eType, false);
                AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(szUrl);
                yield return abcr;
                if (null != abcr.assetBundle)
                {
                    asset = abcr.assetBundle;
                }
                else
                {
                    JZLog.LogError("LoadResources : Load from file Error: " + szUrl);
                }
            }

            if (null != asset)
            {
                if (resourceData.eState == ResourceMgrData.State.Loading)
                {
                    resourceData.assetBundle = asset;
#if UNITY_EDITOR
                    if ("shader_greymaterial.ab".Equals(szPkgName))
                    {
                        Material mat = gDicPkgResource[szPkgName].assetBundle.LoadAsset<Material>("GreyMaterial");
                        mat.shader = Shader.Find(mat.shader.name);
                    }
#endif
                }
            }
            else
            {
                resourceData.assetBundle = null;
            }

            /** @brief: 判断是否提前释放 */
            if (resourceData.eState == ResourceMgrData.State.Loading)
            {
                resourceData.eState = ResourceMgrData.State.LoadFinish;
            }
            else
            {
                if (IsInvalid(resourceData.assetBundle) == false)
                {
                    resourceData.assetBundle.Unload(true);
                    resourceData.assetBundle = null;
                }
            }
        } while (false);

        ResourceMgrData resourceMgrData = gDicPkgResource[szPkgName];
        /** @brief: 等待下载完成进行检测 */
        while (resourceMgrData.eState == ResourceMgrData.State.Loading)
        {
            yield return new WaitForEndOfFrame();
        }

        /** @brief: 提前释放 */
        if (resourceMgrData.eState == ResourceMgrData.State.None)
        {
            callback = null;
            ////JZLog.LogWarning("LoadResources : You have Destroyed this bundle " + szPkgName + " , Ignore ...");
            yield break;
        }

        InitHideFlag(resourceMgrData.assetBundle);

        if (callback != null)
        {
            callback.Invoke(szPkgName, resourceMgrData.assetBundle);
            callback = null;
        }
    }
    #endregion


    #region 判断类型用于删除资源
    /************************************
     * 函数说明: 判断类型
     * 返 回 值: bool
     * 注意事项: 
    ************************************/
    private bool IsGameObjectOrComponents(UnityEngine.Object obj)
    {
        if (obj is GameObject)
        {
            return true;
        }

        if (obj is Component)
        {
            return true;
        }
        return false;
    }

    void InitHideFlag(UnityEngine.Object obj)
    {
        if (IsInvalid(obj) == false)
        {
            obj.hideFlags = HideFlags.None;
        }
    }
    #endregion

    #region 卸载指定资源
    /************************************
     * 函数说明: 卸载指定资源
     * 返 回 值: void
     * 参数说明: resObj
     * 注意事项: 只能删除特定资源
    ************************************/
    private void ResourceUnLoad(UnityEngine.Object resObj)
    {
        if (IsInvalid(resObj) == false)
        {
            if (IsGameObjectOrComponents(resObj) == false)
            {
                Resources.UnloadAsset(resObj);
            }
            else
            {
                if (resObj.name.EndsWith("_animation"))
                {
                    tk2dSpriteAnimation library = ((GameObject)resObj).GetComponent<tk2dSpriteAnimation>();
                    library.UnloadTextures();
                }
                GameObject.DestroyImmediate(resObj, true);
            }
            resObj = null;
        }
    }
    #endregion

    #region 延时销毁无效资源(如有异常请调试)
    /************************************
     * 函数说明: 延时销毁无效资源
     * 返 回 值: System.Collections.Generic.IEnumerator
     * 注意事项: 延时一秒钟后在执行 本函数不要直接调用
    ************************************/
    private IEnumerator UnloadUnusedAssets()
    {
        bIsRunUnloadUnusedAssets = true;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(1.0f);
        if (bIsDestroyAllAssetBundle == false)
        {
            AsyncOperation mAsync = Resources.UnloadUnusedAssets();
            ////JZLog.LogWarning("UnloadUnusedAssets ...... Please wait ... ");
            yield return mAsync;
            ////JZLog.LogWarning("UnloadUnusedAssets ...... Success");
        }
        //GC.Collect();
        bIsRunUnloadUnusedAssets = false;
    }

    private void StartUnloadUnusedAssets()
    {
        if (bIsRunUnloadUnusedAssets == false && bIsDestroyAllAssetBundle == false)
        {
            GameWorld.instance.StartCoroutine(UnloadUnusedAssets());
        }
    }
    #endregion

    #region 判断一个obj是否合法可用
    /************************************
     * 函数说明: 判断一个obj是否合法可用
     * 返 回 值: bool
     * 参数说明: resObject
     * 注意事项: 返回true表示资源无效
    ************************************/
    private static bool IsInvalidObject(object resObject)
    {
        if (resObject == null)
        {
            return true;
        }
        return false;
    }
    private static bool IsInvalid(System.Object resObject)
    {
        return IsInvalidObject(resObject as object);
    }
    private static bool IsInvalid(UnityEngine.Object resObject)
    {
        return IsInvalidObject(resObject as object);
    }
    #endregion

    #region 注销管理器
    /************************************
     * 函数说明: 注销管理器
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    public void OnDestroy()
    {
        DestroyAllAssetBundle();
        //JZLog.Log("Close ResourceMgr ... ");
    }
    #endregion

    #region 销毁资源包(临时代码）
    /************************************
     * 函数说明: 销毁资源包
     * 返 回 值: void
     * 注意事项: 这里可以指定一些资源加载完实例化之后就卸载
    ************************************/
    private void PrivateAutoDestroyAssetBundle(string szPkgName)
    {
       // DestroyAssetBundle(szPkgName, false);
    }
    #endregion

    /// <summary>
    /// 预加载lua包字典
    /// </summary>
    public Dictionary<string, AssetBundle> luaAssetBundles = new Dictionary<string, AssetBundle>();

    #region 添加lua包
    /************************************
     * 函数说明: 添加lua包
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    public void AddLuaAssetBundle(string name, AssetBundle bundle)
    {
        if (luaAssetBundles.ContainsKey(name))
        {
            luaAssetBundles[name] = bundle;
        }
        else
        {
            luaAssetBundles.Add(name, bundle);
        }
    }
    #endregion

    #region 加载需要更新的包
    /************************************
     * 函数说明: 加载需要更新的包
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    public void LoadNeedUpdateBundle(Action onComplate, Action onError)
    {
        GameWorld.instance.StartCoroutine(StartLoadNeedUpdateBundle(onComplate, onError));
    }
    #endregion

    #region 加载需要更新的包
    /************************************
     * 函数说明: 加载需要更新的包
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    IEnumerator StartLoadNeedUpdateBundle(Action onComplate, Action onError)
    {
        int size = VersionHelper.instance.needUpdateBundles.Count;
        int complateCounter = 0;
        int errorCounter = 0;

        foreach (VersionBundle bundle in VersionHelper.instance.needUpdateBundles.Values)
        {
            LoadBundle(bundle, () => { complateCounter++; }, () => { errorCounter++; });
        }

        while(complateCounter + errorCounter < size)
        {
            JZWL.LoadingScene.instance.SetEndValue(0.4f + 0.3f * (complateCounter + errorCounter) / size, null);
            yield return null;
        }

        if (errorCounter > 0)
        {
            if (null != onError) onError();
        }
        else
        {
            VersionHelper.instance.WriteConfig();
            if (null != onComplate) onComplate();
        }
    }
    #endregion

    #region 通过版本文件加载资源包
    /************************************
     * 函数说明: 通过版本文件加载资源包
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    public void LoadBundle(VersionBundle bundle, Action onComplate, Action onError)
    {
        GameWorld.instance.StartCoroutine(LoadFile(bundle, onComplate, onError));
    }
    #endregion

    #region 通过版本文件加载资源包
    /************************************
     * 函数说明: 通过版本文件加载资源包
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    IEnumerator LoadFile(VersionBundle bundle, Action onComplate, Action onError)
    {
        string path = GameConfig.HOST_RES() + "ab/" + bundle.id + "?v=" + bundle.versionValue;
        WWW www = new WWW(path);
        yield return www;
        if (null == www.error)
        {
            byte[] bytes = www.bytes;
            Debug.Log("write path: " + PathUtils.MakeFilePath(bundle.id, PathUtils.PathType.MobileDiskWrite));
            FileInfo fileInfo = new FileInfo(PathUtils.MakeFilePath(bundle.id, PathUtils.PathType.MobileDiskWrite));
            Stream stream = fileInfo.Create();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
            if (null != onComplate) onComplate();
        }
        else
        {
            Debug.LogWarning(" LoadBundle Error : " + bundle.id + " : " + path + " ----- " + www.error);
            if (null != onError) onError();
        }
    }
    #endregion

    /// <summary>
    /// 获取当前已经加载了的资源列表
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, List<string>> GetAllLoadedResInfo()
    {
        Dictionary<string, List<string>> list = new Dictionary<string, List<string>>();
        foreach(string key in gDicPkgResource.Keys)
        {
            List<string> res = new List<string>();
            ResourceMgrData data = gDicPkgResource[key];
            res.Add(data.referenceCount.ToString());
            foreach(string id in data.dicResource.Keys)
            {
                res.Add(id);
            }
            list.Add(key, res);
        }
        return list;
    }

    public void AddDontUnloadAB(string abName)
    {
        if (!abName.EndsWith(gBundleFileName))
        {
            abName = abName + gBundleFileName;
        }

        if (!_dontUnloadABs.Contains(abName))
        {
            _dontUnloadABs.Add(abName);
        }
    }

    RemoveAB tempRemoveAB = null;
    ResourceMgrData tempResourceData = null;
    public void Update()
    {
        for(int i = 0; i < m_toBeRmovedABs.Count;)
        {
            tempRemoveAB = m_toBeRmovedABs[i];
            tempRemoveAB.time -= Time.deltaTime;
            if (tempRemoveAB.time <= 0)
            {
                for(int j = 0; j < tempRemoveAB.removeABs.Count; j++)
                {
                    if (gDicPkgResource.TryGetValue(tempRemoveAB.removeABs[j], out tempResourceData))
                    {
                        if (tempResourceData.referenceCount <= 0)
                        {
                            //Debug.Log(tempRemoveAB.removeABs[j]);
                            DestroyAssetBundle(tempRemoveAB.removeABs[j], false);
                        }
                    }
                }

                m_toBeRmovedABs.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }
}