using System;
using System.Net;
using System.Net.Sockets;

namespace AdsRemote.Router
{
    internal class ResponseResult
    {
        private UdpReceiveResult _result;
        public byte[] Buffer => _result.Buffer;
        public IPAddress RemoteHost => _result.RemoteEndPoint.Address;

        public int Shift { get; set; }

        public ResponseResult(UdpReceiveResult result)
        {
            this._result = result;
            Shift = 0;
        }

        public byte[] NextChunk(int length, bool dontShift = false, int add = 0)
        {
            byte[] to = new byte[length];
            Array.Copy(_result.Buffer, Shift, to, 0, length);

            if (!dontShift)
            {
                Shift += length + add;
            }

            return to;
        }
    }
}
