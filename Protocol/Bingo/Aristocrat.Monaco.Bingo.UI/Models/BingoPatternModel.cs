namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System.Collections.Generic;
    using Application.Contracts.Extensions;
    using MVVM.Model;

    /// <summary>
    ///     Data to display within <see cref="ViewModels.OperatorMenu.BingoGameHistoryDetailsViewModel"/>;
    ///     contains the data for a single pattern.
    /// </summary>
    public class BingoPatternModel : BaseNotify
    {
        /// <summary>
        ///     Constructor for <see cref="BingoPatternModel"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the pattern.</param>
        /// <param name="name">The name of the pattern.</param>
        /// <param name="ballQuantity">The number of balls in which the pattern was achieved.</param>
        /// <param name="bitDaubs">
        ///     The card daubing for the pattern represented in bits.
        ///     This is used to populate <see cref="DaubedNumbers"/>.
        /// </param>
        /// <param name="winAmount">
        ///     The amount awarded by the pattern, in credits. This value is converted to
        ///     currency format for display.
        /// </param>
        public BingoPatternModel(
            int id,
            string name,
            int ballQuantity,
            int bitDaubs,
            long winAmount)
        {
            Id = id.ToString();
            Name = name;
            BallQuantity = ballQuantity.ToString();
            WinAmount = winAmount.CentsToDollars().FormattedCurrencyString();
            BitDaubs = bitDaubs;
        }
        
        public string Id { get; }
        
        public string Name { get; }

        public string BallQuantity { get; }

        public string WinAmount { get; }

        public int BitDaubs { get; }

        public IList<int> DaubedNumbers { get; set; } = new List<int>();
    }
}
