namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class HardwareManagerPageLoader : OperatorMenuPageLoader
    {
        private HardwareManagerPageViewModel _viewModel;

        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardwareSelectionTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new HardwareManagerPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return _viewModel ?? (_viewModel =  new HardwareManagerPageViewModel());
        }
    }
}