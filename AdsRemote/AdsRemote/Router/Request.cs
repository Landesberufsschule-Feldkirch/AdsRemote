using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AdsRemote.Router
{
    internal class Request
    {
        public const int DefaultUdpPort = 48899;

        readonly UdpClient _client;
        public UdpClient Client => _client;

        public int Meintimeout;
        public int Timeout
        {
            get => Meintimeout;

            set
            {
                Meintimeout = value;
                _client.Client.ReceiveTimeout = _client.Client.SendTimeout = Timeout;
            }
        }

        public Request(int timeout = 10000)
        {
            _client = new UdpClient {EnableBroadcast = true};
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            Timeout = timeout;
        }

        public async Task<Response> SendAsync(IPEndPoint endPoint)
        {
            byte[] data = GetRequestBytes;
            await _client.SendAsync(data, data.Length, endPoint);

            return new Response(_client, Timeout);
        }

        readonly List<byte[]> _listOfBytes = new List<byte[]>();
        public byte[] GetRequestBytes
        {
            get { return _listOfBytes.SelectMany(a => a).ToArray(); }
        }

        public void Add(byte[] segment)
        {
            _listOfBytes.Add(segment);
        }

        public void Clear()
        {
            _listOfBytes.Clear();
        }
    }
}
