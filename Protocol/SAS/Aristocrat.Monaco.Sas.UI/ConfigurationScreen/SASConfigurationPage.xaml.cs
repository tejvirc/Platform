namespace Aristocrat.Monaco.Sas.UI.ConfigurationScreen
{
    using System.Windows.Controls;
    using Application.Contracts.Localization;
    using Localization.Properties;
    using Monaco.UI.Common.Controls;

    /// <summary>
    ///     Interaction logic for SasConfigurationPage.xaml
    /// </summary>
    public partial class SasConfigurationPage
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SasConfigurationPage()
        {
            InitializeComponent();
        }

        private void SasAddressTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is AlphaNumericTextBox alphaText))
            {
                return;
            }

            if(string.IsNullOrEmpty(alphaText.Text))
            {
                alphaText.ErrorText = alphaText.CanBeEmpty ? string.Empty : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EmptyStringNotAllowErrorMessage); 
            }
            else if (!(sbyte.TryParse(alphaText.Text, out var value) && value >= 1))
            {
                alphaText.ErrorText = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidValueNotAllowErrorMessage), alphaText.Text);
            }
            else
            {
                alphaText.ErrorText = string.Empty;
            }

        }
    }
}
