namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Contracts.SASProperties;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Progressive;

    /// <summary>
    ///     Handles the <see cref="ProgressiveHitEvent" /> event and figured out which exception needs to be reported.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class ProgressiveHitConsumer : IProtocolProgressiveEventHandler, IConsumer<ProgressiveHitEvent>
    {
        private readonly IProgressiveHitExceptionProvider _hitExceptionProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IProgressiveWinDetailsProvider _progressiveWinDetailsProvider;

        /// <inheritdoc />
        public ProgressiveHitConsumer(
            IProgressiveHitExceptionProvider hitExceptionProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPropertiesManager propertiesManager,
            IProgressiveWinDetailsProvider progressiveWinDetailsProvider)
        {
            _hitExceptionProvider = hitExceptionProvider ?? throw new ArgumentNullException(nameof(hitExceptionProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _propertiesManager = propertiesManager ?? throw new ArgumentException(nameof(propertiesManager));
            _progressiveWinDetailsProvider = progressiveWinDetailsProvider ?? throw new ArgumentNullException(nameof(progressiveWinDetailsProvider));
        }

        /// <summary>
        /// Consumes the progressive hit event.
        /// </summary>
        /// <param name="theEvent">The progressive hit even to consume.</param>
        public void Consume(ProgressiveHitEvent theEvent)
        {
            var assignedProgressiveId = theEvent.Level.AssignedProgressiveId;
            var configuredProgressiveGroupId = _propertiesManager
                .GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ProgressiveGroupId;

            if (assignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(assignedProgressiveId.AssignedProgressiveKey, out var level) &&
                level.ProtocolName == ProgressiveConstants.ProtocolName &&
                level.ProgressiveGroupId == configuredProgressiveGroupId)
            {
                _hitExceptionProvider.StartReportingSasProgressiveHit();
            }
            else
            {
                _progressiveWinDetailsProvider.AddNonSasProgressiveLevelWin(theEvent.Level, theEvent.Jackpot);
                _hitExceptionProvider.ReportNonSasProgressiveHit();
            }
        }

        /// <inheritdoc />
        public void HandleProgressiveEvent<T>(T data)
        {
            Consume(data as ProgressiveHitEvent);
        }

    }
}