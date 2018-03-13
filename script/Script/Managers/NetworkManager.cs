using UnityEngine;
using System;
using NetJZWL;

public class NetworkManager : SimleManagerTemplate<NetworkManager>
{
    private NetClient m_client = null;
    private bool m_inited = false;

    private string ip;
    private int port;

    private bool isDebug;

    LuaInterface.LuaFunction readFunc = null;

    protected override void OnInit()
    {
        m_inited = true;
    }

    public void SetDebug(int debug)
    {
        if (debug == 1)
        {
            isDebug = true;
        } else
        {
            isDebug = false;
        }
    }

    public bool IsDebug
    {
        get
        {
            return isDebug;
        }
    }

    private void InitNetClient()
    {
        m_client = new NetClient(Channel.Normal, new NetReader(), new NetSender());
        m_client.eventSendFailed += SendFailed;
        m_client.eventConnectFailed += ConnectFailed;
        m_client.eventConnectTimeOut += ConnectTimeOut; 
        m_client.eventConnectClose += ConnectClose;
        m_client.eventConnectSucceed += ConnectSucceed;
        readFunc = LuaManager.instance.Lua.GetFunction("Network.OnRead"); 
    }

    void SendFailed(Channel channel)
    {
        Debug.LogError("SendFailed " + this.ip + "  " + this.port);
        LuaManager.instance.CallFunction("Network.Offline");
    }

    void ConnectFailed(Channel channel)
    {
        Debug.LogError("ConnectFailed " + this.ip + "  " + this.port);
        LuaManager.instance.CallFunction("Network.ConnectFailed");
    }

    void ConnectTimeOut(Channel channel)
    {
        Debug.LogError("ConnectTimeOut  " + this.ip + "  " + this.port);
        LuaManager.instance.CallFunction("Network.ConnectTimeOut");
    }

    void ConnectClose(Channel channel)
    {
        Debug.LogError("ConnectClose " + this.ip + "  " + this.port);
        LuaManager.instance.CallFunction("Network.Offline");
    }

    void ConnectSucceed(Channel channel)
    {
        LuaManager.instance.CallFunction("Network.ConnectSucceed");
    }

    public void Error(object p)
    {
        Debug.LogError(p);
    }

    public void Connect(string ip, int port)
    {
        System.Net.IPAddress[] ips = System.Net.Dns.GetHostAddresses(ip);
        if (null != ips && ips.Length > 0)
        {
            if (m_client != null)
            {
                m_client.Close();
            } 
            this.InitNetClient();
            this.ip = ip;
            this.port = port;
            m_client.Connect(ips[0].ToString(), port);
            NetSpeed.SetIp(ips[0].ToString());
        }
    }

    public void Close()
    {
        if (m_client != null)
        {
            m_client.Close();
        }
    }

    public void Update()
    {
        if (m_client != null)
        {
            m_client.Update();
        }
    }

    public PackageOut NewPackagetOut(int commandCode)
    {
        return new PackageOut(commandCode);
    }

    public void Send(PackageOut package)
    {
        package.Pack();
        m_client.Send(package);
    }

    public void Read(PackageIn packageIn)
    {
        if (null != readFunc)
        {
            readFunc.BeginPCall();
            readFunc.PushObject(packageIn);
            readFunc.PCall();
            readFunc.EndPCall();
        }

        //LuaManager.instance.CallFunction("Network.OnRead", packageIn);
    }
}
