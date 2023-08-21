namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public class LongPollReadWagerData : LongPollData
    {
        /// <summary>
        ///     Creates an instance of the LongPollReadWagerData class
        /// </summary>
        /// <param name="gameId">game number to look up</param>
        /// <param name="wagerCategory">wagerCategory for that game</param>
        public LongPollReadWagerData(int gameId, int wagerCategory)
        {
            GameId = gameId;
            WagerCategory = wagerCategory;
        }

        public LongPollReadWagerData()
        {
        }

        public int GameId { get; set; }

        public int WagerCategory { get; set; }

        public long AccountingDenom { get; set; }
    }

    /// <summary>
    ///     Data class to hold the response of a Wager data read
    /// </summary>
    public class LongPollSendWagerResponse : LongPollResponse
    {
        /// <summary>
        ///     Creates an instance of the LongPollSendWagerResponse class
        /// </summary>
        /// <param name="paybackPercentage">wager playback percent</param>
        /// <param name="coinInMeter">coins in meter value</param>
        /// <param name="coinInMeterLength">coin in meter length</param>
        /// <param name="isValid">true, if valid</param>
        public LongPollSendWagerResponse(int paybackPercentage, int coinInMeter, int coinInMeterLength, bool isValid)
        {
            PaybackPercentage = paybackPercentage;
            CoinInMeter = coinInMeter;
            CoinInMeterLength = coinInMeterLength;
            IsValid = isValid;
        }

        public LongPollSendWagerResponse()
        {
            PaybackPercentage = 0;
            CoinInMeter = 0;
            CoinInMeterLength = 0;
            IsValid = false;
        }

        public int PaybackPercentage { get; set; }
        public int CoinInMeter { get; set; }
        public int CoinInMeterLength { get; set; }
        public bool IsValid { get; set; }
    }
}
