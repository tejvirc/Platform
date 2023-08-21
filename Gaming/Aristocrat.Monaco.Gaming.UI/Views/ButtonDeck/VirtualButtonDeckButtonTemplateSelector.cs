namespace Aristocrat.Monaco.Gaming.UI.Views.ButtonDeck
{
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    ///     This class is used by TimeLimitDialog to insert the appropriate UI based on Jurisdiction configuration
    /// </summary>
    public class VirtualButtonDeckButtonTemplateSelector : DataTemplateSelector
    {
        private const string StandardTemplate = "StandardTemplate";
        private const string ServiceButtonTemplate = "ServiceButtonTemplate";

        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is LobbyViewModel viewModel))
            {
                return null;
            }

            var templateKey = viewModel.DisplayVbdServiceButton ? ServiceButtonTemplate : StandardTemplate;

            var element = container as FrameworkElement;

            // For a single game mode, there is no lobby so we don't need to display any
            // button (Service/Cashout) on VBD
            if (element != null && viewModel.IsSingleGameMode)
            {
                element.Visibility = Visibility.Collapsed;
            }

            return element?.TryFindResource(templateKey) as DataTemplate;
        }
    }
}
