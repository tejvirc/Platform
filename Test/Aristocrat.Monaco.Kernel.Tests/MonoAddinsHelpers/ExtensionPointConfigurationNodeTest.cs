namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Reflection;
    using Test.Common.UnitTesting;
    #endregion

    /// <summary>
    ///     Summary description for ExtensionPointConfigurationNodeTest
    /// </summary>
    [TestClass]
    public class ExtensionPointConfigurationNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new ExtensionPointConfigurationNode();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expected = "Test path";
            var target = new ExtensionPointConfigurationNode(expected);
            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.ExtensionPath);
        }

        [TestMethod]
        public void ExtensionPathTest()
        {
            string expected = "Test path";
            var target = new ExtensionPointConfigurationNode();
            Assert.IsNull(target.ExtensionPath);

            target.ExtensionPath = expected;
            Assert.AreEqual(expected, target.ExtensionPath);
        }

        [TestMethod]
        public void ExtensionNodeSpecificationsNullValueTest()
        {
            var target = new ExtensionPointConfigurationNode();

            Assert.IsNotNull(target.ExtensionNodeSpecifications);
        }

        [TestMethod]
        public void ExtensionNodeSpecificationsTest()
        {
            var expected = new List<NodeSpecificationNode>()
            {
                new NodeSpecificationNode(),
                new NodeSpecificationNode()
            };

            var target = new ExtensionPointConfigurationNode();

            var _targetPrivateObject = new PrivateObject(target);
            _targetPrivateObject.SetField(
                "_extensionNodeSpecifications",
                BindingFlags.NonPublic | BindingFlags.Instance,
                expected);

            Assert.AreEqual(expected, target.ExtensionNodeSpecifications);
        }

        [TestMethod]
        public void SerializableExtensionNodeSpecificationsNullValueTest()
        {
            var target = new ExtensionPointConfigurationNode { SerializableExtensionNodeSpecifications = null };

            Assert.AreEqual(0, target.SerializableExtensionNodeSpecifications.Length);
        }

        [TestMethod]
        public void SerializableExtensionNodeSpecificationsTest()
        {
            var expected = new List<NodeSpecificationNode>()
            {
                new NodeSpecificationNode(),
                new NodeSpecificationNode()
            };

            var target = new ExtensionPointConfigurationNode
            {
                SerializableExtensionNodeSpecifications = expected.ToArray()
            };

            Assert.AreEqual(2, target.SerializableExtensionNodeSpecifications.Length);
        }
    }
}