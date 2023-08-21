namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a BetOption manifest object.
    /// </summary>
    /// <remarks>
    ///     A set of bet options that map to the buttons a user can press.
    /// There can be multiple sets in the file that the game can switch between.
    /// Name formats are usually "n to N" ex. "1 to 5" or "1 to 10".
    /// https://confy.aristocrat.com/display/ConfyOverhaulPOC/%5BBetlines%5D+Changing+Betlines+-+Info+and+Gap+Analysis
    /// </remarks>
    public class BetOption : IEquatable<BetOption>
    {
        /// <summary>
        ///     Poker bonus bets must be multiple of 5.
        /// </summary>
        public const int PokerBonusBetMultiple = 5;

        /// <summary>
        ///     The unique identifier for the BetOption.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Defines the Description of the BetOption.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Defines the Max Initial Bet(wager credits) of the BetOption if defined.
        /// </summary>
        public int? MaxInitialBet { get; set; }

        /// <summary>
        ///     Defines the Max Total Bet(wager credits) of the BetOption if defined.
        /// </summary>
        public int? MaxTotalBet { get; set; }

        /// <summary>
        ///     Gets or sets the Bets list.
        /// </summary>
        public IEnumerable<Bet> Bets { get; set; }

        /// <summary>
        ///     Gets or sets the BonusBets list.
        /// </summary>
        public IEnumerable<int> BonusBets { get; set; }

        /// <summary>
        /// Gets or sets the game-driven MaxWin, value in credits.
        /// </summary>
        public long? MaxWin { get; set; }

        /// <summary>
        ///     Gets or sets the BetLinePreset.
        /// </summary>
        public string BetLinePreset { get; set; }

        /// <inheritdoc />
        public bool Equals(BetOption other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Name == other.Name;
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

            if (obj.GetType() != typeof(BetOption))
            {
                return false;
            }

            return Equals((BetOption)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        /// <summary>
        ///     Checks for equality.
        /// </summary>
        public static bool operator ==(BetOption left, BetOption right)

        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Checks for inequality.
        /// </summary>
        public static bool operator !=(BetOption left, BetOption right)
        {
            return !Equals(left, right);
        }
    }
}