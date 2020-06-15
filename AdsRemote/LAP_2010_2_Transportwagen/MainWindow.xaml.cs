using System.Windows;

namespace LAP_2010_2_Transportwagen
{
    public partial class MainWindow : Window
    {
        public bool DebugWindowAktiv { get; set; }
        public SetManual.SetManual SetManualWindow { get; set; }
        public Cx9020.Verbindung Cx9020 { get; set; }


        private readonly ViewModel.ViewModel viewModel;


        public MainWindow()
        {
            Cx9020 = new Cx9020.Verbindung();

            viewModel = new ViewModel.ViewModel(this);

            InitializeComponent();
            DataContext = viewModel;


            if (System.Diagnostics.Debugger.IsAttached) btnDebugWindow.Visibility = System.Windows.Visibility.Visible;
            else btnDebugWindow.Visibility = System.Windows.Visibility.Hidden;
        }

        private void DebugWindowOeffnen(object sender, RoutedEventArgs e)
        {
            DebugWindowAktiv = true;
            SetManualWindow = new SetManual.SetManual(viewModel);
            SetManualWindow.Show();
        }
    }
}