namespace Aristocrat.Monaco.Gaming.UI.Loaders
{
    using System;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Kernel;
    using Localization.Properties;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class BarkeeperConfigurationViewLoader : OperatorMenuPageLoader
    {
        private readonly IPropertiesManager _propertiesManager;

        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Barkeeper);

        public BarkeeperConfigurationViewLoader()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public BarkeeperConfigurationViewLoader(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        public override bool GetVisible()
        {
            return base.GetVisible() && _propertiesManager.GetValue(ApplicationConstants.BarkeeperEnabled, false);
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new BarkeeperConfigurationView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new BarkeeperConfigurationViewModel();
        }
    }
}
