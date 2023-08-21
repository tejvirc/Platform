namespace Aristocrat.Monaco.Kernel.MonoAddinsHelpers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    [CLSCompliant(false)]
    public class MonoAddinsHelperTests
    {
        /// <summary>
        ///     The property manager key used to find the selected addin configuration
        /// </summary>
        private const string AddinConfigurationsSelectedKey = "Mono.SelectedAddinConfigurationHashCode";

        /// <summary>
        ///     The path for that does not have addin extensions.
        /// </summary>
        private const string ExtensionPathWithoutExtensions = "/ExtensionPathWithNoExtensions";

        /// <summary>
        ///     The path for that have two addin extensions.
        /// </summary>
        private const string ExtensionPathWithTwoExtensions = "/ExtensionPathWithTwoExtensions";

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Initialize for each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            AddinManager.Initialize(currentDirectory, currentDirectory);
            AddinManager.Registry.Rebuild(null);

            var propertiesManager = new TestPropertiesManager();
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, new Dictionary<string, string>());
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MoqServiceManager.AddService<IPropertiesManager>(propertiesManager);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
        }

        /// <summary>
        ///     A test for GetSingleTypeExtensionNode() where no extensions exist at the
        ///     specified path.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void GetSingleTypeExtensionNodeTestWithZeroExtensions()
        {
            MonoAddinsHelper.GetSingleTypeExtensionNode(ExtensionPathWithoutExtensions);
        }

        /// <summary>
        ///     A test for GetSingleTypeExtensionNode() where two extensions exist at the
        ///     specified path.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void GetSingleTypeExtensionNodeTestWithTwoExtensions()
        {
            MonoAddinsHelper.GetSingleTypeExtensionNode(ExtensionPathWithTwoExtensions);
        }

        /// <summary>
        ///     Test that values are returned when the extension point is default.
        /// </summary>
        [TestMethod]
        public void TestSelectableConfigurationsDefaultExtensionPoint()
        {
            var selectableConfigurations = MonoAddinsHelper.SelectableConfigurations;
            Assert.AreEqual(3, selectableConfigurations.Count);
        }

        /// <summary>
        ///     Test that values are returned when the extension point is valid.
        /// </summary>
        [TestMethod]
        public void TestGetSelectableConfigurationAddinsValidExtensionPoint()
        {
            var selectableConfigurationAddins = MonoAddinsHelper.GetSelectableConfigurationAddins("TestConfiguration");
            Assert.AreEqual(1, selectableConfigurationAddins.Count);
        }

        /// <summary>
        ///     Test that values are returned when the there are multiple valid extension points.
        /// </summary>
        [TestMethod]
        public void TestGetSelectableConfigurationAddinsMultipleValidExtensionPoint()
        {
            var selectableConfigurationAddins = MonoAddinsHelper.GetSelectableConfigurationAddins("TestConfiguration");
            var selectableConfigurationAddinsTwo =
                MonoAddinsHelper.GetSelectableConfigurationAddins("TestConfigurationDuplicateExtensions");
            Assert.AreEqual(1, selectableConfigurationAddins.Count);
            Assert.AreEqual(2, selectableConfigurationAddinsTwo.Count);
        }

        /// <summary>
        ///     Test that values are returned when the extension point is valid and selected.
        /// </summary>
        [TestMethod]
        public void TestSelectedConfigurationsValidSelectedExtensionPoint()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            Dictionary<string, string> addinConfigurationsDictionary = new Dictionary<string, string>();
            addinConfigurationsDictionary["TestConfiguration"] = "Test Configuration";
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, addinConfigurationsDictionary);

            ICollection<AddinConfigurationGroupNode> selectedConfigurations = MonoAddinsHelper.SelectedConfigurations;
            Assert.IsNotNull(selectedConfigurations);
            Assert.AreEqual(1, selectedConfigurations.Count);
        }

        /// <summary>
        ///     Test that exception is thrown when the extension point is valid and selected but duplicate.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSelectedConfigurationsDuplicateValidSelectedExtensionPoint()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            Dictionary<string, string> addinConfigurationsDictionary = new Dictionary<string, string>();
            addinConfigurationsDictionary["TestConfigurationDuplicateExtensions"] =
                "Test Configuration Duplicate Extensions";
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, addinConfigurationsDictionary);

            ICollection<AddinConfigurationGroupNode> selectedConfigurations = MonoAddinsHelper.SelectedConfigurations;
        }

        /// <summary>
        ///     Test that values are returned when the extension point is valid but unselected.
        /// </summary>
        [TestMethod]
        public void TestSelectedConfigurationsValidUnselectedExtensionPoint()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            Dictionary<string, string> addinConfigurationsDictionary = new Dictionary<string, string>();
            addinConfigurationsDictionary["TestConfiguration"] =
                "Unselected TestConfiguration";
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, addinConfigurationsDictionary);

            ICollection<AddinConfigurationGroupNode> selectedConfigurations = MonoAddinsHelper.SelectedConfigurations;
            Assert.IsNotNull(selectedConfigurations);
            Assert.AreEqual(0, selectedConfigurations.Count);
        }

        /// <summary>
        ///     Test that values are returned when the extension point is invalid.
        /// </summary>
        [TestMethod]
        public void TestSelectedConfigurationsInvalidExtensionPoint()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            Dictionary<string, string> addinConfigurationsDictionary = new Dictionary<string, string>();
            addinConfigurationsDictionary["Invalid"] = "Any";
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, addinConfigurationsDictionary);

            ICollection<AddinConfigurationGroupNode> selectedConfigurations = MonoAddinsHelper.SelectedConfigurations;
            Assert.IsNotNull(selectedConfigurations);
            Assert.AreEqual(0, selectedConfigurations.Count);
        }

        /// <summary>
        ///     Test that values are returned when the property is empty
        /// </summary>
        [TestMethod]
        public void TestSelectedConfigurationsEmptyProperty()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            Dictionary<string, string> addinConfigurationsDictionary = new Dictionary<string, string>();
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, addinConfigurationsDictionary);

            ICollection<AddinConfigurationGroupNode> selectedConfigurations = MonoAddinsHelper.SelectedConfigurations;
            Assert.IsNull(selectedConfigurations);
        }

        /// <summary>
        ///     Test that values are returned when the extension point is invalid.
        /// </summary>
        [TestMethod]
        public void TestGetSelectableConfigurationAddinsInvalidExtensionPoint()
        {
            var selectableConfigurationAddins = MonoAddinsHelper.GetSelectableConfigurationAddins("Invalid");
            Assert.AreEqual(0, selectableConfigurationAddins.Count);
        }

        /// <summary>
        ///     Test that values are returned when the extension point is valid.
        /// </summary>
        [TestMethod]
        public void TestGetSelectedNodesValidNode()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            Dictionary<string, string> addinConfigurationsDictionary = new Dictionary<string, string>();
            addinConfigurationsDictionary["TestConfiguration"] = "Test Configuration";
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, addinConfigurationsDictionary);

            var nodes = new List<ExtensionNode>(
                MonoAddinsHelper.GetSelectedNodes<ExtensionNode>(
                    "/Kernel/SelectableAddinConfiguration/TestConfiguration"));

            Assert.AreEqual("Test Configuration", ((AddinConfigurationGroupNode)nodes[0]).Name);
        }

        /// <summary>
        ///     Test that values are returned when the extension point is invalid.
        /// </summary>
        [TestMethod]
        public void TestGetSelectedNodesInvalidNode()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            Dictionary<string, string> addinConfigurationsDictionary = new Dictionary<string, string>();
            addinConfigurationsDictionary["TestConfiguration"] = "Test Configuration";
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, addinConfigurationsDictionary);

            var nodes = new List<ExtensionNode>(MonoAddinsHelper.GetSelectedNodes<ExtensionNode>("Invalid"));

            Assert.AreEqual(0, nodes.Count);
        }

        /// <summary>
        ///     Test that values are returned when there are multiple filters
        /// </summary>
        [TestMethod]
        public void TestGetConfiguredExtensionNodesMultipleFilters()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            Dictionary<string, string> addinConfigurationsDictionary = new Dictionary<string, string>();
            addinConfigurationsDictionary["TestConfiguration"] = "Test Configuration";
            addinConfigurationsDictionary["TestConfigurationAddFilter"] =
                "Test Configuration Add Filter";
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, addinConfigurationsDictionary);

            List<TypeExtensionNode> result = new List<TypeExtensionNode>(
                MonoAddinsHelper.GetConfiguredExtensionNodes<TypeExtensionNode>(
                    MonoAddinsHelper.SelectedConfigurations,
                    "/Widgets",
                    false
                )
            );

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].Type.Name == "EventBus" || result[0].Type.Name == "ServiceManagerCore", "Failed");
            Assert.IsTrue(result[1].Type.Name == "EventBus" || result[1].Type.Name == "ServiceManagerCore", "Failed");
        }

        /// <summary>
        ///     Test that values are returned when the extension node is null
        /// </summary>
        [TestMethod]
        public void TestGetChildNodesNullNode()
        {
            List<TypeExtensionNode> result =
                new List<TypeExtensionNode>(MonoAddinsHelper.GetChildNodes<TypeExtensionNode>(null));

            Assert.AreEqual(0, result.Count);
        }

        /// <summary>
        ///     Test that an exception is thrown when multiple extension nodes are found when one is requested from the selected
        ///     configuration
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void TestSingleSelectedExtensionNode()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            propertiesManager.SetProperty(AddinConfigurationsSelectedKey, "SelectableGroup1");

            MonoAddinsHelper.GetSingleSelectedExtensionNode<ExtensionNode>(ExtensionPathWithTwoExtensions);
        }
    }
}