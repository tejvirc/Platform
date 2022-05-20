namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.DropMode;
    using AutoMapper;
    using Kernel;
    using Polly;

    /// <summary>
    ///     Handles the <see cref="BillAcceptorMeterReport" /> command.
    /// </summary>
    public class BillAcceptorMeterReportCommandHandler : CommandHandlerBase, ICommandHandler<BillAcceptorMeterReport>
    {
        private const int RetryDelay = 1;

        private readonly IMeterManager _meterManager;
        private readonly IEgm _egm;
        private readonly ILogger<BillAcceptorMeterReportCommandHandler> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BillAcceptorMeterReportCommandHandler" /> class.
        /// </summary>
        public BillAcceptorMeterReportCommandHandler(
            IMeterManager meterManager,
            ILogger<BillAcceptorMeterReportCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IEventBus bus) : base(bus)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public async Task Handle(BillAcceptorMeterReport command)
        {
            command.CashBox = GetMeterValue(AccountingMeters.CurrencyInAmount, true);
            command.CashBoxOnes = GetMeterValue(AccountingMeters.BillCount1s);
            command.CashBoxTwos = GetMeterValue(AccountingMeters.BillAmount2s);
            command.CashBoxFives = GetMeterValue(AccountingMeters.BillCount5s);
            command.CashBoxTens = GetMeterValue(AccountingMeters.BillCount10s);
            command.CashBoxTwenties = GetMeterValue(AccountingMeters.BillCount20s);
            command.CashBoxFifties = GetMeterValue(AccountingMeters.BillCount50s);
            command.CashBoxHundreds = GetMeterValue(AccountingMeters.BillCount100s);
            command.CashBoxVouchers = GetMeterValue(AccountingMeters.VoucherInCashableCount) +
                                      GetMeterValue(AccountingMeters.VoucherInCashablePromoCount) +
                                      GetMeterValue(AccountingMeters.VoucherInNonCashableCount);
            command.CashBoxVouchersTotal = GetMeterValue(AccountingMeters.VoucherInCashableAmount, true) +
                                           GetMeterValue(AccountingMeters.VoucherInCashablePromoAmount, true) +
                                           GetMeterValue(AccountingMeters.VoucherInNonCashableAmount, true);
            command.CashBox += command.CashBoxVouchersTotal;

            await MeterReported(command);
        }

        private async Task MeterReported(BillAcceptorMeterReport command)
        {
            _logger.LogInfo($"MeterReported ${command.CashBox} cash");

            var message = _mapper.Map<Aristocrat.Mgam.Client.Messaging.BillAcceptorMeterReport>(command);

            var response = await MeterReportedRetry(message);

            if (response.ResponseCode != ServerResponseCode.Ok)
            {
                throw new ServerResponseException(response.ResponseCode);
            }
        }

        private async Task<BillAcceptorMeterReportResponse> MeterReportedRetry(
            Aristocrat.Mgam.Client.Messaging.BillAcceptorMeterReport message)
        {
            var meters = _egm.GetService<IBillAcceptorMeter>();

            var policy = Policy<MessageResult<BillAcceptorMeterReportResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) BillAcceptorMeterReport.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await meters.ReportMeters(message));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success ? new BillAcceptorMeterReportResponse { ResponseCode = ServerResponseCode.ServerError } : result.Response;
        }

        private int GetMeterValue(string meterName, bool convertMillicentsToCents = false)
        {
            var result = 0L;

            if (_meterManager.IsMeterProvided(meterName))
            {
                result = _meterManager.GetMeter(meterName).Period;

                if (convertMillicentsToCents)
                {
                    result = result.MillicentsToCents();
                }
            }

            return (int)result;
        }
    }
}