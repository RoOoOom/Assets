using UnityEngine;
using System.Collections;
using System;

public class SDKUtil  {
    public SDKUtil()
    {

    }

    private string className = "com.sz.jzwl.sdk.PlatfromSDK";
    public string ClassName
    {
        get { return className; }
        set { className = value; }
    }

    public void CallAPI(string apiName, params object[] args)
    {
        if (IsTestType())
        {
            return;
        }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR

#elif UNITY_ANDROID
        using(AndroidJavaClass cls = new AndroidJavaClass(ClassName)){
            try
            {
                cls.CallStatic(apiName, args);
            }
            catch (Exception e)
            {
                JZLog.LogError("CallAPI:" + apiName + "," + ClassName + "," + e.Message + "," + e.StackTrace);
            }
        }
#elif UNITY_IPHONE


#endif

    }

    public T CallAPI<T>(string apiName, params object[] args)
    {
        if (IsTestType())
        {
            return default(T);
        }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        return default(T);
#elif UNITY_ANDROID
        T result;
        using(AndroidJavaClass cls = new AndroidJavaClass(ClassName)){
            try
            {
                result = cls.CallStatic<T>(apiName, args);
            }
            catch (Exception e)
            {
                JZLog.LogError("CallAPI:" + apiName + "," + ClassName + "," + e.Message + "," + e.StackTrace);
                result = default(T);
            }
        }
        return result;
#elif UNITY_IPHONE
       return default(T);
#endif

    }

    internal void StartActivityIndicator()
    {
        if (IsTestType())
        {
            return;
        }
#if UNITY_ANDROID
        Handheld.StartActivityIndicator();
#endif
    }

    internal void StopActivityIndicator()
    {
        if (IsTestType())
        {
            return;
        }
#if UNITY_ANDROID
        Handheld.StopActivityIndicator();
#endif
    }

    internal void SetActivityIndicatorStyle()
    {
        if (IsTestType())
        {
            return;
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        Handheld.SetActivityIndicatorStyle(AndroidActivityIndicatorStyle.InversedSmall);
#elif UNITY_IOS
		Handheld.SetActivityIndicatorStyle(UnityEngine.iOS.ActivityIndicatorStyle.Gray);
#endif
    }

    private bool IsTestType()
    {
        if (ServerInfoConfig.GetServerInfoType() == ServerInfoType.inner ||
            ServerInfoConfig.GetServerInfoType() == ServerInfoType.sout ||
            ServerInfoConfig.GetServerInfoType() == ServerInfoType.sout_test)
        {
            return true;
        } else
        {
            return false;
        }
    }
}
