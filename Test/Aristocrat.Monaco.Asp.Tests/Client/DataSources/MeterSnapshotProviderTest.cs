
namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Asp.Client.Contracts;
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class MeterSnapShotProviderTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPersistentStorageTransaction> _persistentStorageTransaction;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<IGamePlayState> _gamePlayState;
        private Action<GameEndedEvent> _gameEndedCallback;
        private GameEndedEvent _gameEndedEvent;
        private Dictionary<string, long> _snapshotDictionary;
        private Dictionary<string, long> _previvousSnapshotDictionary = new Dictionary<string, long>();

        public IEnumerable<string> _meterNames = new List<string>()
        {
            GamingMeters.EgmPaidBonusAmount,
            GamingMeters.EgmPaidGameWonAmount,
            GamingMeters.HandPaidBonusAmount,
            GamingMeters.PlayedCount,
            GamingMeters.TotalEgmPaidAmt,
            GamingMeters.TotalHandPaidAmt,
            GamingMeters.WageredAmount,
            GamingMeters.WonCount,
            GamingMeters.EgmPaidProgWonAmount,
            GamingMeters.HandPaidTotalWonAmount,
            AccountingMeters.CoinDrop,
            AccountingMeters.CurrencyInAmount,
            AccountingMeters.CurrentCredits,
            AccountingMeters.HandpaidCancelAmount,
            AccountingMeters.TotalVouchersIn,
            AccountingMeters.TotalVouchersOut,
            AccountingMeters.TrueCoinIn,
            AccountingMeters.TrueCoinOut,
            AccountingMeters.WatOnTotalAmount,
            AccountingMeters.WatOffTotalAmount
        };

        private MeterSnapshotProvider _meterSnapshotProvider;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _eventBus = new Mock<IEventBus>();
            _persistentStorageTransaction = new Mock<IPersistentStorageTransaction>();
            _persistentStorageAccessor = new Mock<IPersistentStorageAccessor>();
            _meterManager = new Mock<IMeterManager>();
            _persistentStorageManager = new Mock<IPersistentStorageManager>();
            _gamePlayState = new Mock<IGamePlayState>();

            _persistentStorageTransaction.Setup(m => m.Commit());

            _persistentStorageAccessor.Setup(m => m.StartTransaction()).Returns(_persistentStorageTransaction.Object);
            _persistentStorageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistentStorageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_persistentStorageAccessor.Object);

            _persistentStorageAccessor.SetupGet(m => m.Level).Returns(PersistenceLevel.Critical);

            _snapshotDictionary = new Dictionary<string, long>();
            var meters = new List<SnapshotMeter>();

            foreach (var meterName in _meterNames)
            {
                var snapshotMeter = new SnapshotMeter { Name = meterName, Value = 100 };
                meters.Add(snapshotMeter);
                _snapshotDictionary.Add(snapshotMeter.Name, snapshotMeter.Value);
            }

            _meterManager.Setup(m => m.CreateSnapshot(_meterNames, MeterValueType.Lifetime)).Returns(() =>
            {
                _snapshotDictionary.Clear();
                Random rand = new Random(DateTime.Now.Millisecond);

                foreach (var meterName in _meterNames)
                {
                    int newMeterValue = rand.Next(0, 100);
                    while (_previvousSnapshotDictionary.ContainsValue(newMeterValue))
                    {
                        // get a new value if it is already used before
                        newMeterValue = rand.Next(0, 100);
                    }
                    
                    _snapshotDictionary[meterName] = newMeterValue;
                }

                return _snapshotDictionary;
            });

            var componentBytes = StorageUtilities.ToByteArray(meters);
            _persistentStorageAccessor.SetupGet(m => m[0, "AuditUpdate.InitialMeters"]).Returns(componentBytes);

            _gameEndedEvent = new GameEndedEvent(0, 1, string.Empty, null);
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MeterSnapshotProvider>(), It.IsAny<Action<GameEndedEvent>>())).Callback<object, Action<GameEndedEvent>>((subscriber, callback) => _gameEndedCallback = callback);
            _eventBus.Setup(e => e.Publish(It.IsAny<GameEndedEvent>())).Callback<GameEndedEvent>(e => _gameEndedCallback(e));

            _meterSnapshotProvider = new MeterSnapshotProvider(_meterManager.Object, _persistentStorageManager.Object, _gamePlayState.Object, _eventBus.Object);
        }

        [TestMethod]
        public void GetSnapShotMeterTest()
        {
            _gamePlayState.Setup(m => m.Idle).Returns(true);
            _persistentStorageAccessor.SetupGet(m => m[AspConstants.AuditUpdateStatusField]).Returns(0);
            _meterSnapshotProvider.CreatePersistentSnapshot(notifyOnCompletion: true);

            
            foreach (var meter in _snapshotDictionary)
            {
                Assert.AreEqual(meter.Value, _meterSnapshotProvider.GetSnapshotMeter(meter.Key));
                _previvousSnapshotDictionary.Add(meter.Key, meter.Value);
            }

            // If game play state is not idle, the snapshot values of meters should not be regenerated
            _gamePlayState.Setup(m => m.Idle).Returns(false);
            _meterSnapshotProvider.CreatePersistentSnapshot(notifyOnCompletion: true);
            CollectionAssert.AreEqual(_previvousSnapshotDictionary, _snapshotDictionary);

            _gamePlayState.Setup(m => m.Idle).Returns(true);
            _meterSnapshotProvider.CreatePersistentSnapshot(notifyOnCompletion: true);
            CollectionAssert.AreNotEqual(_previvousSnapshotDictionary, _snapshotDictionary);
        }

        [TestMethod]
        public void NullContructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MeterSnapshotProvider(null, _persistentStorageManager.Object, _gamePlayState.Object, _eventBus.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new MeterSnapshotProvider(_meterManager.Object, null, _gamePlayState.Object, null));
            Assert.ThrowsException<ArgumentNullException>(() => new MeterSnapshotProvider(_meterManager.Object, _persistentStorageManager.Object, _gamePlayState.Object, null));
        }

        [TestMethod]
        public void CreatePersistentSnapshotWhenAuditStatusIsEnabledAndGameIdleTest()
        {
            _gamePlayState.Setup(m => m.Idle).Returns(true);
            _persistentStorageAccessor.SetupGet(m => m[AspConstants.AuditUpdateStatusField]).Returns(0);
            _meterSnapshotProvider.CreatePersistentSnapshot(notifyOnCompletion: false);

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Once);
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Once());

            _meterSnapshotProvider.CreatePersistentSnapshot(notifyOnCompletion: true);

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Exactly(2));
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Exactly(2));
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Exactly(2));
            _eventBus.Verify(m => m.Publish(It.IsAny<MeterSnapshotCompletedEvent>()), Times.Once());
        }

        [TestMethod]
        public void CreatePersistentSnapshotWhenAuditStatusIsDisabledOrGameNotIdleTest()
        {
            _gamePlayState.Setup(m => m.Idle).Returns(false);
            _persistentStorageAccessor.SetupGet(m => m[AspConstants.AuditUpdateStatusField]).Returns(1);
            _meterSnapshotProvider.CreatePersistentSnapshot(notifyOnCompletion: true);

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Never);
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Never);
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Never());
            _eventBus.Verify(m => m.Publish(It.IsAny<MeterSnapshotCompletedEvent>()), Times.Never());

            _gamePlayState.Setup(m => m.Idle).Returns(true);
            _persistentStorageAccessor.SetupGet(m => m[AspConstants.AuditUpdateStatusField]).Returns(1);
            _meterSnapshotProvider.CreatePersistentSnapshot(notifyOnCompletion: true);

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Never);
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Never);
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Never());
            _eventBus.Verify(m => m.Publish(It.IsAny<MeterSnapshotCompletedEvent>()), Times.Never());

            _gamePlayState.Setup(m => m.Idle).Returns(false);
            _persistentStorageAccessor.SetupGet(m => m[AspConstants.AuditUpdateStatusField]).Returns(1);
            _meterSnapshotProvider.CreatePersistentSnapshot(notifyOnCompletion: true);

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Never);
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Never);
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Never());
            _eventBus.Verify(m => m.Publish(It.IsAny<MeterSnapshotCompletedEvent>()), Times.Never());
        }

        [TestMethod]
        public void SetStatusInvokeCreateSnapshotTest()
        {
            _gamePlayState.Setup(m => m.Idle).Returns(true);
            _persistentStorageAccessor.SetupSet(m => m[AspConstants.AuditUpdateStatusField] = It.IsAny<MeterSnapshotStatus>())
            .Callback<string, object>((key, value) => { _persistentStorageAccessor.SetupGet(m => m[AspConstants.AuditUpdateStatusField]).Returns(value); });

            _meterSnapshotProvider.SnapshotStatus = MeterSnapshotStatus.Enabled;

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Once());
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Once());
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Once());
            _eventBus.Verify(m => m.Publish(It.IsAny<MeterSnapshotCompletedEvent>()), Times.Once());

            _meterSnapshotProvider.SnapshotStatus = MeterSnapshotStatus.Disabled;

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Once());
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Once());
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Once());
            _eventBus.Verify(m => m.Publish(It.IsAny<MeterSnapshotCompletedEvent>()), Times.Once());
        }

        [TestMethod]
        public void OnGameEndedCreateSnapshotTest()
        {
            _gamePlayState.Setup(m => m.Idle).Returns(true);
            _persistentStorageAccessor.SetupGet(m => m[AspConstants.AuditUpdateStatusField]).Returns(0);
            _eventBus.Object.Publish(_gameEndedEvent);

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Once());
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Once());
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Once());
            _eventBus.Verify(m => m.Publish(It.IsAny<MeterSnapshotCompletedEvent>()), Times.Once());

            _eventBus.Object.Publish(_gameEndedEvent);

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Exactly(2));
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Exactly(2));
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Exactly(2));
            _eventBus.Verify(m => m.Publish(It.IsAny<MeterSnapshotCompletedEvent>()), Times.Exactly(2));

            _gamePlayState.Setup(m => m.Idle).Returns(false);
            _eventBus.Object.Publish(_gameEndedEvent);

            _persistentStorageAccessor.Verify(m => m.StartTransaction(), Times.Exactly(2));
            _persistentStorageTransaction.Verify(m => m.Commit(), Times.Exactly(2));
            _meterManager.Verify(m => m.CreateSnapshot(It.IsAny<IEnumerable<string>>(), MeterValueType.Lifetime), Times.Exactly(2));
            _eventBus.Verify(m => m.Publish(It.IsAny<MeterSnapshotCompletedEvent>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            //Call dispose twice - should only unsubscribe/deregister from events once
            _meterSnapshotProvider.Dispose();
            _meterSnapshotProvider.Dispose();

            _eventBus.Verify(v => v.UnsubscribeAll(It.IsAny<object>()), Times.Once);
        }
    }
}
