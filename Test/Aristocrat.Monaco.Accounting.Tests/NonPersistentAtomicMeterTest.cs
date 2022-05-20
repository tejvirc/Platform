namespace Aristocrat.Monaco.Accounting.Tests
{
    using Application.Contracts;
    using Application.Contracts.Metering;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class NonPersistentAtomicMeterTest
    {
        MeterClassification _classification;
        string _meterName = "Test";
        private Mock<IPropertiesManager> _propertiesManager;
        private NonPersistentAtomicMeter _target;

        /// <summary>
        ///     Setup the environment for each test
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            Mock<IMeterProvider> provider = new Mock<IMeterProvider>(MockBehavior.Strict);
            provider.Setup(m => m.RegisterMeterClearDelegate(It.IsAny<ClearPeriodMeter>())).Verifiable();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            _classification = new CurrencyMeterClassification();

            _target = new NonPersistentAtomicMeter(_meterName, _classification, provider.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(_meterName, _target.Name);
            Assert.AreEqual(_classification, _target.Classification);
            Assert.AreEqual(0, _target.Lifetime);
            Assert.AreEqual(0, _target.Period);
            Assert.AreEqual(0, _target.Session);
        }

        [TestMethod]
        public void IncrementTest()
        {
            long upperBounds = _target.Classification.UpperBounds;

            _target.Increment(upperBounds - 1);

            Assert.AreEqual(upperBounds - 1, _target.Lifetime);
            Assert.AreEqual(upperBounds - 1, _target.Period);
            Assert.AreEqual(upperBounds - 1, _target.Session);
        }

        [TestMethod]
        [ExpectedException(typeof(MeterException))]
        public void IncrementOverUpperBoundsTest()
        {
            long upperBounds = _target.Classification.UpperBounds;

            _target.Increment(upperBounds + 1);
        }

        [TestMethod]
        public void ClearPeriodTest()
        {
            long upperBounds = _target.Classification.UpperBounds;

            dynamic accessor = new DynamicPrivateObject(_target);

            _target.Increment(upperBounds - 1);

            Assert.AreEqual(upperBounds - 1, _target.Lifetime);
            Assert.AreEqual(upperBounds - 1, _target.Period);
            Assert.AreEqual(upperBounds - 1, _target.Session);

            accessor.ClearPeriod();

            Assert.AreEqual(upperBounds - 1, _target.Lifetime);
            Assert.AreEqual(0, _target.Period);
            Assert.AreEqual(upperBounds - 1, _target.Session);
        }
    }
}