namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.CreditServices;
    using AutoMapper;
    using Kernel;
    using Newtonsoft.Json;
    using Polly;
    using Services.CreditValidators;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handles the <see cref="Commands.CreditVoucher" /> command.
    /// </summary>
    public class CreditVoucherCommandHandler : TransactionalCommandHandlerBase, ICommandHandler<CreditVoucher>
    {
        private const int RetryDelay = 1;

        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreditVoucherCommandHandler" /> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger" />.</param>
        /// <param name="egm"><see cref="IEgm" />.</param>
        /// <param name="mapper"><see cref="IMapper" />.</param>
        /// <param name="idProvider"><see cref="IIdProvider" />.</param>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        /// <param name="transactionRetry"><see cref="ITransactionRetryHandler"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public CreditVoucherCommandHandler(
            ILogger<CreditVoucherCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IIdProvider idProvider,
            ILockup lockup,
            INotificationLift notificationLift,
            ITransactionRetryHandler transactionRetry,
            IEventBus bus)
            : base(idProvider, transactionRetry, bus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _lockup = lockup ?? throw new ArgumentNullException((nameof(lockup)));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));

            transactionRetry.RegisterCommand(typeof(Aristocrat.Mgam.Client.Messaging.CreditVoucher), CreditVoucherRetry);
        }

        /// <inheritdoc />
        public async Task Handle(CreditVoucher command)
        {
            await SendRequest(_mapper.Map<Aristocrat.Mgam.Client.Messaging.CreditVoucher>(command));
        }

        /// <inheritdoc />
        protected override async Task Handle(Request arg, CancellationToken cancellationToken = default(CancellationToken), long transactionId = 0)
        {
            var message = arg as Aristocrat.Mgam.Client.Messaging.CreditVoucher;

            _logger.LogInfo(
                $"CreditVoucher Barcode:{message?.VoucherBarcode} LocalTransactionId:{message?.LocalTransactionId}");

            var response = await CreditVoucher(message);

            if (response.ResponseCode != ServerResponseCode.Ok)
            {
                _lockup.LockupForEmployeeCard(NotificationCode.LockedCreditVoucherFailed.ToString());

                await _notificationLift.Notify(NotificationCode.LockedCreditVoucherFailed, message?.VoucherBarcode);

                throw new ServerResponseException(response.ResponseCode);
            }
        }

        private async Task<IResponse> CreditVoucherRetry(object message)
        {
            if (message is Aristocrat.Mgam.Client.Messaging.CreditVoucher command)
            {
                return await CreditVoucher(command);
            }

            var creditVoucher = JsonConvert.DeserializeObject<Aristocrat.Mgam.Client.Messaging.CreditVoucher>(message.ToString());
            return await CreditVoucher(creditVoucher);
        }

        private async Task<CreditResponse> CreditVoucher(Aristocrat.Mgam.Client.Messaging.CreditVoucher message)
        {
            var voucher = _egm.GetService<IVoucher>();

            var policy = Policy<MessageResult<CreditResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to credit cash.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await voucher.CreditVoucher(message));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success ? new CreditResponse { ResponseCode = ServerResponseCode.ServerError } : result.Response;
        }
    }
}