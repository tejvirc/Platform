namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Session;
    using AutoMapper;
    using Common.Data.Models;
    using Kernel;
    using Polly;
    using Protocol.Common.Storage.Entity;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handles the <see cref="Commands.BeginSessionWithSessionId"/> command.
    /// </summary>
    public class BeginSessionWithSessionIdCommandHandler : CommandHandlerBase, ICommandHandler<BeginSessionWithSessionId>
    {
        private const int RetryDelay = 1;

        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BeginSessionWithSessionIdCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public BeginSessionWithSessionIdCommandHandler(
            ILogger<BeginSessionWithSessionIdCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            ILockup lockup,
            INotificationLift notificationLift,
            IUnitOfWorkFactory unitOfWorkFactory,
            IEventBus bus) : base(bus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        /// <inheritdoc />
        public async Task Handle(BeginSessionWithSessionId command)
        {
            await BeginSessionWithSessionId(command);
        }

        private async Task BeginSessionWithSessionId(BeginSessionWithSessionId command)
        {
            _logger.LogInfo("BeginSessionWithSessionId command");

            var response = await BeginSessionWithSessionId(_mapper.Map<Aristocrat.Mgam.Client.Messaging.BeginSessionWithSessionId>(command));

            if (response.ResponseCode == ServerResponseCode.SessionEndedVoucherPrintedOffLine)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var session = unitOfWork.Repository<Session>().Queryable().SingleOrDefault();

                    if (session != null)
                    {
                        unitOfWork.Repository<Session>().Delete(session);
                    }

                    unitOfWork.SaveChanges(); 
                }

                return;
            }

            if (response.ResponseCode == ServerResponseCode.Ok)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var session = unitOfWork.Repository<Session>().Queryable().SingleOrDefault();

                    if (session == null)
                    {
                        session = _mapper.Map<Session>(response);
                        unitOfWork.Repository<Session>().AddOrUpdate(session);

                        unitOfWork.SaveChanges();
                    }
                }

                if (response.SessionCashBalance == 0 &&
                        response.SessionCouponBalance == 0)
                {
                    throw new ServerResponseException(ServerResponseCode.InvalidAmount);
                }
            }
            else
            {
                _lockup.LockupForEmployeeCard($"SessionId {command.SessionId}");

                await _notificationLift.Notify(NotificationCode.LockedBeginSessionWithSessionIdFailed, command.SessionId.ToString());

                throw new ServerResponseException(response.ResponseCode);
            }
        }

        private async Task<BeginSessionResponse> BeginSessionWithSessionId(Aristocrat.Mgam.Client.Messaging.BeginSessionWithSessionId message)
        {
            var session = _egm.GetService<ISession>();

            var policy = Policy<MessageResult<BeginSessionResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to begin session.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await session.BeginSessionWithSessionId(message));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success ? new BeginSessionResponse { ResponseCode = ServerResponseCode.ServerError } : result.Response;
        }
    }
}
