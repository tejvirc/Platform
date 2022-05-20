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
    ///     Handles the extended progressive broadcast data
    /// </summary>
    public class LP7AExtendedProgressiveBroadcastHandler :
        ISasLongPollHandler<LongPollAck, ExtendedProgressiveBroadcastData>
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LP7AExtendedProgressiveBroadcastHandler" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="protocolLinkedProgressiveAdapter">Linked progressive provider</param>
        /// <param name="propertiesManager">The properties manager</param>
        public LP7AExtendedProgressiveBroadcastHandler(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPropertiesManager propertiesManager)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                         throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _propertiesManager = propertiesManager ??
                                 throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } =
            new List<LongPoll> { LongPoll.ExtendedProgressiveBroadcast };

        /// <inheritdoc />
        public LongPollAck Handle(ExtendedProgressiveBroadcastData data)
        {
            if (!_propertiesManager.GetValue(SasProperties.EnhancedProgressiveDataReportingKey, false))
            {
                return new LongPollAck(false);
            }

            var configuredProgressiveGroupId =
                _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ProgressiveGroupId;
            if (data.ProgId != configuredProgressiveGroupId)
            {
                return new LongPollAck(true);
            }

            var expirationDate = DateTime.UtcNow.AddMilliseconds(ProgressiveConstants.LevelTimeout);

            var levels = data.LevelInfo.Select(
                    item => new LinkedProgressiveLevel
                    {
                        ProtocolName = ProgressiveConstants.ProtocolName,
                        ProgressiveGroupId = data.ProgId,
                        LevelId = item.Key,
                        Amount = item.Value.Amount,
                        Expiration = expirationDate,
                        CurrentErrorStatus = ProgressiveErrors.None
                    })
                .ToList();

            if (levels.Any())
            {
                _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevelsAsync(levels, ProgressiveConstants.ProtocolName);
            }

            return new LongPollAck(true);
        }
    }
}