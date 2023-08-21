namespace Aristocrat.Monaco.Hhr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Client.Messages;
    using Events;
    using Exceptions;
    using Gaming.Contracts.Central;
    using Kernel;
    using log4net;
    using Services;
    using Storage.Helpers;

    public class RequestPlayCommandHandler : ICommandHandler<RequestPlay>
    {
        // RequestPlay can only be retried once
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICentralProvider _centralProvider;
        private readonly IGamePlayEntityHelper _gamePlayEntityHelper;
        private readonly IPrizeDeterminationService _prizeDeterminationService;
        private readonly IRequestTimeoutBehaviorService _requestTimeoutBehaviorService;
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RequestPlayCommandHandler" /> class.
        /// </summary>
        public RequestPlayCommandHandler(
            ICentralProvider centralProvider,
            IPrizeDeterminationService prizeDeterminationService,
            IGamePlayEntityHelper gamePlayEntityHelper,
            IRequestTimeoutBehaviorService requestTimeoutBehaviorService,
            IEventBus eventBus)
        {
            _centralProvider = centralProvider ??
                throw new ArgumentNullException(nameof(centralProvider));
            _prizeDeterminationService = prizeDeterminationService ??
                throw new ArgumentNullException(nameof(prizeDeterminationService));
            _gamePlayEntityHelper = gamePlayEntityHelper ??
                throw new ArgumentNullException(nameof(gamePlayEntityHelper));
            _requestTimeoutBehaviorService = requestTimeoutBehaviorService ??
                throw new ArgumentNullException(nameof(requestTimeoutBehaviorService));
            _eventBus = eventBus ??
                throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public async Task Handle(RequestPlay command)
        {
            try
            {
                Logger.Debug("Clear game play response");
                _gamePlayEntityHelper.GamePlayResponse = null;

                if (!await _requestTimeoutBehaviorService.CanPlay())
                {
                    Logger.Debug(
                        "Transaction request is pending with server, cannot continue this game play request.");
                    ProduceOutcome(OutcomeException.Invalid, Enumerable.Empty<Outcome>());
                    return;
                }

                await DeterminePrize();
            }
            catch (PrizeCalculationException ex)
            {
                Logger.Debug($"Failed to retrieve outcomes for transaction {ex.Message}");
                ProduceOutcome(OutcomeException.Invalid, Enumerable.Empty<Outcome>());
            }
            catch (GameRecoveryFailedException ex)
            {
                Logger.Debug($"Unable to recover game with Transaction {command.TransactionId} - {ex.Message}");
                ProduceOutcome(OutcomeException.TimedOut, Enumerable.Empty<Outcome>());
            }
            catch (UnexpectedResponseException ex)
            {
                Logger.Debug($"Unexpected response for transaction ({command.TransactionId})", ex);
                ProduceOutcome(OutcomeException.TimedOut, Enumerable.Empty<Outcome>());
            }
            catch (IgnoreOutcomesException ex)
            {
                Logger.Debug($"Invalid GameResponse or GameRecovery message received, ignoring. {ex.Message}");
                // NOTE: We don't call ProduceOutcome so we will not clear the request information.
            }

            void ProduceOutcome(
                OutcomeException exception,
                IEnumerable<Outcome> outcomes,
                string gameRoundInfo = "")
            {
                if (exception != OutcomeException.None)
                {
                    _eventBus.Publish(new GamePlayRequestFailedEvent());
                }

                _centralProvider.OutcomeResponse(
                    command.TransactionId,
                    outcomes,
                    exception,
                    gameRoundInfo);

                Logger.Debug("Clear game play requests");
                _gamePlayEntityHelper.GamePlayRequest = null;
                _gamePlayEntityHelper.RaceStartRequest = null;
            }

            async Task DeterminePrize()
            {
                var prizeInformation =
                    await _prizeDeterminationService.DeterminePrize(
                        (uint)command.GameUpcNumber,
                        (uint)command.NumberOfCredits,
                        (uint)command.Denomination,
                        command.TransactionId,
                        command.IsRecovering);

                if (prizeInformation.BOverride)
                {
                    Logger.Debug("Outcome overridden");
                }

                ProduceOutcome(OutcomeException.None, prizeInformation.Outcomes, prizeInformation.GameRoundInfo);
            }
        }
    }
}