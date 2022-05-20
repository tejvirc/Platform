namespace Aristocrat.Monaco.Gaming.Tests
{
    using System.Collections.ObjectModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PRNGLib;

    [TestClass]
    public class NonGamingRngTest
    {
        [TestMethod]
        public void NonCryptoRangeTest()
        {
            var rng = new MSCryptoRNG();

            RangeTest(rng);
        }

        private static void RangeTest(IPRNG rng)
        {
            for (var i = 0; i < 64; i++)
            {
                var range = (ulong)(1 << i);
                var val = rng.GetValue(range);
                Assert.IsTrue(val <= range);
            }

            var ranges = new Collection<ulong>();
            for (var i = 0; i < 64; i++)
            {
                var range = (ulong)(1 << i);
                ranges.Add(range);
            }

            var values = new Collection<ulong>();
            rng.GetValues(ranges, values);

            Assert.IsTrue(values.Count == ranges.Count);
            for (var i = 0; i < values.Count; i++)
            {
                Assert.IsTrue(values[i] <= ranges[i]);
            }
        }
    }
}