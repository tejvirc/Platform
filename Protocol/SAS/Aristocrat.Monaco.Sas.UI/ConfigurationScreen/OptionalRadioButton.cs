namespace Aristocrat.Monaco.Sas.UI.ConfigurationScreen
{
    using System.Windows.Controls;

    /// <summary>
    /// Optional RadioButton to allow uncheck.
    /// </summary>
    public class OptionalRadioButton : RadioButton
    {
        /// <summary>
        /// OnClick
        /// </summary>
        protected override void OnClick()
        {
            var selected = IsChecked ?? false;
            base.OnClick();

            if (selected)
            {
                IsChecked = false;
            }
        }
    }
}
