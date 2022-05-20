namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public enum ResetMethod : byte
    {
        StandardHandpay = 0x00,
        ResetToTheCreditMeter = 0x01
    }

    public enum AckCode : byte
    {
        ResetMethodEnabled = 0x00,
        UnableToEnableResetMethod = 0x01,
        NotCurrentlyInAHandpayCondition = 0x02
    }

    /// <inheritdoc />
    public class EnableJackpotHandpayResetMethodData : LongPollData
    {
        /// <summary>
        ///     Gets or sets the reset method
        /// </summary>
        public ResetMethod Method { get; set; }
    }

    /// <inheritdoc />
    public class EnableJackpotHandpayResetMethodResponse : LongPollResponse
    {
        /// <summary>
        ///     Creates a EnableJackpotHandpayResetMethodResponse instance
        /// </summary>
        /// <param name="code">The ACK code to be returned</param>
        public EnableJackpotHandpayResetMethodResponse(AckCode code)
        {
            Code = code;
        }

        /// <summary>
        ///     Gets the validation number status
        /// </summary>
        public AckCode Code { get; }
    }
}