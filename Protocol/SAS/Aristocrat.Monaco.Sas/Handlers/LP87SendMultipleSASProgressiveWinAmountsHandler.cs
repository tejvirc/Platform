namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Progressive;

    /// <summary>
    ///     The handler for LP 87 Send Multiple Sas Progressive Win Amounts
    /// </summary>
    public class LP87SendMultipleSasProgressiveWinAmountsHandler : ISasLongPollHandler<
        SendMultipleSasProgressiveWinAmountsResponse, LongPollData>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGamePlayState _gamePlay;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IProgressiveHitExceptionProvider _hitExceptionProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LP87SendMultipleSasProgressiveWinAmountsHandler" /> class.
        /// </summary>
        /// <param name="propertiesManager">An <see cref="IPropertiesManager" /> instance.</param>
        /// <param name="gamePlay">The game play state</param>
        /// <param name="protocolLinkedProgressiveAdapter">An instance of <see cref="IProtocolLinkedProgressiveAdapter"/></param>
        /// <param name="persistentStorage">An instance of <see cref="IPersistentStorageManager"/></param>
        /// <param name="hitExceptionProvider">An instance of <see cref="IProgressiveHitExceptionProvider"/></param>
        public LP87SendMultipleSasProgressiveWinAmountsHandler(
            IPropertiesManager propertiesManager,
            IGamePlayState gamePlay,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPersistentStorageManager persistentStorage,
            IProgressiveHitExceptionProvider hitExceptionProvider)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _hitExceptionProvider = hitExceptionProvider ?? throw new ArgumentNullException(nameof(hitExceptionProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendMultipleSasProgressiveWinAmounts };

        /// <inheritdoc />
        public SendMultipleSasProgressiveWinAmountsResponse Handle(LongPollData data)
        {
            var groupId = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .ProgressiveGroupId;

            if (!_gamePlay.InGameRound)
            {
                return new SendMultipleSasProgressiveWinAmountsResponse(new List<LinkedProgressiveWinData>(), groupId);
            }

            var hitLevels = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Where(
                    x =>
                        x.ClaimStatus.Status == LinkedClaimState.Hit &&
                        x.ProgressiveGroupId == groupId &&
                        x.ProtocolName == ProgressiveConstants.ProtocolName)
                .OrderBy(x => x.ClaimStatus.HitTime)
                .Select(x => new LinkedProgressiveWinData(x.LevelId, x.ClaimStatus.WinAmount, x.LevelName)).ToList();

            if (hitLevels.Any())
            {
                // Stop reporting for now until we award so we don't have an exception possibly sent in error
                _hitExceptionProvider.StopSasProgressiveHitReporting();
            }

            return new SendMultipleSasProgressiveWinAmountsResponse(hitLevels, groupId)
            {
                Handlers = new HostAcknowledgementHandler
                {
                    ImpliedAckHandler = () => Task.Run(() => AcknowledgeProgressiveWins(hitLevels, groupId))
                }
            };
        }

        private void AcknowledgeProgressiveWins(IEnumerable<LinkedProgressiveWinData> levels, int groupId)
        {
            using (var scope = _persistentStorage.ScopedTransaction())
            {
                foreach (var level in levels)
                {
                    _protocolLinkedProgressiveAdapter.ClaimLinkedProgressiveLevel(level.LevelName, ProgressiveConstants.ProtocolName);
                    _protocolLinkedProgressiveAdapter.AwardLinkedProgressiveLevel(level.LevelName, ProgressiveConstants.ProtocolName);
                }

                scope.Complete();
            }

            if (_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Any(
                    x => x.ClaimStatus.Status == LinkedClaimState.Hit &&
                         x.ProgressiveGroupId == groupId &&
                         x.ProtocolName == ProgressiveConstants.ProtocolName))
            {
                // If we still have claims left we need to keep reporting
                _hitExceptionProvider.StartReportingSasProgressiveHit();
            }
        }
    }
}