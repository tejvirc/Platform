namespace Aristocrat.Monaco.Application.Contracts.Tests.Metering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Tests for CompositeMeter
    /// </summary>
    [TestClass]
    public class CompositeMeterTest
    {
        private const long MeterAToIncrement = 100;
        private const long MeterBToIncrement = 10;
        private const long MeterCToIncrement = 20;
        private const string ExpectedTargetName = "TEST_COMPOSITE";
        private dynamic _accessor;
        private long _actualChange;

        private int _eventHanderCalledCount;

        private Mock<IMeterManager> _meterManager;
        private BaseMeterProvider _meterProvider;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPersistentStorageAccessor> _storageAccessor;

        private Mock<IDisposable> _disposable;

        private CompositeMeter _target;

        /// <summary>
        ///     Test setup for Composite meters.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _storageAccessor = new Mock<IPersistentStorageAccessor>();
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Default);
            _persistentStorage.Setup(mock => mock.GetBlock("CompositeMeterTest"))
                .Returns(_storageAccessor.Object);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, 10000000000000))
                .Returns(10000000000000);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, 100000000))
                .Returns(100000000L);

            _meterProvider = new TestMeterProvider();

            _eventHanderCalledCount = 0;
            _actualChange = 0;
        }

        /// <summary>
        ///     Cleans up resources used by each test.
        /// </summary>
        [TestCleanup]
        public void MyTestCleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void SessionWithSmallValuesTest()
        {
            // Create list of arbitrary meter values and expression for
            // computing the composite value.
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 73, classification);
            AddAtomicMeter("MeterCashOut", 3456, classification);
            AddAtomicMeter("MeterCashWon", 21, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            long expectedValue = 73 + 3456 * 21;

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Session);
        }

        [TestMethod]
        public void SessionLargeValueTest()
        {
            // Create list of arbitrary meter values and expression for
            // computing the composite value.  Make the composite value
            // at least bigger than 32-bit int max.
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            var meter = AddAtomicMeter("MeterCashIn", int.MaxValue, classification);
            AddAtomicMeter("MeterCashOut", 456, classification);
            AddAtomicMeter("MeterCashWon", 21, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            var expectedValue = int.MaxValue % meter.Classification.UpperBounds;
            expectedValue += 456 * 21;

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Session);
        }

        [TestMethod]
        public void SessionRollOverTest()
        {
            MeterClassification classification = new OccurrenceMeterClassification();

            long expectedValue = 1234000;

            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut" };
            AddAtomicMeter("MeterCashIn", classification.UpperBounds, classification);
            AddAtomicMeter("MeterCashOut", expectedValue, classification);
            var formula = "MeterCashIn + MeterCashOut";

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Session);
        }

        [TestMethod]
        public void SessionPercentageTest()
        {
            MeterClassification classification = new PercentageMeterClassification();
            var meterNames = new List<string> { "JackpotWonGame0", "CashWonGame0", "CashPlayedGame0" };
            AddAtomicMeter("JackpotWonGame0", 100, classification);
            AddAtomicMeter("CashWonGame0", 200, classification);
            AddAtomicMeter("CashPlayedGame0", 1000, classification);
            var formula = "(JackpotWonGame0 + CashWonGame0) / CashPlayedGame0";

            var expectedValue = (long)((100 + 200) / 1000.0 * PercentageMeterClassification.Multiplier);

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Session);
        }

        [TestMethod]
        public void SessionPercentageDivideByZeroTest()
        {
            MeterClassification classification = new PercentageMeterClassification();
            var meterNames = new List<string> { "JackpotWonGame0", "CashWonGame0", "CashPlayedGame0" };
            AddAtomicMeter("JackpotWonGame0", 100, classification);
            AddAtomicMeter("CashWonGame0", 200, classification);
            AddAtomicMeter("CashPlayedGame0", 0, classification);
            var formula = "(JackpotWonGame0 + CashWonGame0) / CashPlayedGame0";

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(0, _target.Session);
        }

        [TestMethod]
        public void PeriodWithSmallValuesTest()
        {
            // Create list of arbitrary meter values and expression for
            // computing the composite value.
            MeterClassification classification = new CurrencyMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 321, classification);
            AddAtomicMeter("MeterCashOut", 456, classification);
            AddAtomicMeter("MeterCashWon", 21, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            long expectedValue = 321 + 456 * 21;

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Period);
        }

        [TestMethod]
        public void PeriodLargeValuesTest()
        {
            // Create list of arbitrary meter values and expression for
            // computing the composite value.  Make the composite value
            // at least bigger than 32-bit int max.
            MeterClassification classification = new CurrencyMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", int.MaxValue, classification);
            AddAtomicMeter("MeterCashOut", 3456, classification);
            AddAtomicMeter("MeterCashWon", 21, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            long expectedValue = int.MaxValue;
            expectedValue += 3456 * 21;

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Period);
        }

        [TestMethod]
        public void PeriodRolloverTest()
        {
            MeterClassification classification = new OccurrenceMeterClassification();

            long expectedValue = 1234;

            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut" };
            AddAtomicMeter("MeterCashIn", classification.UpperBounds, classification);
            AddAtomicMeter("MeterCashOut", expectedValue, classification);
            var formula = "MeterCashIn + MeterCashOut";

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Period);
        }

        [TestMethod]
        public void PeriodPercentageTest()
        {
            MeterClassification classification = new PercentageMeterClassification();
            var meterNames = new List<string> { "JackpotWonGame0", "CashWonGame0", "CashPlayedGame0" };
            AddAtomicMeter("JackpotWonGame0", 100, classification);
            AddAtomicMeter("CashWonGame0", 200, classification);
            AddAtomicMeter("CashPlayedGame0", 1000, classification);
            var formula = "(JackpotWonGame0 + CashWonGame0) / CashPlayedGame0";

            var expectedValue = (long)((100 + 200) / 1000.0 * PercentageMeterClassification.Multiplier);

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Period);
        }

        [TestMethod]
        public void PeriodPercentageDivideByZeroTest()
        {
            MeterClassification classification = new PercentageMeterClassification();
            var meterNames = new List<string> { "JackpotWonGame0", "CashWonGame0", "CashPlayedGame0" };
            AddAtomicMeter("JackpotWonGame0", 100, classification);
            AddAtomicMeter("CashWonGame0", 200, classification);
            AddAtomicMeter("CashPlayedGame0", 0, classification);
            var formula = "(JackpotWonGame0 + CashWonGame0) / CashPlayedGame0";

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(0, _target.Period);
        }

        [TestMethod]
        public void NameTest()
        {
            InitializeTarget(new List<string>(), string.Empty, "Occurrence");
            Assert.AreEqual(ExpectedTargetName, _target.Name);
        }

        [TestMethod]
        public void LifetimeWithSmallValuesTest()
        {
            // Create list of arbitrary meter values and expression for
            // computing the composite value.
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 13, classification);
            AddAtomicMeter("MeterCashOut", 456, classification);
            AddAtomicMeter("MeterCashWon", 21, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            long expectedValue = 13 + 456 * 21;

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Lifetime);
        }

        [TestMethod]
        public void LifetimeLargeValuesTest()
        {
            // Create list of arbitrary meter values and expression for
            // computing the composite value.  Make the composite value
            // at least bigger than 32-bit int max.
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            var meter = AddAtomicMeter("MeterCashIn", int.MaxValue, classification);
            AddAtomicMeter("MeterCashOut", 3456, classification);
            AddAtomicMeter("MeterCashWon", 21, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            var expectedValue = int.MaxValue % meter.Classification.UpperBounds;
            expectedValue += 3456 * 21;

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Lifetime);
        }

        [TestMethod]
        public void LifetimeRolloverCurrencyTest()
        {
            MeterClassification classification = new CurrencyMeterClassification();

            long expectedValue = 1234000;

            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut" };
            AddAtomicMeter("MeterCashIn", classification.UpperBounds, classification);
            AddAtomicMeter("MeterCashOut", expectedValue, classification);
            var formula = "MeterCashIn + MeterCashOut";

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Lifetime);
        }

        [TestMethod]
        public void LifetimeRolloverOccurrenceTest()
        {
            MeterClassification classification = new OccurrenceMeterClassification();

            long expectedValue = 1234;

            var meterNames = new List<string> { "JackpotWonGame0", "CashWonGame0", "CashPlayedGame0" };
            AddAtomicMeter("JackpotWonGame0", classification.UpperBounds, classification);
            AddAtomicMeter("CashWonGame0", classification.UpperBounds, classification);
            AddAtomicMeter("CashPlayedGame0", expectedValue, classification);
            var formula = "JackpotWonGame0 + CashWonGame0 + CashPlayedGame0";

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Lifetime);
        }

        [TestMethod]
        public void LifetimePercentageTest()
        {
            MeterClassification classification = new PercentageMeterClassification();
            var meterNames = new List<string> { "JackpotWonGame0", "CashWonGame0", "CashPlayedGame0" };
            AddAtomicMeter("JackpotWonGame0", 100, classification);
            AddAtomicMeter("CashWonGame0", 200, classification);
            AddAtomicMeter("CashPlayedGame0", 1000, classification);
            var formula = "(JackpotWonGame0 + CashWonGame0) / CashPlayedGame0";

            var expectedValue = (long)((100 + 200) / 1000.0 * PercentageMeterClassification.Multiplier);

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Lifetime);
        }

        [TestMethod]
        public void LifetimePercentageDivideByZeroTest()
        {
            MeterClassification classification = new PercentageMeterClassification();
            var meterNames = new List<string> { "CashWonGame0", "JackpotWonGame0", "CashPlayedGame0" };
            AddAtomicMeter("JackpotWonGame0", 100, classification);
            AddAtomicMeter("CashWonGame0", 200, classification);
            AddAtomicMeter("CashPlayedGame0", 0, classification);
            var formula = "(JackpotWonGame0 + CashWonGame0) / CashPlayedGame0";

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(0, _target.Lifetime);
        }

        [TestMethod]
        public void LifetimeMeterValueChangeTest()
        {
            // Create list of arbitrary meter values and expression for
            // computing the composite value.
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 13, classification);
            AddAtomicMeter("MeterCashOut", 456, classification);
            AddAtomicMeter("MeterCashWon", 21, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            long expectedValue = 13 + 456 * 21;

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(expectedValue, _target.Lifetime);

            // Modify one of the meters
            IncreaseAtomicMeter("MeterCashIn", 10);
            expectedValue += 10;
            Assert.AreEqual(expectedValue, _target.Lifetime);
        }

        [TestMethod]
        public void FormulaTest()
        {
            MeterClassification classification = new OccurrenceMeterClassification();

            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 6, classification);
            AddAtomicMeter("MeterCashOut", 1, classification);
            AddAtomicMeter("MeterCashWon", 9, classification);
            var formula = "MeterCashIn * MeterCashWon + MeterCashOut";

            InitializeTarget(meterNames, formula, classification.Name);

            Assert.AreEqual(formula, _target.Formula);
        }

        [TestMethod]
        public void ClassificationTest()
        {
            MeterClassification classification = new CurrencyMeterClassification();

            InitializeTarget(new List<string>(), string.Empty, classification.Name);

            Assert.AreEqual(classification.Name, _target.Classification.Name);
            Assert.AreEqual(classification.UpperBounds, _target.Classification.UpperBounds);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void IncrementWithEmptyFormulaTest()
        {
            InitializeTarget(new List<string>(), string.Empty, "Currency");

            _target.Increment(500);
        }

        [TestMethod]
        public void FormulaTestWithCompositeMeter()
        {
            // Create a composite meter for testing.
            MeterClassification classification = new OccurrenceMeterClassification();
            var formula = "TestAtomicMeter1 + TestAtomicMeter2";
            AddAtomicMeter("TestAtomicMeter1", 10, classification);
            AddAtomicMeter("TestAtomicMeter2", 10, classification);
            var testCompositeMeter = new CompositeMeter(
                "TestCompositeMeter",
                formula,
                new List<string> { "TestAtomicMeter1", "TestAtomicMeter2" },
                classification.Name);
            _meterManager.Setup(mock => mock.GetMeter("TestCompositeMeter")).Returns(testCompositeMeter);

            // Let the target contain the composite meter created above.
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 6, classification);
            AddAtomicMeter("MeterCashOut", 1, classification);
            AddAtomicMeter("MeterCashWon", 9, classification);

            meterNames.Add("TestCompositeMeter");
            formula = "MeterCashIn * MeterCashWon + MeterCashOut + TestCompositeMeter";

            InitializeTarget(meterNames, formula, classification.Name);
            _target.Initialize(_meterManager.Object);

            _accessor.GetValue(MeterTimeframe.Lifetime);

            // Make sure the composite meter in the formula is used.
            dynamic testCompositeMeterAccessor = new DynamicPrivateObject(testCompositeMeter);
            Assert.IsTrue(testCompositeMeterAccessor._initialized);
        }

        [TestMethod]
        public void CompositeMeterConstructorTest()
        {
            // Create list of arbitrary meter names and expression for
            // computing the composite value.
            var expectedMeters = new List<string> { "MeterCashIn", "MeterCashOut" };
            var formula = "MeterCashIn + MeterCashOut";

            MeterClassification classification = new CurrencyMeterClassification();

            _target = new CompositeMeter(ExpectedTargetName, formula, expectedMeters, classification.Name);
            _accessor = new DynamicPrivateObject(_target);

            Assert.AreEqual(ExpectedTargetName, _target.Name);
            Assert.AreEqual(formula, _target.Formula);
            Assert.AreEqual(classification.Name, _target.Classification.Name);
            Assert.AreEqual(classification.UpperBounds, _target.Classification.UpperBounds);
            Assert.IsNull(_accessor._meters);
            Assert.AreEqual(expectedMeters.Count, _accessor._meterNames.Count);
            for (var i = 0; i < expectedMeters.Count; ++i)
            {
                Assert.AreEqual(expectedMeters[i], _accessor._meterNames[i]);
            }
        }

        [TestMethod]
        public void MeterChangedEventTest()
        {
            // Create list of arbitrary meter values and expression for
            // computing the composite value.
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 0, classification);
            AddAtomicMeter("MeterCashOut", 0, classification);
            AddAtomicMeter("MeterCashWon", 0, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            InitializeTarget(meterNames, formula, classification.Name);
            Assert.AreEqual(_accessor._currentLifeTimeValue, -1);

            _target.Initialize(_meterManager.Object);
            _target.MeterChangedEvent += OnMeterChanged;

            var expectedChange = MeterAToIncrement;

            IncreaseAtomicMeter("MeterCashIn", MeterAToIncrement);

            Assert.AreEqual(_eventHanderCalledCount, 1);
            Assert.AreEqual(expectedChange, _actualChange);

            // Since the meter "MeterCashWon" is still 0, so the target's life meter is not changed even 
            // though the meter "MeterCashOut" is incremented. The amount on the hard meter will remain 
            // the same, thus the expected delta must be 0;
            expectedChange = MeterBToIncrement * _meterManager.Object.GetMeter("MeterCashWon").Lifetime;

            IncreaseAtomicMeter("MeterCashOut", MeterBToIncrement);

            Assert.AreEqual(_eventHanderCalledCount, 2);
            Assert.AreEqual(expectedChange, _actualChange);

            expectedChange = _meterManager.Object.GetMeter("MeterCashOut").Lifetime * MeterCToIncrement;

            IncreaseAtomicMeter("MeterCashWon", MeterCToIncrement);

            Assert.AreEqual(_eventHanderCalledCount, 3);
            Assert.AreEqual(expectedChange, _actualChange);

            _target.MeterChangedEvent -= OnMeterChanged;
        }

        [TestMethod]
        public void InitializeTestWhenAlreadyInitialized()
        {
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 0, classification);
            AddAtomicMeter("MeterCashOut", 0, classification);
            AddAtomicMeter("MeterCashWon", 0, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            InitializeTarget(meterNames, formula, classification.Name);

            _accessor._initialized = true;
            _target.Initialize(_meterManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitializeTestWithNullMeterManager()
        {
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 0, classification);
            AddAtomicMeter("MeterCashOut", 0, classification);
            AddAtomicMeter("MeterCashWon", 0, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            InitializeTarget(meterNames, formula, classification.Name);
            _target.Initialize(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetValueTestWithInvalidTimeframe()
        {
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut", "MeterCashWon" };
            AddAtomicMeter("MeterCashIn", 0, classification);
            AddAtomicMeter("MeterCashOut", 0, classification);
            AddAtomicMeter("MeterCashWon", 0, classification);
            var formula = "MeterCashIn + MeterCashOut * MeterCashWon";

            InitializeTarget(meterNames, formula, classification.Name);

            var invalidEnum = Enum.GetValues(typeof(MeterTimeframe)).Cast<int>().Max() + 1;
            _accessor.GetValue((MeterTimeframe)invalidEnum);
        }

        [TestMethod]
        public void GetMeterTestWithNonNullMeters()
        {
            MeterClassification classification = new OccurrenceMeterClassification();
            var meterNames = new List<string> { "MeterCashIn", "MeterCashOut" };
            AddAtomicMeter("MeterCashIn", 0, classification);
            AddAtomicMeter("MeterCashOut", 0, classification);
            var formula = "MeterCashIn + MeterCashOut";

            InitializeTarget(meterNames, formula, classification.Name);
            _accessor._meters = new List<IMeter>();

            _accessor.GetMeters((IMeterManager)null);

            Assert.AreEqual(0, _accessor._meters.Count);
        }

        /// <summary>
        ///     Creates an AtomicMeter set to the passed-in values and adds it to the
        ///     test MeterManager.
        /// </summary>
        /// <param name="name">The name of the desired meter</param>
        /// <param name="value">The desired meter value</param>
        /// <param name="classification">The atomic meter's classification</param>
        /// <returns>The meter that was created and added to the MeterManager</returns>
        private IMeter AddAtomicMeter(string name, long value, MeterClassification classification)
        {
            var meter = new AtomicMeter(
                name,
                _storageAccessor.Object,
                classification,
                _meterProvider);

            var lifeTimeMeterName = name + "Lifetime";
            var periodMeterName = name + "Period";

            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), lifeTimeMeterName]).Returns(0L);
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), periodMeterName]).Returns(0L);

            meter.Increment(value);
            _meterManager.Setup(mock => mock.GetMeter(name)).Returns(meter);

            return meter;
        }

        /// <summary>
        ///     Increases a meter by a value indicated.
        /// </summary>
        /// <param name="name">The name of meter.</param>
        /// <param name="value">The value to increase.</param>
        private void IncreaseAtomicMeter(string name, long value)
        {
            var originalfLifetime = _meterManager.Object.GetMeter(name).Lifetime;
            var originalPeriod = _meterManager.Object.GetMeter(name).Period;
            var lifeTimeMeterName = name + "Lifetime";
            var periodMeterName = name + "Period";

            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), lifeTimeMeterName])
                .Returns(value + originalfLifetime);
            _storageAccessor.SetupGet(mock => mock[It.IsAny<int>(), periodMeterName]).Returns(value + originalPeriod);

            // The "session" meter is not persisted so still need to call Increment.
            _meterManager.Object.GetMeter(name).Increment(value);
        }

        /// <summary>
        ///     Initializes the target composite meter using the passed-in meters and formula.
        /// </summary>
        /// <param name="meterNames">A list of meters</param>
        /// <param name="formula">The expression for evaluating the composite value</param>
        /// <param name="classification">The name of the classification for the composite meter</param>
        private void InitializeTarget(IList<string> meterNames, string formula, string classification)
        {
            _target = new CompositeMeter(ExpectedTargetName, formula, meterNames, classification);
            _accessor = new DynamicPrivateObject(_target);
            _target.Initialize(_meterManager.Object);
        }

        /// <summary>
        ///     The event handler invoked when the meter value is changed.
        /// </summary>
        /// <param name="sender">Indicates who is sending the event.</param>
        /// <param name="e">The event data.</param>
        private void OnMeterChanged(object sender, MeterChangedEventArgs e)
        {
            ++_eventHanderCalledCount;
            _actualChange = e.Amount;
        }
    }
}