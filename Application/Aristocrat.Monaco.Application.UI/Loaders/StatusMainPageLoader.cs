﻿namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class StatusMainPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StatusLabel);

        protected override IOperatorMenuPage CreatePage()
        {
            return new StatusMainPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new StatusMainPageViewModel(PageName);
        }
    }
}
