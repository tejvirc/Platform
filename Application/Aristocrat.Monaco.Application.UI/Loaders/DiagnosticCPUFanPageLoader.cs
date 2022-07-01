namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.OperatorMenu;
    using Hardware.Services;
    using ViewModels;
    using Views;

    public class DiagnosticCpuFanPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => "CPU Metrics";

        protected override IOperatorMenuPage CreatePage()
        {
            return new DiagnosticCpuFanPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            var fanService = new FanService();
            fanService.Initialize();
            return new DiagnosticCpuFanPageViewModel(fanService);
        }
    }
}