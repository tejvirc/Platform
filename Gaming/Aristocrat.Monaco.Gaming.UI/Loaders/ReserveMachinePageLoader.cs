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

    public class ReserveMachinePageLoader : OperatorMenuPageLoader
    {
        private readonly IPropertiesManager _propertiesManager;

        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReserveMachineTitle);

        public ReserveMachinePageLoader()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public ReserveMachinePageLoader(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new ReserveMachinePage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new ReserveMachineViewModel();
        }

        public override bool GetVisible()
        {
            return base.GetVisible() && _propertiesManager.GetValue(ApplicationConstants.ReserveServiceAllowed, true);
        }
    }
}
