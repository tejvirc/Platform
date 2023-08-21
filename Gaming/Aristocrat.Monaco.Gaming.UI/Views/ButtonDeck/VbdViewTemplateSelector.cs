namespace Aristocrat.Monaco.Gaming.UI.Views.ButtonDeck
{
    using System.Windows;
    using System.Windows.Controls;
    using Contracts.ButtonDeck;
    using ViewModels;

    /// <summary>
    ///     This class is used by vbdview to pick the right vbd template
    /// </summary>
    public class VbdViewTemplateSelector : DataTemplateSelector
    {
        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is LobbyViewModel))
            {
                return null;
            }

            var element = container as FrameworkElement;

            var templateKey = $"Vbd{VirtualButtonDeckHelper.GetVbdPrefixNameByCabinetType()}Template";

            return element?.TryFindResource(templateKey) as DataTemplate;
        }
    }
}