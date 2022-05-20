namespace Aristocrat.Monaco.Sas.HandPay
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Data class to hold the response of a handpay info
    /// </summary>
    [Serializable]
    public class HandpayDataResponse
    {
        /// <summary>
        ///     Converts the handpay data to the long poll response
        /// </summary>
        /// <param name="response">The response to convert</param>
        /// <returns>The converted data</returns>
        public static explicit operator LongPollHandpayDataResponse(HandpayDataResponse response) =>
            new()
            {
                ProgressiveGroup = response.ProgressiveGroup,
                Level = response.Level,
                Amount = response.Amount,
                PartialPayAmount = response.PartialPayAmount,
                SessionGamePayAmount = response.SessionGamePayAmount,
                SessionGameWinAmount = response.SessionGameWinAmount,
                ResetId = response.ResetId,
                TransactionId = response.TransactionId
            };

        /// <summary>
        ///     Converts the long poll response to handpay data
        /// </summary>
        /// <param name="response">The response to convert</param>
        /// <returns>The converted data</returns>
        public static explicit operator HandpayDataResponse(LongPollHandpayDataResponse response) =>
            new()
            {
                ProgressiveGroup = response.ProgressiveGroup,
                Level = response.Level,
                Amount = response.Amount,
                PartialPayAmount = response.PartialPayAmount,
                SessionGamePayAmount = response.SessionGamePayAmount,
                SessionGameWinAmount = response.SessionGameWinAmount,
                ResetId = response.ResetId,
                TransactionId = response.TransactionId
            };

        /// <summary>
        ///     Gets or sets the progressive group
        /// </summary>
        public uint ProgressiveGroup { get; set; }

        /// <summary>
        ///     Gets or sets the level
        /// </summary>
        public LevelId Level { get; set; }

        /// <summary>
        ///     Gets or sets the amount
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets or sets the partial pay amount
        /// </summary>
        public long PartialPayAmount { get; set; }

        /// <summary>
        ///     Gets or sets the reset id
        /// </summary>
        public ResetId ResetId { get; set; }

        /// <summary>
        ///     Gets or sets the session game win amount
        /// </summary>
        public long SessionGameWinAmount { get; set; }

        /// <summary>
        ///     Gets or sets the session game pay amount
        /// </summary>
        public long SessionGamePayAmount { get; set; }

        /// <summary>
        ///     Gets or sets the transaction id
        /// </summary>
        public long TransactionId { get; set; }
    }
}