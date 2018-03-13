using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;

/// <summary>
/// JZWL输出log控制类
/// </summary>
public class JZLog
{
    public enum JZLogLevel
    {
        Info = 0,
        Warning,
        Error
    }
    
    //是否使用log
    public static bool useLog = true;
    //log输出等级
    public static JZLogLevel m_level = JZLogLevel.Info;
    //log标签，类似： “JZWL====>>”
    public static string tag = "";

    public static void Init(bool log, bool reportError, JZLogLevel level)
    {
        useLog = log;
        m_level = level;
        if (reportError)
        {
            Application.logMessageReceived += CaptureLog;
        }
    }

    public static void Log(object message)
	{
        if (useLog && (int)m_level <= (int)JZLogLevel.Info)
		{
			Debug.Log(tag + message); 
		}
    }

    public static void LogWarning(object message) 
    {
        if (useLog && (int)m_level <= (int)JZLogLevel.Warning)
		{
            Debug.LogWarning(tag + message); 
		}
    }

    public static void LogError(object message)
    {
        if (useLog && (int)m_level <= (int)JZLogLevel.Error)
		{
            Debug.LogError(tag + message);
		}
    }

    private static Dictionary<string, string> errorDict = new Dictionary<string, string>();

    private static void CaptureLog (string condition, string stacktrace, LogType type)
    {
        if (type == LogType.Error)
        {
            errorDict.Clear();

            string url = ServerInfoConfig.GetErrorURL();
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            errorDict.Add("msg", condition + "\n" + stacktrace);
            errorDict.Add("game", ServerInfoConfig.GetGame());
            errorDict.Add("environment", "android");
            string deviceID = "";
            //string deviceID = SystemInfo.deviceUniqueIdentifier;
            if (!string.IsNullOrEmpty(deviceID)) errorDict.Add("deviceId", deviceID);
            string deviceName = "";
            if (!string.IsNullOrEmpty(deviceName)) errorDict.Add("deviceName", deviceName);
            string platform = "jzwl";
            if (!string.IsNullOrEmpty(platform)) errorDict.Add("platform", platform);
            string accountName = "";
            if (!string.IsNullOrEmpty(accountName)) errorDict.Add("account", accountName);
            string accountTicket = "";
            if (!string.IsNullOrEmpty(accountName)) errorDict.Add("ticket", accountTicket);
            long playerId = 0;
            if (playerId != 0) errorDict.Add("playerId", playerId.ToString());

            if (errorDict != null && errorDict.Count > 0)
            {
               string reqUrl = url + "?";
               foreach (KeyValuePair<string, string> post_arg in errorDict)
                {
                    reqUrl += post_arg.Key + "=" + WWW.EscapeURL(post_arg.Value, System.Text.Encoding.UTF8) + "&";
                }
                reqUrl = reqUrl.Remove(reqUrl.Length - 1);

                try
                {
                    System.Net.WebRequest wReq = System.Net.WebRequest.Create(reqUrl);
                    System.Net.WebResponse wResp = wReq.GetResponse();
                    wResp.GetResponseStream();
                }
                catch (System.Exception)
                {
                    //errorMsg = ex.Message;
                }
            }
        }
    }

    private static void OpenReadWithHttps(string URL, string strPostdata, string strEncoding)
    {
        Encoding encoding = Encoding.Default;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
        request.Method = "post";
        request.Accept = "text/html, application/xhtml+xml, */*";
        request.ContentType = "application/x-www-form-urlencoded";
        byte[] buffer = encoding.GetBytes(strPostdata);
        request.ContentLength = buffer.Length;
        request.GetRequestStream().Write(buffer, 0, buffer.Length);
        request.GetResponse();
    }
}