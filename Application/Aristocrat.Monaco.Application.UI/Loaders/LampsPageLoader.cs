namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using System;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class LampsPageLoader : OperatorMenuPageLoader
    {
        private readonly ICabinetDetectionService _cabinetService;
        private readonly IPropertiesManager _properties;

        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LampsLabel);

        /// <summary>
        ///     Creates an instance of <see cref="LampsPageLoader"/>
        /// </summary>
        public LampsPageLoader()
            : this(
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Creates an instance of <see cref="LampsPageLoader"/>
        /// </summary>
        /// <param name="cabinetService">An instance of <see cref="ICabinetDetectionService"/></param>
        /// <param name="properties">An instance of <see cref="IPropertiesManager"/></param>
        public LampsPageLoader(ICabinetDetectionService cabinetService, IPropertiesManager properties)
        {
            _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new LampsPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new LampsPageViewModel(_cabinetService, _properties, IsWizardPage);
        }

        /// <inheritdoc />
        public override bool GetVisible()
        {
            var towerLightManager = ServiceManager.GetInstance().TryGetService<ITowerLightManager>();
            return _cabinetService.HasLamps(_properties) ||
                   !(towerLightManager?.TowerLightsDisabled ?? false) && base.GetVisible();
        }
    }
}