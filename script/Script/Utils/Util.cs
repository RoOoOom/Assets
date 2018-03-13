using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Reflection;
using LuaInterface;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Util {
    public static int Int(object o) {
        return Convert.ToInt32(o);
    }

    public static float Float(object o) {
        return (float)Math.Round(Convert.ToSingle(o), 2);
    }

    public static long Long(object o) {
        return Convert.ToInt64(o);
    }

    public static int Random(int min, int max) {
        return UnityEngine.Random.Range(min, max);
    }

    public static float Random(float min, float max) {
        return UnityEngine.Random.Range(min, max);
    }

    public static string Uid(string uid) {
        int position = uid.LastIndexOf('_');
        return uid.Remove(0, position + 1);
    }

    public static long GetTime() {
        TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
        return (long)ts.TotalMilliseconds;
    }

    public static GameObject GetChildByUrl(GameObject go, string url)
    {
        Transform findTarget = Get<Transform>(go, url);
        if (null != findTarget) return findTarget.gameObject;
        return null;
    }

    public static Component GetChildComponent(GameObject go, string name, bool deep)
    {
        return GetChildByName<Component>(go, name, deep);
    }

    public static GameObject GetChildGameObject(GameObject go, string name, bool deep)
    {
        Transform findTarget = GetChildByName<Transform>(go, name, deep);
        if (findTarget == null) return null;
        return findTarget.gameObject;
    }

    private static T GetChildByName<T>(GameObject go, string name) where T : Component
    {
        foreach (Transform t in go.transform)
        {
            if (t.gameObject.name == name)
            {
                T c = t.GetComponent<T>();
                if (c != null) return c;
            }
        }
        return null;
    }
    public static T GetChildByName<T>(GameObject go, string name, bool deep) where T : Component
    {
        if (!deep) return GetChildByName<T>(go, name);

        T[] cs = go.GetComponentsInChildren<T>(true);
        return GetComponentByName<T>(cs, name);
    }

    private static T GetComponentByName<T>(T[] cs, string name) where T : Component
    {
        if (cs != null)
        {
            return Array.Find<T>(cs, c => c.gameObject.name == name);
        }
        return null;
    }


    /// <summary>
    /// 搜索子物体组件-GameObject版
    /// </summary>
    public static T Get<T>(GameObject go, string subnode) where T : Component {
        if (go != null) {
            Transform sub = go.transform.Find(subnode);
            if (sub != null) return sub.GetComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// 搜索子物体组件-Transform版
    /// </summary>
    public static T Get<T>(Transform go, string subnode) where T : Component {
        if (go != null) {
            Transform sub = go.Find(subnode);
            if (sub != null) return sub.GetComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// 搜索子物体组件-Component版
    /// </summary>
    public static T Get<T>(Component go, string subnode) where T : Component {
        return go.transform.Find(subnode).GetComponent<T>();
    }

    /// <summary>
    /// 添加组件
    /// </summary>
    public static T Add<T>(GameObject go) where T : Component {
        if (go != null) {
            T[] ts = go.GetComponents<T>();
            for (int i = 0; i < ts.Length; i++) {
                if (ts[i] != null) GameObject.Destroy(ts[i]);
            }
            return go.gameObject.AddComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// 添加组件
    /// </summary>
    public static T Add<T>(Transform go) where T : Component {
        return Add<T>(go.gameObject);
    }

    /// <summary>
    /// 取平级对象
    /// </summary>
    public static GameObject Peer(Transform go, string subnode) {
        Transform tran = go.parent.Find(subnode);
        if (tran == null) return null;
        return tran.gameObject;
    }

    /// <summary>
    /// 计算字符串的MD5值
    /// </summary>
    public static string md5(string source) {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
        byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
        md5.Clear();

        string destString = "";
        for (int i = 0; i < md5Data.Length; i++) {
            destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
        }
        destString = destString.PadLeft(32, '0');
        return destString;
    }

    /// <summary>
    /// 计算文件的MD5值
    /// </summary>
    public static string md5file(string file) {
        try {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++) {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        } catch (Exception ex) {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

    /// <summary>
    /// 清除所有子节点
    /// </summary>
    public static void ClearChild(Transform go) {
        if (go == null) return;
        for (int i = go.childCount - 1; i >= 0; i--) {
            GameObject.Destroy(go.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 清理内存
    /// </summary>
    public static void ClearMemory() {
        GC.Collect(); 
        Resources.UnloadUnusedAssets();
        LuaManager.instance.LuaGC();
    }

    /// <summary>
    /// 取得数据存放目录
    /// </summary>
    public static string DataPath {
        get {
            string game = AppConst.AppName.ToLower();
            if (Application.isMobilePlatform) {
                return Application.persistentDataPath + "/" + game + "/";
            }
            if (Application.platform == RuntimePlatform.WindowsPlayer) {
                return Application.streamingAssetsPath + "/";
            }
            if (Application.isEditor) {
                return Application.streamingAssetsPath + "/";
            }
            if (Application.platform == RuntimePlatform.OSXEditor) {
                int i = Application.dataPath.LastIndexOf('/');
                return Application.dataPath.Substring(0, i + 1) + game + "/";
            }
            return "c:/" + game + "/";
        }
    }

    /// <summary>
    /// 应用程序内容路径
    /// </summary>
    public static string AppContentPath() {
        string path = string.Empty;
        switch (Application.platform) {
            case RuntimePlatform.Android:
                path = "jar:file://" + Application.dataPath + "!/assets/";
            break;
            case RuntimePlatform.IPhonePlayer:
                path = Application.dataPath + "/Raw/";
            break;
            default:
                path = Application.dataPath + "/StreamingAssets/";
            break;
        }
        return path;
    }

    public static GameObject LoadAsset(AssetBundle bundle, string name) {
#if UNITY_5
    return bundle.LoadAsset(name, typeof(GameObject)) as GameObject;
#else
        return bundle.Load(name, typeof(GameObject)) as GameObject;
#endif
    }

    public static Component AddComponent(GameObject go, string assembly, string classname) {
        Assembly asmb = Assembly.Load(assembly);
        Type t = asmb.GetType(assembly + "." + classname);
        return go.AddComponent(t);
    }

    /// <summary>
    /// 载入Prefab
    /// </summary>
    /// <param name="name"></param>
    public static GameObject LoadPrefab(string name) {
        return Resources.Load(name, typeof(GameObject)) as GameObject;
    }

    /// <summary>
    /// 大小端转换
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] GetConvertEdian(byte[] data)
    {
        int len = data.Length;
        byte[] result = new byte[len];
        for (int i = 0; i < len; i++)
        {
            result[i] = data[len - i - 1];
        }
        return result;
    }

    public static string ArrayToString<T>(T[] list)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.Length; i++)
        {
            sb.Append(list[i].ToString()).Append("&");
        }
        return sb.ToString();
    }

    public static void BindParent(Transform parent, Transform child)
    {
        child.SetParent(parent, false);
        child.localRotation = Quaternion.identity;
        child.localPosition = Vector3.zero;
        child.localScale = Vector3.one;
        child.gameObject.layer = parent.gameObject.layer;
    }

    public static Vector3 TransformPoint(Transform transform, float x, float y, float z)
    {
        return transform.TransformPoint(new Vector3(x, y, z));
    }

    public static Vector3 InverseTransformPoint(Transform transform, float x, float y, float z)
    {
        return transform.InverseTransformPoint(new Vector3(x, y, z));
    }

    public static void AddBtnClick(UnityEngine.UI.Button btn, LuaFunction func)
    {
        if (null != btn && null != func)
        {
            btn.onClick.AddListener(() => 
            {
                //LuaManager.instance.CallFunctionNoGC(func, false, btn.gameObject);
                //func.Call(btn.gameObject); 
                func.BeginPCall();
                func.Push(btn.gameObject);
                func.PCall();
                func.EndPCall();
            });
        }
    }

    public static T GetOrAddComponent<T>(GameObject go) where T : MonoBehaviour
    {
        if (go == null) return null;

        T t = go.GetComponent<T>();
        if (t == null)
        {
            t = go.AddComponent<T>();
        }
        return t;
    }

    public static int ParseVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            return -1;
        }

        string ret = version.Replace(".", "");
        return int.Parse(ret);
    }
}
