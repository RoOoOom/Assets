using UnityEngine;
using System;

namespace NetJZWL
{
    public class PackageIn : ByteBuffer
    {
        private int _code;
        private int _errCode;

        public PackageIn(byte[] source, int start, int length)
            : base(source, start, length)
        {
            _ReadHeader();
        }

        private void _ReadHeader()
        {
            //PopInt();
            _code = PopInt();
            _errCode = PopInt();
        }

        public int code
        {
            get { return _code; }
        }

        public int errCode
        {
            get { return _errCode; }
        }

        //public bool HasBody()
        //{
        //    return p_length > NetClient.SIZE_HEAD;
        //}
    }
}


