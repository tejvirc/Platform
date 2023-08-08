namespace Aristocrat.Monaco.Gaming.Consumers
{
    using Accounting.Contracts.Hopper;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Handles the HopperPayOutCompletedEvent.
    /// </summary>
    public class HopperPayOutConsumer : Consumes<HopperPayOutCompletedEvent>
    {
        private readonly IMessageDisplay _messageDisplay;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HopperPayOutConsumer" /> class.
        /// </summary>
        public HopperPayOutConsumer(IMessageDisplay messageDisplay)
        {
            _messageDisplay = messageDisplay;
        }

        /// <inheritdoc />
        public override void Consume(HopperPayOutCompletedEvent theEvent)
        {
            _messageDisplay.DisplayMessage(
                new DisplayableMessage(
                    () => Localizer.For(CultureFor.PlayerTicket).FormatString(ResourceKeys.HopperPayOut, theEvent.Amount.MillicentsToDollars().FormattedCurrencyString()),
                    DisplayableMessageClassification.Informative,
                    DisplayableMessagePriority.Normal,
                    typeof(HopperPayOutCompletedEvent)));
        }
    }
}
