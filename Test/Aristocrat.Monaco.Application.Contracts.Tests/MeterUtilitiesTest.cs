namespace Aristocrat.Monaco.Application.Contracts.Tests
{
    using System;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Monaco.Test.Common;

    /// <summary>
    ///     Contains tests for the MeterUtilities class
    /// </summary>
    [TestClass]
    public class MeterUtilitiesTest
    {
        readonly Mock<IMeterProvider> _meterProvider = new Mock<IMeterProvider>();
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _meterProvider.Setup(m => m.RegisterMeterClearDelegate(It.IsAny<ClearPeriodMeter>())).Verifiable();
        }

        [TestMethod]
        public void ParseClassificationOccurrenceTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            var target = new AtomicMeter("Test", null, 1, "Occurrence", _meterProvider.Object);
            Assert.AreEqual(typeof(OccurrenceMeterClassification), target.Classification.GetType());
        }

        [TestMethod]
        public void ParseClassificationCurrencyTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            var target = new AtomicMeter("Test", null, 1, "Currency", _meterProvider.Object);
            Assert.AreEqual(typeof(CurrencyMeterClassification), target.Classification.GetType());
        }

        [TestMethod]
        public void ParseClassificationPercentageTest()
        {
            var target = new AtomicMeter("Test", null, 1, "Percentage", _meterProvider.Object);
            Assert.AreEqual(typeof(PercentageMeterClassification), target.Classification.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ParseClassificationUnknownTest()
        {
            var target = new AtomicMeter("Test", null, 1, "Unknown", null);
        }
    }
}
