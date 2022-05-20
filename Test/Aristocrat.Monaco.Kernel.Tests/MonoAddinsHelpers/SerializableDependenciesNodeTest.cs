namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for SerializableDependenciesNodeTest
    /// </summary>
    [TestClass]
    public class SerializableDependenciesNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new SerializableDependenciesNode();
            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.AddinDependencies.Count);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expected1 = "Test id";
            string expected2 = "Test version";
            var list = new List<SerializableAddinDependencyNode>()
            {
                new SerializableAddinDependencyNode(),
                new SerializableAddinDependencyNode(expected1, expected2)
            };

            var target = new SerializableDependenciesNode(list);
            Assert.IsNotNull(target);
            Assert.AreEqual(list, target.AddinDependencies);
        }

        [TestMethod]
        public void SerializableAddinDependenciesTest()
        {
            string expected1 = "Test id";
            string expected2 = "Test version";
            var list = new List<SerializableAddinDependencyNode>()
            {
                new SerializableAddinDependencyNode(),
                new SerializableAddinDependencyNode(expected1, expected2)
            };

            var target = new SerializableDependenciesNode { SerializableAddinDependencies = list.ToArray() };

            Assert.AreEqual(2, target.AddinDependencies.Count);
            Assert.IsNull(target.SerializableAddinDependencies[0].Id);
            Assert.IsNull(target.SerializableAddinDependencies[0].Version);
            Assert.AreEqual(expected1, target.SerializableAddinDependencies[1].Id);
            Assert.AreEqual(expected2, target.SerializableAddinDependencies[1].Version);
        }

        [TestMethod]
        public void SerializableAddinDependenciesNullValueTest()
        {
            var target = new SerializableDependenciesNode { SerializableAddinDependencies = null };

            Assert.AreEqual(0, target.AddinDependencies.Count);
        }
    }
}