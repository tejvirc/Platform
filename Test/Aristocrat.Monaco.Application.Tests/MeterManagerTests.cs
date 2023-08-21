namespace Aristocrat.Monaco.Accounting.UI.Tests
{
    using Application.Contracts;
    using Aristocrat.Monaco.Application.Meters;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Test.Common;

    /// <summary>
    ///     MeterManagerTests
    /// </summary>
    [TestClass]
    public class MeterManagerTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IMeterProvider> _meterProvider;
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<IScopedTransaction> _scopedTransaction;
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IPersistentStorageTransaction> _transaction;

        private IMeterManager _target;
        private Mock<IMeter> _meter;
        private DateTime _periodClearDateForProvider;

        private const string TestMeterName = "TestMeter";
        private const string TestMeterProviderName = "TestMeterProvider";
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Default);
            
            _persistentStorageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Default);
            _scopedTransaction = new Mock<IScopedTransaction>(MockBehavior.Default);
            _persistentStorageManager.Setup(p => p.ScopedTransaction()).Returns(_scopedTransaction.Object);
            _block = new Mock<IPersistentStorageAccessor>(MockBehavior.Default);
            _persistentStorageManager.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _transaction = new Mock<IPersistentStorageTransaction>();
            _block.Setup(b => b.StartTransaction()).Returns(_transaction.Object);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(e => e.Publish(It.IsAny<MeterProviderAddedEvent>()));

            _meter = new Mock<IMeter>(MockBehavior.Default);
            _meter.Setup(m => m.Name).Returns(TestMeterName);
            _meter.Setup(m => m.Period).Returns(0);
            _meter.Setup(m => m.Lifetime).Returns(0);

            _meterProvider = new Mock<IMeterProvider>(MockBehavior.Strict);
            // _meterProvider.Setup(m => m.RegisterMeterClearDelegate(It.IsAny<ClearPeriodMeter>());
            _meterProvider.SetupGet(m => m.Name).Returns(TestMeterProviderName);
            _meterProvider.SetupGet(m => m.MeterNames).Returns(new List<string> { _meter.Object.Name });
            _meterProvider.Setup(m => m.GetMeter(TestMeterName)).Returns(_meter.Object);
            
            _periodClearDateForProvider = DateTime.UtcNow;
            _meterProvider.SetupGet(m => m.LastPeriodClear).Returns(_periodClearDateForProvider);

            _target = new MeterManager(_persistentStorageManager.Object, _eventBus.Object, _propertiesManager.Object);
            _target.AddProvider(_meterProvider.Object);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            _target = null;
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ClearPeriodMetersTest()
        {
            SetupAnotherMeterProvider("AltTestProvider", "AltTestMeter", DateTime.UtcNow);
            _meterProvider.Setup(m => m.ClearPeriodMeters()).Verifiable();
            _eventBus.Setup(e => e.Publish(It.Is<PeriodMetersClearedEvent>(evt => evt.ProviderName == TestMeterProviderName))).Verifiable();
            _target.ClearPeriodMeters(TestMeterProviderName);
            _meterProvider.Verify();
            _eventBus.Verify();
            _eventBus.Verify(e => e.Publish(It.Is<PeriodMetersClearedEvent>(evt => evt.ProviderName == "AltTestProvider")), Times.Never());
        }

        [TestMethod]
        public void ClearAllPeriodMetersTest()
        {
            var altMeterProvider = SetupAnotherMeterProvider("AltTestProvider", "AltTestMeter", DateTime.UtcNow);
            _meterProvider.Setup(m => m.ClearPeriodMeters()).Verifiable();
            altMeterProvider.Setup(m => m.ClearPeriodMeters()).Verifiable();
            _eventBus.Setup(e => e.Publish(It.IsAny<PeriodMetersClearedEvent>())).Verifiable();
            _transaction.Setup(t => t["LastPeriodClearTime"]);
            _target.ClearAllPeriodMeters();
            _meterProvider.Verify();
            _eventBus.Verify();
        }

        [TestMethod]
        public void ExemptProviderFromClearAllPeriodMetersTest()
        {
            var altMeterProvider = SetupAnotherMeterProvider("AltTestProvider", "AltTestMeter", DateTime.UtcNow);
            _meterProvider.Setup(m => m.ClearPeriodMeters()).Verifiable();
            _eventBus.Setup(e => e.Publish(It.IsAny<PeriodMetersClearedEvent>())).Verifiable();
            _target.ExemptProviderFromClearAllPeriodMeters("AltTestProvider");
            _target.ClearAllPeriodMeters();
            _meterProvider.Verify();
            _eventBus.Verify();
            altMeterProvider.Verify(m => m.ClearPeriodMeters(), Times.Never());
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void ExemptNonexistantProviderFromClearAllPeriodMetersTest()
        {
            var altMeterProvider = SetupAnotherMeterProvider("AltTestProvider", "AltTestMeter", DateTime.UtcNow);
            _meterProvider.Setup(m => m.ClearPeriodMeters()).Verifiable();
            _eventBus.Setup(e => e.Publish(It.IsAny<PeriodMetersClearedEvent>())).Verifiable();
            _target.ExemptProviderFromClearAllPeriodMeters("NonexistantProvider");
        }

        [TestMethod]
        public void GetPeriodMetersClearanceDateProviderGivenTest()
        {
            var retrievedDateTime = _target.GetPeriodMetersClearanceDate(TestMeterProviderName);
            Assert.AreEqual(_periodClearDateForProvider, retrievedDateTime);
        }

        [TestMethod]
        public void GetPeriodMetersClearanceDateManagerGivenTest()
        {
            var expectedDate = _target.LastPeriodClear;
            _meterProvider.SetupGet(m => m.LastPeriodClear).Returns(DateTime.MinValue);
            var retrievedDateTime = _target.GetPeriodMetersClearanceDate(TestMeterProviderName);
            Assert.AreEqual(expectedDate, retrievedDateTime);
        }

        private Mock<IMeterProvider> SetupAnotherMeterProvider(string providerName, string meterName, DateTime clearDateTime)
        {
            var meterProvider = new Mock<IMeterProvider>(MockBehavior.Strict);
            var meter = new Mock<IMeter>(MockBehavior.Default);
            meter.Setup(m => m.Name).Returns(meterName);
            meter.Setup(m => m.Period).Returns(0);
            meter.Setup(m => m.Lifetime).Returns(0);

            meterProvider.SetupGet(m => m.Name).Returns(providerName);
            meterProvider.SetupGet(m => m.MeterNames).Returns(new List<string> { meter.Object.Name });
            meterProvider.Setup(m => m.GetMeter(meterName)).Returns(meter.Object);

            meterProvider.SetupGet(m => m.LastPeriodClear).Returns(clearDateTime);
            _target.AddProvider(meterProvider.Object);
            return meterProvider;
        }
    }
}
