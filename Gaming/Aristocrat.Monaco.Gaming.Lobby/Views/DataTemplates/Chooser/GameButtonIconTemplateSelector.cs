namespace Aristocrat.Monaco.Gaming.Lobby.Views.DataTemplates.Chooser;

using System.Windows;
using System.Windows.Controls;
using Models;

public class GameButtonIconTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not IImageIcon icon)
        {
            return null;
        }

        var element = container as FrameworkElement;

        var key = icon.IsBinkImage
            ? DataTemplateKeys.Chooser.GameButtonIconVideo
            : DataTemplateKeys.Chooser.GameButtonIconImage;

        return element?.TryFindResource(key) as DataTemplate;
    }
}
