namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Metering;
    using Aristocrat.Bingo.Client.Messages;
    using Gaming.Contracts;
    using log4net;
    using Services.Reporting;

    public class StatusResponseHandler : ICommandHandler<StatusResponseMessage>
    {
        private readonly ICommandService _statusResponseService;
        private readonly IMeterManager _meterManager;
        private readonly IEgmStatusService _egmStatusService;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public StatusResponseHandler(ICommandService statusResponseService, IMeterManager meterManager, IEgmStatusService egmStatusService)
        {
            _statusResponseService = statusResponseService ?? throw new ArgumentNullException(nameof(statusResponseService));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _egmStatusService = egmStatusService ?? throw new ArgumentNullException(nameof(egmStatusService));
        }

        public async Task Handle(StatusResponseMessage message, CancellationToken token = default)
        {
            try
            {
                var cashOutMeter = _meterManager.GetMeter(ApplicationMeters.TotalOut);
                var cashInMeter = _meterManager.GetMeter(ApplicationMeters.TotalIn);
                var cashWonMeter = _meterManager.GetMeter(GamingMeters.TotalPaidAmt);
                var cashPlayedMeter = _meterManager.GetMeter(GamingMeters.WageredAmount);

                message.CashPlayedMeterValue = cashPlayedMeter.GetValue(MeterTimeframe.Session);
                message.CashWonMeterValue = cashWonMeter.GetValue(MeterTimeframe.Session);
                message.CashInMeterValue = cashInMeter.GetValue(MeterTimeframe.Session);
                message.CashOutMeterValue = cashOutMeter.GetValue(MeterTimeframe.Session);
                message.EgmStatusFlags = (int)_egmStatusService.GetCurrentEgmStatus();

                await _statusResponseService.ReportStatus(message, token);
            }
            catch (Exception ex)
            {
                Logger.Error($"Send StatusResponse failed : {ex.Message}");
            }
        }
    }
}