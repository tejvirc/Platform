namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using Microsoft.Extensions.DependencyInjection;
    using UI.Common;
    using ViewModels;

    /// <summary>
    /// Interaction logic for PlatformTopper.xaml
    /// </summary>
    public partial class PlatformTopper
    {
        public PlatformTopper()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<PlatformTopperViewModel>();
        }
    }
}
