namespace _TestProjekt
{
    public partial class MainWindow
    {
        private readonly ViewModel.ViewModel _viewModel;

        private readonly Kommunikation.Cx9020 _cx9020;
        public MainWindow()
        {
            _cx9020 = new Kommunikation.Cx9020();
            _viewModel = new ViewModel.ViewModel();


            InitializeComponent();
            DataContext = _viewModel;

        }
    }
}