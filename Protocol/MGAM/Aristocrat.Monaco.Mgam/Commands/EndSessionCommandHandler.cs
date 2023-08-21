namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Session;
    using AutoMapper;
    using Common.Data.Models;
    using Common.Data.Repositories;
    using Common;
    using Kernel;
    using Newtonsoft.Json;
    using Polly;
    using Protocol.Common.Storage.Entity;
    using Services.CreditValidators;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handles the <see cref="Commands.EndSession" /> command.
    /// </summary>
    public class EndSessionCommandHandler : TransactionalCommandHandlerBase, ICommandHandler<EndSession>
    {
        private const int RetryDelay = 1;
        private const string DefaultBalance = "$0.00";
        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICashOut _cashOut;
        private string _currentBalance;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EndSessionCommandHandler" /> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger" />.</param>
        /// <param name="egm"><see cref="IEgm" />.</param>
        /// <param name="mapper"><see cref="IMapper" />.</param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory" />.</param>
        /// <param name="idProvider"><see cref="IIdProvider" /></param>
        /// <param name="lockup">
        ///     <see cref="ILockup" />
        /// </param>
        /// <param name="notificationLift">
        ///     <see cref="INotificationLift" />
        /// </param>
        /// <param name="transactionRetry">
        ///     <see cref="ITransactionRetryHandler" />
        /// </param>
        /// <param name="bus">
        ///     <see cref="IEventBus" />
        /// </param>
        /// <param name="cashOut">
        ///     <see cref="ICashOut" />
        /// </param>
        public EndSessionCommandHandler(
            ILogger<EndSessionCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IUnitOfWorkFactory unitOfWorkFactory,
            IIdProvider idProvider,
            ILockup lockup,
            INotificationLift notificationLift,
            ITransactionRetryHandler transactionRetry,
            IEventBus bus,
            ICashOut cashOut)
            : base(idProvider, transactionRetry, bus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
            _cashOut = cashOut ?? throw new ArgumentNullException(nameof(cashOut));

            transactionRetry.RegisterCommand(typeof(Aristocrat.Mgam.Client.Messaging.EndSession), EndSessionRetry);
        }

        /// <inheritdoc />
        public async Task Handle(EndSession command)
        {
            _currentBalance = command.Balance ?? DefaultBalance;
            await SendRequest(new Aristocrat.Mgam.Client.Messaging.EndSession());
        }

        /// <inheritdoc />
        protected override async Task Handle(Request arg, CancellationToken cancellationToken = default(CancellationToken), long transactionId = 0)
        {
            var message = arg as Aristocrat.Mgam.Client.Messaging.EndSession;

            _logger.LogInfo($"EndSession LocalTransactionId:{message?.LocalTransactionId} command");

            var response = await EndSession(message);

            await HandleEndSessionResponse(response, message?.SessionId);
        }

        private async Task HandleEndSessionResponse(EndSessionResponse response, int? sessionId)
        {
            if (response.ResponseCode != ServerResponseCode.Ok)
            {
                if (response.ResponseCode.Equals(ServerResponseCode.DoNotPrintVoucher) ||
                    response.ResponseCode.Equals(ServerResponseCode.InvalidSessionId))
                {
                    RemoveSession(response, false);

                    if (_cashOut.Balance > 0)
                    {
                        await LockUp(_cashOut.Balance.MillicentsToDollars().FormattedCurrencyString());

                        _cashOut.CashOut();
                    }
                }
                else
                {
                     await LockUp(_currentBalance);
                }

                throw new ServerResponseException(response.ResponseCode);
            }

            RemoveSession(response, true);

            void RemoveSession(EndSessionResponse endSessionResponse, bool voucherOut)
            {

                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    if (voucherOut)
                    {
                        var voucher = _mapper.Map<Voucher>(endSessionResponse);
                        voucher.Validate();
                        unitOfWork.Repository<Voucher>().AddVoucher(voucher);
                    }

                    var session = unitOfWork.Repository<Session>().Queryable().SingleOrDefault();

                    if (session != null)
                    {
                        unitOfWork.Repository<Session>().Delete(session);
                    }

                    unitOfWork.SaveChanges();
                }
            }

            Task LockUp(string balance)
            {
                _lockup.LockupForEmployeeCard($"EndSession failed id:{sessionId} {balance} code:{response.ResponseCode}", SystemDisablePriority.Normal);

                return _notificationLift.Notify(
                    NotificationCode.LockedCloseSessionFailed,
                    sessionId?.ToString());
            }
        }

        private async Task<IResponse> EndSessionRetry(object message)
        {
            EndSessionResponse response;
            if (message is Aristocrat.Mgam.Client.Messaging.EndSession endSession)
            {
                response = await EndSession(endSession);
            }
            else
            {
                endSession = JsonConvert.DeserializeObject<Aristocrat.Mgam.Client.Messaging.EndSession>(message.ToString());
                response = await EndSession(endSession);
            }

            await HandleEndSessionResponse(response, endSession?.SessionId);

            return response;
        }

        private async Task<EndSessionResponse> EndSession(Aristocrat.Mgam.Client.Messaging.EndSession message)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var hostSession = unitOfWork.Repository<Session>().Queryable().SingleOrDefault();

                if (hostSession == null || hostSession.OfflineVoucherPrinted)
                {
                    return new EndSessionResponse { ResponseCode = ServerResponseCode.SessionEndedVoucherPrintedOffLine };
                }
            }

            var session = _egm.GetService<ISession>();

            var basePolicy = Policy<MessageResult<EndSessionResponse>>
                .HandleResult(
                    r => r.Response?.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to end session.");
                        await Task.CompletedTask;
                    });

            var duplicateVoucherPolicy = Policy<MessageResult<EndSessionResponse>>
                .HandleResult(
                    r => r.Response?.ResponseCode == ServerResponseCode.DuplicateVoucherRetry)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries - 1,

                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        UpdateRequestTransactionId(message);

                        _logger.LogDebug(
                            $"Duplicate voucher retry received...retrying ({retryCount}) to end session.");
                        await Task.CompletedTask;
                    });

            var policy = Policy.WrapAsync(basePolicy, duplicateVoucherPolicy);

            var result = await policy.ExecuteAsync(async () => await session.EndSession(message));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success
                ? new EndSessionResponse { ResponseCode = ServerResponseCode.ServerError }
                : result.Response;
        }
    }
}
