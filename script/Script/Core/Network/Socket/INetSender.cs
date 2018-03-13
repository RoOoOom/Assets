using UnityEngine;
using System.Collections;

namespace NetJZWL
{
    public interface INetSender
    {
        void SetPassword();
        byte[] Encode(byte[] bytes);
        void AddPackage(object obj);
        bool HasNext();
        byte[] GetNextData();
        void Clear();
        bool IsSending();
        void SetSending(bool sending);
    }
}


