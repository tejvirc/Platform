namespace Aristocrat.Monaco.Gaming
{
    using Contracts;
    using System;

    /// <summary>
    ///     Holds information about a denomination
    /// </summary>
    public class Denomination : IDenomination
    {
        public Denomination()
        {
        }

        public Denomination(long id, long value, bool active)
        {
            Id = id;
            Value = value;
            Active = active;
        }

        /// <inheritdoc />>
        public long Id { get; set; }

        /// <inheritdoc />>
        public long Value { get; set; }

        /// <inheritdoc />>
        public bool Active { get; set; }

        /// <inheritdoc />>
        public TimeSpan PreviousActiveTime { get; set; }

        /// <inheritdoc />>
        public DateTime ActiveDate { get; set; }

        /// <inheritdoc />>
        public int MinimumWagerCredits { get; set; }

        /// <inheritdoc />>
        public int MaximumWagerCredits { get; set; }

        /// <inheritdoc />>
        public int MaximumWagerOutsideCredits { get; set; }

        /// <inheritdoc />>
        public string BetOption { get; set; }

        /// <inheritdoc />>
        public string LineOption { get; set; }

        /// <inheritdoc />>
        public int BonusBet { get; set; }

        /// <inheritdoc />>
        public bool SecondaryAllowed { get; set; }

        /// <inheritdoc />>
        public bool SecondaryEnabled { get; set; }

        /// <inheritdoc />>
        public bool LetItRideAllowed { get; set; }

        /// <inheritdoc />>
        public long DisplayedValue { get; set; }

        /// <inheritdoc />>
        public bool LetItRideEnabled { get; set; }

        /// <summary>
        ///     Perform a shallow copy of the class
        /// </summary>
        /// <returns>a shallow copy of the class</returns>
        public Denomination ShallowCopy()
        {
            return (Denomination)MemberwiseClone();
        }
    }
}