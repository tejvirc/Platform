namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Registration;
    using AutoMapper;
    using Common;
    using Common.Data.Models;
    using Common.Data.Repositories;
    using Kernel;
    using Polly;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Handles the <see cref="RegisterInstance"/> command.
    /// </summary>
    public class RegisterInstanceCommandHandler : CommandHandlerBase, ICommandHandler<RegisterInstance>
    {
        private readonly ILogger _logger;
        private readonly IPropertiesManager _properties;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ITime _time;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisterInstanceCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="properties"><see cref="IPropertiesManager"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory"/>.</param>
        /// <param name="time"><see cref="ITime"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public RegisterInstanceCommandHandler(
            ILogger<RegisterInstanceCommandHandler> logger,
            IPropertiesManager properties,
            IEgm egm,
            IMapper mapper,
            IUnitOfWorkFactory unitOfWorkFactory,
            ITime time,
            IEventBus bus) : base(bus)
        {
            _logger = logger;
            _properties = properties;
            _egm = egm;
            _mapper = mapper;
            _unitOfWorkFactory = unitOfWorkFactory;
            _time = time;
        }

        /// <inheritdoc />
        public async Task Handle(RegisterInstance command)
        {
            var endPoint = command.EndPoint;

            _logger.LogDebug($"Registering with VLT Service at {endPoint}");

            RegistrationInfo regInfo;

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                regInfo = unitOfWork.Repository<Host>().GetRegistrationInfo();
            }

            _properties.SetProperty(PropertyNames.KnownRegistration, false);

            var result = await Register(_mapper.Map<Aristocrat.Mgam.Client.Messaging.RegisterInstance>(regInfo));

            if (result.Status != MessageStatus.Success)
            {
                throw new RegistrationException(
                    $"Comms error occurred registering instance; status: {result.Status}",
                    RegistrationFailureBehavior.Relocate);
            }

            switch (result.Response.ResponseCode)
            {
                case ServerResponseCode.Ok:
                    _logger.LogDebug(
                        $"Registered instance {result.Response.InstanceId}; response code: {result.Response.ResponseCode}");
                    break;

                case ServerResponseCode.KnownRegistration:
                    _properties.SetProperty(PropertyNames.KnownRegistration, true);
                    _logger.LogDebug(
                        $"Registered known instance {result.Response.InstanceId}; response code: {result.Response.ResponseCode}");
                    break;

                case ServerResponseCode.VltServiceAlreadyRegistered:
                    _properties.SetProperty(PropertyNames.KnownRegistration, true);
                    _logger.LogDebug(
                        $"Instance already registered {_egm.ActiveInstance?.InstanceId}; response code: {result.Response.ResponseCode}");
                    break;

                case ServerResponseCode.DeviceStillRegisteredWithVltSvc:
                    throw new RegistrationException(
                        $"Unexpected response; response code: {result.Response.ResponseCode}",
                        RegistrationFailureBehavior.Relocate);

                default:
                    throw new RegistrationException(
                        $"Error occurred registering instance; response code: {result.Response.ResponseCode}",
                        RegistrationFailureBehavior.Lock);
            }

            if (_egm.ActiveInstance == null)
            {
                if (result.Response.InstanceId <= 0)
                {
                    throw new RegistrationException(
                        $"Invalid Instance ID {result.Response.InstanceId}; response code: {result.Response.ResponseCode}",
                        RegistrationFailureBehavior.Lock);
            }

                _egm.SetActiveInstance(
                _mapper.Map(result.Response, new InstanceInfo { ConnectionString = endPoint.ToString() }));
            }

            try
            {
                var dt = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(result.Response.TimeStamp), TimeZoneInfo.Local);

                _time.Update(dt);
            }
            catch (Exception ex)
            {
                throw new RegistrationException("Error syncing date/time", ex, RegistrationFailureBehavior.Relocate);
            }
        }

        private async Task<MessageResult<RegisterInstanceResponse>> Register(Aristocrat.Mgam.Client.Messaging.RegisterInstance message)
        {
            var registration = _egm.GetService<IRegistration>();

            var policy = Policy<MessageResult<RegisterInstanceResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug($"Retrying ({retryCount}) to register instance.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await registration.Register(message));

            ValidateResponseCode(result.Response);

            return result;
        }
    }
}
