namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Kernel;
    using Services.Reporting;
    using Sas.Contracts.SASProperties;

    /// <summary>
    ///     Handles the <see cref="PropertyChangedEvent"/> event and filters it
    ///     to only notify when the property name is "Sas.FeatureSettings"
    ///     This event is sent by the properties manager when a property changes
    /// </summary>
    public class SasFeatureSettingsPropertyChangedConsumer : Consumes<PropertyChangedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;
        private readonly IPropertiesManager _propertiesManager;
        private bool _aftBonusAllowed;

        public SasFeatureSettingsPropertyChangedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService,
            IPropertiesManager propertiesManager)
            : base(eventBus, consumerContext, @event => @event.PropertyName == SasProperties.SasFeatureSettings)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _aftBonusAllowed =
                ((SasFeatures)_propertiesManager.GetProperty(SasProperties.SasFeatureSettings, new SasFeatures()))
                .AftBonusAllowed;
        }

        public override void Consume(PropertyChangedEvent theEvent)
        {
            // watch for Sas.SasFeatureSettings.AftBonusAllowed to change
            var aftBonusAllowed =
                ((SasFeatures)_propertiesManager.GetProperty(SasProperties.SasFeatureSettings, new SasFeatures()))
                .AftBonusAllowed;

            if (aftBonusAllowed != _aftBonusAllowed)
            {
                ReportCurrentSetting(aftBonusAllowed);
                _aftBonusAllowed = aftBonusAllowed;
            }
        }

        private void ReportCurrentSetting(bool aftBonusAllowed)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(
                aftBonusAllowed ? ReportableEvent.BonusingEnabled : ReportableEvent.BonusingDisabled);
        }
    }
}