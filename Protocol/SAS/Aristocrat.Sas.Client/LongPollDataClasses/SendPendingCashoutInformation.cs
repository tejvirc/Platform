namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public enum CashoutTypeCode
    {
        CashableTicket = 0x00,
        RestrictedPromotionalTicket = 0x01,
        NotWaitingForSystemValidation = 0x80
    }

    /// <inheritdoc />
    public class SendPendingCashoutInformation : LongPollResponse
    {
        /// <summary>
        ///     Creates a SendPendingCashoutInformation instance
        /// </summary>
        /// <param name="typeCode">the cashout type</param>
        /// <param name="amount">The amount</param>
        public SendPendingCashoutInformation(CashoutTypeCode typeCode, ulong amount)
            : this(true, typeCode, amount)
        {
        }

        /// <summary>
        ///     Creates a SendPendingCashoutInformation instance
        /// </summary>
        /// <param name="validResponse">Whether or not the response is valid</param>
        /// <param name="typeCode">the cashout type</param>
        /// <param name="amount">The amount</param>
        public SendPendingCashoutInformation(
            bool validResponse,
            CashoutTypeCode typeCode = CashoutTypeCode.CashableTicket,
            ulong amount = ulong.MinValue)
        {
            ValidResponse = validResponse;
            TypeCode = typeCode;
            Amount = amount;
        }

        /// <summary>
        ///     Gets whether or not the response is valid
        /// </summary>
        public bool ValidResponse { get; }

        /// <summary>
        ///     Gets the cashout type
        /// </summary>
        public CashoutTypeCode TypeCode { get; }

        /// <summary>
        ///     Gets the amount
        /// </summary>
        public ulong Amount { get; }
    }
}