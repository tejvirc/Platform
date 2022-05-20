namespace Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu
{
    using System.Text.RegularExpressions;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for GamePreferencesView.xaml
    /// </summary>
    public partial class GamePreferencesView
    {
        public GamePreferencesView()
        {
            InitializeComponent();
        }

        private void PreviewShowProgramPinInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
