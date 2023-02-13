namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for CurrencyInMetersProvider and is intended
    ///     to contain all CurrencyInMetersProvider Unit Tests
    /// </summary>
    [TestClass]
    public class CurrencyInMetersProviderTest
    {
        private const string DenominationsPropertyName = "Denominations";
        private dynamic _accessor;
        private Mock<IPersistentStorageAccessor> _block;

        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<INoteAcceptor> _noteAcceptor;
        private Mock<IEventBus> _eventBus;
        private Mock<IMeterManager> _meterManager;
        private Mock<ISystemDisableManager> _disableManager;
        private readonly PrivateType _staticAccessor = new PrivateType(typeof(CurrencyInMetersProvider));
        private CurrencyInMetersProvider _target;
        private DateTime _periodClearDate;
        private Action<PropertyChangedEvent> _billClearanceCheckCallback;

        private Mock<IDisposable> _disposable;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Static, It.IsAny<string>(), 1))
                .Returns(_block.Object);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), 1))
                .Returns(_block.Object);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(a => a.GetProperty(It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);
            _propertiesManager.Setup(a => a.SetProperty(It.IsAny<string>(), It.IsAny<object>()));
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _disableManager.Setup(a => a.Enable(It.IsAny<Guid>()));
            _noteAcceptor = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _eventBus.Setup(a => a.Subscribe(It.IsAny<object>(), It.IsAny<Action<NoteUpdatedEvent>>()));
            _eventBus.Setup(a => a.Subscribe(It.IsAny<object>(), It.IsAny<Action<PersistentStorageClearedEvent>>()));
            _eventBus.Setup(a => a.Subscribe(It.IsAny<object>(), It.IsAny<Action<PeriodMetersClearedEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<CurrencyInMetersProvider>(), It.IsAny<Action<PropertyChangedEvent>>())).Callback<object, Action<PropertyChangedEvent>>((s, c) => _billClearanceCheckCallback = c);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManager.Setup(a => a.InvalidateProvider(It.IsAny<IMeterProvider>()));
            _meterManager.Setup(m => m.ExemptProviderFromClearAllPeriodMeters(typeof(CurrencyInMetersProvider).ToString()));
            _periodClearDate = DateTime.UtcNow;
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
            MoqServiceManager.RemoveInstance();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        ///     A test for GetDenominationMeterName
        /// </summary>
        [TestMethod]
        public void GetDenominationMeterNameTest()
        {
            // Choose a random denomination
            int denomination = 429;

            // The meter name is built by concatenating a prefix,
            // the denomination as a string, and a postfix.
            string expected = "BillCount429s";

            string actual = (string)_staticAccessor.InvokeStatic(
                "GetDenominationMeterName",
                denomination);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for GetPersistedDenominations()
        /// </summary>
        [TestMethod]
        public void GetPersistedDenominationsTest()
        {
            var results = new Dictionary<int, Dictionary<string, object>>
            {
                { 0, new Dictionary<string, object> { { "Denomination", 100 } } },
                { 1, new Dictionary<string, object> { { "Denomination", 200 } } },
                { 2, new Dictionary<string, object> { { "Denomination", 300 } } },
                { 3, new Dictionary<string, object> { { "Denomination", 400 } } }
            };

            _block.Setup(m => m.GetAll()).Returns(results);

            // Create a block big enough to store a random number of denominations (4 in this case).
            // The block size is (# denominations * (sizeof(int) + AtomicMeter.PersistedDataSize));
            _block.Setup(m => m.Count).Returns(results.Count).Verifiable();

            // Create a collection of # denominations where # == above numDenominations
            // Write random denominations to the block and collection.  For this test,
            // just use 100 * (i + 1), for denominations 100, 200, 300, & 400.  Save them
            // for later comparison.
            Collection<int> expected = new Collection<int> { 100, 200, 300, 400 };

            Collection<int> actual = (Collection<int>)_staticAccessor.InvokeStatic(
                "GetPersistedDenominations",
                _block.Object);

            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; ++i)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }

            _persistentStorage.Verify();
            _block.Verify();
        }

        /// <summary>
        ///     A test for StoreDenominationsInBlock()
        /// </summary>
        [TestMethod]
        public void StoreDenominationsInBlockTest()
        {
            // Create some random denominations
            Collection<int> denominations = new Collection<int> { 132, 333 };
            _block.SetupSet(m => m[0, "Denomination"] = denominations[0]).Verifiable();
            _block.SetupSet(m => m[1, "Denomination"] = denominations[1]).Verifiable();

            _staticAccessor.InvokeStatic("StoreDenominationsInBlock", _block.Object, denominations);

            _persistentStorage.Verify();
            _block.Verify();
        }

        /// <summary>
        ///     A test for the constructor() where no persistence block exists.
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithoutExistingBlock()
        {
            var results = new Dictionary<int, Dictionary<string, object>>
            {
                { 0, new Dictionary<string, object> { { "Denomination", 10 } } },
                { 1, new Dictionary<string, object> { { "Denomination", 20 } } }
            };
            var testDenominations = new Collection<int> { 10, 20 };
            _propertiesManager.Setup(
                    mock => mock.GetProperty(DenominationsPropertyName, null))
                .Returns(testDenominations);
            _noteAcceptor.Setup(a => a.GetSupportedNotes(It.IsAny<string>())).Returns(testDenominations);
            _propertiesManager.Setup(
                    mock => mock.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<double>()))
                .Returns(1000.0).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            _persistentStorage.Setup(mock => mock.BlockExists(typeof(CurrencyInMetersProvider).ToString()))
                .Returns(true).Verifiable();

            _block.Setup(m => m.GetAll()).Returns(results);
            _block.SetupGet(m => m["LastPeriodClearTime"]).Returns(_periodClearDate);
            _block.Setup(mock => mock.Level).Returns(PersistenceLevel.Critical).Verifiable();
            _block.Setup(m => m.GetAll()).Returns(results);

            // Get the block and verify its data
            _persistentStorage.Setup(mock => mock.GetBlock(typeof(CurrencyInMetersProvider).ToString()))
                .Returns(_block.Object).Verifiable();
            _block.Setup(mock => mock.Count).Returns(results.Count).Verifiable();

            // The CurrencyInMetersProvider does not have any data fields.  The constructor
            // sends the meters it creates to the BaseMeterProvider implementation for which
            // everything is private.  We cannot access its collection of provided meters.
            // We can only check that the constructor did not throw an exception.
            _target = new CurrencyInMetersProvider();

            _propertiesManager.Verify();
            _persistentStorage.Verify();
            _block.Verify();

            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.DocumentsRejectedCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.BillsRejectedCount));
        }

        private void InitExistingBlock()
        {
            // Store some random denom properties
            Collection<int> denominations = new Collection<int> { 1, 5 };
            _propertiesManager.Setup(
                    mock => mock.GetProperty(DenominationsPropertyName, null))
                .Returns(denominations);
            _propertiesManager.Setup(
                    mock => mock.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<double>()))
                .Returns(1000.0).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            _noteAcceptor.Setup(a => a.GetSupportedNotes(It.IsAny<string>())).Returns(denominations);

            // Create the persistence block used by the CurrencyInMetersProvider and
            // pre-populate it with the expected data for the above denominations.
            // For the meter value, juse use i, so values 0, 1, 2, 3...
            // The block size is (# denominations * (sizeof(int) + AtomicMeter.PersistedDataSize));
            _persistentStorage.Setup(
                    mock => mock.CreateBlock(
                        PersistenceLevel.Static,
                        typeof(CurrencyInMetersProvider).ToString(),
                        denominations.Count))
                .Returns(_block.Object).Verifiable();
            _block.Setup(mock => mock.Count).Returns(denominations.Count).Verifiable();
            _block.SetupSet(mock => mock[0, "Denomination"] = denominations[0]).Verifiable();
            _block.SetupSet(mock => mock[1, "Denomination"] = denominations[1]).Verifiable();
            _block.Setup(mock => mock[0, "Lifetime"]).Returns((long)0).Verifiable();
            _block.Setup(mock => mock[1, "Lifetime"]).Returns((long)1).Verifiable();
            _block.Setup(mock => mock.Level).Returns(PersistenceLevel.Critical);
        }

        /// <summary>
        ///     A test for the constructor() where a persistence block already exists.
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithExistingBlock()
        {
            InitExistingBlock();
            // The CurrencyInMetersProvider does not have any data fields.  The constructor
            // sends the meters it creates to the BaseMeterProvider implementation for which
            // everything is private.  We cannot access its collection of provided meters.
            // We can only check that the constructor did not throw an exception.
            _target = new CurrencyInMetersProvider();

            _propertiesManager.Verify();
            _persistentStorage.Verify();

            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.DocumentsRejectedCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.BillsRejectedCount));
        }

        /// <summary>
        ///     A test for CreateDenominationMeters()
        /// </summary>
        [TestMethod]
        public void CreateDenominationMetersTest()
        {
            var results = new Dictionary<int, Dictionary<string, object>>
            {
                { 0, new Dictionary<string, object> { { "Denomination", 100 } } }
            };

            _block.Setup(m => m.GetAll()).Returns(results);

            // Create another set of denominations for this test
            Collection<int> testDenominations = new Collection<int> { 100, 200 };

            _propertiesManager.Setup(
                    mock => mock.GetProperty(DenominationsPropertyName, null))
                .Returns(new Collection<int> { 100, 200 });
            _propertiesManager.Setup(
                    mock => mock.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<double>()))
                .Returns(1000.0).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            _persistentStorage.Setup(mock => mock.BlockExists(typeof(CurrencyInMetersProvider).ToString()))
                .Returns(true).Verifiable();
            _persistentStorage.Setup(mock => mock.ResizeBlock(typeof(CurrencyInMetersProvider).ToString(), 2));
            _persistentStorage.Setup(mock => mock.GetBlock(typeof(CurrencyInMetersProvider).ToString()))
                .Returns(_block.Object).Verifiable();
            _block.Setup(mock => mock.Count).Returns(results.Count).Verifiable();
            _block.Setup(mock => mock.Level).Returns(PersistenceLevel.Critical).Verifiable();
            _noteAcceptor.Setup(a => a.GetSupportedNotes(It.IsAny<string>())).Returns(testDenominations);

            // Create a block that is big enough to persist the test denominations and meters.
            // The block size is (# denomations * (sizeof(int) + AtomicMeter.PersistedDataSize));
            _block.Setup(mock => mock.Count).Returns(testDenominations.Count).Verifiable();
            _block.SetupSet(mock => mock[0, "Denomination"] = testDenominations[0]).Verifiable();
            _block.SetupSet(mock => mock[1, "Denomination"] = testDenominations[1]).Verifiable();
            _block.Setup(mock => mock[0, "Lifetime"]).Returns((long)0).Verifiable();
            _block.Setup(mock => mock[1, "Lifetime"]).Returns((long)1).Verifiable();
            _block.SetupGet(mock => mock["LastPeriodClearTime"]).Returns(_periodClearDate).Verifiable();

            _target = new CurrencyInMetersProvider();
            _accessor = new DynamicPrivateObject(_target);

            Collection<IMeter> meters =
                (Collection<IMeter>)_accessor.CreateDenominationMeters(_block.Object, testDenominations);

            Assert.AreEqual(2, meters.Count);
            for (int i = 0; i < meters.Count; ++i)
            {
                Assert.AreEqual(typeof(AtomicMeter), meters[i].GetType());
                string expectedName =
                    "BillCount" + testDenominations[i].ToString(CultureInfo.InvariantCulture) + "s";
                Assert.AreEqual(expectedName, meters[i].Name);
                Assert.AreEqual(i, meters[i].Lifetime);
            }

            _propertiesManager.Verify();
            _persistentStorage.Verify();
        }

        /// <summary>
        ///     A test to exempt self from ClearAllPeriodMeters if BillClearance functionality is enabled
        /// </summary>
        [DataTestMethod]
        [DataRow(true, DisplayName = "Provider will be exempted by registering with meter manager")]
        [DataRow(false, DisplayName = "Provider will NOT be exempted")]
        public void BillClearanceEnabledTest(bool billClearanceEnabled)
        {
            InitExistingBlock();
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.BillClearanceEnabled, false))
                .Returns(billClearanceEnabled);

            _target = new CurrencyInMetersProvider();
            _billClearanceCheckCallback(new PropertyChangedEvent(AccountingConstants.BillClearanceEnabled));
            if (billClearanceEnabled)
            {
                _meterManager.Verify(
                    m => m.ExemptProviderFromClearAllPeriodMeters(typeof(CurrencyInMetersProvider).ToString()),
                    Times.Once);
            }
            else
            {
                _meterManager.Verify(
                    m => m.ExemptProviderFromClearAllPeriodMeters(typeof(CurrencyInMetersProvider).ToString()),
                    Times.Never);
            }
        }
    }
}