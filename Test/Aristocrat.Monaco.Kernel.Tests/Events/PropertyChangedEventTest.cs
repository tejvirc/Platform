namespace Aristocrat.Monaco.Kernel.Tests.Events
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for PropertyChangedEventTest
    /// </summary>
    [TestClass]
    public class PropertyChangedEventTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            PropertyChangedEvent target = new PropertyChangedEvent();

            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expectedPropertyName = "test";
            PropertyChangedEvent target = new PropertyChangedEvent(expectedPropertyName);

            Assert.IsNotNull(target);
            Assert.AreEqual(expectedPropertyName, target.PropertyName);
        }
    }
}