using UnityEngine;
using System.Collections;

namespace NetJZWL
{
    public interface INetReader
    {
        void SetPassword();
        void Decode();
        void AddPackage(PackageIn package);
        bool HasNext();
        void HandleNext();
        void Clear();
    }
}


