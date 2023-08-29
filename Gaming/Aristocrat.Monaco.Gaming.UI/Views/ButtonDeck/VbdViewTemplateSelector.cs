namespace Aristocrat.Monaco.Gaming.UI.Views.ButtonDeck
{
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using Contracts.ButtonDeck;
    using log4net;
    using ViewModels;

    /// <summary>
    ///     This class is used by vbdview to pick the right vbd template
    /// </summary>
    public class VbdViewTemplateSelector : DataTemplateSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is LobbyViewModel))
            {
                return null;
            }

            var element = container as FrameworkElement;

            var templateKey = $"Vbd{VirtualButtonDeckHelper.GetVbdPrefixNameByCabinetType()}Template";

            Logger.Debug($"Vbd View Template Selector: {templateKey}");

            return element?.TryFindResource(templateKey) as DataTemplate;
        }
    }
}