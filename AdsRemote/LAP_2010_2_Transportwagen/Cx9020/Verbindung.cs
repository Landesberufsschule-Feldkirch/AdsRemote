using AdsRemote;
using AdsRemote.Common;
using Newtonsoft.Json;
using System.IO;
using LAP_2010_2_Transportwagen.Kommunikation;

namespace LAP_2010_2_Transportwagen.Cx9020
{
    public class Verbindung
    {
        public Kommunikation.Kommunikation Kommunikation { get; set; }

        private string _status;
        private string _farbe;

        public Verbindung()
        {
            _status = "Keine Verbindung!";
            _farbe = "LightBlue";

            var ipAdressen = JsonConvert.DeserializeObject<IpAdressen>(File.ReadAllText(@"Kommunikation/IpAdressen.json"));

            var plc = new PLC(ipAdressen.AmsNetId) {Tune_ReinitInterval = 2};
            plc.DeviceReady += PlcDeviceReady;
            plc.DeviceLost += PlcDeviceLost;

            Kommunikation = plc.Class<Kommunikation.Kommunikation>();

            _status = "Mit SPS Verbinden [" + ipAdressen.AmsNetId + "]";
            _farbe = "LightBlue";
        }

        internal string GetStatus() => _status;
        internal string GetFarbe() => _farbe;

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