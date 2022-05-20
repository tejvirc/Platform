namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     Represents the data reported in exception 4F
    /// </summary>
    public class BillData
    {
        /// <summary>
        ///     Gets or sets amount in cents of a bill
        /// </summary>
        public long AmountInCents { get; set; }

        /// <summary>
        ///     Gets or sets the sas country code
        /// </summary>
        public BillAcceptorCountryCode CountryCode { get; set; }

        /// <summary>
        ///     Gets or sets the lifetime bill count of this type
        /// </summary>
        public long LifetimeCount { get; set; }
    }
}
