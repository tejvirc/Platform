namespace Aristocrat.Bingo.Client.Messages
{
    /// <summary>
    ///     Progressive contribution request message
    /// </summary>
    public class ProgressiveContributionRequestMessage : IMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveContributionRequestMessage" /> class.
        /// </summary>
        /// <param name="coinIn">The wager amount to contribute</param>
        /// <param name="machineSerial">The machine serial number</param>
        /// <param name="gameTitleId">The game title id</param>
        /// <param name="initialCoin">Whether this is the initial coin contribution</param>
        /// <param name="offlineCoin">Whether this is an offline coin contribution</param>
        /// <param name="denomination">The denomination</param>
        public ProgressiveContributionRequestMessage(
            long coinIn,
            string machineSerial,
            int gameTitleId,
            bool initialCoin,
            bool offlineCoin,
            int denomination)
        {
            CoinIn = coinIn;
            MachineSerial = machineSerial;
            GameTitleId = gameTitleId;
            InitialCoin = initialCoin;
            OfflineCoin = offlineCoin;
            Denomination = denomination;
        }

        /// <summary>
        ///     The wager amount to contribute
        /// </summary>
        public long CoinIn { get; }

        /// <summary>
        ///     The machine serial number
        /// </summary>
        public string MachineSerial { get; }

        /// <summary>
        ///     The game title id for the game making the contribution
        /// </summary>
        public int GameTitleId { get; }

        /// <summary>
        ///     Whether this is the initial coin contribution
        /// </summary>
        public bool InitialCoin { get; }

        /// <summary>
        ///     Whether this is an offline coin contribution
        /// </summary>
        public bool OfflineCoin { get; }

        /// <summary>
        ///     The denomination for the game making the contribution
        /// </summary>
        public int Denomination { get; }
    }
}