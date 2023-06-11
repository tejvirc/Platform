namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell
    {
        public Shell()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<ShellViewModel>();
        }
    }
}
