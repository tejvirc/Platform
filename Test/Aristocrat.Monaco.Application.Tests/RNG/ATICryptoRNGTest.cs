namespace Aristocrat.Monaco.Application.Tests.RNG
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Aristocrat.CryptoRng;
    using Moq;
    using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class ATICryptoRNGTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void BasicHistogramTest()
        {
            var state = new Mock<IRandomStateProvider>();

            var rng = new AtiCryptoRng(state.Object);

            var histogram = new SortedDictionary<ulong, uint>();

            ulong numRandomValues = 0;

            ulong rangeCovered = 0;
            const ulong maxRange = 100;

            const ulong valuesNeeded = maxRange * 10000;

            while (numRandomValues < valuesNeeded)
            {
                var randomValue = rng.GetValue(maxRange);
                numRandomValues++;
                var hcount = !histogram.ContainsKey(randomValue) ? 0 : histogram[randomValue];

                histogram[randomValue] = hcount + 1;

                rangeCovered++;
                if (rangeCovered == maxRange)
                {
                    rangeCovered = 0;
                }
            }

            uint missingValues = 0;
            var min = uint.MaxValue;
            var max = uint.MinValue;
            float avg = 0;
            for (ulong i = 0; i < maxRange; i++)
            {
                if (!histogram.ContainsKey(i))
                {
                    missingValues++;
                }
                else
                {
                    var value = histogram[i];
                    avg += value;
                    if (value < min)
                    {
                        min = value;
                    }

                    if (value > max)
                    {
                        max = value;
                    }
                }
            }

            avg /= maxRange;
            float variance = 0;
            for (ulong i = 0; i < maxRange; i++)
            {
                if (!histogram.ContainsKey(i))
                {
                    continue;
                }

                var value = histogram[i];
                var diff = value - avg;
                variance += diff * diff;
            }

            variance /= maxRange;
            var standardDeviation = Math.Round(Math.Sqrt(variance), 2);
            var rsd = Math.Round(standardDeviation * 100 / avg);

            // In generating a number of values many times the max value,
            // we expect zero holes, and a relative standard deviation approaching 1%.
            IsFalse(missingValues > 0);
            IsFalse(rsd > 2);
        }

        [TestMethod]
        public void CryptoRangeTest()
        {
            var state = new Mock<IRandomStateProvider>();

            var rng = new AtiCryptoRng(state.Object);
            RangeTest(rng);
        }

        private static void RangeTest(IRandom rng)
        {
            for (var i = 0; i < 64; i++)
            {
                var range = (ulong)(1 << i);
                var val = rng.GetValue(range);
                IsTrue(val <= range);
            }

            var ranges = new Collection<ulong>();
            for (var i = 0; i < 64; i++)
            {
                var range = (ulong)(1 << i);
                ranges.Add(range);
            }

            var values = new Collection<ulong>();
            rng.GetValues(ranges, values);

            IsTrue(values.Count == ranges.Count);
            for (var i = 0; i < values.Count; i++)
            {
                IsTrue(values[i] <= ranges[i]);
            }
        }
    }
}