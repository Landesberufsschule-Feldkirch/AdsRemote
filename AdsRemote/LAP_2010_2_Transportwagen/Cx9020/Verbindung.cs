using AdsRemote.Common;
using LAP_2010_2_Transportwagen.Kommunikation;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace LAP_2010_2_Transportwagen.Cx9020
{
    public class Verbindung
    {
        public Kommunikation.Kommunikation Kommunikation { get; set; }

        private string _status;
        private string _farbe;
        private readonly bool _lokaleIpAdresseFalsch;
        public Verbindung()
        {
            _status = "Keine Verbindung!";
            _farbe = "LightBlue";

            var ipAdressen = JsonConvert.DeserializeObject<IpAdressen>(File.ReadAllText(@"Kommunikation/IpAdressen.json"));

            _lokaleIpAdresseFalsch = LokaleIpAdresseTesten(ipAdressen.AmsNetIdPc);

            var plc = new AdsRemote.Plc(ipAdressen.AmsNetIdSps) { TuneReinitInterval = 2 };
            plc.DeviceReady += PlcDeviceReady;
            plc.DeviceLost += PlcDeviceLost;

            Kommunikation = plc.Class<Kommunikation.Kommunikation>();

            _status = "Mit SPS Verbinden [" + ipAdressen.AmsNetIdPc + "]";
            _farbe = "LightBlue";
        }

        private bool LokaleIpAdresseTesten(string ipAdresse)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && ipAdresse.Contains(ip.ToString())) return false;
            }
            return true; // blendet die Fehlermeldung ein
        }

        internal string GetStatus() => _status;
        internal string GetFarbe() => _farbe;
        internal bool GetLokaleAmsIdFalsch() => _lokaleIpAdresseFalsch;
        private void PlcDeviceLost(object sender, AdsDevice e)
        {
            _status = "Verbindung zur SPS verloren [Port " + e.Address.Port.ToString() + "]";
            _farbe = "Red";
        }
        private void PlcDeviceReady(object sender, AdsDevice e)
        {
            _status = "Verbindung zur SPS OK [Port " + e.Address.Port.ToString() + "]";
            _farbe = "LawnGreen";
        }

    }
}