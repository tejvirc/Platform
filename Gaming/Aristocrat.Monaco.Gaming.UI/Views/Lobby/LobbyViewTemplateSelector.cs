﻿namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
{
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
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
        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is not LobbyViewModel lobby)
            {
                return null;
            }

            var element = container as FrameworkElement;

            var dataTemplateKey = lobby.Config.MidKnightLobbyEnabled
                ? DefaultLobbyMidKnightTemplate
                : Screen.PrimaryScreen.WorkingArea.Width < Screen.PrimaryScreen.WorkingArea.Height
                    ? DefaultLobbyPortraitTemplate
                    : DefaultLobbyLandscapeTemplate;

            Logger.Debug($"Lobby Template Selector: {dataTemplateKey}");

            return element?.TryFindResource(dataTemplateKey) as DataTemplate;
        }
    }
}
