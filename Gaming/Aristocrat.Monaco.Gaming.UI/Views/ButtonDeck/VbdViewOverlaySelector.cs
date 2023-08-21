﻿namespace Aristocrat.Monaco.Gaming.UI.Views.ButtonDeck
{
    using System.Windows;
    using System.Windows.Controls;
    using Contracts.ButtonDeck;
    using ViewModels;

    /// <summary>
    ///     This class is used by vbd view to pick the right overlay template
    /// </summary>
    public class VbdViewOverlaySelector : DataTemplateSelector
    {
        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is LobbyViewModel))
            {
                return null;
            }

            var element = container as FrameworkElement;
            var templateKey = $"Vbd{VirtualButtonDeckHelper.GetVbdPrefixNameByCabinetType()}OverlayTemplate";

            return element?.TryFindResource(templateKey) as DataTemplate;
        }
    }
}