namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;
    /// <summary>
    ///     Data class for LP A0 handler's response
    /// </summary>
    public class LongPollSendEnabledFeaturesResponse : LongPollResponse
    {
        /// <summary>
        ///     Provides bit flag locations for Feature Codes 1 (table 7.14c)
        /// </summary>
        [Flags]
        public enum Features1
        {
            JackpotMultiplier = 1,
            AftBonusAwards = 1 << 1,
            LegacyBonusAwards = 1 << 2,
            Tournament = 1 << 3,
            ValidationExtensions = 1 << 4,
            ValidationStyleBit0 = 1 << 5,
            ValidationStyleBit1 = 1 << 6,
            TicketRedemption = 1 << 7
        }
        /// <summary>
        ///     Provides bit flag locations for Feature Codes 2 (table 7.14d)
        /// </summary>
        [Flags]
        public enum Features2
        {
            MeterModelBit0 = 1,
            MeterModelBit1 = 1 << 1,
            TicketsToTotalDropAndTotalCancelledCredits = 1 << 2,
            ExtendedMeters = 1 << 3,
            ComponentAuthentication = 1 << 4,
            JackpotKeyoffToMachinePayException = 1 << 5,
            AdvancedFundTransfer = 1 << 6,
            MultidenomExtensions = 1 << 7
        }
        /// <summary>
        ///     Provides bit flag locations for Feature Codes 3 (table 7.14e)
        /// </summary>
        [Flags]
        public enum Features3
        {
            MaxPollingRateBit0 = 1,
            MultipleSasProgressiveWinReporting = 1 << 1,
            MeterChangeNotification = 1 << 2,
            // Reserved bit 3
            SessionPlay = 1 << 4,
            ForeignCurrencyRedemption = 1 << 5,
            NonSasProgressiveHitReporting = 1 << 6,
            EnhancedProgressiveDataReporting = 1 << 7
        }
        /// <summary>
        ///     Provides bit flag locations for Feature Codes 4 (table 7.14f)
        /// </summary>
        [Flags]
        public enum Features4
        {
            MaxProgressivePayback = 1
            // Reserved bits 1-7
        }
        /// <summary>
        ///     Generates a new empty LongPollSendEnabledFeaturesResponse
        /// </summary>
        public LongPollSendEnabledFeaturesResponse()
        {
        }

        /// <summary>
        ///     Generates a new LongPollSendEnabledFeaturesResponse with the flag data filled in.
        /// </summary>
        /// <param name="newFeatures1Data">Flags for Features1.</param>
        /// <param name="newFeatures2Data">Flags for Features2.</param>
        /// <param name="newFeatures3Data">Flags for Features3.</param>
        /// <param name="newFeatures4Data">Flags for Features4.</param>
        public LongPollSendEnabledFeaturesResponse(Features1 newFeatures1Data, Features2 newFeatures2Data, Features3 newFeatures3Data, Features4 newFeatures4Data)
        {
            Features1Data = newFeatures1Data;
            Features2Data = newFeatures2Data;
            Features3Data = newFeatures3Data;
            Features4Data = newFeatures4Data;
        }
        /// <summary>
        ///     Byte containing Features1 flag data.
        /// </summary>
        public Features1 Features1Data { get; set; }
        /// <summary>
        ///     Byte containing Features2 flag data.
        /// </summary>
        public Features2 Features2Data { get; set; }
        /// <summary>
        ///     Byte containing Features3 flag data.
        /// </summary>
        public Features3 Features3Data { get; set; }
        /// <summary>
        ///     Byte containing Features4 flag data.
        /// </summary>
        public Features4 Features4Data { get; set; }
    }
}
