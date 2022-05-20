namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for FilterableExtensionNodeTest
    /// </summary>
    [TestClass]
    public class FilterableExtensionNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new FilterableExtensionNode();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expected1 = "Test filter";
            var target = new FilterableExtensionNode(expected1);
            Assert.IsNotNull(target);
            Assert.AreEqual(expected1, target.FilterId);
        }

        [TestMethod]
        public void FilterIdTest()
        {
            string expected = "Test name";
            var target = new FilterableExtensionNode();
            Assert.IsNull(target.FilterId);

            target.FilterId = expected;
            Assert.AreEqual(expected, target.FilterId);
        }
    }
}