using AdsRemote;
using AdsRemote.Common;
using Newtonsoft.Json;
using System.IO;

namespace LAP_2010_2_Transportwagen.Kommunikation
{
    public class Cx9020Verbindung
    {
        public Kommunikation Kommunikation { get; set; }

        private string status;

        public Cx9020Verbindung()
        {
            IpAdressen cx9020Client;
            PLC cx9020;

            status = "Keine Verbindung!";

            cx9020Client = JsonConvert.DeserializeObject<IpAdressen>(File.ReadAllText(@"Kommunikation/IpAdressen.json"));


            cx9020 = new PLC(cx9020Client.AmsNetId);
            cx9020.DeviceReady += Cx9020DeviceReady;
            cx9020.DeviceLost += Cx9020DeviceLost;
            
            Kommunikation = cx9020.Class<Kommunikation>();
        }

        internal string GetStatus() => status;

        private void Cx9020DeviceLost(object sender, AdsDevice e) => status = "Lost [" + e.Address.Port.ToString() + "]";
        private void Cx9020DeviceReady(object sender, AdsDevice e) => status = "READY [" + e.Address.Port.ToString() + "]";
    }
}