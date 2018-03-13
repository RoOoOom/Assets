using System;
namespace NetJZWL
{
    /// <summary>
    /// 消息序列化的实现类
    /// </summary>
    public class ByteBuffer : IMsgOutPutByteArray
    {
        //引用的数组
		protected byte[] p_source;
        //起点
        protected int p_start;
        //终点
        protected int p_stop;
        //长度
        protected int p_length = 0;
        //当前Pop指针位置
        protected int p_position = 0;

        public ByteBuffer(int size = 2048)
        {
            p_source = new byte[size];
            p_source.Initialize();
            p_start = 0;
            p_length = 0;
            p_stop = 0;
            p_position = 0;
        }

        public ByteBuffer(byte[] source, int start, int length)
        {
            p_source = source;
            p_start = start;
            p_length = length;
            p_stop = start + length;
            p_position = p_start;
        }

        public int length
        {
            get
            {
                return p_length;
            }
        }

        public int position
        {
            get
            {
                return p_position;
            }
            set
            {
                p_position = value;
            }
        }

        public byte[] ToByteArray()
        {
            //分配大小
            byte[] bytes = new byte[p_length];
            //调整指针
            Array.Copy(p_source, p_start, bytes, 0, p_length);
            return bytes;
        }

        public void PushByte(byte by)
        {
            p_source[p_position++] = by;
            p_length++;
        }

        public void PushSByte(sbyte by)
        {
            PushByte((byte)by);
        }

        public void PushByteArray(byte[] sourceBytes)
        {
            int length = sourceBytes.Length;
            Array.Copy(sourceBytes, 0, p_source, p_position, length);
            p_length += length;
            p_position += length;
        }

        public void PushUShort(ushort Num)
        {
            p_source[p_position++] = (byte)(Num >> 8);
            p_source[p_position++] = (byte)(Num >> 0);
            p_length += 2;
        }

        public void PushShort(short Num)
        {
            PushUShort((ushort)Num);
        }

        public void PushUInt(uint Num)
        {
            p_source[p_position++] = (byte)(Num >> 24);
            p_source[p_position++] = (byte)(Num >> 16);
            p_source[p_position++] = (byte)(Num >> 8);
            p_source[p_position++] = (byte)(Num >> 0);
            p_length += 4;
        }

        public void PushInt(int Num)
        {
            PushUInt((uint)Num);
        }

        public void PushULong(ulong Num)
        {
            p_source[p_position++] = (byte)(Num >> 56);
            p_source[p_position++] = (byte)(Num >> 48);
            p_source[p_position++] = (byte)(Num >> 40);
            p_source[p_position++] = (byte)(Num >> 32);
            p_source[p_position++] = (byte)(Num >> 24);
            p_source[p_position++] = (byte)(Num >> 16);
            p_source[p_position++] = (byte)(Num >> 8);
            p_source[p_position++] = (byte)(Num >> 0);
            p_length += 8;
        }

        public void PushLong(long Num)
        {
            PushULong((ulong)Num);
        }

        public void PushLuaInt64(long low, long high)
        {
            long i = high * (long)Math.Pow(2, 32) + low;
            PushLong(i);
        }

        public void PushString(string value)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            PushShort((short)bytes.Length);
            PushByteArray(bytes);
        }

        public byte PopByte()
        {
            byte ret = p_source[p_position++];
            return ret;
        }

        public sbyte PopSByte()
        {
            return (sbyte)PopByte();
        }

        public ushort PopUShort()
        {
            //溢出
            if (p_position + 2 > p_stop)
            {
                return 0;
            }
            ushort ret = (ushort)(p_source[p_position] << 0x08 | p_source[p_position + 1]);
            p_position += 2;
            return ret;
        }

        public short PopShort()
        {
            return (short)PopUShort();
        }

        public uint PopUInt()
        {            
            if (p_position + 4 > p_stop)
                return 0;
            uint ret = (uint)(p_source[p_position] << 0x18 | p_source[p_position + 1] << 0x10 | p_source[p_position + 2] << 0x08 | p_source[p_position + 3]);
            p_position += 4;
            return ret;
        }

        public int PopInt()
        {
            return (int)PopUInt();
        }

        public ulong PopULong()
        {
            if (p_position + 8 > p_stop)
                return 0;
            ulong ret = (ulong)((uint)(p_source[p_position] << 0x18 | p_source[p_position + 1] << 0x10 | p_source[p_position + 2] << 0x08 | p_source[p_position + 3]) * 0x100000000) +
                (uint)(p_source[p_position + 4] << 0x18 | p_source[p_position + 5] << 0x10 | p_source[p_position + 6] << 0x08 | p_source[p_position + 7]);
            p_position += 8;
            return ret;
        }

        public long PopLong()
        {
            return (long)PopULong();
        }
        
        public string PopLuaInt64()
        {
            return PopULong().ToString();
        }

        public string PopString()
        {
            short len = PopShort();
            return System.Text.Encoding.UTF8.GetString(PopByteArray(len));
        }

        public byte[] PopByteArray(int Length)
        {
            //溢出
            if (p_position + Length > p_stop)
            {
                return new byte[0];
            }
            byte[] ret = new byte[Length];
            Array.Copy(p_source, p_position, ret, 0, Length);
            //提升位置
            p_position += Length;
            return ret;
        }
        public byte[] GetByteArray(int index, int Length)
        {
            if (index + length > length)
            {
                return new byte[0];
            }

            byte[] data = new byte[Length];
            Array.Copy(p_source, index, data, 0, Length);
            return data;
        }

        public void Reset()
        {
            p_start = 0;
            p_position = 0;
            p_length = 0;
            p_stop = 0;
        }
    }
}