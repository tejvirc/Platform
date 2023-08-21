namespace Aristocrat.Mgam.Client.Services.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Attribute;
    using Logging;
    using Messaging;
    using Polly;
    using Routing;

    /// <summary>
    ///     Registers the VLT with the site-controller.
    /// </summary>
    internal sealed class RegistrationService : IRegistration
    {
        private const int DoubleRetries = 2;
        private readonly ILogger _logger;
        private readonly IRequestRouter _router;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegistrationService"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="router"><see cref="IRequestRouter"/>.</param>
        /// <param name="services"><see cref="IHostServiceCollection"/>.</param>
        public RegistrationService(
            ILogger<RegistrationService> logger,
            IRequestRouter router,
            IHostServiceCollection services)
        {
            _logger = logger;
            _router = router;

            services.Add(this);
        }

        /// <inheritdoc />
        public async Task<MessageResult<RegisterInstanceResponse>> Register(
            RegisterInstance message,
            CancellationToken cancellationToken)
        {
            return await Register<RegisterInstance, RegisterInstanceResponse>(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<RegisterAttributeResponse>> Register(
            RegisterAttribute message,
            CancellationToken cancellationToken)
        {
            return await Register<RegisterAttribute, RegisterAttributeResponse>(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<RegisterCommandResponse>> Register(
            RegisterCommand message,
            CancellationToken cancellationToken)
        {
            return await Register<RegisterCommand, RegisterCommandResponse>(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<RegisterNotificationResponse>> Register(
            RegisterNotification message,
            CancellationToken cancellationToken)
        {
            return await Register<RegisterNotification, RegisterNotificationResponse>(message, cancellationToken);
        }

        public async Task<MessageResult<RegisterActionResponse>> Register(
            RegisterAction message,
            CancellationToken cancellationToken)
        {
            return await Register<RegisterAction, RegisterActionResponse>(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<RegisterGameResponse>> Register(
            RegisterGame message,
            CancellationToken cancellationToken)
        {
            return await Register<RegisterGame, RegisterGameResponse>(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<RegisterProgressiveResponse>> Register(
            RegisterProgressive message,
            CancellationToken cancellationToken)
        {
            return await Register<RegisterProgressive, RegisterProgressiveResponse>(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<RegisterDenominationResponse>> Register(
            RegisterDenomination message,
            CancellationToken cancellationToken)
        {
            return await Register<RegisterDenomination, RegisterDenominationResponse>(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<ReadyToPlayResponse>> ReadyToPlay(CancellationToken cancellationToken)
        {
            var policy = Policy<MessageResult<ReadyToPlayResponse>>
                .HandleResult(
                    r => r.Response?.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, _) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) sending ReadyToPlay message.");
                        await Task.CompletedTask;
                    });

            return await policy.ExecuteAsync(
                async () => await _router.Send<ReadyToPlay, ReadyToPlayResponse>(
                    new ReadyToPlay(),
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<AttributeItem>, ServerResponseCode)> GetAttributes(
            CancellationToken cancellationToken)
        {
            var policy = Policy<MessageResult<GetAttributesResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && !(r.Response.ResponseCode == ServerResponseCode.Ok ||
                                                                r.Response.ResponseCode ==
                                                                ServerResponseCode.ServerError ||
                                                                r.Response.Attributes.Count != 0))
                .WaitAndRetryAsync(
                    DoubleRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, _) =>
                    {
                        _logger.LogDebug($"Retrying ({retryCount}) to get attributes.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(
                async () => await _router.Send<GetAttributes, GetAttributesResponse>(
                    new GetAttributes(),
                    cancellationToken));

            if (result.Status != MessageStatus.Success)
            {
                _logger.LogError(
                    $"Comms error occurred sending {nameof(GetAttributes)} message, Status: {result.Status}");
                return (Enumerable.Empty<AttributeItem>().ToList(), ServerResponseCode.ServerError);
            }

            if (result.Response.ResponseCode != ServerResponseCode.Ok)
            {
                _logger.LogError(
                    $"Error occurred sending {nameof(GetAttributes)} message, Response: {result.Response.ResponseCode}");
                return (Enumerable.Empty<AttributeItem>().ToList(), result.Response.ResponseCode);
            }

            return (result.Response.Attributes, result.Response.ResponseCode);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<GameAssignment>> GetGameAssignments(CancellationToken cancellationToken)
        {
            var policy = Policy<MessageResult<GetGameAssignmentsResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, _) =>
                    {
                        _logger.LogDebug($"Retrying ({retryCount}) to get attributes.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(
                async () => await _router.Send<GetGameAssignments, GetGameAssignmentsResponse>(
                    new GetGameAssignments(),
                    cancellationToken));

            if (result.Status != MessageStatus.Success)
            {
                _logger.LogError(
                    $"Comms error occurred sending {nameof(GetGameAssignments)} message, Status: {result.Status}");
                return Enumerable.Empty<GameAssignment>().ToList();
            }

            if (result.Response.ResponseCode != ServerResponseCode.Ok)
            {
                _logger.LogError(
                    $"Error occurred sending {nameof(GetGameAssignments)} message, Response: {result.Response.ResponseCode}");
                return Enumerable.Empty<GameAssignment>().ToList();
            }

            return result.Response.Assignments;
        }

        /// <inheritdoc />
        public async Task SetAttribute(string name, object value, CancellationToken cancellationToken)
        {
            var attribute = SupportedAttributes.Get().Single(x => x.Name == name);

            var result = await _router.Send<SetAttribute, SetAttributeResponse>(
                new SetAttribute
                    {
                    Scope = attribute.Scope, Name = attribute.Name, Value = value?.ToString() ?? string.Empty
                },
                cancellationToken);

            if (result.Status != MessageStatus.Success || result.Response == null)
            {
                _logger.LogError(
                    $"Comms error occurred sending {nameof(SetAttribute)} message, Status: {result.Status}");
                throw new ServerResponseException(ServerResponseCode.Unknown);
            }

            if (result.Response.ResponseCode != ServerResponseCode.Ok)
            {
                _logger.LogError(
                    $"Error occurred sending {nameof(SetAttribute)} message, Response: {result.Response.ResponseCode}");
                throw new ServerResponseException(result.Response.ResponseCode);
            }
        }

        private async Task<MessageResult<TResponse>> Register<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken)
            where TRequest : class, IRequest
            where TResponse : Response, new()
        {
            return await _router.Send<TRequest, TResponse>(request, cancellationToken);
        }
    }
}
