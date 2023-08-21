namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;

    public enum ResetId
    {
        OnlyStandardHandpayResetIsAvailable = 0x00,
        HandpayResetToTheCreditMeterIsAvailable = 0x01
    }

    public enum LevelId
    {
        NonProgressiveWin = 0x00,
        HighestProgressiveLevel = 0x01,
        LowestProgressiveLevel = 0x20,
        NonProgressiveTopWin = 0x40,
        HandpayCanceledCredits = 0x80
    }

    /// <summary>
    ///     Data class to hold the response of a handpay info
    /// </summary>
    [Serializable]
    public class LongPollHandpayDataResponse : LongPollResponse, ICloneable
    {
        /// <summary>
        ///     Creates an instance of the LongPollHandpayDataResponse class
        /// </summary>
        public LongPollHandpayDataResponse()
        {
            ProgressiveGroup = 0;
            Level = 0;
            Amount = 0;
            PartialPayAmount = 0;
            ResetId = 0;
            SessionGameWinAmount = 0;
            SessionGamePayAmount = 0;
            Handlers = null;
        }

        public uint ProgressiveGroup { get; set; }

        public LevelId Level { get; set; }

        public long Amount { get; set; }

        public long PartialPayAmount { get; set; }

        public ResetId ResetId { get; set; }

        public long SessionGameWinAmount { get; set; }

        public long SessionGamePayAmount { get; set; }

        public long TransactionId { get; set; }

        public bool HasProgressiveWin =>
            Level <= LevelId.LowestProgressiveLevel && Level >= LevelId.HighestProgressiveLevel;

        public object Clone()
        {
            return new LongPollHandpayDataResponse
            {
                Amount = Amount,
                ProgressiveGroup = ProgressiveGroup,
                Level = Level,
                PartialPayAmount = PartialPayAmount,
                ResetId = ResetId,
                SessionGamePayAmount = SessionGamePayAmount,
                TransactionId = TransactionId,
                Handlers = Handlers,
                SessionGameWinAmount = SessionGameWinAmount
            };
        }
    }

    public class LongPollHandpayData : LongPollData
    {
        public byte ClientNumber { get; set; }

        public long AccountingDenom { get; set; }
    }
}
