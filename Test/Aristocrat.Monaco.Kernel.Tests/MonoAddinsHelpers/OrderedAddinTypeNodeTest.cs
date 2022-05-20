namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for OrderedAddinTypeNodeTest
    /// </summary>
    [TestClass]
    public class OrderedAddinTypeNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void AddinOrderingTest()
        {
            string expected = "test";
            OrderedAddinTypeNode target = new OrderedAddinTypeNode();
            Assert.IsNotNull(target);

            target.TypeString = expected;
            var actual = target.TypeString;

            Assert.AreEqual(expected, actual);
        }
    }
}