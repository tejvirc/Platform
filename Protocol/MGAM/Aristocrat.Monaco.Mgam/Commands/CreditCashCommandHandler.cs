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
    ///     Handles the <see cref="Commands.CreditCash" /> command.
    /// </summary>
    public class CreditCashCommandHandler : TransactionalCommandHandlerBase, ICommandHandler<CreditCash>
    {
        private const int RetryDelay = 1;

        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreditCashCommandHandler" /> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger" />.</param>
        /// <param name="egm"><see cref="IEgm" />.</param>
        /// <param name="mapper"><see cref="IMapper" />.</param>
        /// <param name="idProvider"><see cref="IIdProvider" />.</param>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        /// <param name="transactionRetry"><see cref="ITransactionRetryHandler"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public CreditCashCommandHandler(
            ILogger<CreditCashCommandHandler> logger,
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
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));

            transactionRetry.RegisterCommand(typeof(Aristocrat.Mgam.Client.Messaging.CreditCash), CreditCashRetry);
        }

        /// <inheritdoc />
        public async Task Handle(CreditCash command)
        {
            await SendRequest(_mapper.Map<Aristocrat.Mgam.Client.Messaging.CreditCash>(command));
        }

        /// <inheritdoc />
        protected override async Task Handle(Request arg, CancellationToken cancellationToken = default(CancellationToken), long transactionId = 0)
        {
            var message = arg as Aristocrat.Mgam.Client.Messaging.CreditCash;

            _logger.LogInfo($"CreditCash Amount:{message?.Amount} LocalTransactionId:{message?.LocalTransactionId}");

            var response = await CreditCash(message);

            if (response.ResponseCode != ServerResponseCode.Ok)
            {
                _lockup.LockupForEmployeeCard(NotificationCode.LockedCreditCashFailed.ToString());

                await _notificationLift.Notify(NotificationCode.LockedCreditCashFailed, message?.Amount.ToString());

                throw new ServerResponseException(response.ResponseCode);
            }
        }

        private async Task<IResponse> CreditCashRetry(object message)
        {
            if (message is Aristocrat.Mgam.Client.Messaging.CreditCash command)
            {
                return await CreditCash(command);
            }

            var creditCash = JsonConvert.DeserializeObject<Aristocrat.Mgam.Client.Messaging.CreditCash>(message.ToString());
            return await CreditCash(creditCash);
        }

        private async Task<CreditResponse> CreditCash(Aristocrat.Mgam.Client.Messaging.CreditCash message)
        {
            var currency = _egm.GetService<ICurrency>();

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

            var result = await policy.ExecuteAsync(async () => await currency.CreditCash(message));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success ? new CreditResponse { ResponseCode = ServerResponseCode.ServerError } : result.Response;
        }
    }
}