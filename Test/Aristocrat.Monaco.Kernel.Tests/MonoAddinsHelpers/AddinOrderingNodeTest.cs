namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for AddinOrderingNodeTest
    /// </summary>
    [TestClass]
    public class AddinOrderingNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ExtensionPathTest()
        {
            string expected = "Test path";
            var target = new AddinOrderingNode();
            Assert.IsNotNull(target);

            Assert.IsNull(target.ExtensionPath);
            target.ExtensionPath = expected;
            Assert.AreEqual(expected, target.ExtensionPath);
        }

        [TestMethod]
        public void DefaultOrderedAddinBehaviorTest()
        {
            var expected = new DefaultOrderedAddinBehavior();
            var target = new AddinOrderingNode();

            Assert.IsNotNull(target.DefaultOrderedAddinBehavior);
            target.DefaultOrderedAddinBehavior = expected;
            Assert.AreEqual(expected, target.DefaultOrderedAddinBehavior);
        }

        [TestMethod]
        public void OrderedAddinsTypesNullValueTest()
        {
            var target = new AddinOrderingNode { OrderedAddinsTypes = null };

            Assert.AreEqual(0, target.OrderedAddinsTypes.Length);
        }

        [TestMethod]
        public void OrderedAddinsTypesTest()
        {
            var list = new List<OrderedAddinTypeNode> { new OrderedAddinTypeNode() };
            var target = new AddinOrderingNode { OrderedAddinsTypes = list.ToArray() };

            Assert.AreEqual(1, target.OrderedAddinsTypes.Length);
        }
    }
}