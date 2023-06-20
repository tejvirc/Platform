namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Linq;
    using Contracts.Protocol;
    using Kernel;
    using Monaco.Common;
    using OperatorMenu;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class MultiProtocolConfigDisplayPageViewModel : OperatorMenuPageViewModelBase
    {
        private readonly CommsProtocol? _validationProtocol;
        private readonly CommsProtocol? _fundTransferProtocol;
        private readonly CommsProtocol? _progressiveProtocol;
        private readonly CommsProtocol? _centralDeterminationSystemProtocol;

        public MultiProtocolConfigDisplayPageViewModel()
        {
            var multiProtocolConfiguration = ServiceManager.GetInstance()
                .GetService<IMultiProtocolConfigurationProvider>().MultiProtocolConfiguration.ToList();

            _validationProtocol = multiProtocolConfiguration.FirstOrDefault(x => x.IsValidationHandled)?.Protocol;
            _fundTransferProtocol = multiProtocolConfiguration.FirstOrDefault(x => x.IsFundTransferHandled)?.Protocol;
            _progressiveProtocol = multiProtocolConfiguration.FirstOrDefault(x => x.IsProgressiveHandled)?.Protocol;
            _centralDeterminationSystemProtocol = multiProtocolConfiguration.FirstOrDefault(x => x.IsCentralDeterminationHandled)?.Protocol;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            EventBus.Subscribe<OperatorCultureChangedEvent>(this,
                _ =>
                {
                    RaisePropertyChanged(
                        nameof(ValidationProtocol),
                        nameof(FundTransferProtocol),
                        nameof(ProgressiveProtocol),
                        nameof(CentralDeterminationSystemProtocol));
                });
        }

        public string ValidationProtocol => _validationProtocol.HasValue
            ? EnumParser.ToName(_validationProtocol)
            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None);

        public string FundTransferProtocol => _fundTransferProtocol.HasValue
            ? EnumParser.ToName(_fundTransferProtocol)
            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None);

        public string ProgressiveProtocol => _progressiveProtocol.HasValue
            ? EnumParser.ToName(_progressiveProtocol)
            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None);

        public string CentralDeterminationSystemProtocol => _centralDeterminationSystemProtocol.HasValue
            ? EnumParser.ToName(_centralDeterminationSystemProtocol)
            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None);
    }
}