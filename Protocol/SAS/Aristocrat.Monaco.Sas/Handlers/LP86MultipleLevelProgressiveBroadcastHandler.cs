namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Progressive;
    using LongPollAck = Aristocrat.Sas.Client.LongPollDataClasses.LongPollReadSingleValueResponse<bool>;

    /// <summary>
    ///     Handles the Multiple level progressive broadcast data
    /// </summary>
    public class LP86MultipleLevelProgressiveBroadcastHandler :
        ISasLongPollHandler<LongPollAck, MultipleLevelProgressiveBroadcastData>
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IPropertiesManager _propertyProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LP86MultipleLevelProgressiveBroadcastHandler" /> class.
        /// </summary>
        /// <param name="protocolLinkedProgressiveAdapter">An <see cref="IProtocolLinkedProgressiveAdapter" /> instance.</param>
        /// <param name="propertyProvider">The property manager</param>
        public LP86MultipleLevelProgressiveBroadcastHandler(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPropertiesManager propertyProvider)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                         throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _propertyProvider = propertyProvider ?? throw new ArgumentNullException(nameof(propertyProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } = new List<LongPoll>
        {
            LongPoll.MultipleLevelProgressiveBroadcastValues
        };

        /// <inheritdoc />
        public LongPollAck Handle(MultipleLevelProgressiveBroadcastData data)
        {
            var configuredProgressiveGroupId = _propertyProvider
                .GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ProgressiveGroupId;
            var expirationDate = DateTime.UtcNow.AddMilliseconds(ProgressiveConstants.LevelTimeout);
            var levels = data.LevelInfo.Select(
                item => new LinkedProgressiveLevel
                {
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = data.ProgId,
                    LevelId = item.Key,
                    Amount = item.Value,
                    Expiration = expirationDate,
                    CurrentErrorStatus = ProgressiveErrors.None
                }).Where(l => l.ProgressiveGroupId == configuredProgressiveGroupId).ToList();

            if (levels.Any())
            {
                _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevelsAsync(levels, ProgressiveConstants.ProtocolName);
            }

            return new LongPollAck(true);
        }
    }
}