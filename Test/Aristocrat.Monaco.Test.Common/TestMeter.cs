namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using Application.Contracts;

    public class TestMeter : IMeter
    {
        public TestMeter(string name, MeterClassification classification)
        {
            Name = name;
            Classification = classification;
        }

        public string Name { get; set; }

        public MeterClassification Classification { get; set; }

        public long Lifetime { get; set; }

        public long Period => throw new NotImplementedException();

        public long Session => throw new NotImplementedException();

        public event EventHandler<MeterChangedEventArgs> MeterChangedEvent;

        public void Increment(long amount)
        {
            Lifetime = amount;
            MeterChangedEvent?.Invoke(this, new MeterChangedEventArgs(amount));
        }
    }
}