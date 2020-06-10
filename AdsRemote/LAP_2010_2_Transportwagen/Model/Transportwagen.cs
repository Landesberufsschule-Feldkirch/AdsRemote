using System.Threading;

namespace LAP_2010_2_Transportwagen.Model
{
    public class Transportwagen
    {
        public double Position { get; set; }
        public double AbstandRadRechts { get; set; }
        public bool Fuellen { get; internal set; }

        private const double geschwindigkeit = 1;
        private const double randLinks = 30;
        private const double randRechts = 430;
        private const double maximaleFuellzeit = 700; // Zykluszeit ist 10ms --> 7"
        private double laufzeitFuellen = 0;
        private readonly MainWindow mainWindow;


        public Transportwagen(MainWindow mw)
        {
            mainWindow = mw;

            Position = 30;
            AbstandRadRechts = 100;


            mainWindow.Cx9020.Kommunikation.F1.RemoteValue = true;
            mainWindow.Cx9020.Kommunikation.S2.RemoteValue = true;

            System.Threading.Tasks.Task.Run(() => TransportwagtenTask());
        }

        private void TransportwagtenTask()
        {
            while (true)
            {
                if (mainWindow.Cx9020.Kommunikation.B1) laufzeitFuellen = 0;
                if (mainWindow.Cx9020.Kommunikation.B2 && laufzeitFuellen <= maximaleFuellzeit) laufzeitFuellen++;
                if (laufzeitFuellen > 1 && laufzeitFuellen < maximaleFuellzeit) Fuellen = true; else Fuellen = false;

                if (mainWindow.Cx9020.Kommunikation.Q1) Position -= geschwindigkeit;
                if (mainWindow.Cx9020.Kommunikation.Q2) Position += geschwindigkeit;

                if (Position < randLinks) Position = randLinks;
                if (Position > randRechts) Position = randRechts;

                mainWindow.Cx9020.Kommunikation.B1.RemoteValue = Position < (randLinks + 2);
                mainWindow.Cx9020.Kommunikation.B2.RemoteValue = Position > (randRechts - 2);

                Thread.Sleep(10);
            }
        }

        internal void SetF1()
        {
            if (mainWindow.Cx9020.Kommunikation.F1)
                mainWindow.Cx9020.Kommunikation.F1.RemoteValue = false;
            else
                mainWindow.Cx9020.Kommunikation.F1.RemoteValue = true;
        }

        internal void SetS2()
        {
            if (mainWindow.Cx9020.Kommunikation.S2)
                mainWindow.Cx9020.Kommunikation.S2.RemoteValue = false;
            else
                mainWindow.Cx9020.Kommunikation.S2.RemoteValue = true;
        }
    }
}