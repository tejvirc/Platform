namespace Aristocrat.Monaco.Application.UI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using Contracts.ConfigWizard;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for AutoConfigurator and is intended to contain all
    ///     AutoConfigurator Unit Tests.
    /// </summary>
    [TestClass]
    public class AutoConfiguratorTest
    {
        private const string AutoConfigurationFilePropertyName = "AutoConfigFile";
        private const string GoodConfigFileName = "TestAutoConfigFile_Good.xml";
        private const string BadConfigFileName = "TestAutoConfigFile_Bad.xml";
        private Mock<IPropertiesManager> _propertiesManager;
        private AutoConfigurator _target;

        /// <summary>Initializes class members and prepares for execution of a TestMethod.</summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _target = new AutoConfigurator();
        }

        /// <summary>Cleans up class members after execution of a TestMethod.</summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.AutoConfigurationExists);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Auto Configurator", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            ICollection<Type> ServiceTypes = _target.ServiceTypes;

            Assert.AreEqual(1, ServiceTypes.Count);
            Assert.IsTrue(ServiceTypes.Contains(typeof(IAutoConfigurator)));
        }

        [TestMethod]
        public void InitializeTestWithoutConfigPropertySet()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null)).Returns(null);

            _target.Initialize();

            Assert.IsFalse(_target.AutoConfigurationExists);
        }

        [TestMethod]
        public void InitializeTestWithEmptyConfigFileName()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(string.Empty);

            _target.Initialize();

            Assert.IsFalse(_target.AutoConfigurationExists);
        }

        [TestMethod]
        public void InitializeTestWithConfigFileNotFound()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns("nonexistantfile");

            _target.Initialize();

            Assert.IsFalse(_target.AutoConfigurationExists);
        }

        [TestMethod]
        public void InitializeTestWithInvalidConfigFilePath()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns("some\\::/path");

            _target.Initialize();

            Assert.IsFalse(_target.AutoConfigurationExists);
        }

        [TestMethod]
        public void InitializeTestWithInvalidConfigFilePath2()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns("some|path");

            _target.Initialize();

            Assert.IsFalse(_target.AutoConfigurationExists);
        }

        [TestMethod]
        public void InitializeTestWithBadXml()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(BadConfigFileName);

            _target.Initialize();

            Assert.IsFalse(_target.AutoConfigurationExists);
        }

        [TestMethod]
        public void InitializeTestWithGoodData()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            Assert.IsTrue(_target.AutoConfigurationExists);
        }

        [TestMethod]
        public void GetValueStringTestWithoutConfiguration()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(BadConfigFileName);

            _target.Initialize();

            string value = null;
            Assert.IsFalse(_target.GetValue(null, ref value));
        }

        [TestMethod]
        public void GetValueStringTestWithKeyNotFound()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            string value = null;
            Assert.IsFalse(_target.GetValue("badkey", ref value));
        }

        [TestMethod]
        public void GetValueStringTestWithNullOrEmptyValue()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            string value = null;
            Assert.IsFalse(_target.GetValue("Item11", ref value));
        }

        [TestMethod]
        public void GetValueStringTestWithGoodValue()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            string value = null;
            Assert.IsTrue(_target.GetValue("Item1", ref value));
            Assert.AreEqual("Value1", value);
        }

        [TestMethod]
        public void GetValueBoolTestWithoutConfiguration()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(BadConfigFileName);

            _target.Initialize();

            bool value = false;
            Assert.IsFalse(_target.GetValue(null, ref value));
        }

        [TestMethod]
        public void GetValueBoolTestWithKeyNotFound()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            bool value = false;
            Assert.IsFalse(_target.GetValue("badkey", ref value));
        }

        [TestMethod]
        public void GetValueBoolTestWithNullOrEmptyValue()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            bool value = false;
            Assert.IsFalse(_target.GetValue("Item11", ref value));
        }

        [TestMethod]
        public void GetValueBoolTestWithTrueValues()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            bool value = false;
            Assert.IsTrue(_target.GetValue("Item3", ref value));
            Assert.IsTrue(value);

            value = false;
            Assert.IsTrue(_target.GetValue("Item4", ref value));
            Assert.IsTrue(value);

            value = false;
            Assert.IsTrue(_target.GetValue("Item5", ref value));
            Assert.IsTrue(value);

            value = false;
            Assert.IsTrue(_target.GetValue("Item6", ref value));
            Assert.IsTrue(value);
        }

        [TestMethod]
        public void GetValueBoolTestWithFalseValues()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            bool value = true;
            Assert.IsTrue(_target.GetValue("Item7", ref value));
            Assert.IsFalse(value);

            value = true;
            Assert.IsTrue(_target.GetValue("Item8", ref value));
            Assert.IsFalse(value);

            value = true;
            Assert.IsTrue(_target.GetValue("Item9", ref value));
            Assert.IsFalse(value);

            value = true;
            Assert.IsTrue(_target.GetValue("Item10", ref value));
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void SetToggleButtonTestWithoutConfiguration()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(BadConfigFileName);

            _target.Initialize();

            Assert.IsFalse(_target.SetToggleButton(null, null));
        }

        [TestMethod]
        public void SetToggleButtonTestWithKeyNotFound()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            Assert.IsFalse(_target.SetToggleButton(null, "badkey"));
        }

        [TestMethod]
        public void SetToggleButtonTestWithInvalidValue()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            Assert.IsFalse(_target.SetToggleButton(null, "Item1"));
        }

        [RequireSTA]
        [TestMethod]
        public void SetToggleButtonTestWithFalseValues()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            ToggleButton button = new ToggleButton();

            button.IsChecked = true;
            Assert.IsTrue(_target.SetToggleButton(button, "Item7"));
            Assert.AreEqual(false, button.IsChecked);

            button.IsChecked = true;
            Assert.IsTrue(_target.SetToggleButton(button, "Item8"));
            Assert.AreEqual(false, button.IsChecked);

            button.IsChecked = true;
            Assert.IsTrue(_target.SetToggleButton(button, "Item9"));
            Assert.AreEqual(false, button.IsChecked);

            button.IsChecked = true;
            Assert.IsTrue(_target.SetToggleButton(button, "Item10"));
            Assert.AreEqual(false, button.IsChecked);
        }

        [RequireSTA]
        [TestMethod]
        public void SetToggleButtonTestWithTrueValues()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            ToggleButton button = new ToggleButton();

            button.IsChecked = false;
            Assert.IsTrue(_target.SetToggleButton(button, "Item3"));
            Assert.AreEqual(true, button.IsChecked);

            button.IsChecked = false;
            Assert.IsTrue(_target.SetToggleButton(button, "Item4"));
            Assert.AreEqual(true, button.IsChecked);

            button.IsChecked = false;
            Assert.IsTrue(_target.SetToggleButton(button, "Item5"));
            Assert.AreEqual(true, button.IsChecked);

            button.IsChecked = false;
            Assert.IsTrue(_target.SetToggleButton(button, "Item6"));
            Assert.AreEqual(true, button.IsChecked);
        }

        [TestMethod]
        public void SetSelectorTestWithoutConfiguration()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null)).Returns(null);

            Assert.IsFalse(_target.SetSelector(null, null));
        }

        [TestMethod]
        public void SetSelectorTestWithKeyNotFound()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(BadConfigFileName);

            _target.Initialize();

            Assert.IsFalse(_target.SetSelector(null, "badkey"));
        }

        [RequireSTA]
        [TestMethod]
        public void SetSelectorTestWithValueNotFound()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            ComboBox comboBox = new ComboBox();
            comboBox.ItemsSource = new List<string> { "Value1" };

            Assert.IsFalse(_target.SetSelector(comboBox, "Item2"));
        }

        [RequireSTA]
        [TestMethod]
        public void SetSelectorTestWithValueFound()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(AutoConfigurationFilePropertyName, null))
                .Returns(GoodConfigFileName);

            _target.Initialize();

            ComboBox comboBox = new ComboBox();
            comboBox.ItemsSource = new List<string> { "Value1", "Value2" };

            comboBox.SelectedIndex = 0;
            Assert.IsTrue(_target.SetSelector(comboBox, "Item2"));
            Assert.AreEqual(1, comboBox.SelectedIndex);
        }
    }
}