namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using ConfigWizard;
    using Contracts;
    using System;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class CompletionPageViewModel : ConfigWizardViewModelBase
    {
        public CompletionPageViewModel()
            : base(true)
        {
            ShowGameSetupMessage = (bool)PropertiesManager.GetProperty(
                ApplicationConstants.ConfigWizardCompletionPageShowGameSetupMessage,
                false);
        }

        public string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CompleteTitle);

        public bool ShowGameSetupMessage { get; }

        protected override void SaveChanges()
        {
        }
    }
}
