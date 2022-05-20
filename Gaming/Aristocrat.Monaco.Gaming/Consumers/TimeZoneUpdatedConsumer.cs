namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Application.Contracts;
    using Kernel;
    using Runtime;

    public class TimeZoneUpdatedConsumer : Consumes<TimeZoneUpdatedEvent>
    {
        private readonly IPropertiesManager _properties;
        private readonly IRuntime _runtime;

        public TimeZoneUpdatedConsumer(IRuntime runtime, IPropertiesManager properties)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public override void Consume(TimeZoneUpdatedEvent theEvent)
        {
            _runtime.UpdateLocalTimeTranslationBias(
                _properties.GetValue(ApplicationConstants.TimeZoneBias, TimeSpan.Zero));
        }
    }
}