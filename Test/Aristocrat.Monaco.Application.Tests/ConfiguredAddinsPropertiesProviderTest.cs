namespace Aristocrat.Monaco.Application.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Common;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;

    /// <summary>
    ///     This is a test class for ConfiguredAddinsPropertiesProviderTest and is intended
    ///     to contain all ConfiguredAddinsPropertiesProviderTest Unit Tests
    /// </summary>
    [TestClass]
    public class ConfiguredAddinsPropertiesProviderTest
    {
        /// <summary>
        ///     The log file used for logging unit testing.
        /// </summary>
        private const string LogFileName = "ConfiguredAddinsPropertiesProviderTest.log";

        /// <summary>
        ///     The key for the selected configurations property
        /// </summary>
        private const string ConfigurationSelectedKey = "Mono.SelectedAddinConfigurationHashCode";

        /// <summary>
        ///     The target's persistence block field for the array of selectable configuration names
        /// </summary>
        private const string SelectableConfigurationNameKey = "SelectableConfigurationName";

        /// <summary>
        ///     The target's persistence block field for the array of selected configuration values
        /// </summary>
        private const string SelectedConfigurationValueKey = "SelectedConfigurationValue";

        /// <summary>
        ///     The maximum number of selections the target can persist.
        /// </summary>
        private const int MaxSelections = 5;

        /// <summary>
        ///     A mock IPersistentStorageManager object.
        /// </summary>
        private Mock<IPersistentStorageManager> _persistence;

        /// <summary>
        ///     A mock IPersistentStorageAccessor object.
        /// </summary>
        private Mock<IPersistentStorageAccessor> _storageAccessor;

        /// <summary>
        ///     A mock IPropertiesManager object.
        /// </summary>
        private Mock<IPropertiesManager> _propertiesManager;

        /// <summary>
        ///     Selectable configuration names passed to the mocked IPersistentStorageAccessor
        /// </summary>
        private string[] _storedSelectableNames;

        /// <summary>
        ///     Selected configuration values passed to the mocked IPersistentStorageAccessor
        /// </summary>
        private string[] _storedSelectedValues;

        /// <summary>
        ///     Reference to the object to test
        /// </summary>
        private ConfiguredAddinsPropertiesProvider _target;

        /// <summary>
        ///     A dictionary of the hash code mappings used during the test;
        /// </summary>
        private Dictionary<string, string> _testData = new Dictionary<string, string>();

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _persistence = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);

            _storageAccessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            _storedSelectableNames = new string[MaxSelections];
            _storedSelectedValues = new string[MaxSelections];
            _storageAccessor.SetupGet(mock => mock[SelectableConfigurationNameKey])
                .Returns(_storedSelectableNames);
            _storageAccessor.SetupSet(mock => mock[SelectableConfigurationNameKey] = It.IsAny<string[]>())
                .Callback<string, object>((name, dataValue) => _storedSelectableNames = (string[])dataValue);
            _storageAccessor.SetupGet(mock => mock[SelectedConfigurationValueKey]).Returns(_storedSelectedValues);
            _storageAccessor.SetupSet(mock => mock[SelectedConfigurationValueKey] = It.IsAny<string[]>())
                .Callback<string, object>((name, dataValue) => _storedSelectedValues = (string[])dataValue);
        }

        /// <summary>
        ///     Method to cleanup objects for the test run.
        /// </summary>
        [TestCleanup]
        public void MyTestCleanUp()
        {
            AddinManager.Shutdown();
        }

        /// <summary>
        ///     A test for constructing the target object wih no pre-existing persistence data.
        /// </summary>
        [TestMethod]
        public void ConstructionTestWithNoPersistence()
        {
            MockNoExistingPersistenceBlock();
            MockMachineSettingsReimportProperties();
            InstantiateTestTarget();

            Dictionary<string, string> selectedConfigs =
                _target.GetProperty(ConfigurationSelectedKey) as Dictionary<string, string>;
            Assert.IsNotNull(selectedConfigs);
            Assert.AreEqual(0, selectedConfigs.Count);
        }

        /// <summary>
        ///     A test for constructing the target object wih pre-existing persistence data.
        /// </summary>
        [TestMethod]
        public void ConstructionTestWithPersistedData()
        {
            MockExistingPersistenceBlock();
            MockMachineSettingsReimportProperties();

            _testData["999"] = "1123";
            _testData["5432"] = "88";
            SetPersistedData(_testData);

            InstantiateTestTarget();

            VerifyData(_testData);
        }

        /// <summary>
        ///     A test for GetCollection
        /// </summary>
        [TestMethod]
        public void GetCollectionTest()
        {
            MockExistingPersistenceBlock();

            _testData["864"] = "2222";
            _testData["599"] = "8888888";
            SetPersistedData(_testData);

            InstantiateTestTarget();

            ICollection<KeyValuePair<string, object>> collection = _target.GetCollection;
            Assert.IsNotNull(collection);
            Assert.AreEqual(1, collection.Count);
            KeyValuePair<string, object> pair = collection.First();
            Assert.AreEqual(ConfigurationSelectedKey, pair.Key);
            Dictionary<string, string> selectedConfigs = pair.Value as Dictionary<string, string>;
            Assert.IsNotNull(selectedConfigs);
            VerifyData(_testData, selectedConfigs);
        }

        /// <summary>
        ///     A test for GetProperty where the passed-in property name is not recognized
        /// </summary>
        [TestMethod]
        public void GetPropertyTestWithUnknownPropertyName()
        {
            MockNoExistingPersistenceBlock();
            InstantiateTestTarget();

            object value = _target.GetProperty("foo");
            Assert.IsNull(value);
        }

        /// <summary>
        ///     A test for SetProperty where the passed-in property name is not recognized
        /// </summary>
        [TestMethod]
        public void SetPropertyTestWithUnknownPropertyName()
        {
            MockExistingPersistenceBlock();
            MockMachineSettingsReimportProperties();

            _testData["33"] = "76";
            SetPersistedData(_testData);

            InstantiateTestTarget();

            _target.SetProperty("foo", new object());

            VerifyData(_testData);
        }

        /// <summary>
        ///     A test for SetProperty where the passed-in property value is not the expected Dictionary type
        /// </summary>
        [TestMethod]
        public void SetPropertyTestWithBadValueType()
        {
            MockExistingPersistenceBlock();
            MockMachineSettingsReimportProperties();

            _testData["4545636"] = "8654";
            SetPersistedData(_testData);

            InstantiateTestTarget();

            bool caughtException = false;
            try
            {
                _target.SetProperty(ConfigurationSelectedKey, new object());
            }
            catch (UnknownPropertyException)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);

            VerifyData(_testData);
        }

        /// <summary>
        ///     A test for SetProperty where the passed-in property value is a Dictionary with too many elements
        /// </summary>
        [TestMethod]
        public void SetPropertyTestWithTooManySelections()
        {
            MockExistingPersistenceBlock();
            MockMachineSettingsReimportProperties();

            _testData["3764"] = "10";
            SetPersistedData(_testData);

            InstantiateTestTarget();

            // Setup new property value with too many elements
            Dictionary<string, string> newSelections = new Dictionary<string, string>();
            for (int i = 0; i <= MaxSelections; ++i)
            {
                newSelections[i.ToString()] = (i + 1).ToString();
            }

            // Try to set the property
            bool caughtException = false;
            try
            {
                _target.SetProperty(ConfigurationSelectedKey, newSelections);
            }
            catch (UnknownPropertyException)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);

            VerifyData(_testData);
        }

        /// <summary>
        ///     A test for SetProperty where the target's persistence block does not yet exist
        /// </summary>
        [TestMethod]
        public void SetPropertyTestWhenNoPersistenceBlockExists()
        {
            MockNoExistingPersistenceBlock();
            MockMachineSettingsReimportProperties();
            InstantiateTestTarget();

            for (int i = 0; i < MaxSelections; ++i)
            {
                _testData[i.ToString()] = (i + 1).ToString();
            }

            MockCreationOfPersistenceBlock();
            _target.SetProperty(ConfigurationSelectedKey, _testData);

            VerifyData(_testData);
        }

        /// <summary>
        ///     A test for SetProperty where the target's persistence block exists with different data
        /// </summary>
        [TestMethod]
        public void SetPropertyTestWhenPersistenceBlockExists()
        {
            MockExistingPersistenceBlock();
            MockMachineSettingsReimportProperties();

            _testData["1111"] = "11";
            _testData["2222"] = "22";
            SetPersistedData(_testData);

            InstantiateTestTarget();

            _testData = new Dictionary<string, string>();
            for (int i = 0; i < MaxSelections; ++i)
            {
                _testData[i.ToString()] = (i + 1).ToString();
            }

            _target.SetProperty(ConfigurationSelectedKey, _testData);

            VerifyData(_testData);
        }

        /// <summary>
        ///     A test for SetProperty when re-importing machine settings
        /// </summary>
        [TestMethod]
        public void SetPropertyTestWhenReimportingMachineSettings()
        {
            MockExistingPersistenceBlock();
            MockMachineSettingsReimportProperties(true);

            _testData["1111"] = "11";
            _testData["2222"] = "22";
            SetPersistedData(_testData);

            InstantiateTestTarget();

            _testData = new Dictionary<string, string>{{ "1111", "11"}};

            _target.SetProperty(ConfigurationSelectedKey, _testData);

            VerifyData(_testData);
        }

        /// <summary>
        ///     Instantiates the target object to test and sets up an associated PrivateObject
        /// </summary>
        private void InstantiateTestTarget()
        {
            _target = new ConfiguredAddinsPropertiesProvider();
        }

        /// <summary>
        ///     Mock that no persistence block exists
        /// </summary>
        private void MockNoExistingPersistenceBlock()
        {
            _persistence.Setup(mock => mock.BlockExists(typeof(ConfiguredAddinsPropertiesProvider).ToString()))
                .Returns(false);
        }

        /// <summary>
        ///     Mock creation of persistence block
        /// </summary>
        private void MockCreationOfPersistenceBlock()
        {
            PersistenceLevel level = PersistenceLevel.Static;
            string name = typeof(ConfiguredAddinsPropertiesProvider).ToString();
            int size = MaxSelections * 2 * sizeof(int);
            _persistence.Setup(mock => mock.CreateBlock(level, name, size))
                .Callback(() => MockExistingPersistenceBlock())
                .Returns(_storageAccessor.Object);
        }

        /// <summary>
        ///     Mock existing persistence block
        /// </summary>
        private void MockExistingPersistenceBlock()
        {
            _persistence.Setup(mock => mock.BlockExists(typeof(ConfiguredAddinsPropertiesProvider).ToString()))
                .Returns(true);
            _persistence.Setup(mock => mock.GetBlock(typeof(ConfiguredAddinsPropertiesProvider).ToString()))
                .Returns(_storageAccessor.Object);
        }

        /// <summary>
        ///     Mock machine settings re-import properties
        /// </summary>
        private void MockMachineSettingsReimportProperties(bool reimport = false)
        {
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsReimport, false)).Returns(reimport);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsReimported, false)).Returns(false);
        }

        /// <summary>
        ///     Stores the provided dictionary's hash code mappings in the class member arrays that back the mocked persistence
        ///     block.
        /// </summary>
        /// <param name="data">The hash code mappings to store</param>
        private void SetPersistedData(Dictionary<string, string> data)
        {
            Assert.IsTrue(data.Count <= MaxSelections);

            int i = 0;
            foreach (KeyValuePair<string, string> pair in data)
            {
                _storedSelectableNames[i] = pair.Key;
                _storedSelectedValues[i] = pair.Value;
                ++i;
            }

            for (; i < MaxSelections; ++i)
            {
                _storedSelectableNames[i] = null;
                _storedSelectedValues[i] = null;
            }
        }

        /// <summary>
        ///     Verifies that the provided target dictionary and persistence data contain exactly the provided, expected values.
        /// </summary>
        /// <param name="expectedValues">The expected hash code mappings</param>
        /// <param name="targetValues">The actual hash code values retrieved from the target</param>
        private void VerifyData(Dictionary<string, string> expectedValues, Dictionary<string, string> targetValues)
        {
            // Verify the dictionary
            Assert.IsNotNull(expectedValues);
            Assert.IsNotNull(targetValues);
            Assert.AreEqual(expectedValues.Count, targetValues.Count);
            foreach (KeyValuePair<string, string> pair in expectedValues)
            {
                Assert.IsTrue(targetValues.ContainsKey(pair.Key));
                Assert.AreEqual(pair.Value, targetValues[pair.Key]);
            }

            // Verify the persisted data
            Assert.AreEqual(MaxSelections, _storedSelectableNames.Length);
            Assert.AreEqual(MaxSelections, _storedSelectedValues.Length);
            List<string> foundKeys = new List<string>();
            for (int i = 0; i < MaxSelections; ++i)
            {
                string key = _storedSelectableNames[i];
                string value = _storedSelectedValues[i];
                if ((!string.IsNullOrEmpty(key)) || (!string.IsNullOrEmpty(value)))
                {
                    Assert.IsFalse(foundKeys.Contains(key));
                    foundKeys.Add(key);
                    Assert.IsTrue(expectedValues.ContainsKey(key));
                    Assert.AreEqual(value, expectedValues[key]);
                }
            }

            Assert.AreEqual(expectedValues.Count, foundKeys.Count);
        }

        /// <summary>
        ///     Verifies that the test target has exactly the same selection data as is setup by the
        ///     SetupArbitraryTestData method above.
        /// </summary>
        /// <param name="expectedValues">The expected hash code mappings</param>
        private void VerifyData(Dictionary<string, string> expectedValues)
        {
            Dictionary<string, string> propertyValues = _target.GetProperty(ConfigurationSelectedKey) as Dictionary<string, string>;
            VerifyData(expectedValues, propertyValues);
        }
    }
}