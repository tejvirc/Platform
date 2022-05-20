namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for SerializableRuntimeNode
    /// </summary>
    [TestClass]
    public class SerializableRuntimeNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new SerializableRuntimeNode();
            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.Imports.Count);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expected = "Test assembly";
            var list = new List<SerializableImportNode>()
            {
                new SerializableImportNode(),
                new SerializableImportNode(expected)
            };

            var target = new SerializableRuntimeNode(list);
            Assert.IsNotNull(target);
            Assert.AreEqual(list, target.Imports);
        }

        [TestMethod]
        public void SerializableAddinDependenciesTest()
        {
            string expected = "Test assembly";
            var list = new List<SerializableImportNode>()
            {
                new SerializableImportNode(),
                new SerializableImportNode(expected)
            };

            var target = new SerializableRuntimeNode { SerializableDependencies = list.ToArray() };

            Assert.AreEqual(2, target.Imports.Count);
            Assert.IsNull(target.SerializableDependencies[0].Assembly);
            Assert.AreEqual(expected, target.SerializableDependencies[1].Assembly);
        }

        [TestMethod]
        public void SerializableAddinDependenciesNullValueTest()
        {
            var target = new SerializableRuntimeNode { SerializableDependencies = null };

            Assert.AreEqual(0, target.SerializableDependencies.Length);
        }
    }
}