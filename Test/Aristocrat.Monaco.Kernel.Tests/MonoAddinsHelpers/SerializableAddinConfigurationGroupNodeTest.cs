namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for SerializableAddinConfigurationGroupNode
    /// </summary>
    [TestClass]
    public class SerializableAddinConfigurationGroupNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SerializableAddinConfigurationGroupNodeConstructor1Test()
        {
            var target = new SerializableAddinConfigurationGroupNode();

            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.AddinConfigurationGroupNodes.Count);
        }

        [TestMethod]
        public void SerializableAddinConfigurationGroupNodeConstructor2Test()
        {
            var path = "Test path";
            var target = new SerializableAddinConfigurationGroupNode(path);

            Assert.IsNotNull(target);
            Assert.AreEqual(path, target.Path);
            Assert.AreEqual(0, target.AddinConfigurationGroupNodes.Count);
        }

        [TestMethod]
        public void SerializableAddinConfigurationGroupNodeConstructor3Test()
        {
            var path = "Test path";
            var nodes = new List<AddinConfigurationGroupNode>();
            var target = new SerializableAddinConfigurationGroupNode(path, nodes);

            Assert.IsNotNull(target);
            Assert.AreEqual(path, target.Path);
            Assert.AreEqual(0, target.AddinConfigurationGroupNodes.Count);
        }

        [TestMethod]
        public void SerializableAddinConfigurationGroupNodesTest()
        {
            var target = new SerializableAddinConfigurationGroupNode();
            AddinConfigurationGroupNode[] data = { new AddinConfigurationGroupNode("Hello", "World") };

            target.SerializableAddinConfigurationGroupNodes = data;
            Assert.AreEqual(data[0].Description, target.SerializableAddinConfigurationGroupNodes[0].Description);

            // ensure data doesn't change
            target.SerializableAddinConfigurationGroupNodes = null;
            Assert.AreEqual(data[0].Description, target.SerializableAddinConfigurationGroupNodes[0].Description);
        }
    }
}