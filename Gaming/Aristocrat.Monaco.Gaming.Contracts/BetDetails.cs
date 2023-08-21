namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    ///     Some paytables vary the paytable based on bet details.
    /// </summary>
    public class BetDetails : IBetDetails, IEquatable<BetDetails>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BetDetails" /> class.
        /// </summary>
        /// <param name="betLinePresetId">Bet line preset ID</param>
        /// <param name="betPerLine">Bet per line</param>
        /// <param name="numberLines">Number of lines bet</param>
        /// <param name="ante">Ante</param>
        /// <param name="stake">Stake</param>
        [JsonConstructor]
        public BetDetails(
            int betLinePresetId,
            int betPerLine,
            int numberLines,
            int ante,
            long stake)
        {
            BetLinePresetId = betLinePresetId;
            BetPerLine = betPerLine;
            NumberLines = numberLines;
            Ante = ante;
            Stake = stake;
        }

        /// <inheritdoc />
        public int BetLinePresetId { get; }

        /// <inheritdoc />
        public int BetPerLine { get; }

        /// <inheritdoc />
        public int NumberLines { get; }

        /// <inheritdoc />
        public int Ante { get; }

        /// <inheritdoc />
        public long Stake { get; }

        /// <inheritdoc />
        public bool Equals(BetDetails other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return BetLinePresetId == other.BetLinePresetId && BetPerLine == other.BetPerLine &&
                   NumberLines == other.NumberLines && Ante == other.Ante && Stake == other.Stake;
        }

        /// <summary>
        ///     Checks if the <see cref="BetDetails" /> are equal to each other
        /// </summary>
        /// <param name="left">The left hand side of the equality comparision</param>
        /// <param name="right">The right hand side of the equality comparision</param>
        /// <returns>Whether or not the two <see cref="BetDetails" /> are equal</returns>
        public static bool operator ==(BetDetails left, BetDetails right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Checks if the <see cref="BetDetails" /> are equal to each other
        /// </summary>
        /// <param name="left">The left hand side of the equality comparision</param>
        /// <param name="right">The right hand side of the equality comparision</param>
        /// <returns>Whether or not the two <see cref="BetDetails" /> are equal</returns>
        public static bool operator !=(BetDetails left, BetDetails right)
        {
            return !Equals(left, right);
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

            return obj.GetType() == GetType() && Equals((BetDetails)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BetLinePresetId;
                hashCode = (hashCode * 397) ^ BetPerLine;
                hashCode = (hashCode * 397) ^ NumberLines;
                hashCode = (hashCode * 397) ^ Ante;
                hashCode = (hashCode * 397) ^ Stake.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{typeof(BetDetails)} [BetLinePresetId={BetLinePresetId}, BetPerLine={BetPerLine}, NumberLines={NumberLines}, Ante={Ante}, Stake={Stake}]";
        }
    }
}