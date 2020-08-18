using System.Windows;

namespace LAP_2010_2_Transportwagen
{
    public partial class MainWindow
    {
        public string VersionInfo { get; set; }
        public string VersionNummer { get; set; }
        public bool DebugWindowAktiv { get; set; }
        public SetManual.SetManual SetManualWindow { get; set; }
        public Cx9020.Verbindung Cx9020 { get; set; }


        private readonly ViewModel.ViewModel _viewModel;


        public MainWindow()
        {
            const string versionText = "LAP 2010/2 Transportwagen";
            VersionNummer = "V2.0";
            VersionInfo = versionText + " - " + VersionNummer;
            
            Cx9020 = new Cx9020.Verbindung();

            _viewModel = new ViewModel.ViewModel(this);

            InitializeComponent();
            DataContext = _viewModel;

            BtnDebugWindow.Visibility = System.Diagnostics.Debugger.IsAttached ? Visibility.Visible : Visibility.Hidden;
        }

        private void DebugWindowOeffnen(object sender, RoutedEventArgs e)
        {
            DebugWindowAktiv = true;
            SetManualWindow = new SetManual.SetManual(_viewModel);
            SetManualWindow.Show();
        }
    }
}