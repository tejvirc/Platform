namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for NodeSpecificationNodeTest
    /// </summary>
    [TestClass]
    public class NodeSpecificationNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new NodeSpecificationNode();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expected1 = "Test addinId";
            int expected2 = 100;
            string expect3 = "Test typeName";
            string expect4 = "Test filterId";
            var target = new NodeSpecificationNode(expected1, expected2, expect3, expect4);
            Assert.IsNotNull(target);
            Assert.AreEqual(expected1, target.AddinId);
            Assert.AreEqual(expected2, target.Order);
            Assert.AreEqual(expect3, target.TypeName);
            Assert.AreEqual(expect4, target.FilterId);
        }

        [TestMethod]
        public void AddinIdPropertyTest()
        {
            string expected = "Test id";
            var target = new NodeSpecificationNode();
            Assert.IsNull(target.AddinId);

            target.AddinId = expected;
            Assert.AreEqual(expected, target.AddinId);
        }

        [TestMethod]
        public void OrderPropertyTest()
        {
            int expected = 1;
            var target = new NodeSpecificationNode();
            Assert.AreEqual(int.MaxValue, target.Order);

            target.Order = expected;
            Assert.AreEqual(expected, target.Order);
        }

        [TestMethod]
        public void TypeNamePropertyTest()
        {
            string expected = "Test type name";
            var target = new NodeSpecificationNode();
            Assert.IsNull(target.TypeName);

            target.TypeName = expected;
            Assert.AreEqual(expected, target.TypeName);
        }

        [TestMethod]
        public void FilterIdPropertyTest()
        {
            string expected = "Test filter";
            var target = new NodeSpecificationNode();
            Assert.IsNull(target.FilterId);

            target.FilterId = expected;
            Assert.AreEqual(expected, target.FilterId);
        }
    }
}