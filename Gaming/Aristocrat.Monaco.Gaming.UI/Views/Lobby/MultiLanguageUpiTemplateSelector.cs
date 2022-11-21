namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
{
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    ///     This class is used by MultiLanguageUpi to insert the appropriate UI based on Jurisdiction configuration
    /// </summary>
    public class MultiLanguageUpiTemplateSelector : DataTemplateSelector
    {
        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is LobbyViewModel viewModel))
            {
                return null;
            }

            var element = container as FrameworkElement;
            return element?.TryFindResource(string.IsNullOrEmpty(viewModel.Config.UpiTemplate) ? "MultiLanguageUpiTemplate" : viewModel.Config.UpiTemplate) as DataTemplate;
        }
    }
}