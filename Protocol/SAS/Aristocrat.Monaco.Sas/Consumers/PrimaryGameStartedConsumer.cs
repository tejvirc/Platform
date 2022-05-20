namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Exceptions;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Progressive;
    using Storage.Models;

    /// <summary>
    ///     Handles the <see cref="PrimaryGameStartedEvent" /> event.
    /// </summary>
    public class PrimaryGameStartedConsumer : Consumes<PrimaryGameStartedEvent>
    {
        private const byte MaxBet = 0x80;
        private const byte MultiDenomination = 0x40;
        private const byte NoProgressiveLevels = 0;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IGameProvider _gameProvider;
        private readonly IMeterManager _meterManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IProtocolLinkedProgressiveAdapter _progressiveAdapter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameStartedConsumer" /> class.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or illegal values.</exception>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="gameProvider">The IGameProvider.</param>
        /// <param name="meterManager">The IMeterManager.</param>
        /// <param name="propertiesManager">The IPropertiesManager.</param>
        /// <param name="progressiveAdapter">The progressive adapter</param>
        public PrimaryGameStartedConsumer(
            ISasExceptionHandler exceptionHandler,
            IGameProvider gameProvider,
            IMeterManager meterManager,
            IPropertiesManager propertiesManager,
            IProtocolLinkedProgressiveAdapter progressiveAdapter)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _progressiveAdapter = progressiveAdapter ?? throw new ArgumentNullException(nameof(progressiveAdapter));
        }

        /// <inheritdoc />
        public override void Consume(PrimaryGameStartedEvent theEvent)
        {
            var denominationInCents = (int)theEvent.Denomination.MillicentsToCents();

            var gameStartData = new GameStartData { ProgressiveGroup = 0 };
            var game = _gameProvider.GetGame(theEvent.GameId);
            var denomination = game.Denominations.FirstOrDefault(d => d.Value == theEvent.Denomination);
            var isMaxBet = (ulong)(theEvent.Log.InitialWager / denominationInCents) == (ulong)game.MaximumWagerCredits(denomination);
            var maxBetFlag = isMaxBet ? MaxBet: (byte)0x00;
            // We should always report as a multi-denom game.
            gameStartData.WagerType = (byte)(MultiDenomination | maxBetFlag |
                                             DenominationCodes.GetCodeForDenomination(denominationInCents));

            gameStartData.CreditsWagered = theEvent.Log.InitialWager / denominationInCents;
            gameStartData.CoinInMeter = _meterManager.GetMeter(GamingMeters.WageredAmount).Lifetime;
            gameStartData.ProgressiveGroup = GetProgressiveGroupId(theEvent, game.GetBetOption(theEvent.Denomination)?.Name);
            var denoms = _propertiesManager.GetValue(SasProperties.SasHosts, Enumerable.Empty<Host>())
                .Select(x => x.AccountingDenom).ToList();
            _exceptionHandler.ReportException(
                client => GetGameStartedException(gameStartData, denoms, client),
                GeneralExceptionCode.GameHasStarted);
        }

        private static GameStartedExceptionBuilder GetGameStartedException(
            GameStartData data,
            IReadOnlyList<long> accountingDenoms,
            byte clientNumber)
        {
            return accountingDenoms.Count <= clientNumber
                ? null
                : new GameStartedExceptionBuilder(data, accountingDenoms[clientNumber]);
        }

        private byte GetProgressiveGroupId(BaseGameEvent theEvent, string betOption)
        {
            return _progressiveAdapter.ViewConfiguredProgressiveLevels(theEvent.GameId, theEvent.Denomination).Any(
                x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                    (string.IsNullOrEmpty(x.BetOption) || x.BetOption == betOption) &&
                     _progressiveAdapter.ViewLinkedProgressiveLevel(
                         x.AssignedProgressiveId.AssignedProgressiveKey,
                         out var level) && level.ProtocolName == ProgressiveConstants.ProtocolName)
                ? (byte)_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                    .ProgressiveGroupId
                : NoProgressiveLevels;
        }
    }
}