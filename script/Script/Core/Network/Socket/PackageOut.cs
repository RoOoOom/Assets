using System;

namespace NetJZWL
{
    public class PackageOut : ByteBuffer
    {
        public int code { get { return m_code; } }
        private int m_code;

        /// <summary>
        /// 协议包序列号
        /// </summary>
        private static int packageIndex = 0;

        public PackageOut(int code)
        {
            PushInt(0x00);	//长度
            PushInt(code);	//协议号
            if (packageIndex == int.MaxValue - 1)
            {
                packageIndex = 0;
            }
            PushInt(++packageIndex);//序列号
            m_code = code;
        }

        public static void ResetIndex()
        {
            packageIndex = 0;
        }

        public void Pack()
        {
            byte[] len;
            if (BitConverter.IsLittleEndian)
            {
                len = Util.GetConvertEdian(BitConverter.GetBytes(p_length));
            }
            else
            {
                len = BitConverter.GetBytes(p_length);
            }
            Array.Copy(len, 0, p_source, 0, len.Length);
        }

    }
}