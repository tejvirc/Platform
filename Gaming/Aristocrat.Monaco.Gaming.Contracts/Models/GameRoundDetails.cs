namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;
    using static System.FormattableString;

    /// <summary>
    ///     Represents the game round details from the facade
    /// </summary>
    [Serializable]
    public class GameRoundDetails  : ICloneable, IEquatable<GameRoundDetails>
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

        /// <summary>
        ///     Implements the equal operator
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(GameRoundDetails other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return PresentationIndex == other.PresentationIndex;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((GameRoundDetails)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return PresentationIndex.GetHashCode();
        }
    }
}