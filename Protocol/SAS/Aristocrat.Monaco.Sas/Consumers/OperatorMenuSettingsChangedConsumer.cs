namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Progressive;

    /// <summary>
    ///     Handles the <see cref="OperatorMenuSettingsChangedEvent" /> event.
    /// </summary>
    public class OperatorMenuSettingsChangedConsumer : Consumes<OperatorMenuSettingsChangedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _properties;
        private readonly IProgressiveWinDetailsProvider _progressiveWinDetailsProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuSettingsChangedConsumer" /> class.
        /// </summary>
        public OperatorMenuSettingsChangedConsumer(ISasExceptionHandler exceptionHandler, IPropertiesManager propertiesManager, IProgressiveWinDetailsProvider progressiveWinDetailsProvider)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _progressiveWinDetailsProvider = progressiveWinDetailsProvider ?? throw new ArgumentNullException(nameof(progressiveWinDetailsProvider));
        }

        /// <inheritdoc />
        public override void Consume(OperatorMenuSettingsChangedEvent theEvent)
        {
            _progressiveWinDetailsProvider.UpdateSettings();

            var sasFeatures = _properties.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            if (sasFeatures.ConfigNotification == ConfigNotificationTypes.ExcludeSAS)
            {
                return;
            }

            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.OperatorChangedOptions));
        }
    }
}
