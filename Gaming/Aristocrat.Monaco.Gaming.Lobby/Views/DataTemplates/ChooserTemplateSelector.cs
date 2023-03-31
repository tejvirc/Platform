namespace Aristocrat.Monaco.Gaming.Lobby.Views.DataTemplates;

using System.Windows;
using System.Windows.Controls;
using Services;
using ViewModels;

public class ChooserTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is not IChooserViewTarget target)
        {
            return null;
        }

        var viewType = Application.Current.GetService<IViewCollection>().GetViewType(target.ViewName);

        var template = new DataTemplate();

        var view = new FrameworkElementFactory(viewType);

        template.VisualTree = view;

        return template;
    }
}
