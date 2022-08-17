namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;
    using static System.FormattableString;

    /// <summary>
    ///     Represents the game round details from the facade
    /// </summary>
    [Serializable]
    public class GameRoundDetails  : ICloneable
    {
        /// <summary>
        ///     Gets or sets the presentation index
        /// </summary>
        public long PresentationIndex { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"GameRoundDetails [{PresentationIndex}]");
        }
    }
}