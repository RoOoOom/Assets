using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;

namespace NetJZWL
{
    public delegate void NetDelegate(Channel channel);

    /// <summary>
    /// 频道
    /// </summary>
    public enum Channel
    {
        Normal = 0,
        Count
    }

    /// <summary>
    /// 客户端网络类
    /// </summary>
    public class NetClient
    {
        public NetClient(Channel channel, INetReader reader, INetSender sender)
        {
            m_channel = channel;
            m_netReader = reader;
            m_netSender = sender;
            m_Receive = new NetReceive();
        }

        /// <summary>
        /// 连接状态
        /// </summary>
        private enum SocketStatus
        {
            Connecting,
            Connected,
            Closed,
        }

        /// <summary>
        /// 同步状态，对应各个事件
        /// </summary>
        private enum AsyncStatus
        {
            None,
            ConnectSucceed,
            ConnectFailed,
            SendFailed,
            ReceiveFailed
        }

        public event NetDelegate eventConnectSucceed;   //连接成功事件
        public event NetDelegate eventConnectFailed;    //连接失败事件
        public event NetDelegate eventConnectTimeOut;   //连接超时事件
        public event NetDelegate eventSendFailed;       //发送失败事件
        public event NetDelegate eventConnectClose;     //连接断开事件 

        public const int SIZE_READ_BUFFER = 8192;       //读取缓存区大小
        public const int SIZE_WRITE_BUFFER = 1024;      //发送缓存区大小
        public const int SIZE_DATA_BUFFER = 50120;      //数据解析缓冲区
        public const int SIZE_HEAD = 12;               //协议头长度
        public const int CONNECT_TIME_OUT_TIME = 10000; //连接超时时间，单位毫秒
        public const int SEND_TIME_OUT = 10000;          //发送超时时间，单位毫秒
        public const int WAIT_EACH_RECEIVE = 5;        //读包间隔
        public const int WAIT_EACH_SEND = 5;           //发包间隔
        public const int HEAD_LENGTH = 4;

        private Channel m_channel;
        private Socket m_socket;

        private SocketStatus m_status = SocketStatus.Closed;
        private AsyncStatus m_asyncStatus = AsyncStatus.None;

        private INetSender m_netSender = null;
        private INetReader m_netReader = null;
        private NetReceive m_Receive = null;

        private byte[] m_byteBuffer = new byte[SIZE_READ_BUFFER];
        private byte[] m_recvBuffer = new byte[SIZE_DATA_BUFFER];
        private byte[] m_headBuffer = new byte[4];
        private int m_readOffset;		 	//读索引
        private int m_writeOffset;        	//写索引

        private long m_connectStartTime; //开始连接时间，单位：毫秒
        private bool m_active = false;
        
        private static int sendCout;

        private Thread m_recvThread;

        private bool m_canTargetEvent ;

        public static int SendCount
        {
            get
            {
                return sendCout;
            }
        }

        /// <summary>
        /// 连接制定ip和端口
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            m_connectStartTime = TimeUtils.CurLocalTimeMilliSecond();
            m_status = SocketStatus.Connecting;
            m_active = false;
            m_netReader.Clear();
            m_netSender.Clear();
            m_canTargetEvent = true;
            PackageOut.ResetIndex();
            lock (this)
            {
                sendCout = 0;
            }
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, port);
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, SEND_TIME_OUT);
                m_socket.BeginConnect(ipEndpoint, new AsyncCallback(M_ConnectCallBack), m_socket);
            }
            catch (Exception)
            {
                Debug.LogError("服务器关闭，请稍后重试");
                m_socket.Close();
                m_status = SocketStatus.Closed;
            }
        }

        /// <summary>
        /// 异步连接回调
        /// </summary>
        /// <param name="ar"></param>
        private void M_ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                Socket client = ar.AsyncState as Socket;
                if (null != client)
                {
                    client.EndConnect(ar);
                    if (m_socket.Connected)
                    {
                        m_socket.ReceiveBufferSize = SIZE_READ_BUFFER;
                        m_socket.SendBufferSize = SIZE_WRITE_BUFFER;
                        m_socket.NoDelay = true;

                        m_status = SocketStatus.Connected;
                        m_asyncStatus = AsyncStatus.ConnectSucceed;
                        m_active = true;
                        m_Receive.PrepareReadHeader(HEAD_LENGTH);
                        Receive();
                    }
                }
                else
                {
                    JZLog.LogError(" connect fail reason  ar.AsyncStat return null ");
                    m_asyncStatus = AsyncStatus.ConnectFailed;
                    m_socket.Close();
                    m_status = SocketStatus.Closed;
                }
            }
            catch (Exception e)
            {
                JZLog.LogError("M_ConnectCallBack fail error " + e.Message);
                m_asyncStatus = AsyncStatus.ConnectFailed;
                m_socket.Close();
                m_status = SocketStatus.Closed;
            }
        }
        

        public void Send(PackageOut pkgOut)
        {
            if (m_status != SocketStatus.Connected)
            {
                return;
            }
            try
            {
                byte[] data = pkgOut.ToByteArray();
                if (NetworkManager.instance.IsDebug)
                {
                    NetworkManager.instance.Error("pkg send now code " + pkgOut.code + " process " + Thread.CurrentThread.ManagedThreadId.ToString());
                }
                lock(this)
                {
                    sendCout++;
                }
                
                m_socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(this.SendCallback), m_socket);
            }
            catch(Exception ex)
            {
                NetworkManager.instance.Error("pkg send error: " + ex.Message);
                lock (this)
                {
                    sendCout--;
                }
            }
        }
        

        void SendCallback(IAsyncResult ar)
        {
            try
            {
                m_netSender.SetSending(false);
                lock (this)
                {
                    sendCout--;
                }
                Socket socket = (Socket)ar.AsyncState;
                int send = socket.EndSend(ar); 
                if (NetworkManager.instance.IsDebug)
                {
                    NetworkManager.instance.Error("pkg SendCallback now  process " + Thread.CurrentThread.ManagedThreadId.ToString());
                }
                if (send <= 0)
                {
                    NetworkManager.instance.Error("Socket Send: Socket Send Data Length Is Zero");
                    m_asyncStatus = AsyncStatus.SendFailed;
                    return;
                } 

            }
            //catch (ObjectDisposedException)
            //{
            //    return;
            //}
            catch (SocketException ex)
            {
                NetworkManager.instance.Error("SocketException Send: " + ex.Message);
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    m_asyncStatus = AsyncStatus.SendFailed;
                else
                    throw;
            }
            catch (Exception ex)
            {
                NetworkManager.instance.Error("Socket Send: " + ex.Message);
            }
        }

        // 关闭连接
        public void Close()
        {
            m_status = SocketStatus.Closed;
            m_asyncStatus = AsyncStatus.None;

            lock(m_netSender)
            {
                m_netSender.Clear();
                m_netSender.SetSending(false);
            }

            if (null != m_socket)
            {
                if (m_socket.Connected)
                {
                    m_socket.Disconnect(true);
                }
                m_socket.Close();
                m_socket = null;
            }
        }

        public void Update()
        {
            if (m_socket == null)
            {
                if (m_canTargetEvent) 
                {
                    if (null != eventConnectClose) eventConnectClose(m_channel);
                    m_canTargetEvent = false;
                }
                return;
            } 
            if (m_status == SocketStatus.Connecting)
            {
                if ((TimeUtils.CurLocalTimeMilliSecond() - m_connectStartTime) >= CONNECT_TIME_OUT_TIME)
                {
                    m_socket.Close();
                    m_status = SocketStatus.Closed;
                    m_asyncStatus = AsyncStatus.None;
                    if (null != eventConnectTimeOut) 
                        eventConnectTimeOut(m_channel);
                }
            }
            else if (m_status == SocketStatus.Connected && !m_socket.Connected)
            {
                m_status = SocketStatus.Closed;
                m_asyncStatus = AsyncStatus.None;
                if (null != eventConnectClose)
                    eventConnectClose(m_channel);
            }
            else if (m_asyncStatus == AsyncStatus.ConnectFailed)
            {
                m_netReader.Clear();
                m_netSender.Clear();
                m_asyncStatus = AsyncStatus.None;
                if (null != eventConnectFailed) 
                    eventConnectFailed(m_channel);
            }
            else if (m_asyncStatus == AsyncStatus.ConnectSucceed)
            {
                m_asyncStatus = AsyncStatus.None;
                if (null != eventConnectSucceed) 
                    eventConnectSucceed(m_channel);
            }
            else if (m_asyncStatus == AsyncStatus.SendFailed)
            {
                m_socket.Close();
                m_status = SocketStatus.Closed;
                m_asyncStatus = AsyncStatus.None;
                if (null != eventSendFailed)
                    eventSendFailed(m_channel);
            }

            while (m_status == SocketStatus.Connected && m_netReader.HasNext())
            {
                m_netReader.HandleNext();
            }
        }
 

        private void Receive()
        {
            try
            { 
                m_socket.BeginReceive(m_Receive.Stream.GetBuffer(), (int)m_Receive.Stream.Position, m_Receive.ReadLength - (int)m_Receive.Stream.Position, SocketFlags.None, ReceiveCallback, m_socket);
            }
            catch (Exception e)
            {
                NetworkManager.instance.Error("Socket ReveiveCallback: " + e.Message);
                return; 
            }
           
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            if (m_status != SocketStatus.Connected)
            {
                return;
            }
            int bytesReceived = 0;
            try
            {
                bytesReceived = socket.EndReceive(ar);
            }
            catch (Exception e)
            {
                NetworkManager.instance.Error("Socket ReveiveCallback: " + e.Message);
                return;
            }
            if (bytesReceived <= 0)
            {
                return;
            }

            m_Receive.Stream.Position += bytesReceived;
            if (m_Receive.Stream.Position < m_Receive.ReadLength)
            {
                Receive();
                return;
            }
            bool processSuccess = false;
            if (m_Receive.IsBody)
            {
                processSuccess = ProcessPacket();
            } else
            {
                processSuccess = ProcessPacketHeader();
            }
            if (processSuccess)
            {
                Receive();
            }
        }

        private bool ProcessPacketHeader()
        {
            int reqLen = 0;
            try
            { 
                Array.Copy(m_Receive.Stream.GetBuffer(), 0, m_headBuffer, 0, 4);
                if (BitConverter.IsLittleEndian)
                { 
                    reqLen = BitConverter.ToInt32(Util.GetConvertEdian(m_headBuffer), 0);
                }
                else
                {
                    reqLen = BitConverter.ToInt32(m_headBuffer, 0);
                }
                if (reqLen > 0)
                {
                    m_Receive.PrepareReadBody(reqLen - HEAD_LENGTH);
                }
                else
                {
                    m_Receive.PrepareReadHeader(HEAD_LENGTH);
                }
            } 
            catch (Exception e)
            { 
                NetworkManager.instance.Error("Socket ProcessPacketHeader: " + e.Message);
                return false;
            } 
            return true;
        }

        private bool ProcessPacket()
        {
            try
            {
                byte[] body = new byte[m_Receive.ReadLength];
                Array.Copy(m_Receive.Stream.GetBuffer(), 0, body, 0, m_Receive.ReadLength);
                PackageIn package = new PackageIn(body, 0, m_Receive.ReadLength);
                m_netReader.AddPackage(package);
                m_Receive.PrepareReadHeader(HEAD_LENGTH);
                if (NetworkManager.instance.IsDebug)
                {
                    NetworkManager.instance.Error("pkg recevie now " + package.code + " process " + Thread.CurrentThread.ManagedThreadId.ToString());
                }
            }
            catch (Exception e)
            {
                NetworkManager.instance.Error("Socket ProcessPacket: " + e.Message);
                return false;
            }
            return true;
        }



    }


}


