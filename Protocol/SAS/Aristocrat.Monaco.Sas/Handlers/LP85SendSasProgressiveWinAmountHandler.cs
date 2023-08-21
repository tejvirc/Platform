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
    ///     The handler for LP 85 Send Sas Progressive Win Amount
    /// </summary>
    public class LP85SendSasProgressiveWinAmountHandler :
        ISasLongPollHandler<SendProgressiveWinAmountResponse, LongPollData>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IProgressiveHitExceptionProvider _hitExceptionProvider;
        private readonly IGamePlayState _playState;
        private readonly IPersistentStorageManager _persistentStorage;

        /// <summary>
        ///     Creates a new instance of the LP85SendSasProgressiveWinAmountHandler
        /// </summary>
        /// <param name="propertiesManager">An reference of the IPropertiesManager</param>
        /// <param name="protocolLinkedProgressiveAdapter">Linked progressive provider</param>
        /// <param name="hitExceptionProvider">The progressive hit exception provider</param>
        /// <param name="playState">The game play state</param>
        /// <param name="persistentStorage">An instance of <see cref="IPersistentStorageManager"/></param>
        public LP85SendSasProgressiveWinAmountHandler(
            IPropertiesManager propertiesManager,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IProgressiveHitExceptionProvider hitExceptionProvider,
            IGamePlayState playState,
            IPersistentStorageManager persistentStorage)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _hitExceptionProvider =
                hitExceptionProvider ?? throw new ArgumentNullException(nameof(hitExceptionProvider));
            _playState = playState ?? throw new ArgumentNullException(nameof(playState));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendSasProgressiveWinAmount };

        /// <inheritdoc />
        public SendProgressiveWinAmountResponse Handle(LongPollData data)
        {
            // Get the group id we are configured for
            var groupId = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .ProgressiveGroupId;

            // Create a default structure per the spec
            var response = new SendProgressiveWinAmountResponse
            {
                GroupId = 0,
                LevelId = 0,
                WinAmount = 0L
            };

            if (!_playState.InGameRound)
            {
                return response;
            }

            // Get the pending claims by group id, protocol name, and status and order by hit date
            var firstPendingClaim = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Where(x =>
                    x.ClaimStatus.Status == LinkedClaimState.Hit &&
                    x.ProgressiveGroupId == groupId &&
                    x.ProtocolName == ProgressiveConstants.ProtocolName)
                .OrderBy(x => x.ClaimStatus.HitTime)
                .FirstOrDefault();

            if (firstPendingClaim != null)
            {
                // Stop reporting for now until we award so we don't have an exception possibly sent in error
                _hitExceptionProvider.StopSasProgressiveHitReporting();

                response.GroupId = firstPendingClaim.ProgressiveGroupId;
                response.LevelId = firstPendingClaim.LevelId;
                response.WinAmount = firstPendingClaim.ClaimStatus.WinAmount;
                response.Handlers = new HostAcknowledgementHandler
                {
                    ImpliedAckHandler = () => ProgressiveWinAcknowledgedAsync(firstPendingClaim)
                };
            }

            return response;
        }

        private void ProgressiveWinAcknowledgedAsync(IViewableLinkedProgressiveLevel level)
        {
            Task.Run(() => ProgressiveWinAcknowledged(level));
        }

        private void ProgressiveWinAcknowledged(IViewableLinkedProgressiveLevel level)
        {
            // Get the group id we are configured for
            var groupId = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .ProgressiveGroupId;

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                _protocolLinkedProgressiveAdapter.ClaimLinkedProgressiveLevel(level.LevelName, ProgressiveConstants.ProtocolName);
                _protocolLinkedProgressiveAdapter.AwardLinkedProgressiveLevel(level.LevelName, ProgressiveConstants.ProtocolName);
                scope.Complete();
            }

            if (_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Any(
                    x =>
                        x.ClaimStatus.Status == LinkedClaimState.Hit &&
                        x.ProgressiveGroupId == groupId &&
                        x.ProtocolName == ProgressiveConstants.ProtocolName))
            {
                // If we still have claims left we need to keep reporting
                _hitExceptionProvider.StartReportingSasProgressiveHit();
            }
        }
    }
}