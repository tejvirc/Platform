namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.IO;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.CoinAcceptor;
    using Hardware.Contracts;
    using Hardware.Contracts.CoinAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for CoinInMetersProvider and is intended
    ///     to contain all CoinInMetersProvider Unit Tests
    /// </summary>
    [TestClass]
    public class CoinMetersProviderTest
    {
        private Mock<IPersistentStorageAccessor> _block;

        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IMeterManager> _meterManager;
        private Mock<ISystemDisableManager> _disableManager;
        private CoinMetersProvider _target;
        private DateTime _periodClearDate;

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
            _block.SetupGet(m => m.Level).Returns(PersistenceLevel.Critical);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(a => a.GetProperty(It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);
            _propertiesManager.Setup(a => a.SetProperty(It.IsAny<string>(), It.IsAny<object>()));
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _disableManager.Setup(a => a.Enable(It.IsAny<Guid>()));
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _eventBus.Setup(a => a.Subscribe(It.IsAny<object>(), It.IsAny<Action<PeriodMetersClearedEvent>>()));

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManager.Setup(a => a.InvalidateProvider(It.IsAny<IMeterProvider>()));
            _meterManager.Setup(m => m.ExemptProviderFromClearAllPeriodMeters(typeof(CoinMetersProvider).ToString()));
            _periodClearDate = DateTime.UtcNow;
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            AddinManager.Shutdown();
            MoqServiceManager.RemoveInstance();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        ///     A test for the constructor() where no persistence block exists.
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithoutExistingBlock()
        {
            _persistentStorage.Setup(mock => mock.BlockExists(typeof(CoinMetersProvider).ToString()))
                .Returns(true).Verifiable();

            _block.SetupGet(m => m["LastPeriodClearTime"]).Returns(_periodClearDate);
            _block.Setup(mock => mock.Level).Returns(PersistenceLevel.Critical).Verifiable();

            // Get the block and verify its data
            _propertiesManager.Setup(
                    mock => mock.GetProperty(HardwareConstants.CoinValue, It.IsAny<long>()))
                .Returns(100000L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            _persistentStorage.Setup(mock => mock.GetBlock(typeof(CoinMetersProvider).ToString()))
                .Returns(_block.Object).Verifiable();

            // The CoinInMetersProvider does not have any data fields.  The constructor
            // sends the meters it creates to the BaseMeterProvider implementation for which
            // everything is private.  We cannot access its collection of provided meters.
            // We can only check that the constructor did not throw an exception.
            _target = new CoinMetersProvider();

            _propertiesManager.Verify();
            _persistentStorage.Verify();

            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.TrueCoinInCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.TrueCoinOutCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.CoinToCashBoxCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.CoinToHopperCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.CoinToCashBoxInsteadHopperCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.CoinToHopperInsteadCashBoxCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.CurrentHopperLevelCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.HopperRefillCount));
            Assert.IsTrue(_target.MeterNames.Contains(AccountingMeters.HopperRefillAmount));
        }
    }
}
