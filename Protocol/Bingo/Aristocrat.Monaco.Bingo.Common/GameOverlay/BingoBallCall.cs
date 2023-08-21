namespace Aristocrat.Monaco.Bingo.Common.GameOverlay
{
    using System.Collections.Generic;

    /// <summary>
    ///     Represent ball call.
    /// </summary>
    public class BingoBallCall
    {
        /// <summary>
        ///     Constructor for <see cref="BingoBallCall"/>
        /// </summary>
        public BingoBallCall()
        {
            Numbers = new List<BingoNumber>();
        }

        /// <summary>
        ///     Constructor for <see cref="BingoBallCall"/>
        /// </summary>
        /// <param name="numbers">Numbers list</param>
        public BingoBallCall(List<BingoNumber> numbers)
        {
            Numbers = numbers;
        }

        /// <summary>
        ///     Get the numbers list.
        /// </summary>
        public List<BingoNumber> Numbers { get; }
    }
}
