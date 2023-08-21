namespace Aristocrat.Monaco.Gaming.Presentation.Views.DataTemplates.Chooser;

using System.Windows;
using System.Windows.Controls;
using UI.Models;

public class GameButtonIconTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        // if (item is not IImageIcon icon)
        if (item is not DependencyObject icon)
        {
            return null;
        }

        var element = container as FrameworkElement;

        // var key = icon.IsBinkImage
        var key = icon == null
            ? DataTemplateKeys.Chooser.GameButtonIconVideo
            : DataTemplateKeys.Chooser.GameButtonIconImage;

        return element?.TryFindResource(key) as DataTemplate;
    }
}
