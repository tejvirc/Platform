namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Definition of the IdentityConfigLoader class.
    /// </summary>
    public class IdentityConfigLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IdentityScreen);

        public override List<CommsProtocol> RequiredProtocols => new List<CommsProtocol> { CommsProtocol.G2S, CommsProtocol.MGAM };

        protected override IOperatorMenuPage CreatePage()
        {
            return new IdentityPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new IdentityPageViewModel(IsWizardPage);
        }

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.ConfigWizardIdentityPageVisibility, true);
        }
    }
}