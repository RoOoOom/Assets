using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.IO;

public enum WebRequestStatus
{
    Downloading,
    OK,
    Error,
}

public class WebRequestBuffer
{
    public byte[] Buffer;

    public FileStream fs;

    public const int BufferSize = 1024;

    public Stream OrginalStream;

    public HttpWebResponse WebResponse;

    public WebRequestBuffer(FileStream fileStream)
    {
        Buffer = new byte[1024];
        fs = fileStream;
    }
}

public class MyHttp
{
    private string m_path = null;
    public string url = null;
    private int m_version = -1;
    private int m_crc = -1;
    public WebRequestStatus status = WebRequestStatus.Downloading;
    public byte[] data = null;

    public MyHttp(string path, string url)
    {
        m_path = path;
        this.url = url;
    }

    public MyHttp(string path, string url, int version, int crc)
        : this(path, url)
    {
        m_version = version;
        m_crc = crc;
    }

    public void Start()
    {
        Loom.RunAsync(() =>
        {
            if (m_version != -1 || m_crc != -1)
            {
                try
                {
                    using (FileStream fs = new FileStream(m_path + ".info", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        if (fs != null && fs.Length >= 8)
                        {
                            byte[] bytes = new byte[4];
                            fs.Read(bytes, 0, 4);
                            int version = BitConverter.ToInt32(bytes, 0);
                            fs.Read(bytes, 0, 4);
                            int crc = BitConverter.ToInt32(bytes, 0);
                            if (m_version == version && m_crc == crc)
                            {
                                using (FileStream fsData = new FileStream(m_path, FileMode.Open, FileAccess.Read))
                                {
                                    data = new byte[fsData.Length];
                                    fsData.Read(data, 0, data.Length);
                                    status = WebRequestStatus.OK;
                                    return;
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Loom.QueueOnMainThread(() => { Debug.Log(e.Message); });
                }
            }

            try
            {
                HttpWebRequest httpRequest = WebRequest.Create(url) as HttpWebRequest;
                httpRequest.BeginGetResponse(new AsyncCallback(ResponseCallback), httpRequest);
            }
            catch (WebException e)
            {
                Loom.QueueOnMainThread(() => { Debug.LogError(e.Message); });
                status = WebRequestStatus.Error;
            }
            catch (Exception e)
            {
                Loom.QueueOnMainThread(() => { Debug.LogError(e.Message); });
                status = WebRequestStatus.Error;
            }
        });
    }

    void ResponseCallback(IAsyncResult ar)
    {
        HttpWebRequest req = ar.AsyncState as HttpWebRequest;
        if(req == null) return;
        HttpWebResponse response = req.EndGetResponse(ar) as HttpWebResponse;
        if(response.StatusCode != HttpStatusCode.OK)
        {
            status = WebRequestStatus.Error;
            response.Close();
            return;
        }
        FileStream fileStream = new FileStream(m_path, FileMode.Create, FileAccess.ReadWrite, FileShare.Write);
        WebRequestBuffer st = new WebRequestBuffer(fileStream);
        st.WebResponse = response;
        Stream responseStream = response.GetResponseStream();
        st.OrginalStream = responseStream;
        responseStream.BeginRead(st.Buffer, 0, WebRequestBuffer.BufferSize, new AsyncCallback(HttpReadCallback), st);
    }

    void HttpReadCallback(IAsyncResult ar)
    {
        //Loom.QueueOnMainThread(() => { Debug.Log(123123); });
        try
        {
            WebRequestBuffer rs = ar.AsyncState as WebRequestBuffer;
            int read = rs.OrginalStream.EndRead(ar);
            if (read > 0)
            {
                rs.fs.Write(rs.Buffer, 0, read);
                rs.fs.Flush();
                rs.OrginalStream.BeginRead(rs.Buffer, 0, WebRequestBuffer.BufferSize, new AsyncCallback(HttpReadCallback), rs);
            }
            else
            {
                long length = rs.fs.Length;
                data = new byte[length];
                rs.fs.Seek(0, SeekOrigin.Begin);
                rs.fs.Read(data, 0, data.Length);
                rs.fs.Close();
                rs.OrginalStream.Close();
                rs.WebResponse.Close();

                using (FileStream fs = new FileStream(m_path + ".info", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] v = BitConverter.GetBytes(m_version);
                    fs.Write(v, 0, v.Length);
                    byte[] c = BitConverter.GetBytes(m_crc);
                    fs.Write(c, 0, c.Length);
                    fs.Flush();
                }
            }

            status = WebRequestStatus.OK;
        }
        catch (Exception e)
        {
            status = WebRequestStatus.Error;
            Loom.QueueOnMainThread(() => { Debug.LogError(e.Message); });
        }
    }
}
