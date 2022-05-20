namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Progressive;
    using LongPollAck = Aristocrat.Sas.Client.LongPollDataClasses.LongPollReadSingleValueResponse<bool>;

    /// <summary>
    ///     Handles the single level progressive broadcast data
    /// </summary>
    public class LP80SingleLevelProgressiveBroadcastHandler :
        ISasLongPollHandler<LongPollAck, SingleLevelProgressiveBroadcastData>
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LP80SingleLevelProgressiveBroadcastHandler" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="protocolLinkedProgressiveAdapter">An <see cref="IProtocolLinkedProgressiveAdapter" /> instance.</param>
        /// <param name="propertiesManager">The properties manager</param>
        public LP80SingleLevelProgressiveBroadcastHandler(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPropertiesManager propertiesManager)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                         throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _propertiesManager = propertiesManager ??
                                 throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.SingleLevelProgressiveBroadcastValue };

        /// <inheritdoc />
        public LongPollAck Handle(SingleLevelProgressiveBroadcastData data)
        {
            var groupId = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .ProgressiveGroupId;

            if (data.ProgId != groupId)
            {
                return new LongPollAck(true);
            }

            var expirationDate =
                DateTime.UtcNow.AddMilliseconds(ProgressiveConstants.LevelTimeout).ToUniversalTime();

            var levels = new List<LinkedProgressiveLevel>
            {
                new LinkedProgressiveLevel
                {
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = data.ProgId,
                    LevelId = data.LevelId,
                    Amount = data.LevelAmount,
                    Expiration = expirationDate,
                    CurrentErrorStatus = ProgressiveErrors.None
                }
            };

            _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevelsAsync(levels, ProgressiveConstants.ProtocolName);
            return new LongPollAck(true);
        }
    }
}