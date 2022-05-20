namespace Aristocrat.Monaco.Sas.Tests
{
    using Gaming.Contracts;
    using System;

    internal class MockDenomination : IDenomination
    {
        public MockDenomination(long value)
            : this(value, value)
        {
        }

        public MockDenomination(long value, long id, bool active = true)
        {
            Id = id;
            Value = value;
            Active = active;
        }

        public long Id { get; }

        public long Value { get; }

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
    }
}