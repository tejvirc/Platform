namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.CreditServices;
    using AutoMapper;
    using Kernel;
    using Polly;

    /// <summary>
    ///     Handles the <see cref="EscrowCash"/> command.
    /// </summary>
    public class EscrowCashCommandHandler : CommandHandlerBase, ICommandHandler<EscrowCash>
    {
        private const int RetryDelay = 1;
        private const int RetryCount = 1;

        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EscrowCashCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public EscrowCashCommandHandler(
            ILogger<EscrowCashCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IEventBus bus) : base(bus)
        {
            _logger = logger;
            _egm = egm;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task Handle(EscrowCash command)
        {
            var message = _mapper.Map<Aristocrat.Mgam.Client.Messaging.EscrowCash>(command);

            await EscrowCash(message);
        }

        private async Task EscrowCash(Aristocrat.Mgam.Client.Messaging.EscrowCash message)
        {
            _logger.LogInfo($"EscrowCash {message.Amount} attribute");
            
            var response = await EscrowCashRetry(message);

            if (response.ResponseCode != ServerResponseCode.Ok)
            {
                throw new ServerResponseException(response.ResponseCode);
            }
        }

        private async Task<EscrowCashResponse> EscrowCashRetry(Aristocrat.Mgam.Client.Messaging.EscrowCash message)
        {
            var currency = _egm.GetService<ICurrency>();

            var policy = Policy<MessageResult<EscrowCashResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    RetryCount,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to escrow cash {message.Amount}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await currency.EscrowCash(message));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success ? new EscrowCashResponse {ResponseCode = ServerResponseCode.ServerError } : result.Response;
        }
    }
}
