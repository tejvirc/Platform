namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using ConfigWizard;
    using Contracts;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class LegalCopyrightPageViewModel : ConfigWizardViewModelBase
    {
        public string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LegalCopyrightScreenTitle);

        public string PageContent =>
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LegalCopyrightScreenContent);

        public LegalCopyrightPageViewModel()
            : base(true)
        {
        }

        protected override void SetupNavigation()
        {
            WizardNavigator.IsBackButtonVisible = false;
            WizardNavigator.CanNavigateForward = true;
        }

        protected override void SaveChanges()
        {
            PropertiesManager.SetProperty(ApplicationConstants.LegalCopyrightAcceptedKey, true);
        }
    }
}