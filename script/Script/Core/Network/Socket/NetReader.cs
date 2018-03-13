using System;
using System.Collections.Generic;

namespace NetJZWL
{
    public class NetReader : INetReader
    {
        /// <summary>
        /// 入包队列
        /// </summary>
        private Queue<PackageIn> m_packages = new Queue<PackageIn>(); 

        /// <summary>
        /// 设置解密码
        /// </summary>
        public void SetPassword()
        {

        }

        /// <summary>
        /// 解密
        /// </summary>
        public void Decode()
        {
            
        }

        /// <summary>
        /// 添加入包
        /// </summary>
        /// <param name="package"></param>
        public void AddPackage(PackageIn package)
        {
            if (package == null)
            {
                UnityEngine.Debug.LogError("AddPackage pack is null ");
            }
            lock(m_packages)
            {
                m_packages.Enqueue(package);
            } 
        }

        /// <summary>
        /// 是否有数据包需要逻辑处理
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            return m_packages.Count > 0;
        }

        /// <summary>
        /// 处理数据包，并发送消息通知
        /// </summary>
        public void HandleNext()
        {
            PackageIn pack = null;
            lock (m_packages)
            {
                pack = m_packages.Dequeue();
            }
            NetworkManager.instance.Read(pack);
        }
        /// <summary>
        /// 清空协议包
        /// </summary>
        public void Clear()
        {
            m_packages.Clear();
        }
    }
}
