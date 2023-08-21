namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Identification;
    using Kernel;
    using Polly;

    /// <summary>
    ///     Handles the <see cref="Commands.Checksum" /> command.
    /// </summary>
    public class ChecksumCommandHandler : CommandHandlerBase, ICommandHandler<Checksum>
    {
        private const int RetryDelay = 1;

        private readonly IEgm _egm;
        private readonly ILogger _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChecksumCommandHandler" /> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger" />.</param>
        /// <param name="egm"><see cref="IEgm" />.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public ChecksumCommandHandler(
            ILogger<ChecksumCommandHandler> logger,
            IEgm egm,
            IEventBus bus) : base(bus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task Handle(Checksum command)
        {
            await Checksum(command);
        }

        private async Task Checksum(Checksum command)
        {
            _logger.LogInfo($"Checksum value {command.ChecksumValue}");

            var result = await Checksum(
                new Aristocrat.Mgam.Client.Messaging.Checksum { ChecksumValue = command.ChecksumValue}, command.CancellationToken);

            command.CancellationToken.ThrowIfCancellationRequested();

            if (result.Status != MessageStatus.Success)
            {
                throw new ServerResponseException(ServerResponseCode.ServerError);
            }

            if (result.Response.ResponseCode != ServerResponseCode.Ok)
            {
                throw new ServerResponseException(result.Response.ResponseCode);
            }
        }

        private async Task<MessageResult<ChecksumResponse>> Checksum(
            Aristocrat.Mgam.Client.Messaging.Checksum message,
            CancellationToken cancellationToken)
        {
            var identification = _egm.GetService<IIdentification>();

            var policy = Policy<MessageResult<ChecksumResponse>>
                .HandleResult(
                    r => r.Response?.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to checksum {message.ChecksumValue}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await identification.Checksum(message, cancellationToken));

            ValidateResponseCode(result.Response);

            return result;
        }
    }
}
