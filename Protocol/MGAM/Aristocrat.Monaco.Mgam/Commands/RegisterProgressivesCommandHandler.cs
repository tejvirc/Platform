namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Registration;
    using AutoMapper;
    using Common;
    using Kernel;
    using Polly;
    using Services.GamePlay;

    public class RegisterProgressivesCommandHandler : CommandHandlerBase, ICommandHandler<RegisterProgressives>
    {
        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;
        private readonly IProgressiveController _progressives;

        /// <summary>
        ///     Initialize a new instance of the <see cref="RegisterProgressivesCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        /// <param name="progressives"><see cref="IProgressiveController"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public RegisterProgressivesCommandHandler(
            ILogger<RegisterProgressivesCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IProgressiveController progressives,
            IEventBus bus) : base(bus)
        {
            _logger = logger;
            _egm = egm;
            _mapper = mapper;
            _progressives = progressives;
        }

        public async Task Handle(RegisterProgressives command)
        {
            foreach (var p in _progressives.GetActiveProgressives())
            {
                await Register(p);
            }
        }

        private async Task Register(ProgressiveInfo progressive)
        {
            var result = await Register(_mapper.Map<RegisterProgressive>(progressive));

            if (result.Status != MessageStatus.Success)
            {
                throw new RegistrationException(
                    $"Comms error occured registering {progressive.PoolName} progressive for {progressive.PoolName} game; status: {result.Status}",
                    RegistrationFailureBehavior.Relocate);
            }

            switch (result.Response.ResponseCode)
            {
                case ServerResponseCode.Ok:
                case ServerResponseCode.ProgressiveAlreadyRegistered:
                    _logger.LogDebug(
                        $"Registered {progressive.PoolName} progressive for {progressive.PoolName} game; response code: {result.Response.ResponseCode}");
                    break;

                case ServerResponseCode.InvalidInstanceId:
                case ServerResponseCode.DeviceStillRegisteredWithVltSvc:
                case ServerResponseCode.VltServiceNotRegistered:
                    throw new RegistrationException(
                        $"Error occured registering {progressive.PoolName} progressive for {progressive.PoolName} game; response code: {result.Response.ResponseCode}",
                        RegistrationFailureBehavior.Relocate);

                default:
                    throw new RegistrationException(
                        $"Error occured registering {progressive.PoolName} progressive for {progressive.PoolName} game; response code: {result.Response.ResponseCode}",
                        RegistrationFailureBehavior.Lock);
            }
        }

        private async Task<MessageResult<RegisterProgressiveResponse>> Register(RegisterProgressive message)
        {
            var registration = _egm.GetService<IRegistration>();

            var policy = Policy<MessageResult<RegisterProgressiveResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to register progressive {message.ProgressiveName}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await registration.Register(message));

            ValidateResponseCode(result.Response);

            return result;
        }
    }
}
