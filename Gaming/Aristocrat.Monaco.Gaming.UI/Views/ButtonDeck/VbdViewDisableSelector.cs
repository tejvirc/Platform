namespace Aristocrat.Monaco.Gaming.UI.Views.ButtonDeck
{
    using System.Windows;
    using System.Windows.Controls;
    using Contracts.ButtonDeck;
    using ViewModels;

    /// <summary>
    ///     This class is used by vbd view to pick the right disable overlay template
    /// </summary>
    public class VbdViewDisableSelector : DataTemplateSelector
    {
        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is LobbyViewModel))
            {
                return null;
            }

            var templateKey = $"Vbd{VirtualButtonDeckHelper.GetVbdPrefixNameByCabinetType()}DisableTemplate";
            var element = container as FrameworkElement;

            return element?.TryFindResource(templateKey) as DataTemplate;
        }
    }
}