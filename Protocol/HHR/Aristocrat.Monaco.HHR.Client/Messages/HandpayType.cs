namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    /// Represents the different handpay types in a transaction
    /// </summary>
    public enum HandpayType
    {
        /// <summary>
        ///     Progressive
        /// </summary>
        HandpayTypeProgressive,

        ///big win - not progressive
        HandpayTypeNonProgressive,

        ///cancelled credits
        HandpayTypeCancelledCredits,

        ///not a Hand pay
        HandpayTypeNone
    }
}