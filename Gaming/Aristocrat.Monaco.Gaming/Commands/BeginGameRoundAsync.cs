namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Central;

    /// <summary>
    ///     Begin game round command
    /// </summary>
    public class BeginGameRoundAsync
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BeginGameRoundAsync" /> class.
        /// </summary>
        /// <param name="denom">The denom of the game round.</param>
        /// <param name="wager">The initial wager amount for the game round</param>
        /// <param name="betLinePresetId">The bet-line-preset-id for the game round</param>
        /// <param name="data">The initial recovery blob</param>
        /// <param name="request">Outcome request(s)</param>
        /// <param name="wagerCategoryId">Wager category for the game round</param>///
        /// <param name="additionalInfo">information about additional games to play</param>
        public BeginGameRoundAsync(
            long denom,
            long wager,
            int betLinePresetId,
            byte[] data,
            IOutcomeRequest request,
            int wagerCategoryId,
            IEnumerable<IAdditionalGamePlayInfo> additionalInfo)
        {
            Denom = denom;
            Wager = wager;
            BetLinePresetId = betLinePresetId;
            Data = data;
            Request = request;
            WagerCategoryId = wagerCategoryId;
            AdditionalInfo = additionalInfo;
        }

        /// <summary>
        ///     Gets the selected denomination.
        /// </summary>
        public long Denom { get; }

        /// <summary>
        ///     Gets the initial wager amount
        /// </summary>
        public long Wager { get; }

        /// <summary>
        ///     Gets the bet-line-preset-id
        /// </summary>
        public int BetLinePresetId { get; }

        /// <summary>
        ///     Gets the wager category id
        /// </summary>
        public int WagerCategoryId { get; }

        /// <summary>
        ///     Gets the recovery blob associated with beginning of the game round
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        ///     Gets the outcome request
        /// </summary>
        public IOutcomeRequest Request { get; }

        /// <summary>
        ///     Info for additional games
        /// </summary>
        public IEnumerable<IAdditionalGamePlayInfo> AdditionalInfo { get; }
    }
}