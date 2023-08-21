namespace Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu
{
    using System.Windows;
    using ViewModels.OperatorMenu;

    /// <summary>
    ///     Interaction logic for AttractPreviewView.xaml
    /// </summary>
    public partial class AttractPreviewView
    {
        public AttractPreviewView()
        {
            InitializeComponent();
        }

        private void OnTopGameAttractVideoCompleted(object sender, RoutedEventArgs e)
        {
            if (DataContext is AttractPreviewViewModel viewModel)
            {
                viewModel.OnTopGameAttractCompleteHandler(sender, e);
            }
        }

        private void OnBottomGameAttractVideoCompleted(object sender, RoutedEventArgs e)
        {
            if (DataContext is AttractPreviewViewModel viewModel)
            {
                viewModel.OnBottomGameAttractCompleteHandler(sender, e);
            }
        }

        private void OnTopperGameAttractVideoCompleted(object sender, RoutedEventArgs e)
        {
            if (DataContext is AttractPreviewViewModel viewModel)
            {
                viewModel.OnTopperGameAttractCompleteHandler(sender, e);
            }
        }
    }
}