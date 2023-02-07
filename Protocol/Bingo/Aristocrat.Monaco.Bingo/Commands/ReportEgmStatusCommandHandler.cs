namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Metering;
    using Aristocrat.Bingo.Client.Messages;
    using Gaming.Contracts;
    using Kernel;
    using log4net;
    using Services.Reporting;

    public class ReportEgmStatusCommandHandler : ICommandHandler<ReportEgmStatusCommand>
    {
        private readonly IStatusReportingService _statusResponseService;
        private readonly IMeterManager _meterManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IEgmStatusService _egmStatusService;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public ReportEgmStatusCommandHandler(
            IStatusReportingService statusResponseService,
            IMeterManager meterManager,
            IPropertiesManager propertiesManager,
            IEgmStatusService egmStatusService)
        {
            _statusResponseService =
                statusResponseService ?? throw new ArgumentNullException(nameof(statusResponseService));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _egmStatusService = egmStatusService ?? throw new ArgumentNullException(nameof(egmStatusService));
        }

        public async Task Handle(ReportEgmStatusCommand message, CancellationToken token = default)
        {
            try
            {
                var cashOutMeter = _meterManager.GetMeter(ApplicationMeters.TotalOut);
                var cashInMeter = _meterManager.GetMeter(ApplicationMeters.TotalIn);
                var cashWonMeter = _meterManager.GetMeter(GamingMeters.TotalPaidAmt);
                var cashPlayedMeter = _meterManager.GetMeter(GamingMeters.WageredAmount);

                var serialNumber = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
                var cashPlayedMeterValue = cashPlayedMeter.GetValue(MeterTimeframe.Lifetime).MillicentsToCents();
                var cashWonMeterValue = cashWonMeter.GetValue(MeterTimeframe.Lifetime).MillicentsToCents();
                var cashInMeterValue = cashInMeter.GetValue(MeterTimeframe.Lifetime).MillicentsToCents();
                var cashOutMeterValue = cashOutMeter.GetValue(MeterTimeframe.Lifetime).MillicentsToCents();
                var egmStatusFlags = (int)_egmStatusService.GetCurrentEgmStatus();
                var statusMessage = new StatusMessage(
                    serialNumber,
                    cashPlayedMeterValue,
                    cashWonMeterValue,
                    cashInMeterValue,
                    cashOutMeterValue,
                    egmStatusFlags);

                await _statusResponseService.ReportStatus(statusMessage, token);
            }
            catch (Exception ex)
            {
                Logger.Error($"Send StatusResponse failed : {ex.Message}");
            }
        }
    }
}