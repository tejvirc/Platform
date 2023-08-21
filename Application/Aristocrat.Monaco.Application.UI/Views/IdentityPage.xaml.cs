namespace Aristocrat.Monaco.Application.UI.Views
{
    using System.Text.RegularExpressions;
    using System.Windows.Input;

    public sealed partial class IdentityPage
    {
        public IdentityPage()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}