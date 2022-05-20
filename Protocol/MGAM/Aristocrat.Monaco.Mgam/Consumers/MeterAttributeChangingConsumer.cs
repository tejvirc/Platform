namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Common.Events;
    using Services.Attributes;

    /// <summary>
    ///     Consumes the <see cref="MeterAttributeChangingEvent"/>.
    /// </summary>
    public class MeterAttributeChangingConsumer : Consumes<MeterAttributeChangingEvent>
    {
        private readonly ILogger _logger;
        private readonly IAttributeManager _attributes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterAttributeChangingConsumer"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="attributes"><see cref="IAttributeManager"/></param>
        public MeterAttributeChangingConsumer(
            ILogger<MeterAttributeChangingConsumer> logger,
            IAttributeManager attributes)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        /// <inheritdoc />
        public override async Task Consume(MeterAttributeChangingEvent theEvent, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug($"Sending attribute '{theEvent.AttributeName}':{theEvent.AttributeAmount}");
                _attributes.Set(theEvent.AttributeName, theEvent.AttributeAmount, AttributeSyncBehavior.LocalAndServer);
            }
            catch (ServerResponseException ex)
            {
                _logger.LogError(ex, $"Error setting meter attributes '{theEvent.AttributeName}':{theEvent.AttributeAmount}");
            }

            await Task.CompletedTask;
        }
    }
}
