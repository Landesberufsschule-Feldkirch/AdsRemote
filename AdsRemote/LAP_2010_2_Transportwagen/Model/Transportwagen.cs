using System.Threading;

namespace LAP_2010_2_Transportwagen.Model
{
    public class Transportwagen
    {
        public double Position { get; set; }
        public double AbstandRadRechts { get; set; }
        public bool Fuellen { get; internal set; }

        private const double Geschwindigkeit = 1;
        private const double RandLinks = 30;
        private const double RandRechts = 430;
        private const double MaximaleFuellzeit = 700; // Zykluszeit ist 10ms --> 7"
        private double _laufzeitFuellen;
        private readonly MainWindow _mainWindow;


        public Transportwagen(MainWindow mw)
        {
            _mainWindow = mw;

            Position = 30;
            AbstandRadRechts = 100;


            _mainWindow.Cx9020.Kommunikation.F1.RemoteValue = true;
            _mainWindow.Cx9020.Kommunikation.S2.RemoteValue = true;

            System.Threading.Tasks.Task.Run(TransportwagtenTask);
        }

        private void TransportwagtenTask()
        {
            while (true)
            {
                if (_mainWindow.Cx9020.Kommunikation.B1) _laufzeitFuellen = 0;
                if (_mainWindow.Cx9020.Kommunikation.B2 && _laufzeitFuellen <= MaximaleFuellzeit) _laufzeitFuellen++;
                if (_laufzeitFuellen > 1 && _laufzeitFuellen < MaximaleFuellzeit) Fuellen = true; else Fuellen = false;

                if (_mainWindow.Cx9020.Kommunikation.Q1) Position -= Geschwindigkeit;
                if (_mainWindow.Cx9020.Kommunikation.Q2) Position += Geschwindigkeit;

                if (Position < RandLinks) Position = RandLinks;
                if (Position > RandRechts) Position = RandRechts;

                _mainWindow.Cx9020.Kommunikation.B1.RemoteValue = Position < (RandLinks + 2);
                _mainWindow.Cx9020.Kommunikation.B2.RemoteValue = Position > (RandRechts - 2);

                Thread.Sleep(10);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        internal void SetF1() => _mainWindow.Cx9020.Kommunikation.F1.RemoteValue = !_mainWindow.Cx9020.Kommunikation.F1;
        internal void SetS2() => _mainWindow.Cx9020.Kommunikation.S2.RemoteValue = !_mainWindow.Cx9020.Kommunikation.S2;
    }
}