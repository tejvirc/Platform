namespace Aristocrat.Monaco.Gaming.Presentation.Views
{
    using Presentation.ViewModels;
    using UI.ViewModels;
    using Aristocrat.MVVM.ViewModel;

    using System.Windows;
    using Aristocrat.Monaco.Gaming.UI.Views.Overlay;
    using Prism.Regions;
    using Prism.Common;
    using System.ComponentModel;
    using Aristocrat.Monaco.Gaming.Presentation.Events;

    /// <summary>
    /// Interaction logic for AttractMainView.xaml
    /// </summary>
    public partial class AttractMainView
    {
        public AttractMainView()
        {
            InitializeComponent();
            ViewModel = new AttractMainViewModel();
        }

        public AttractMainViewModel ViewModel
        {
            get;

            set;
        }

        private void GameAttract_OnVideoCompleted(object sender, RoutedEventArgs e)
        {
            ViewModel.OnGameAttractVideoCompleted();
        }
    }
}
