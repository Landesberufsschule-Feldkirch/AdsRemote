using AdsRemote;
using AdsRemote.Common;
using Newtonsoft.Json;
using System.IO;

namespace LAP_2010_2_Transportwagen.Cx9020
{
    public class Verbindung
    {
        public Kommunikation Kommunikation { get; set; }

        private string status;

        public Verbindung()
        {
            IpAdressen ipAdressen;
            PLC plc;

            status = "Keine Verbindung!";
            ipAdressen = JsonConvert.DeserializeObject<IpAdressen>(File.ReadAllText(@"Kommunikation/IpAdressen.json"));

            plc = new PLC(ipAdressen.AmsNetId);
            plc.DeviceReady += PlcDeviceReady;
            plc.DeviceLost += PlcDeviceLost;
            
            Kommunikation = plc.Class<Kommunikation>();
        }

        internal string GetStatus() => status;

        private void PlcDeviceLost(object sender, AdsDevice e) => status = "Lost [" + e.Address.Port.ToString() + "]";
        private void PlcDeviceReady(object sender, AdsDevice e) => status = "READY [" + e.Address.Port.ToString() + "]";
    }
}