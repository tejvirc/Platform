namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using Aristocrat.Monaco.Gaming.UI.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    public class CashOutButtonTemplateSelector : DataTemplateSelector
    {
        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is LobbyViewModel viewModel))
            {
                return null;
            }

            var element = container as FrameworkElement;


            return element?.TryFindResource(viewModel.Config.CashoutResetHandCountWarningTemplate) as DataTemplate;
        }
    }
}
