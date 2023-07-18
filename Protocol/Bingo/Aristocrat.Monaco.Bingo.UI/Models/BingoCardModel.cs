namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Common;
    using Common.GameOverlay;
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     Data to display within <see cref="ViewModels.OperatorMenu.BingoGameHistoryDetailsViewModel"/>;
    ///     contains the data for a single card and its winning patterns, if any.
    /// </summary>
    public class BingoCardModel : ObservableObject
    {
        private const int ExpectedNumberCount = BingoConstants.BingoCardDimension * BingoConstants.BingoCardDimension;

        /// <summary>
        ///     Constructor for <see cref="BingoCardModel"/>.
        /// </summary>
        /// <param name="numbers">
        ///     The numbers on the bingo card, including the Free Space represented as <see cref="BingoConstants.CardCenterNumber"/>.
        /// </param>
        /// <param name="patterns">The list of patterns won on the card; if none, this should be empty and non-null.</param>
        /// <param name="serialNumber">The unique identifier of the card.</param>
        /// <param name="ballCallDaubs">
        ///     The bit daubing on the card from the matching ball call numbers.
        ///     This is used to populate <see cref="BallCallDaubedNumbers"/>.
        /// </param>
        public BingoCardModel(
            ObservableCollection<BingoNumberModel> numbers,
            IList<BingoPatternModel> patterns,
            uint serialNumber,
            int ballCallDaubs)
        {
            Numbers = numbers ?? throw new ArgumentNullException(nameof(numbers));
            Patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
            SerialNumber = serialNumber.ToString();
            BallCallBitDaubs = ballCallDaubs;

            if (numbers.Count != ExpectedNumberCount)
            {
                throw new ArgumentException(nameof(numbers));
            }

            CreateCardLayout();
            SetPatternDaubNumbers();
        }

        public int BallCallBitDaubs { get; }

        public string SerialNumber { get; }

        public ObservableCollection<BingoNumberModel> Numbers { get; }

        public IList<BingoPatternModel> Patterns { get; }

        public IList<int> BallCallDaubedNumbers { get; } = new List<int>();

        /// <summary>
        ///     Uses the given <see cref="patternDaubs"/> to update the card numbers with the pattern daubing,
        ///     and uses <see cref="BallCallDaubedNumbers"/> to update the card numbers with the ball call daubing.
        /// </summary>
        /// <param name="patternDaubs">A list of card numbers that are daubed by the current pattern.</param>
        public void UpdateDaubs(IList<int> patternDaubs)
        {
            patternDaubs ??= new List<int>();

            foreach (var number in Numbers)
            {
                number.State = BallCallDaubedNumbers.Contains(number.Number)
                    ? BingoNumberState.BallCallInitial
                    : BingoNumberState.CardInitial;
                number.Daubed = patternDaubs.Contains(number.Number);
            }

            RaisePropertyChanged(nameof(Numbers));
        }

        /// <summary>
        ///     Generates the list of card numbers daubed by the ball call.
        /// </summary>
        private void CreateCardLayout()
        {
            var daubed = new BitArray(new[] { BallCallBitDaubs });
            for (var i = 0; i < BingoConstants.BingoCardSquares; i++)
            {
                if (daubed[i])
                {
                    BallCallDaubedNumbers.Add(Numbers.ElementAt(i).Number);
                }
            }
        }

        /// <summary>
        ///     For all patterns won on the card, use the pattern bit daubing to get the list of
        ///     card numbers daubed by the pattern.
        /// </summary>
        private void SetPatternDaubNumbers()
        {
            foreach (var pattern in Patterns)
            {
                var daubed = new BitArray(new[] { pattern.BitDaubs });
                for (var i = 0; i < BingoConstants.BingoCardSquares; i++)
                {
                    if (daubed[i])
                    {
                        pattern.DaubedNumbers.Add(Numbers.ElementAt(i).Number);
                    }
                }
            }
        }
    }
}
