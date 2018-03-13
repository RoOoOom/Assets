using System;
using System.Collections.Generic;

namespace NetJZWL
{
    public class NetSender : INetSender
    {
        Queue<PackageOut> packages = new Queue<PackageOut>();
        bool isSending = false;

        /// <summary>
        /// 设置密码
        /// </summary>
        public void SetPassword()
        {

        }

        /// <summary>
        /// 加密
        /// </summary>
        public byte[] Encode(byte[] bytes)
        {
            return bytes;
        }

        public void AddPackage(object obj)
        {
            packages.Enqueue(obj as PackageOut);
        }

        public bool HasNext()
        {
            return packages.Count > 0;
        }

        public byte[] GetNextData()
        {
            PackageOut pkg = packages.Dequeue();
            return Encode(pkg.ToByteArray());
        }

        public bool IsSending()
        {
            return this.isSending;
        }

        public void SetSending(bool sending)
        {
            this.isSending = sending;
        }

        public void Clear()
        {
            packages.Clear();
        }
    }
}
