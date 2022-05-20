namespace Aristocrat.Monaco.Gaming
{
    using Contracts;
    using System;

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

        public long Id { get; set; }

        public long Value { get; set; }

        public bool Active { get; set; }

        public TimeSpan PreviousActiveTime { get; set; }

        public DateTime ActiveDate { get; set; }

        public int MinimumWagerCredits { get; set; }

        public int MaximumWagerCredits { get; set; }

        public int MaximumWagerOutsideCredits { get; set; }

        public string BetOption { get; set; }

        public string LineOption { get; set; }

        public int BonusBet { get; set; }

        public bool SecondaryAllowed { get; set; }

        public bool SecondaryEnabled { get; set; }

        public bool LetItRideAllowed { get; set; }

        public bool LetItRideEnabled { get; set; }

        public Denomination ShallowCopy()
        {
            return (Denomination)MemberwiseClone();
        }
    }
}