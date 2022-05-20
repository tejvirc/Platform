namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for AddinOrderConfigurationNodeTest
    /// </summary>
    [TestClass]
    public class AddinOrderConfigurationNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void AddinOrderingTest()
        {
            List<AddinOrderingNode> node = new List<AddinOrderingNode>() { new AddinOrderingNode() };
            AddinOrderConfigurationNode target = new AddinOrderConfigurationNode();
            Assert.IsNotNull(target);

            var nodes = node.ToArray();
            target.AddinOrderNodes = nodes;
            var actual = target.AddinOrderNodes;

            Assert.AreEqual(nodes, actual);
        }
    }
}