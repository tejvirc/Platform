namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using Microsoft.Extensions.DependencyInjection;
    using UI.Common;
    using ViewModels;

    /// <summary>
    /// Interaction logic for PlatformButtonDeck.xaml
    /// </summary>
    public partial class PlatformButtonDeck
    {
        public PlatformButtonDeck()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<PlatformButtonDeckViewModel>();
        }
    }
}
