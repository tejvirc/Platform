namespace Aristocrat.Monaco.Bingo.Common.GameOverlay
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    ///     Bingo number
    /// </summary>
    public struct BingoNumber : IEquatable<BingoNumber>
    {
        /// <summary>
        ///     Constructor for <see cref="BingoNumber"/>
        /// </summary>
        /// <param name="number">Number value.</param>
        /// <param name="state">State.</param>
        [JsonConstructor]
        public BingoNumber(int number, BingoNumberState state)
        {
            Number = number;
            State = state;
        }

        /// <summary>
        ///     Get or set the number value.
        /// </summary>
        public int Number { get; }

        /// <summary>
        ///     Get or set the state.
        /// </summary>
        public BingoNumberState State { get; }

        public bool Equals(BingoNumber other)
        {
            return Number == other.Number && State == other.State;
        }

        public override bool Equals(object obj)
        {
            return obj is BingoNumber other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Number * 397) ^ (int)State;
            }
        }

        public static bool operator ==(BingoNumber left, BingoNumber right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BingoNumber left, BingoNumber right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"Ball={Number}, State={State}";
        }
    }
}
