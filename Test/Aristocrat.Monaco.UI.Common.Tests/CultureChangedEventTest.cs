namespace Aristocrat.Monaco.UI.Common.Tests
{
    #region Using

    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Tests for the CultureChangedEvent class
    /// </summary>
    [TestClass]
    public class CultureChangedEventTest
    {
        [TestMethod]
        public void Constructor1Test()
        {
            var target = new CultureChangedEvent();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            CultureInfo newCulture = new CultureInfo("fr-CA");
            var target = new CultureChangedEvent(newCulture);

            Assert.IsNotNull(target);
            Assert.AreEqual(newCulture, target.NewCulture);
        }
    }
}