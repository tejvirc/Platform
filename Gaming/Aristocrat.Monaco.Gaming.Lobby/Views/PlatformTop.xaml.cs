namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using Microsoft.Extensions.DependencyInjection;
    using UI.Common;
    using ViewModels;

    /// <summary>
    /// Interaction logic for PlatformTop.xaml
    /// </summary>
    public partial class PlatformTop
    {
        public PlatformTop()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<PlatformTopViewModel>();
        }
    }
}
