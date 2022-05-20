namespace Aristocrat.Monaco.Application.Contracts.Tests.Metering
{
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Tests for PercentageMeterClassification
    /// </summary>
    [TestClass]
    public class PercentageMeterClassificationTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            PercentageMeterClassification target = new PercentageMeterClassification();
            Assert.AreEqual("Percentage", target.Name);
            Assert.AreEqual(99999999, target.UpperBounds);
        }

        [TestMethod]
        public void CreateValueStringTest()
        {
            // make sure the culture on the dev machines and build server are the
            // same or this test could fail.
            var savedCurrentCulture = CultureInfo.CurrentCulture;
            var newCultureInfo = new CultureInfo("en-US", false)
            {
                NumberFormat = { PercentPositivePattern = 0, PercentNegativePattern = 0 }
            };
            CultureInfo.CurrentCulture = newCultureInfo;

            PercentageMeterClassification target = new PercentageMeterClassification();

            // Percentage meter values are multiplied by PercentageMeterClassification.Multiplier
            // so that they can be of type 'long', like all other meters.  The CreateValueString()
            // method divides this off to get back to a double, before applying the format string.
            long value = 0;
            Assert.AreEqual("0.00 %", target.CreateValueString(value));

            value = 1; // 0.0001 after division
            Assert.AreEqual("0.01 %", target.CreateValueString(value));

            value = 1234; // 0.1234 after division
            Assert.AreEqual("12.34 %", target.CreateValueString(value));

            value = 10000; // 1 after division
            Assert.AreEqual("100.00 %", target.CreateValueString(value));

            CultureInfo.CurrentCulture = savedCurrentCulture;
        }
    }
}
