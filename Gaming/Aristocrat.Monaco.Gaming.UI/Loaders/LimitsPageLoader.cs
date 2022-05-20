namespace Aristocrat.Monaco.Gaming.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class LimitsPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Player).GetString(ResourceKeys.LimitPageTitleText);

        // ReSharper disable once UnusedMember.Global - used by addins
        public LimitsPageLoader() : this(false) { }

        public LimitsPageLoader(bool isWizardPage)
        {
            IsWizardPage = isWizardPage;
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new LimitsPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new LimitsPageViewModel(IsWizardPage);
        }
    }
}
