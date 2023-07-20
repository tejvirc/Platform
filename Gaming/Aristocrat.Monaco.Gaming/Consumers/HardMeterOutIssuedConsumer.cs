namespace Aristocrat.Monaco.Gaming.Consumers
{
    using Accounting.Contracts;
    using Accounting.Contracts.HandCount;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Handles the HardMeterOutCompletedEvent
    /// </summary>
    public class HardMeterOutIssuedConsumer : Consumes<HardMeterOutCompletedEvent>
    {
        private readonly IMessageDisplay _messageDisplay;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardMeterOutIssuedConsumer" /> class.
        /// </summary>
        public HardMeterOutIssuedConsumer(
            IMessageDisplay messageDisplay,
            IPropertiesManager properties)
        {
            _messageDisplay = messageDisplay;
            _properties = properties;
        }

        /// <inheritdoc />
        public override void Consume(HardMeterOutCompletedEvent theEvent)
        {
            if (theEvent.Transaction == null)
            {
                return;
            }

            if (_properties.GetValue(GamingConstants.DisplayVoucherIssuedMessage, true))
            {
                _messageDisplay.DisplayMessage(
                    new DisplayableMessage(
                        () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.VoucherIssued) + " " +
                              theEvent.Transaction.Amount.MillicentsToDollars().FormattedCurrencyString(),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(HardMeterOutCompletedEvent)));
            }
        }
    }
}