namespace Aristocrat.Monaco.Gaming.UI.Views.ButtonDeck
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using ViewModels.ButtonDeck;

    /// <summary>
    ///     Interaction logic for ButtonDeckSimulatorView.xaml
    /// </summary>
    public partial class ButtonDeckSimulatorView
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ButtonDeckSimulatorView" /> class.
        /// </summary>
        public ButtonDeckSimulatorView()
        {
            InitializeComponent();

            ViewModel = new ButtonDeckSimulatorViewModel();
        }

        public ButtonDeckSimulatorViewModel ViewModel
        {
            get => DataContext as ButtonDeckSimulatorViewModel;
            set => DataContext = value;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button ctrl)
            {
                e.Handled = ViewModel.OnMouseDown(ctrl.Name);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button ctrl)
            {
                e.Handled = ViewModel.OnMouseUp(ctrl.Name);
            }
        }
    }
}