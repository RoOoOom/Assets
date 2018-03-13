using System;

/// <summary>
/// 消息序列化的字节数组接口
/// </summary>
public interface IMsgOutPutByteArray
{
    void PushByte(byte by);

    void PushSByte(sbyte by);

    void PushUShort(ushort Num);

    void PushShort(short Num);

    void PushUInt(uint Num);

    void PushInt(int Num);

    void PushULong(ulong value);

    void PushLong(long Num);

    void PushString(string value);

    byte[] ToByteArray();

    byte[] GetByteArray(int index, int Length);

    int length{get;}
}

