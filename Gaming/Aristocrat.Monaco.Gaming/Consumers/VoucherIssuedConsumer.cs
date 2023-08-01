namespace Aristocrat.Monaco.Gaming.Consumers
{
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Handles the OperatorMenuEnteredEvent, which sets the cabinet's state.
    /// </summary>
    public class VoucherIssuedConsumer : Consumes<VoucherIssuedEvent>
    {
        private readonly IMessageDisplay _messageDisplay;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherIssuedConsumer" /> class.
        /// </summary>
        public VoucherIssuedConsumer(
            IMessageDisplay messageDisplay,
            IPropertiesManager properties)
        {
            _messageDisplay = messageDisplay;
            _properties = properties;
        }

        /// <inheritdoc />
        public override void Consume(VoucherIssuedEvent theEvent)
        {
            if (theEvent.Transaction == null)
            {
                return;
            }

            if ((bool)_properties.GetProperty(GamingConstants.DisplayVoucherIssuedMessage, true))
            {
                _messageDisplay.DisplayMessage(
                    new DisplayableMessage(
                        () => Localizer.DynamicCulture().GetString(ResourceKeys.VoucherIssued) + " " +
                              theEvent.Transaction.Amount.MillicentsToDollars().FormattedCurrencyString(),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherIssuedEvent)));
            }
        }
    }
}