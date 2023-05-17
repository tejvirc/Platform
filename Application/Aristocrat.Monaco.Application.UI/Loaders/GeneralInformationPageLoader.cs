namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Application.UI.ViewModels;
    using Aristocrat.Monaco.Application.UI.Views;
    using Aristocrat.Monaco.Localization.Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GeneralInformationPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GeneralInformationPageTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new GeneralInformationPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new GeneralInformationPageViewModel();
        }
    }
}
