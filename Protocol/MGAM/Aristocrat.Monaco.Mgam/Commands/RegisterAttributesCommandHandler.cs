namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Attribute;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Registration;
    using AutoMapper;
    using Common;
    using Common.Data.Models;
    using Kernel;
    using Polly;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Handles the <see cref="RegisterAttributes"/> command.
    /// </summary>
    public class RegisterAttributesCommandHandler : CommandHandlerBase, ICommandHandler<RegisterAttributes>
    {
        private readonly ILogger _logger;
        private readonly IPropertiesManager _properties;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisterAttributesCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="properties"><see cref="IPropertiesManager"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public RegisterAttributesCommandHandler(
            ILogger<RegisterAttributesCommandHandler> logger,
            IPropertiesManager properties,
            IEgm egm,
            IMapper mapper,
            IUnitOfWorkFactory unitOfWorkFactory,
            IEventBus bus) : base(bus)
        {
            _logger = logger;
            _properties = properties;
            _egm = egm;
            _mapper = mapper;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        /// <inheritdoc />
        public async Task Handle(RegisterAttributes command)
        {
            foreach (var a in SupportedAttributes.Get().Where(a => a.Scope != AttributeScope.Application))
            {
                await Register(a);
            }

            if (_properties.GetValue(PropertyNames.KnownRegistration, false))
            {
                return;
            }

            string applicationName;
            string applicationVersion;

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var application = unitOfWork.Repository<Application>().Queryable().First();

                applicationName = application.Name;
                applicationVersion = application.Version;
            }

            foreach (var a in SupportedAttributes.Get().Where(a => a.Scope == AttributeScope.Application))
            {
                var attribute = a;

                switch (a.Name)
                {
                    case AttributeNames.VersionName:
                        attribute.DefaultValue = applicationName;
                        break;

                    case AttributeNames.VersionNumber:
                        attribute.DefaultValue = applicationVersion;
                        break;
                }

                await Register(attribute);
            }
        }

        private async Task Register(AttributeInfo attribute)
        {
            var result = await Register(_mapper.Map<RegisterAttribute>(attribute));

            if (result.Status != MessageStatus.Success)
            {
                throw new RegistrationException(
                    $"Comms error occurred registering attribute {attribute.Name}; status: {result.Status}",
                    RegistrationFailureBehavior.Relocate);
            }

            switch (result.Response.ResponseCode)
            {
                case ServerResponseCode.Ok:
                case ServerResponseCode.AttributeAlreadyRegistered:
                    _logger.LogDebug(
                        $"Registered attribute {attribute.Name}; response code: {result.Response.ResponseCode}");
                    break;

                case ServerResponseCode.InvalidInstanceId:
                case ServerResponseCode.DeviceStillRegisteredWithVltSvc:
                case ServerResponseCode.VltServiceNotRegistered:
                    throw new RegistrationException(
                        $"Error occurred registering attribute {attribute.Name}; response code: {result.Response.ResponseCode}",
                        RegistrationFailureBehavior.Relocate);

                default:
                    throw new RegistrationException(
                        $"Error occurred registering attribute {attribute.Name}; response code: {result.Response.ResponseCode}",
                        RegistrationFailureBehavior.Lock);
            }
        }

        private async Task<MessageResult<RegisterAttributeResponse>> Register(RegisterAttribute message)
        {
            var registration = _egm.GetService<IRegistration>();

            var policy = Policy<MessageResult<RegisterAttributeResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to register attribute {message.ItemName}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await registration.Register(message));

            ValidateResponseCode(result.Response);

            return result;
        }
    }
}
