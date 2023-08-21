namespace Aristocrat.Monaco.UI.Common.Tests
{
    #region Using

    using System;
    using Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Tests for the ExpirationTextValueConvert class
    /// </summary>
    [TestClass]
    public class ExpirationTextValueConvertTests
    {
        private const string DaysFormatter = "{0} Days";
        private const string NeverExpires = "Never Expires";

        [TestMethod]
        public void ConvertTestNoDisplayText()
        {
            var target = new ExpirationTextValueConvert(DaysFormatter, NeverExpires, false);
            Assert.AreEqual("1", target.Convert(1, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(0, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert(null, null, null, null));
            Assert.AreEqual(string.Empty, target.Convert("Test", null, null, null));
        }

        [TestMethod]
        public void ConvertTestDisplayText()
        {
            var target = new ExpirationTextValueConvert(DaysFormatter, NeverExpires, true);
            Assert.AreEqual(string.Format(DaysFormatter, 1), target.Convert(1, null, null, null));
            Assert.AreEqual(NeverExpires, target.Convert(0, null, null, null));
            Assert.AreEqual(NeverExpires, target.Convert(null, null, null, null));
            Assert.AreEqual(NeverExpires, target.Convert("Test", null, null, null));
        }

        [TestMethod]
        public void ConvertBackTest()
        {
            var target = new ExpirationTextValueConvert(DaysFormatter, NeverExpires, false);
            Assert.AreEqual(string.Empty, target.ConvertBack(string.Format(DaysFormatter, 1), null, null, null));
        }
    }
}