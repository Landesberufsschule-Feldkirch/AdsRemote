﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AdsRemote.Router
{
    internal class Response
    {
        readonly UdpClient _client;
        public UdpClient Client => _client;
        public int Timeout;

        public Response(UdpClient client, int timeout = 10000)
        {
            this._client = client;
            Timeout = timeout;
        }

        public async Task<ResponseResult> ReceiveAsync()
        {
            ResponseResult result = null;

            var worker = _client.ReceiveAsync();
            var task = await Task.WhenAny(worker, Task.Delay(Timeout));

            if (task == worker)
            {
                UdpReceiveResult udpResult = await worker;
                result = new ResponseResult(udpResult);
            }
            else
            {
                _client.Close();
            }

            return result;
        }

        public async Task<List<ResponseResult>> ReceiveMultipleAsync()
        {
            List<ResponseResult> results = new List<ResponseResult>();
            int start = Environment.TickCount;
            while (true)
            {
                var worker = _client.ReceiveAsync();
                var task = await Task.WhenAny(worker, Task.Delay(Timeout));

                long interval = (long)TimeSpan.FromTicks(Environment.TickCount - start).TotalMilliseconds - start;
                if ((interval < Timeout) && (task == worker))
                {
                    UdpReceiveResult udpResult = await worker;
                    results.Add(new ResponseResult(udpResult));
                }
                else
                {
                    _client.Close();
                    break;
                }
            }

            return results;
        }
    }
}
