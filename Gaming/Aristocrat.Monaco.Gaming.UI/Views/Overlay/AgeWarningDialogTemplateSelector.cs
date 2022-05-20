namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    ///     This class is used by TimeLimitDialog to insert the appropriate UI based on Jurisdiction configuration
    /// </summary>
    public class AgeWarningDialogTemplateSelector : DataTemplateSelector
    {
        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is LobbyViewModel viewModel))
            {
                return null;
            }

            var element = container as FrameworkElement;

            return element?.TryFindResource(viewModel.Config.AgeWarningTemplate) as DataTemplate;
        }
    }
}
