namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Kernel;
    using Services.Reporting;
    using Sas.Contracts.Events;
    using Sas.Contracts.SASProperties;

    /// <summary>
    ///     Handles the <see cref="PropertyChangedEvent"/> event and filters it
    ///     to only notify when the property name is "Sas.FeatureSettings"
    ///     This event is sent by the properties manager when a property changes
    /// </summary>
    public class SasFeatureSettingsPropertyChangedConsumer : Consumes<PropertyChangedEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private bool _aftBonusAllowed;
        private bool _legacyBonusingAllowed;

        public SasFeatureSettingsPropertyChangedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IPropertiesManager propertiesManager)
            : base(eventBus, consumerContext, @event => @event.PropertyName == SasProperties.SasFeatureSettings)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            var sasFeatures = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            _aftBonusAllowed = sasFeatures.AftBonusAllowed;
            _legacyBonusingAllowed = sasFeatures.LegacyBonusAllowed;
        }

        public override void Consume(PropertyChangedEvent theEvent)
        {
            // watch for Sas.SasFeatureSettings.AftBonusAllowed or Sas.SasFeatureSettings.LegacyBonusAllowed to change
            var sasFeatures = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            var aftBonusAllowed = sasFeatures.AftBonusAllowed;
            var legacyBonusingAllowed = sasFeatures.LegacyBonusAllowed;
            var valueChanged = false;

            if (aftBonusAllowed != _aftBonusAllowed)
            {
                _aftBonusAllowed = aftBonusAllowed;
                valueChanged = true;
            }

            if (legacyBonusingAllowed != _legacyBonusingAllowed)
            {
                _legacyBonusingAllowed = legacyBonusingAllowed;
                valueChanged = true;
            }

            if (valueChanged)
            {
                _eventBus.Publish(new RestartProtocolEvent());
            }
        }
    }
}