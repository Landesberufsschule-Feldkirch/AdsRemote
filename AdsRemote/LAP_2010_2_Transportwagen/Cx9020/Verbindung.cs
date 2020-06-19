using AdsRemote;
using AdsRemote.Common;
using Newtonsoft.Json;
using System;
using System.IO;

namespace LAP_2010_2_Transportwagen.Cx9020
{
    public class Verbindung
    {
        public Kommunikation Kommunikation { get; set; }

        private string status;
        private string farbe;

        public Verbindung()
        {
            IpAdressen ipAdressen;
            PLC plc;

            status = "Keine Verbindung!";
            farbe = "LightBlue";

            ipAdressen = JsonConvert.DeserializeObject<IpAdressen>(File.ReadAllText(@"Kommunikation/IpAdressen.json"));

            plc = new PLC(ipAdressen.AmsNetId);
            plc.Tune_ReinitInterval = 2;
            plc.DeviceReady += PlcDeviceReady;
            plc.DeviceLost += PlcDeviceLost;

            Kommunikation = plc.Class<Kommunikation>();

            status = "Mit SPS Verbinden [" + ipAdressen.AmsNetId + "]";
            farbe = "LightBlue";
        }

        internal string GetStatus() => status;
        internal string GetFarbe() => farbe;

        private void PlcDeviceLost(object sender, AdsDevice e)
        {
            status = "Verbindung zur SPS verloren [Port " + e.Address.Port.ToString() + "]";
            farbe = "Red";
        }
        private void PlcDeviceReady(object sender, AdsDevice e)
        {
            status = "Verbindung zur SPS OK [Port " + e.Address.Port.ToString() + "]";
            farbe = "LawnGreen";
        }

    }
}