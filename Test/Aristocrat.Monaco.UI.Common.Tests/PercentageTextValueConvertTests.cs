namespace Aristocrat.Monaco.UI.Common.Tests
{
    #region Using

    using System;
    using Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Tests for the PercentageTextValueConvert class
    /// </summary>
    [TestClass]
    public class PercentageTextValueConvertTests
    {
        private const string PercentEditingFormatter = "{0:0.0}";
        private const string PercentDisplayFormatter = "{0:0.0}%";
        private const string RatioEditingFormatter = "{0:0.00}";
        private const string RatioDisplayFormatter = "{0:0.00}x Wager";
        private const string ZeroPercentageString = "Zero percent";

        [TestMethod]
        public void Convert_Percent_NoDisplayText()
        {
            var target = new PercentageTextValueConvert(PercentEditingFormatter, PercentDisplayFormatter, ZeroPercentageString, false);
            Assert.AreEqual("1.0", target.Convert(1m, null, null, null));
            Assert.AreEqual("23.5", target.Convert(23.45m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(null, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert("Test", null, null, null));
        }

        [TestMethod]
        public void Convert_Ratio_NoDisplayText()
        {
            var target = new PercentageTextValueConvert(RatioEditingFormatter, RatioDisplayFormatter, ZeroPercentageString, false);
            Assert.AreEqual("1.00", target.Convert(1m, null, null, null));
            Assert.AreEqual("23.45", target.Convert(23.45m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(null, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert("Test", null, null, null));
        }

        [TestMethod]
        public void Convert_Percent_DisplayText()
        {
            var target = new PercentageTextValueConvert(PercentEditingFormatter, PercentDisplayFormatter, string.Empty, true);
            Assert.AreEqual("1.0%", target.Convert(1m, null, null, null));
            Assert.AreEqual("23.5%", target.Convert(23.45m, null, null, null));
            Assert.AreEqual("0.0%", target.Convert(0m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(null, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert("Test", null, null, null));
        }

        [TestMethod]
        public void Convert_Ratio_DisplayText()
        {
            var target = new PercentageTextValueConvert(RatioEditingFormatter, RatioDisplayFormatter, string.Empty, true);
            Assert.AreEqual("1.00x Wager", target.Convert(1m, null, null, null));
            Assert.AreEqual("23.45x Wager", target.Convert(23.45m, null, null, null));
            Assert.AreEqual("0.00x Wager", target.Convert(0m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(null, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert("Test", null, null, null));
        }

        [TestMethod]
        public void Convert_Percent_WithZeroString()
        {
            var target = new PercentageTextValueConvert(PercentEditingFormatter, PercentDisplayFormatter, ZeroPercentageString, true);
            Assert.AreEqual("1.0%", target.Convert(1m, null, null, null));
            Assert.AreEqual("1.1%", target.Convert(1.12m, null, null, null));
            Assert.AreEqual(ZeroPercentageString, target.Convert(0m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(null, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert("Test", null, null, null));
        }

        [TestMethod]
        public void Convert_NoDisplayText_WithNullInputs()
        {
            var target = new PercentageTextValueConvert(null, null, null, false);
            Assert.AreEqual(string.Empty, target.Convert(1m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(1.12m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(null, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert("Test", null, null, null));
        }

        [TestMethod]
        public void Convert_DisplayText_WithNullInputs()
        {
            var target = new PercentageTextValueConvert(null, null, null, true);
            Assert.AreEqual(string.Empty, target.Convert(1m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(1.12m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0m, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(null, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert("Test", null, null, null));
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ConvertBackTest()
        {
            var target = new PercentageTextValueConvert(RatioEditingFormatter, RatioDisplayFormatter, ZeroPercentageString, false);
            target.ConvertBack("123", null, null, null);
        }
    }
}