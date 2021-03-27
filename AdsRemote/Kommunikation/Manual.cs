using System;
using System.Text;
using System.Threading;

namespace Kommunikation
{
    public class Manual : IPlc
    {
        private readonly Action<Datenstruktur> _callbackInput;
        private readonly Action<Datenstruktur> _callbackOutput;

        private readonly Datenstruktur _datenstruktur;

        private string _spsStatus;
        private bool _spsError;
        private bool _taskRunning = true;

        private string _plcModus = "Manual";

        public Manual(Datenstruktur datenstruktur, Action<Datenstruktur> cbInput, Action<Datenstruktur> cbOutput)
        {
            _datenstruktur = datenstruktur;

            _callbackInput = cbInput;
            _callbackOutput = cbOutput;

            _datenstruktur.VersionInputSps = Encoding.ASCII.GetBytes("KeineVersionsinfo");
            System.Threading.Tasks.Task.Run(SPS_Pingen_Task);
        }

        public void SPS_Pingen_Task()
        {
            while (_taskRunning)
            {
                _spsStatus = "Manueller Modus aktiv";
                _spsError = false;

                _callbackInput(_datenstruktur);
                _callbackOutput(_datenstruktur);

                Thread.Sleep(10);
            }
        }

        public string GetSpsStatus() => _spsStatus;
        public bool GetSpsError() => _spsError;
        public string GetVersion() => "42";
        public string GetPlcModus() => _plcModus;

        public void SetPlcModus(string modus) => _plcModus = modus;
        public void SetTaskRunning(bool active) => _taskRunning = active;
        public void SetManualModeReferenz(Datenstruktur manualModeDatenstruktur) { /*nicht erforderlich*/}
        public void SetZyklusZeitKommunikation(int zeit) { /*nicht erforderlich*/}


    }
}