namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Collections.Generic;
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
    using Common;
    using Common.Data.Models;
    using Common.Data.Repositories;
    using Common.Events;
    using Gaming.Contracts.Central;
    using Kernel;
    using Newtonsoft.Json;
    using Polly;
    using Protocol.Common.Storage.Entity;
    using Services.CreditValidators;
    using OutcomeType = Gaming.Contracts.Central.OutcomeType;

    /// <summary>
    ///     Handles the <see cref="RequestPlay" /> command.
    /// </summary>
    public class RequestPlayCommandHandler : TransactionalCommandHandlerBase, ICommandHandler<RequestPlay>
    {
        // RequestPlay can only be retried once
        private const int RetryCount = 1;
        private const int RetryDelay = 1;

        private readonly ICentralProvider _centralProvider;
        private readonly IEgm _egm;
        private readonly ILogger<RequestPlayCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RequestPlayCommandHandler" /> class.
        /// </summary>
        public RequestPlayCommandHandler(
            ILogger<RequestPlayCommandHandler> logger,
            ICentralProvider centralProvider,
            IEgm egm,
            IMapper mapper,
            IUnitOfWorkFactory unitOfWorkFactory,
            IEventBus bus,
            IPropertiesManager properties,
            ITransactionRetryHandler transactionRetry,
            IIdProvider idProvider) : base(idProvider, transactionRetry, bus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));

            transactionRetry.RegisterCommand(typeof(Aristocrat.Mgam.Client.Messaging.RequestPlay), RequestPlayRetry);
        }

        /// <inheritdoc />
        public async Task Handle(RequestPlay command)
        {
            await SendRequest(_mapper.Map<Aristocrat.Mgam.Client.Messaging.RequestPlay>(command), command.CancellationToken, command.TransactionId);
        }

        /// <inheritdoc />
        protected override async Task Handle(Request arg, CancellationToken cancellationToken = default(CancellationToken), long transactionId = 0)
        {
            var message = arg as Aristocrat.Mgam.Client.Messaging.RequestPlay;

            await RequestPlay(message, cancellationToken, transactionId);
        }

        private async Task<IResponse> RequestPlay(Aristocrat.Mgam.Client.Messaging.RequestPlay message, CancellationToken cancellationToken = default(CancellationToken), long transactionId = 0)

        {
            var response = await Request(message, cancellationToken);

            try
            {
                if (response.ResponseCode != ServerResponseCode.Ok)
                {
                    if (response.PrizeIndex == 0)
                    {
                        _logger.LogError($"RequestPlay failed with response: {response.ResponseCode}");

                        var exception = response.ResponseCode == ServerResponseCode.InvalidPayTableIndex
                            ? OutcomeException.InvalidPaytableIndex
                            : OutcomeException.Invalid;

                        Failed(exception);

                        return response;
                    }

                    _properties.SetProperty(MgamConstants.ForceCashoutAfterGameRoundKey, true);
                }

                var outcomes = new List<Outcome>
                {
                    new Outcome(
                        response.ServerTransactionId,
                        response.PrizeIndex,
                        0,
                        OutcomeReference.Direct,
                        OutcomeType.Standard,
                        response.PrizeValue.CentsToMillicents(),
                        response.PrizeIndex,
                        JsonConvert.SerializeObject(new LookupData { AwardId = response.ExtendedInfo}))
                };

                if (response.IsProgressiveWin)
                {
                    outcomes.Add(
                        new Outcome(
                            response.ServerTransactionId,
                            response.PrizeIndex,
                            0,
                            OutcomeReference.Direct,
                            OutcomeType.Progressive,
                            response.ProgressivePrizeValue.CentsToMillicents(),
                            response.PrizeIndex,
                            string.Empty));
                }

                _centralProvider.OutcomeResponse(transactionId, outcomes);

                if (response is RequestPlayVoucherResponse voucher)
                {
                    _logger.LogInfo($"Received voucher response: {voucher.Id}");

                    using var unitOfWork = _unitOfWorkFactory.Create();
                    var voucherData = _mapper.Map<Voucher>(voucher);

                    voucherData.Validate();

                    unitOfWork.Repository<Voucher>().AddVoucher(voucherData);

                    unitOfWork.SaveChanges();
                }

                if (message != null)
                {
                    Bus.Publish(new WagerPlacedEvent(message.NumberOfCredits * message.Denomination));
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, $"Failed to retrieve outcomes for transaction ({transactionId})");

                Failed(OutcomeException.TimedOut);
            }

            return response;

            void Failed(OutcomeException outcomeException)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var voucherData = new Voucher();
                    voucherData.Validate();
                    voucherData.OfflineReason = VoucherOutOfflineReason.RequestPlay;

                    unitOfWork.Repository<Voucher>().AddVoucher(voucherData);

                    unitOfWork.SaveChanges();
                }

                _centralProvider.OutcomeResponse(transactionId, Enumerable.Empty<Outcome>(), outcomeException);
            }
        }

        private async Task<IResponse> RequestPlayRetry(object message)
        {
            if (message is Aristocrat.Mgam.Client.Messaging.RequestPlay requestPlay)
            {
                return await RequestPlay(requestPlay, CancellationToken.None);
            }

            requestPlay =
                JsonConvert.DeserializeObject<Aristocrat.Mgam.Client.Messaging.RequestPlay>(message.ToString());
            return await RequestPlay(requestPlay, CancellationToken.None);
        }

        private async Task<RequestPlayResponse> Request(Aristocrat.Mgam.Client.Messaging.RequestPlay message, CancellationToken cancellationToken)
        {
            var session = _egm.GetService<ISession>();

            // The request play command is different than most other messages in that it can only be retried once. Upon failure, specific actions must be performed.
            var policy = Policy<MessageResult<RequestPlayResponse>>
                .HandleResult(
                    r => r.Response?.ResponseCode == ServerResponseCode.ServerError ||
                         r.Response?.ResponseCode == ServerResponseCode.SessionNoSessionInProgress)
                .WaitAndRetryAsync(
                    RetryCount,
                    _ => TimeSpan.FromSeconds(RetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug($"Retrying ({retryCount}) request play {message.LocalTransactionId}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await session.RequestPlay(message, cancellationToken));

            ValidateResponseCode(result.Response);

            return result.Status != MessageStatus.Success ? new RequestPlayResponse { ResponseCode = ServerResponseCode.ServerError } : result.Response;
        }
    }
}
