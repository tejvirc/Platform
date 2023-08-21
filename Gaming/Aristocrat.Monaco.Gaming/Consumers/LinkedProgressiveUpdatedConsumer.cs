namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts.Progressives.Linked;
    using Progressives;

    /// <summary>
    ///     Consumes the LinkedProgressiveUpdated event and handles related logic.
    /// </summary>
    public class LinkedProgressiveUpdatedConsumer : Consumes<LinkedProgressiveUpdatedEvent>
    {
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IProgressiveConfigurationProvider _progressiveConfiguration;

        public LinkedProgressiveUpdatedConsumer(IProgressiveGameProvider progressiveGameProvider, IProgressiveConfigurationProvider progressiveConfiguration)
        {
            _progressiveGameProvider = progressiveGameProvider ??
                                       throw new ArgumentNullException(nameof(progressiveGameProvider));
            _progressiveConfiguration = progressiveConfiguration ?? throw new ArgumentNullException(nameof(progressiveConfiguration));
        }

        public override void Consume(LinkedProgressiveUpdatedEvent theEvent)
        {
            _progressiveConfiguration.ValidateLinkedProgressivesUpdates(theEvent.LinkedProgressiveLevels);
            _progressiveGameProvider.UpdateLinkedProgressiveLevels(theEvent.LinkedProgressiveLevels);
        }
    }
}