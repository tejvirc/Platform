namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <inheritdoc />
    public class SendLastAcceptedBillInformationResponse : LongPollResponse
    {
        /// <summary>
        ///     The Send Last Accepted Bill Information Response message
        /// </summary>
        /// <param name="countryCode">The SAS country code</param>
        /// <param name="denomCode">The SAS denom code</param>
        /// <param name="count">The bill count</param>
        public SendLastAcceptedBillInformationResponse(BillAcceptorCountryCode countryCode, BillDenominationCodes denomCode, ulong count)
        {
            CountryCode = countryCode;
            DenominationCode = denomCode;
            Count = count;
        }

        public SendLastAcceptedBillInformationResponse()
            : this(0, 0, 0)
        {
        }

        /// <summary>
        ///     Gets SAS country code
        /// </summary>
        public BillAcceptorCountryCode CountryCode { get; }
        /// <summary>
        ///     Gets SAS denomination code
        /// </summary>
        public BillDenominationCodes DenominationCode { get; }
        /// <summary>
        ///     Gets the count of last bill
        /// </summary>
        public ulong Count { get; }
    }
}
