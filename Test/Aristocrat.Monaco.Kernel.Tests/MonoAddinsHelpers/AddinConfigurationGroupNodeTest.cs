namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Test.Common.UnitTesting;
    #endregion

    /// <summary>
    ///     Summary description for AddinConfigurationGroupNodeTest
    /// </summary>
    [TestClass]
    public class AddinConfigurationGroupNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new AddinConfigurationGroupNode();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expected1 = "Test name";
            string expected2 = "Test description";
            var target = new AddinConfigurationGroupNode(expected1, expected2);
            Assert.IsNotNull(target);
            Assert.AreEqual(expected1, target.Name);
            Assert.AreEqual(expected2, target.Description);
        }

        [TestMethod]
        public void GroupReferencesNullValueTest()
        {
            var target = new AddinConfigurationGroupNode();

            Assert.IsNotNull(target.GroupReferences);
        }

        [TestMethod]
        public void GroupReferencesTest()
        {
            var expected = new List<AddinConfigurationGroupReferenceNode>()
            {
                new AddinConfigurationGroupReferenceNode(),
                new AddinConfigurationGroupReferenceNode()
            };

            var target = new AddinConfigurationGroupNode();

            var _targetPrivateObject = new PrivateObject(target);
            _targetPrivateObject.SetField("_groupReferences", BindingFlags.NonPublic | BindingFlags.Instance, expected);

            Assert.AreEqual(2, target.GroupReferences.Count);
        }

        [TestMethod]
        public void SerializableExtensionPointConfigurationsNullValueTest()
        {
            var target = new AddinConfigurationGroupNode { SerializableExtensionPointConfigurations = null };

            Assert.AreEqual(0, target.SerializableExtensionPointConfigurations.Length);
        }

        [TestMethod]
        public void SerializableExtensionPointConfigurationsTest()
        {
            var expected = new List<ExtensionPointConfigurationNode>()
            {
                new ExtensionPointConfigurationNode(),
                new ExtensionPointConfigurationNode()
            };

            var target = new AddinConfigurationGroupNode
            {
                SerializableExtensionPointConfigurations = expected.ToArray()
            };

            Assert.AreEqual(2, target.SerializableExtensionPointConfigurations.Length);
        }

        [TestMethod]
        public void SerializableGroupReferencesNullValueTest()
        {
            var target = new AddinConfigurationGroupNode { SerializableGroupReferences = null };

            Assert.AreEqual(0, target.SerializableGroupReferences.Length);
        }

        [TestMethod]
        public void SerializableGroupReferencesTest()
        {
            var expected = new List<AddinConfigurationGroupReferenceNode>()
            {
                new AddinConfigurationGroupReferenceNode(),
                new AddinConfigurationGroupReferenceNode()
            };

            var target = new AddinConfigurationGroupNode
            {
                SerializableGroupReferences = expected.ToArray()
            };

            Assert.AreEqual(2, target.SerializableGroupReferences.Length);
        }

        [TestMethod]
        public void GetNoNodesTest()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            Assert.IsNull(AddinConfigurationGroupNode.Get("Test"));

            if (AddinManager.IsInitialized == true)
            {
                try
                {
                    AddinManager.Shutdown();
                }
                catch (InvalidOperationException)
                {
                    // temporarily swallow exception
                }
            }
        }
    }
}