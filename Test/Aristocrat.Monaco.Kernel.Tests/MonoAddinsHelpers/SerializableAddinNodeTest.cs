namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml.Serialization;
    using Test.Common.UnitTesting;
    #endregion

    /// <summary>
    ///     Summary description for SerializableAddinNodeTest
    /// </summary>
    [TestClass]
    public class SerializableAddinNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new SerializableAddinNode();
            Assert.IsNotNull(target);

            Assert.IsNull(target.Id);
            Assert.IsNull(target.Namespace);
            Assert.IsNull(target.Version);
            Assert.IsFalse(target.IsRoot);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string id = "Test id";
            string addinNamespace = "Test addin namespace";
            string version = "Test version";
            var target = new SerializableAddinNode(id, addinNamespace, version, true);
            Assert.IsNotNull(target);

            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(addinNamespace, target.Namespace);
            Assert.AreEqual(version, target.Version);
            Assert.IsTrue(target.IsRoot);

            Assert.IsNotNull(target.Runtime);
            Assert.IsNotNull(target.Dependencies);
            Assert.IsNotNull(target.Extensions);
        }

        [TestMethod]
        public void Constructor3Test()
        {
            string id = "Test id";
            string addinNamespace = "Test addin namespace";
            string version = "Test version";
            var runtime = new SerializableRuntimeNode();
            var dependancies = new SerializableDependenciesNode();
            var extensions = new LinkedList<SerializableBaseExtensionNode>();

            var target = new SerializableAddinNode(
                id,
                addinNamespace,
                version,
                true,
                runtime,
                dependancies,
                extensions);
            Assert.IsNotNull(target);

            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(addinNamespace, target.Namespace);
            Assert.AreEqual(version, target.Version);
            Assert.IsTrue(target.IsRoot);

            Assert.AreEqual(runtime, target.Runtime);
            Assert.AreEqual(dependancies, target.Dependencies);
            Assert.AreEqual(extensions, target.Extensions);
        }

        [TestMethod]
        public void SerializableExtensionsNullValueTest()
        {
            var target = new SerializableAddinNode();

            target.SerializableExtensions = null;
            Assert.IsNotNull(target.SerializableExtensions);
        }

        [TestMethod]
        public void SerializableExtensionsTest()
        {
            var expected = new List<SerializableBaseExtensionNode>()
            {
                new SerializableAddinConfigurationGroupNode(),
                new SerializableAddinConfigurationGroupNode()
            };

            var target = new SerializableAddinNode();

            target.SerializableExtensions = expected.ToArray();

            Assert.AreEqual(2, target.SerializableExtensions.Length);
        }

        [TestMethod]
        public void XmlOverridesTest()
        {
            var target = new SerializableAddinNode();
            var _targetPrivateObject = new PrivateObject(target);
            var results = (XmlAttributeOverrides)_targetPrivateObject.GetProperty(
                "XmlOverrides",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.AreEqual(typeof(XmlAttributeOverrides), results.GetType());
        }

        [TestMethod]
        public void ToXmlDocumentTest()
        {
            var target = new SerializableAddinNode();
            var results = target.ToXmlDocument();
            Assert.AreEqual("#document", results.Name);
        }
    }
}