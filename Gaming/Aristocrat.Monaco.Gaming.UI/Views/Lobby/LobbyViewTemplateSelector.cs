namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
{
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using Aristocrat.Monaco.Application.UI.Views;
    using log4net;
    using ViewModels;

    /// <summary>
    ///     This class is used by Lobby view to pick the portrait or landscape view
    /// </summary>
    public class LobbyViewTemplateSelector : DataTemplateSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string DefaultLobbyPortraitTemplate = "LobbyViewPortraitTemplate";
        private const string DefaultLobbyLandscapeTemplate = "LobbyViewLandscapeTemplate";
        private const string DefaultLobbyMidKnightTemplate = "LobbyViewMidKnightTemplate";

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is not LobbyViewModel lobby)
            {
                return null;
            }

            var element = container as FrameworkElement;

            var dataTemplateKey = lobby.Config.MidKnightLobbyEnabled
                ? DefaultLobbyMidKnightTemplate
                : WindowToScreenMapper.IsPortrait()
                    ? DefaultLobbyPortraitTemplate
                    : DefaultLobbyLandscapeTemplate;

            Logger.Debug($"Lobby Template Selector: {dataTemplateKey}");

            return element?.TryFindResource(dataTemplateKey) as DataTemplate;
        }
    }
}
