namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
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
    ///     Handles the <see cref="Commands.BeginSession" /> command.
    /// </summary>
    public class BeginSessionCommandHandler : CommandHandlerBase, ICommandHandler<BeginSession>
    {
        private const int RetryDelay = 1;
        private const int RetryCount = 1;

        private readonly IEgm _egm;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BeginSessionCommandHandler" /> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger" />.</param>
        /// <param name="egm"><see cref="IEgm" />.</param>
        /// <param name="mapper"><see cref="IMapper" />.</param>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory" />.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public BeginSessionCommandHandler(
            ILogger<BeginSessionCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            ILockup lockup,
            INotificationLift notificationLift,
            IUnitOfWorkFactory unitOfWorkFactory,
            IEventBus bus) : base(bus)
        {
            _logger = logger;
            _egm = egm;
            _mapper = mapper;
            _lockup = lockup;
            _notificationLift = notificationLift;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        /// <inheritdoc />
        public async Task Handle(BeginSession command)
        {
            await BeginSession(command);
        }

        private async Task BeginSession(BeginSession command)
        {
            _logger.LogInfo("BeginSession command");

            var response = await BeginSession(
                _mapper.Map<Aristocrat.Mgam.Client.Messaging.BeginSession>(command));

            if (response.ResponseCode == ServerResponseCode.Ok)
            {
                var session = _mapper.Map<Session>(response);

                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    unitOfWork.Repository<Session>().AddOrUpdate(session);

                    unitOfWork.SaveChanges(); 
                }

                return;
            }

            _lockup.LockupForEmployeeCard($"Begin Session failed ResponseCode:{response.ResponseCode}");

            if (response.SessionCashBalance > 0)
            {
                await _notificationLift.Notify(NotificationCode.LockedBeginSessionWithCashFailed, response.SessionCashBalance.ToString());
            }

            if (response.SessionCouponBalance > 0)
            {
                await _notificationLift.Notify(NotificationCode.LockedBeginSessionWithCashFailed, response.SessionCouponBalance.ToString());
            }

            throw new ServerResponseException(response.ResponseCode);
        }

        private async Task<BeginSessionResponse> BeginSession(
            Aristocrat.Mgam.Client.Messaging.BeginSession message)
        {
            var session = _egm.GetService<ISession>();

            var policy = Policy<MessageResult<BeginSessionResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    RetryCount,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to begin session.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await session.BeginSession(message));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success ? new BeginSessionResponse { ResponseCode = ServerResponseCode.ServerError } : result.Response;
        }
    }
}