namespace Aristocrat.Monaco.Application.Contracts.Tests.Metering
{
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Monaco.Test.Common;

    /// <summary>
    ///     Tests for AtomicMeter
    /// </summary>
    [TestClass]
    public class AtomicMeterTest
    {
        private const long ValueToIncrement = 100;
        private dynamic _accessor;
        private long _actualChange;
        private int _eventHanderCalledCount;

        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPersistentStorageAccessor> _storageAccessor;

        private AtomicMeter _target;

        /// <summary>
        ///     Gets the lifetime meter name used in the persistence.
        /// </summary>
        private string LifeMeterName => _accessor._useGenericName
            ? _accessor.LifetimeMeterSuffix
            : _target.Name + _accessor.LifetimeMeterSuffix;

        /// <summary>
        ///     Gets the period meter name used in the persistence.
        /// </summary>
        private string PeriodMeterName => _accessor._useGenericName
            ? _accessor.PeriodMeterSuffix
            : (_target.Name + _accessor.PeriodMeterSuffix);

        /// <summary>
        ///     Use TestInitialize to run code before running each test.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _storageAccessor = new Mock<IPersistentStorageAccessor>();
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);

            _persistentStorage.Setup(mock => mock.GetBlock("AtomicMeterTestBlock"))
                .Returns(_storageAccessor.Object);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, 10000000000000))
                .Returns(10000000000000);

            MeterClassification classification = new CurrencyMeterClassification();
            IMeterProvider provider = new TestMeterProvider();

            _target = new AtomicMeter("MeterCashIn", _storageAccessor.Object, classification, provider);

            _accessor = new DynamicPrivateObject(_target);

            _eventHanderCalledCount = 0;
            _actualChange = 0;
        }

        /// <summary>Cleans up class members after execution of a TestMethod.</summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorWithStringClassificationTest()
        {
            _target = new AtomicMeter(
                "MeterCashIn",
                _storageAccessor.Object,
                0,
                "Currency",
                new TestMeterProvider());
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void PersistedDataSizeTest()
        {
            Assert.AreEqual(_accessor.TotalPersistedDataSize, AtomicMeter.PersistedDataSize);
        }

        /// <summary>
        ///     A test for Session
        /// </summary>
        [TestMethod]
        public void SessionTest()
        {
            // Use non-generic name by default.
            long randomStartValue = 2953084;
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns((long)0);
            _storageAccessor.SetupSet(mock => mock[0, LifeMeterName] = randomStartValue).Verifiable();
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = randomStartValue).Verifiable();
            _target.Increment(randomStartValue);

            Assert.AreEqual(randomStartValue, _target.Session);

            _storageAccessor.VerifySet(mock => mock[0, LifeMeterName] = randomStartValue, Times.Once());
            _storageAccessor.VerifySet(mock => mock[0, PeriodMeterName] = randomStartValue, Times.Once());

            // Use generic name.
            _accessor._useGenericName = true;
            AtomicMeter anotherMeter = new AtomicMeter(
                "MeterCashWon",
                _storageAccessor.Object,
                1,
                new CurrencyMeterClassification(),
                new TestMeterProvider());

            _storageAccessor.SetupGet(mock => mock[1, It.IsAny<string>()]).Returns((long)0).Verifiable();
            _storageAccessor.SetupSet(mock => mock[1, LifeMeterName] = randomStartValue).Verifiable();
            _storageAccessor.SetupSet(mock => mock[1, PeriodMeterName] = randomStartValue).Verifiable();
            anotherMeter.Increment(randomStartValue);

            Assert.AreEqual(randomStartValue, anotherMeter.Session);
            _storageAccessor.Verify();
        }

        [TestMethod]
        public void PeriodTest()
        {
            long randomStartValue = 38202937;

            // When use the non-generic name.
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns(0L);

            _storageAccessor.SetupSet(mock => mock[0, LifeMeterName] = randomStartValue).Verifiable();
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = randomStartValue).Verifiable();

            _target.Increment(randomStartValue);

            Assert.AreEqual(randomStartValue, _target.Period);
            _storageAccessor.Verify();

            // // When use the generic name.
            _accessor._useGenericName = true;
            AtomicMeter anotherMeter = new AtomicMeter(
                "MeterCashWon",
                _storageAccessor.Object,
                1,
                new CurrencyMeterClassification(),
                new TestMeterProvider());

            _storageAccessor.SetupGet(mock => mock[1, It.IsAny<string>()]).Returns(0L);

            _storageAccessor.SetupSet(mock => mock[1, LifeMeterName] = randomStartValue).Verifiable();
            _storageAccessor.SetupSet(mock => mock[1, PeriodMeterName] = randomStartValue).Verifiable();
            anotherMeter.Increment(randomStartValue);

            Assert.AreEqual(randomStartValue, anotherMeter.Period);
            _storageAccessor.Verify();
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("MeterCashIn", _target.Name);
        }

        [TestMethod]
        public void LifetimeTest()
        {
            long randomStartValue = 4321;

            // When use the non-generic name.
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns(0L);

            _storageAccessor.SetupSet(mock => mock[0, LifeMeterName] = randomStartValue).Verifiable();
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = randomStartValue).Verifiable();

            _target.Increment(randomStartValue);

            Assert.AreEqual(randomStartValue, _target.Lifetime);
            _storageAccessor.Verify();

            // When use the generic name.
            _accessor._useGenericName = true;
            AtomicMeter anotherMeter = new AtomicMeter(
                "MeterCashWon",
                _storageAccessor.Object,
                1,
                new CurrencyMeterClassification(),
                new TestMeterProvider());

            _storageAccessor.SetupGet(mock => mock[1, It.IsAny<string>()]).Returns(0L);

            _storageAccessor.SetupSet(mock => mock[1, LifeMeterName] = randomStartValue).Verifiable();
            _storageAccessor.SetupSet(mock => mock[1, PeriodMeterName] = randomStartValue).Verifiable();
            anotherMeter.Increment(randomStartValue);

            Assert.AreEqual(randomStartValue, anotherMeter.Lifetime);
            _storageAccessor.Verify();
        }

        [TestMethod]
        public void ClassificationTest()
        {
            MeterClassification classification = new CurrencyMeterClassification();

            Assert.AreEqual(classification.Name, _target.Classification.Name);
            Assert.AreEqual(classification.UpperBounds, _target.Classification.UpperBounds);
        }

        [TestMethod]
        public void IncrementTest()
        {
            long randomStartValue = 1234;
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns(0L);

            _storageAccessor.SetupSet(mock => mock[0, LifeMeterName] = randomStartValue).Verifiable();
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = randomStartValue).Verifiable();

            _target.Increment(randomStartValue);
            Assert.AreEqual(randomStartValue, _target.Lifetime);
            _storageAccessor.Verify();

            long addedValue = 29730293;
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()])
                .Returns(randomStartValue + addedValue);

            _storageAccessor.SetupSet(mock => mock[0, LifeMeterName] = randomStartValue + addedValue).Verifiable();
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = randomStartValue + addedValue).Verifiable();

            _target.Increment(addedValue);
            Assert.AreEqual(randomStartValue + addedValue, _target.Lifetime);
            _storageAccessor.Verify();
        }

        [TestMethod]
        public void IncrementRolloverTest()
        {
            long randomStartValue = _target.Classification.UpperBounds;
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns(0L);
            _storageAccessor.SetupSet(mock => mock[0, LifeMeterName] = (long)0).Verifiable();
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = (long)0).Verifiable();

            _target.Increment(randomStartValue);
            Assert.AreEqual(0, _target.Lifetime);
            _storageAccessor.Verify();

            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns(1L);

            _storageAccessor.SetupSet(mock => mock[0, LifeMeterName] = 1L).Verifiable();
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = 1L).Verifiable();

            _target.Increment(1);
            Assert.AreEqual(1, _target.Lifetime);
            _storageAccessor.Verify();
        }

        [TestMethod]
        public void ResetMeterTest()
        {
            long randomStartValue = 1234;
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns(randomStartValue);
            _storageAccessor.SetupSet(mock => mock[0, LifeMeterName] = randomStartValue).Verifiable();
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = randomStartValue).Verifiable();
            _target.MeterChangedEvent += OnMeterChanged;

            _target.ResetMeter(randomStartValue);
            Assert.AreEqual(randomStartValue, _target.Lifetime);
            Assert.AreEqual(randomStartValue, _target.Period);
            Assert.AreEqual(randomStartValue, _target.Session);

            Assert.AreEqual(1, _eventHanderCalledCount);
            Assert.AreEqual(randomStartValue, _actualChange);

            _target.MeterChangedEvent -= OnMeterChanged;

            _storageAccessor.Verify();
        }

        [TestMethod]
        public void ResetMeterRolloverTest()
        {
            long randomStartValue = _target.Classification.UpperBounds;
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns((long)0);
            _storageAccessor.SetupSet(mock => mock[0, LifeMeterName] = (long)0).Verifiable();
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = (long)0).Verifiable();

            _target.ResetMeter(randomStartValue);
            Assert.AreEqual(0, _target.Lifetime);
            Assert.AreEqual(0, _target.Period);
            Assert.AreEqual(0, _target.Session);
            _storageAccessor.Verify();
        }

        [TestMethod]
        public void ClearPeriodTest()
        {
            long randomStartValue = 1000;
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns(0L);
            _target.Increment(randomStartValue);
            Assert.AreEqual(randomStartValue, _target.Lifetime);

            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns((long)0);
            _storageAccessor.SetupSet(mock => mock[0, PeriodMeterName] = (long)0).Verifiable();

            _accessor.ClearPeriod();
            Assert.AreEqual(0, _target.Period);
            _storageAccessor.Verify();
            _storageAccessor.VerifySet(mock => mock[0, LifeMeterName] = (long)0, Times.Never());
        }

        [TestMethod]
        public void MeterChangedEventTest()
        {
            // Assert the value in the handler.
            _target.MeterChangedEvent += OnMeterChanged;

            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), It.IsAny<string>()]).Returns(ValueToIncrement);

            _target.Increment(ValueToIncrement);
            Assert.AreEqual(_eventHanderCalledCount, 1);
            Assert.AreEqual(ValueToIncrement, _actualChange);
            _target.MeterChangedEvent -= OnMeterChanged;
        }

        [TestMethod]
        public void AtomicMeterConstructorTest()
        {
            string meterName = "MeterCashIn";
            MeterClassification classification = new CurrencyMeterClassification();
            IMeterProvider provider = new TestMeterProvider();
            _target = new AtomicMeter(
                meterName,
                _storageAccessor.Object,
                classification,
                provider);
            _accessor = new DynamicPrivateObject(_target);

            Assert.AreEqual(_storageAccessor.Object, _accessor._block);
            Assert.AreEqual(classification, _target.Classification);
            Assert.AreEqual(meterName, _target.Name);
        }

        /// <summary>
        ///     A 2nd test for AtomicMeter Constructor
        /// </summary>
        [TestMethod]
        public void AtomicMeterConstructorTest2()
        {
            string meterName = "MeterCashIn";
            MeterClassification classification = new CurrencyMeterClassification();
            IMeterProvider provider = new TestMeterProvider();
            _target = new AtomicMeter(
                meterName,
                _storageAccessor.Object,
                0,
                classification,
                provider);
            _accessor = new DynamicPrivateObject(_target);

            Assert.AreEqual(_storageAccessor.Object, _accessor._block);
            Assert.AreEqual(classification, _target.Classification);
            Assert.AreEqual(meterName, _target.Name);
        }

        /// <summary>
        ///     The event handler invoked when the meter value is changed.
        /// </summary>
        /// <param name="sender">Indicates who is sending the event</param>
        /// <param name="e">The event date.</param>
        private void OnMeterChanged(object sender, MeterChangedEventArgs e)
        {
            ++_eventHanderCalledCount;
            _actualChange = e.Amount;
        }
    }
}
