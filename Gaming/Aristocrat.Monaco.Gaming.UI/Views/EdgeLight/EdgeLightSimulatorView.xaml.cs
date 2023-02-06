namespace Aristocrat.Monaco.Gaming.UI.Views.EdgeLight
{
    using System.Windows;
    using System.Windows.Input;
    using ViewModels.EdgeLight;

    /// <summary>
    ///     Interaction logic for EdgeLightSimulatorView.xaml
    /// </summary>
    public partial class EdgeLightSimulatorView
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EdgeLightSimulatorView" /> class.
        /// </summary>
        public EdgeLightSimulatorView()
        {
            InitializeComponent();

            ViewModel = new EdgeLightSimulatorViewModel();
        }

        public EdgeLightSimulatorViewModel ViewModel
        {
            get => DataContext as EdgeLightSimulatorViewModel;
            set => DataContext = value;
        }

        private void Control_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_MaxExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        private void CommandBinding_MinExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
    }
}