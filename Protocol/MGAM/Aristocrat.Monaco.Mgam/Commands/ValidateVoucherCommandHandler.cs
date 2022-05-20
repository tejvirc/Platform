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

    /// <summary>
    ///     Handles the <see cref="ValidateVoucher"/> command.
    /// </summary>
    public class ValidateVoucherCommandHandler : CommandHandlerBase, ICommandHandler<ValidateVoucher>
    {
        private const int RetryDelay = 1;
        private const int RetryCount = 1;

        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidateVoucherCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public ValidateVoucherCommandHandler(
            ILogger<ValidateVoucherCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IEventBus bus) : base(bus)
        {
            _logger = logger;
            _egm = egm;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task Handle(ValidateVoucher command)
        {
            await ValidateVoucher(command);
        }

        private async Task ValidateVoucher(ValidateVoucher command)
        {
            _logger.LogInfo($"ValidateVoucher {command.Barcode} attribute");

            var message = _mapper.Map<Aristocrat.Mgam.Client.Messaging.ValidateVoucher>(command);

            var response = await ValidateVoucherRetry(message);
            
            if (response.ResponseCode != ServerResponseCode.Ok)
            {
                throw new ServerResponseException(response.ResponseCode);
            }

            command.VoucherCashValue = response.VoucherCashValue;
            command.VoucherCouponValue = response.VoucherCouponValue;
        }

        private async Task<ValidateVoucherResponse> ValidateVoucherRetry(Aristocrat.Mgam.Client.Messaging.ValidateVoucher message)
        {
            var voucher = _egm.GetService<IVoucher>();

            var policy = Policy<MessageResult<ValidateVoucherResponse>>
                .HandleResult(r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    RetryCount,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to validate voucher {message.VoucherBarcode}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await voucher.ValidateVoucher(message));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success ? new ValidateVoucherResponse { ResponseCode = ServerResponseCode.ServerError } : result.Response;
        }
    }
}
