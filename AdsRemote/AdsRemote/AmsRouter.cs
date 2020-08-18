using AdsRemote.Common;
using AdsRemote.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace AdsRemote
{
    public class AmsRouter
    {
        public readonly AmsNetId AmsNetId;

        public AmsRouter(AmsNetId amsNetId)
        {
            AmsNetId = amsNetId;
        }

        /// <summary>
        /// Add new record to the AMS router.
        /// </summary>
        /// <param name="localhost"></param>
        /// <param name="remoteHost"></param>
        /// <param name="localAmsNetId"></param>
        /// <param name="name">The name of a new record</param>
        /// <param name="isTemporaryRoute"></param>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="localIpName">IP or machine name</param>
        /// <param name="timeout"></param>
        /// <param name="adsUdpPort"></param>
        /// <returns>True - if route added, False - otherwise</returns>
        // ReSharper disable once UnusedMember.Global
        public static async Task<bool> AddRecordAsync(
            IPAddress localhost,
            IPAddress remoteHost,
            AmsNetId localAmsNetId,
            string localIpName = null,
            string name = null,
            string login = "Administrator",
            string password = "1",
            bool isTemporaryRoute = false,
            int timeout = 10000,
            int adsUdpPort = Request.DefaultUdpPort)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = Environment.MachineName;
            if (string.IsNullOrWhiteSpace(localIpName))
                localIpName = Environment.MachineName;
            if (string.IsNullOrWhiteSpace(login))
                login = Environment.UserName;

            byte[] segmentAmsnetid = localAmsNetId.ToBytes();

            byte[] segmentRoutename = name.GetAdsBytes();
            byte[] segmentRoutenameLength = Segment.RoutenameL;
            segmentRoutenameLength[2] = (byte)segmentRoutename.Length;

            byte[] segmentUsername = login.GetAdsBytes();
            byte[] segmentUsernameLength = Segment.UsernameL;
            segmentUsernameLength[2] = (byte)segmentUsername.Length;

            byte[] segmentPassword = password.GetAdsBytes();
            byte[] segmentPasswordLength = Segment.PasswordL;
            segmentPasswordLength[2] = (byte)segmentPassword.Length;

            byte[] segmentIpaddress = localIpName.GetAdsBytes();
            byte[] segmentIpaddressLength = Segment.LocalhostL;
            segmentIpaddressLength[2] = (byte)segmentIpaddress.Length;

            Request request = new Request(timeout);

            request.Add(Segment.Header);
            request.Add(Segment.End);
            request.Add(Segment.RequestAddroute);
            request.Add(segmentAmsnetid);
            request.Add(Segment.Port);
            request.Add(isTemporaryRoute ?
                        Segment.RoutetypeTemp :
                        Segment.RoutetypeStatic);
            request.Add(segmentRoutenameLength);
            request.Add(segmentRoutename);
            request.Add(Segment.AmsnetidL);
            request.Add(segmentAmsnetid);
            request.Add(Segment.UsernameL);
            request.Add(segmentUsername);
            request.Add(Segment.PasswordL);
            request.Add(segmentPassword);
            request.Add(Segment.LocalhostL);
            request.Add(segmentIpaddress);

            if (isTemporaryRoute)
                request.Add(
                        Segment.TemprouteTail);

            IPEndPoint endpoint = new IPEndPoint(remoteHost, adsUdpPort);
            Response response = await request.SendAsync(endpoint);
            ResponseResult rr = await response.ReceiveAsync();
            bool isAck = ParseAddRecordResponse(rr);

            return isAck;
        }

        /// <summary>
        /// Parses response early recieved by AdsFinder
        /// </summary>
        /// <param name="rr"></param>
        /// <returns>True - if route added, False - otherwise</returns>
        private static bool ParseAddRecordResponse(ResponseResult rr)
        {
            if (rr == null)
                return false;

            if (!rr.Buffer.Take(4).ToArray().SequenceEqual(Segment.Header))
                return false;
            if (!rr.Buffer.Skip(4).Take(Segment.End.Length).ToArray().SequenceEqual(Segment.End))
                return false;
            if (!rr.Buffer.Skip(8).Take(Segment.ResponseDiscover.Length).ToArray().SequenceEqual(Segment.ResponseAddroute))
                return false;

            rr.Shift =
                Segment.Header.Length +
                Segment.End.Length +
                Segment.ResponseAddroute.Length +
                Segment.Amsnetid.Length +
                Segment.Port.Length +
                Segment.End.Length +
                Segment.End.Length;

            byte[] ack = rr.NextChunk(Segment.LRouteack);

            return (ack[0] == 0) && (ack[1] == 0);
        }

        #region Device Finder
        public static async Task<List<RemotePlcInfo>> BroadcastSearchAsync(IPAddress localhost, int timeout = 10000, int adsUdpPort = Request.DefaultUdpPort)
        {
            Request request = CreateSearchRequest(localhost, timeout);

            IPEndPoint broadcast =
                new IPEndPoint(
                    IpHelper.GetBroadcastAddress(localhost),
                    adsUdpPort);

            Response response = await request.SendAsync(broadcast);
            List<ResponseResult> responses = await response.ReceiveMultipleAsync();

            List<RemotePlcInfo> devices = new List<RemotePlcInfo>();
            foreach (var r in responses)
            {
                RemotePlcInfo device = ParseBroadcastSearchResponse(r);
                devices.Add(device);
            }

            return devices;
        }

        // ReSharper disable once UnusedMember.Global
        public static async Task<RemotePlcInfo> GetRemotePlcInfoAsync(IPAddress localhost, IPAddress remoteHost, int timeout = 10000, int adsUdpPort = Request.DefaultUdpPort)
        {
            Request request = CreateSearchRequest(localhost, timeout);

            IPEndPoint broadcast = new IPEndPoint(remoteHost, adsUdpPort);

            Response response = await request.SendAsync(broadcast);
            ResponseResult rr = await response.ReceiveAsync();

            return (rr == null) ? null : ParseBroadcastSearchResponse(rr);
        }

        private static Request CreateSearchRequest(IPAddress localhost, int timeout = 10000)
        {
            Request request = new Request(timeout);

            byte[] segmentAmsnetid = Segment.Amsnetid;
            localhost.GetAddressBytes().CopyTo(segmentAmsnetid, 0);

            request.Add(Segment.Header);
            request.Add(Segment.End);
            request.Add(Segment.RequestDiscover);
            request.Add(segmentAmsnetid);
            request.Add(Segment.Port);
            request.Add(Segment.End);

            return request;
        }

        private static RemotePlcInfo ParseBroadcastSearchResponse(ResponseResult rr)
        {
            RemotePlcInfo device = new RemotePlcInfo {Address = rr.RemoteHost};


            if (!rr.Buffer.Take(4).ToArray().SequenceEqual(Segment.Header))
                return device;
            if (!rr.Buffer.Skip(4).Take(Segment.End.Length).ToArray().SequenceEqual(Segment.End))
                return device;
            if (!rr.Buffer.Skip(8).Take(Segment.ResponseDiscover.Length).ToArray().SequenceEqual(Segment.ResponseDiscover))
                return device;

            rr.Shift = Segment.Header.Length + Segment.End.Length + Segment.ResponseDiscover.Length;

            // AmsNetId
            // then skip 2 bytes of PORT + 4 bytes of ROUTE_TYPE
            byte[] amsNetId = rr.NextChunk(Segment.Amsnetid.Length, add: Segment.Port.Length + Segment.RoutetypeStatic.Length);
            device.AmsNetId = new AmsNetId(amsNetId);

            // PLC NameLength
            byte[] bNameLen = rr.NextChunk(Segment.LNamelength);
            int nameLen =
                bNameLen[0] == 5 && bNameLen[1] == 0 ?
                    bNameLen[2] + bNameLen[3] * 256 :
                    0;

            byte[] bName = rr.NextChunk(nameLen - 1, add: 1);
            device.Name = ASCIIEncoding.Default.GetString(bName);

            // TCat type
            byte[] tcatType = rr.NextChunk(Segment.TcattypeRuntime.Length);
            if (tcatType[0] == Segment.TcattypeRuntime[0])
                if (tcatType[2] == Segment.TcattypeRuntime[2])
                    device.IsRuntime = true;

            // OS version
            byte[] osVer = rr.NextChunk(Segment.LOsversion);
            ushort osKey = (ushort)(osVer[0] * 256 + osVer[4]);
            device.OsVersion = OsIds.ContainsKey(osKey) ? OsIds[osKey] : osKey.ToString("X2");
            //??? device.OS += " build " + (osVer[8] + osVer[9] * 256).ToString();

            bool isUnicode = false;

            // looking for packet with tcat version;
            // usually it is in the end of the packet
            byte[] tail = rr.NextChunk(rr.Buffer.Length - rr.Shift, true);

            int ci = tail.Length - 4;
            for (int i = ci; i > 0; i -= 4)
                if (tail[i + 0] == 3 &&
                    tail[i + 2] == 4)
                {
                    isUnicode = tail[i + 4] > 2; // Tc3 uses unicode

                    device.TcVersion.Version = tail[i + 4];
                    device.TcVersion.Revision = tail[i + 5];
                    device.TcVersion.Build = tail[i + 6] + tail[i + 7] * 256;
                    break;
                }

            // Comment
            byte[] descMarker = rr.NextChunk(Segment.LDescriptionmarker);
            int len = 0;
            int c = rr.Buffer.Length;
            if (descMarker[0] == 2)
            {
                if (isUnicode)
                    for (int i = 0; i < c; i += 2)
                    {
                        if (rr.Buffer[rr.Shift + i] == 0 &&
                            rr.Buffer[rr.Shift + i + 1] == 0)
                            break;
                        len += 2;
                    }
                else
                    for (int i = 0; i < c; i++)
                    {
                        if (rr.Buffer[rr.Shift + i] == 0)
                            break;
                        len++;
                    }

                if (len > 0)
                {
                    byte[] description = rr.NextChunk(len);

                    if (!isUnicode)
                        device.Comment = ASCIIEncoding.Default.GetString(description);
                    else
                    {
                        byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, description);
                        char[] asciiChars = new char[Encoding.ASCII.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
                        Encoding.ASCII.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
                        device.Comment = new string(asciiChars);
                    }
                }
            } // if (descMarker[0] == 2)

            return device;
        }

        public static readonly Dictionary<ushort, string> OsIds =
            new Dictionary<ushort, string>
            {
                {0x0700, "Windows CE 7"},
                {0x0602, "Windows 8/8.1/10"},
                {0x0601, "Windows 7 Embedded Standart"},
                {0x0600, "Windows CE 6"},
                {0x0500, "Windows CE 5"},
                {0x0501, "Windows XP"}
            };
        #endregion
    }
}
