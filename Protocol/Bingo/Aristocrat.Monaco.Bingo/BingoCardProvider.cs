namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.GameOverlay;

    public class BingoCardProvider : IBingoCardProvider
    {
        private readonly IPseudoRandomNumberGenerator _mersenneRng;

        /// <summary>
        ///     Constructor called by dependency injection and unit tests
        /// </summary>
        /// <param name="mersenneTwister">The MersenneTwister Random number generator</param>
        public BingoCardProvider(IPseudoRandomNumberGenerator mersenneTwister)
        {
            _mersenneRng = mersenneTwister ?? throw new ArgumentNullException(nameof(mersenneTwister));
        }

        /// <inheritdoc />
        public BingoCard GetCardBySerial(uint cardSerial)
        {
            _mersenneRng.Seed(cardSerial);
            var card = new BingoCard { SerialNumber = cardSerial };

            // used to prevent duplicate numbers on the bingo card
            var isNumberAlreadyUsed = new HashSet<int>();

            for (var row = 0; row < BingoConstants.BingoCardDimension; ++row)
            {
                for (var col = 0; col < BingoConstants.BingoCardDimension; ++col)
                {
                    if (col == BingoConstants.MidRowColumnIndex && row == BingoConstants.MidRowColumnIndex)
                    {
                        // Create a Free Square.
                        card.Numbers[row, col] = new BingoNumber(BingoConstants.CardCenterNumber, BingoNumberState.CardInitial);
                    }
                    else
                    {
                        var randomNumber = 0;
                        while (randomNumber == 0 || isNumberAlreadyUsed.Contains(randomNumber))
                        {
                            randomNumber = col * BingoConstants.ColumnNumberRange +
                                           (int)_mersenneRng.Random(1, BingoConstants.ColumnNumberRange);
                        }

                        isNumberAlreadyUsed.Add(randomNumber);
                        card.Numbers[row, col] = new BingoNumber(randomNumber, BingoNumberState.CardInitial);
                    }
                }
            }

            return card;
        }
    }
}