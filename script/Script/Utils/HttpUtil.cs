using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
public class HttpUtil
{
    private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

    public static string Get(string url)
    {
        HttpWebResponse response = CreateGetHttpResponse(url, null, null, null);
        Stream myResponseStream = response.GetResponseStream();
        StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
        string retString = myStreamReader.ReadToEnd();
        myStreamReader.Close();
        myResponseStream.Close();
        return retString;
    }
    
    public static HttpWebResponse CreateGetHttpResponse(string url, int? timeout, string userAgent, CookieCollection cookies)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentNullException("url");
        }
        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "GET";
        request.UserAgent = DefaultUserAgent;
        request.ContentType = "text/html;charset=UTF-8";
        if (!string.IsNullOrEmpty(userAgent))
        {
            request.UserAgent = userAgent;
        }
        if (timeout.HasValue)
        {
            request.Timeout = timeout.Value;
        }
        if (cookies != null)
        {
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(cookies);
        }
        return request.GetResponse() as HttpWebResponse;
    }

    public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int? timeout, string userAgent, Encoding requestEncoding, CookieCollection cookies)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentNullException("url");
        }
        if (requestEncoding == null)
        {
            throw new ArgumentNullException("requestEncoding");
        }
        HttpWebRequest request = null;
        //如果是发送HTTPS请求  
        if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            request = WebRequest.Create(url) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version10;
        }
        else
        {
            request = WebRequest.Create(url) as HttpWebRequest;
        }
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";

        if (!string.IsNullOrEmpty(userAgent))
        {
            request.UserAgent = userAgent;
        }
        else
        {
            request.UserAgent = DefaultUserAgent;
        }

        if (timeout.HasValue)
        {
            request.Timeout = timeout.Value;
        }
        if (cookies != null)
        {
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(cookies);
        }
        //如果需要POST数据  
        if (!(parameters == null || parameters.Count == 0))
        {
            StringBuilder buffer = new StringBuilder();
            int i = 0;
            foreach (string key in parameters.Keys)
            {
                if (i > 0)
                {
                    buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                }
                else
                {
                    buffer.AppendFormat("{0}={1}", key, parameters[key]);
                }
                i++;
            }
            byte[] data = requestEncoding.GetBytes(buffer.ToString());
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
        }
        return request.GetResponse() as HttpWebResponse;
    }

    private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        return true; //总是接受  
    } 

    public static string GetIpByDNS(string dns)
    {
        System.Net.IPAddress[] ips = System.Net.Dns.GetHostAddresses(dns);
        if (null != ips && ips.Length > 0)
        {
            return ips[0].ToString();
        }

        return dns;
    }
}
