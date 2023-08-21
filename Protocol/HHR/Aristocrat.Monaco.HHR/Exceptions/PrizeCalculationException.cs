using System;
using System.Runtime.Serialization;

namespace Aristocrat.Monaco.Hhr.Exceptions
{
	[Serializable]
    public class PrizeCalculationException : Exception
    {
        public string Expected { get; set; }

        public string Calculated { get; set; }

        public PrizeCalculationException(string expected, string calculated)
        {
            Expected = expected;
            Calculated = calculated;
        }
        public override string Message => $"Prize calculation error. Expected prize: {Expected}, Calculated Prize: {Calculated}";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Expected", Expected);
            info.AddValue("Calculated", Calculated);
        }
    }
}