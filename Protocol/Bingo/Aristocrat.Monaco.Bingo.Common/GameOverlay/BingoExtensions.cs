namespace Aristocrat.Monaco.Bingo.Common.GameOverlay
{
    /// <summary>
    ///     Extensions for bingo related extensions
    /// </summary>
    public static class BingoExtensions
    {
        /// <summary>
        ///     Converts the bit pattern into a multi-denominational array for each space relating to the bingo card
        /// </summary>
        /// <param name="bitPattern">The bit masked daubed pattern</param>
        /// <returns>The multi-denominational array for each space of the bingo card saying if it is daubed</returns>
        public static bool[][] BitPatternToFlags(this int bitPattern)
        {
            var flags = new bool[BingoConstants.BingoCardDimension][];
            for (var row = BingoConstants.BingoCardDimension - 1; row >= 0; row--)
            {
                flags[row] = new bool[BingoConstants.BingoCardDimension];
                for (var col = BingoConstants.BingoCardDimension - 1; col >= 0; col--)
                {
                    flags[row][col] = (bitPattern & 1) == 1;
                    bitPattern >>= 1;
                }
            }

            return flags;
        }
    }
}