namespace Aristocrat.Monaco.Gaming.UI.Views.ButtonDeck
{
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using log4net;
    using ViewModels;

    /// <summary>
    ///     This class is used by TimeLimitDialog to insert the appropriate UI based on Jurisdiction configuration
    /// </summary>
    public class VirtualButtonDeckButtonTemplateSelector : DataTemplateSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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

            Logger.Debug($"Vbd Button Template Selector: {templateKey}");

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
