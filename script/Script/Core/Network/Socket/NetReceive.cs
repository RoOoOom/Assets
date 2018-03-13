using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace NetJZWL
{
    public class NetReceive
    {
        private const int DefaultBufferLength = 1024 * 64;
        private MemoryStream stream;
        private int readLength;
        private bool isBody;


        public NetReceive()
        {
            stream = new MemoryStream(DefaultBufferLength);
            readLength = 0;
            isBody = false;
        }

        public MemoryStream Stream
        {
            get
            {
                return stream;
            }
        }

        public bool IsBody
        {
            get
            {
                return isBody;
            }
        }

        public int ReadLength
        {
            get
            {
                return readLength;
            }
        }

        public void PrepareReadHeader(int headLength)
        {
            Reset(headLength, false);
        }

        public void PrepareReadBody(int bodyLength)
        {
            Reset(bodyLength, true);
        }

        private void Reset(int readLength, bool isBody)
        {
            if (readLength < 0)
            {
                throw new Exception("reset target length is Error");
            }
            stream.SetLength(0);
            this.readLength = readLength;
            this.isBody = isBody;
        }

    }
}
    
