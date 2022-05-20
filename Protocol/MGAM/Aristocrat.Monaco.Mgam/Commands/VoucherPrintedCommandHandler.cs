namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.CreditServices;
    using AutoMapper;
    using Kernel;
    using Polly;
    using Services.Lockup;

    /// <summary>
    ///     Handles the <see cref="VoucherPrinted" /> command.
    /// </summary>
    public class VoucherPrintedCommandHandler : CommandHandlerBase, ICommandHandler<VoucherPrinted>
    {
        private const int RetryDelay = 1;

        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;
        private readonly ILockup _lockup;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherPrintedCommandHandler" /> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger" />.</param>
        /// <param name="egm"><see cref="IEgm" />.</param>
        /// <param name="mapper"><see cref="IMapper" />.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        /// <param name="lockup"><see cref="ILockup"/>.</param>
        public VoucherPrintedCommandHandler(
            ILogger<VoucherPrintedCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IEventBus bus,
            ILockup lockup) : base(bus)
        {
            _logger = logger;
            _egm = egm;
            _mapper = mapper;
            _lockup = lockup;
        }

        /// <inheritdoc />
        public async Task Handle(VoucherPrinted command)
        {
            await VoucherPrinted(command);
        }

        private async Task VoucherPrinted(VoucherPrinted command)
        {
            _logger.LogInfo($"VoucherPrinted {command.Barcode} attribute");

            var message = _mapper.Map<Aristocrat.Mgam.Client.Messaging.VoucherPrinted>(command);

            var response = await VoucherPrintedRetry(message);

            if (response.ResponseCode != ServerResponseCode.Ok)
            {
                if (response.ResponseCode == ServerResponseCode.BarcodeNotFound ||
                    response.ResponseCode == ServerResponseCode.InvalidBarcode ||
                    response.ResponseCode == ServerResponseCode.InvalidBarcodeLength)
                {
                    _lockup.LockupForEmployeeCard($"VoucherPrinted failed ResponseCode:{response.ResponseCode}");
                }


                throw new ServerResponseException(response.ResponseCode);
            }
        }

        private async Task<VoucherPrintedResponse> VoucherPrintedRetry(
            Aristocrat.Mgam.Client.Messaging.VoucherPrinted message)
        {
            var voucher = _egm.GetService<IVoucher>();

            var policy = Policy<MessageResult<VoucherPrintedResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to validate voucher {message.VoucherBarcode}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await voucher.VoucherPrinted(message));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success ? new VoucherPrintedResponse { ResponseCode = ServerResponseCode.ServerError } : result.Response;
        }
    }
}
