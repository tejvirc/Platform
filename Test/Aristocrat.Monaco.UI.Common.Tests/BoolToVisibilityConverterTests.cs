namespace Aristocrat.Monaco.UI.Common.Tests
{
    #region Using

    using System.Windows;
    using Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Tests for the BoolToVisibilityConverter class
    /// </summary>
    [TestClass]
    public class BoolToVisibilityConverterTests
    {
        private readonly BoolToVisibilityConverter _target = new BoolToVisibilityConverter();

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void TrueAndFalseValueTest()
        {
            Assert.AreEqual(Visibility.Visible, _target.TrueValue);
            _target.TrueValue = Visibility.Hidden;
            Assert.AreEqual(Visibility.Hidden, _target.TrueValue);

            Assert.AreEqual(Visibility.Collapsed, _target.FalseValue);
            _target.FalseValue = Visibility.Visible;
            Assert.AreEqual(Visibility.Visible, _target.FalseValue);
        }

        [TestMethod]
        public void ConvertTest()
        {
            Assert.AreEqual(Visibility.Collapsed, _target.Convert(false, null, null, null));
            Assert.AreEqual(Visibility.Visible, _target.Convert(true, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, _target.Convert("Test", null, null, null));
        }

        [TestMethod]
        public void ConvertBackTest()
        {
            Assert.AreEqual(true, _target.ConvertBack(Visibility.Visible, null, null, null));
            Assert.AreEqual(false, _target.ConvertBack(Visibility.Collapsed, null, null, null));
            Assert.IsNull(_target.ConvertBack("Test", null, null, null));
        }
    }
}