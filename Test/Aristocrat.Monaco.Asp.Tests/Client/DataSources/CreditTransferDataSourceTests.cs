namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Monaco.Asp.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using Aristocrat.Monaco.Gaming.Diagnostics;
    using Aristocrat.Monaco.Test.Common;
    using Asp.Client.DataSources;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using static Aristocrat.Monaco.Asp.Client.DataSources.LogicSealDataSource;

    [TestClass]
    public class CreditTransferDataSourceTests
    {
        private const int MaxDCOMCommunicationTimeout = 200;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IGameDiagnostics> _gameDiagnostics;
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<IMeter> _meter;
        private Mock<IMeterManager> _meterManager;
        private Mock<IEventBus> _eventBus;
        private CreditTransferDataSource _source;
        private Mock<IPersistentStorageAccessor> _storageAccessor;
        private Mock<IBonusHandler> _bonusHandler;
        private Mock<IPersistentStorageTransaction> _transaction = new Mock<IPersistentStorageTransaction>();
        private Action<BonusAwardedEvent> bonusAwardedEventHandler;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Loose);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty("GamePlay.IsGameRunning", It.IsAny<object>())).Returns(true);

            _gameDiagnostics = MoqServiceManager.CreateAndAddService<IGameDiagnostics>(MockBehavior.Strict);
            _gameDiagnostics.Setup(m => m.IsActive).Returns(false);

            _persistentStorageManager = new Mock<IPersistentStorageManager>(MockBehavior.Strict);
            _meter = new Mock<IMeter>(MockBehavior.Strict);
            _meterManager = new Mock<IMeterManager>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _bonusHandler = new Mock<IBonusHandler>(MockBehavior.Strict);
            _storageAccessor = new Mock<IPersistentStorageAccessor>();
            SetupPersistenceMocks(true);

            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<BonusAwardedEvent>>()))
                .Callback<object, Action<BonusAwardedEvent>>((subscriber, callback) => bonusAwardedEventHandler = callback);

            _eventBus.Setup(s => s.UnsubscribeAll(It.IsAny<object>())).Verifiable();

            _source = new CreditTransferDataSource(_eventBus.Object, _bonusHandler.Object, _propertiesManager.Object, _persistentStorageManager.Object);
        }

        private void SetupPersistenceMocks(bool blockExists)
        {
            _persistentStorageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(blockExists);

            if (blockExists)
            {
                _persistentStorageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_storageAccessor.Object);
            }
            else
            {
                _persistentStorageManager.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), 1))
                    .Returns(_storageAccessor.Object);
            }

            _storageAccessor.Setup(m => m.StartTransaction()).Returns(_transaction.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEventBusTest()
        {
            var _ = new CreditTransferDataSource(null, _bonusHandler.Object, _propertiesManager.Object, _persistentStorageManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullBonusHandlerTest()
        {
            var _ = new CreditTransferDataSource(_eventBus.Object, null, _propertiesManager.Object, _persistentStorageManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            var _ = new CreditTransferDataSource(_eventBus.Object, _bonusHandler.Object, null, _persistentStorageManager.Object);
        }

        [TestMethod]
        public void MemberTest()
        {
            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 0, GetMemberValues(1) } });

            foreach (var memberName in _source.Members)
            {
                Assert.AreEqual(GetMemberValues(1)[memberName], _source.GetMemberValue(memberName));
            }

            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 0, null } });
            Assert.AreEqual(0, _source.GetMemberValue("Source_1stbyte"));

            Assert.AreEqual(0, _source.GetMemberValue(null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RollbackTest()
        {
            _source.SetMemberValue(CreditTransferDataSource.BonusCreditsWin, 20u);
            _source.RollBack();
            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>>());

            //Commit method is sensitive to CreditsWin,
            //it should throw ArgumentNullException exception,
            //since RollBack clear all the value members.
            _source.Commit();
        }

        [TestMethod]
        public async Task StandardBonusRequestTest() => await ProcessCreditTransfer(_source);

        [DataRow((byte)0, BonusMode.Standard)]
        [DataRow((byte)1, BonusMode.NonDeductible)]
        [DataRow((byte)2, BonusMode.Standard)]
        [DataTestMethod]
        public async Task BonusRequestTypeTest(byte bonusCreditType, BonusMode expectedBonusMode)
        {
            var nameValues = GetMemberValues(bonusCreditType);
            var dictionary = new Dictionary<string, object>();
            nameValues.ForEach(p => _transaction.SetupSet(r => r[p.Key] = It.IsAny<object>()).Callback<string, object>((key, value) => dictionary[key] = value));

            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 1, dictionary } });
            var result = false;
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>()))
                .Callback((IBonusRequest r) => result = r is StandardBonus request && request.Mode == expectedBonusMode)
                .Returns(new BonusTransaction());

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));
            _source.Commit();

            await Task.Delay(MaxDCOMCommunicationTimeout);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void BonusReasonTest()
        {
            var nameValues = GetMemberValues(1, 0);
            var dictionary = new Dictionary<string, object>();
            nameValues.ForEach(p => _transaction.SetupSet(r => r[p.Key] = It.IsAny<object>()).Callback<string, object>((key, value) => dictionary[key] = value));

            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 1, dictionary } });
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction());

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));
            _source.Commit();
        }

        private static Dictionary<string, object> GetMemberValues(byte bonusCreditType, byte bonusReason = 1) => new Dictionary<string, object> {
            { CreditTransferDataSource.BonusCreditsWin, 1u },
            { "Bonus_Credit_Type", bonusCreditType },
            { CreditTransferDataSource.BonusReason,  bonusReason},
            { "Source_1stbyte", (byte)1 },
            { "Source_2ndbyte", (byte)1 },
            { "Source_3rdbyte", (byte)1 },
            { "Current_JP_Number", (ushort)1 },
        };

        [TestMethod]
        public async Task OnBonusAwardedEventTest()
        {
            var result = 0;
            var snapshot = new Dictionary<string, object>();
            var expectedSnapshot = new Dictionary<string, object>
            {
                { "BCredits_Win", 123L },
                { "BReason", BonusReason.CashComponentOfPrizeCash },
                { "Source_1stbyte", 100 },
                { "Source_2ndbyte", 102 },
                { "Source_3rdbyte", 103 },
                { "Current_JP_Number", 4 },
                { "Bonus_Credit_Type", (byte)1 }
            };

            _source.MemberValueChanged += (s, e) =>
            {
                snapshot = e;
                ++result;
            };

            var tx = default(BonusTransaction);
            var nameValues = GetMemberValues(1);
            nameValues["BonusID"] = string.Empty;
            var dictionary = new Dictionary<string, object>();
            nameValues.ForEach(p => _transaction.SetupSet(r => r[p.Key] = It.IsAny<object>()).Callback<string, object>((key, value) => dictionary[key] = value));

            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 1, dictionary } });
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>()))
                .Callback((IBonusRequest r) => tx = new BonusTransaction(0, default, r.BonusId, 123, 0, 0, 0, 0, PayMethod.Any)
                {
                    Message = "Cash Component of Prize + Cash",
                    SourceID = "100102103",
                    JackpotNumber = 4,
                })
                .Returns(() => tx);

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));
            _source.Commit();

            await Task.Delay(MaxDCOMCommunicationTimeout);
            bonusAwardedEventHandler(new BonusAwardedEvent(tx));
            Assert.AreEqual(1, result);
            CollectionAssert.AreEqual(expectedSnapshot, snapshot);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void DuplicateTransferTest_CommitThrowException()
        {
            var nameValues = GetMemberValues(1);
            var existingPair = new Dictionary<int, Dictionary<string, object>> { { 0, GetMemberValues(1) } };

            _storageAccessor.Setup(m => m.GetAll()).Returns(existingPair);
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction());

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));
            _source.Commit();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void UnsupportedBonusCreditTypeTest()
        {
            var nameValues = GetMemberValues(1);
            var existingPair = new Dictionary<int, Dictionary<string, object>> { { 0, GetMemberValues(1) } };

            _storageAccessor.Setup(m => m.GetAll()).Returns(existingPair);
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction());

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));

            _source.SetMemberValue("Current_JP_Number", (ushort)2);
            _source.SetMemberValue("Bonus_Credit_Type", (byte)200);
            _source.Commit();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void UnsupportedBonusReasonTest()
        {
            var nameValues = GetMemberValues(1);
            var existingPair = new Dictionary<int, Dictionary<string, object>> { { 0, GetMemberValues(1) } };

            _storageAccessor.Setup(m => m.GetAll()).Returns(existingPair);
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction());

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));

            _source.SetMemberValue("Current_JP_Number", (ushort)2);
            _source.SetMemberValue(CreditTransferDataSource.BonusReason, (byte)200);
            _source.Commit();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GameNotRunningTest()
        {
            var nameValues = GetMemberValues(1);
            var dictionary = new Dictionary<string, object>();

            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 1, dictionary } });
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction());
            _propertiesManager.Setup(m => m.GetProperty("GamePlay.IsGameRunning", It.IsAny<object>())).Returns(false);

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));
            _source.Commit();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void InCombinationTestModeTest()
        {
            _gameDiagnostics.Setup(m => m.IsActive).Returns(true);
            _gameDiagnostics.Setup(m => m.Context).Returns(new CombinationTestContext());

            var nameValues = GetMemberValues(1);
            var dictionary = new Dictionary<string, object>();

            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 1, dictionary } });
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction());

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));
            _source.Commit();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void LogicSealBrokenTest()
        {
            _storageAccessor.Setup(m => m["LogicSealStatusField"]).Returns(LogicSealStatusEnum.Broken);

            var nameValues = GetMemberValues(1);
            var dictionary = new Dictionary<string, object>();

            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 1, dictionary } });
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction());

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));
            _source.Commit();
        }

        [TestMethod]
        public void BonusTransactionExceptionTest()
        {
            var nameValues = GetMemberValues(1);
            var dictionary = new Dictionary<string, object>();

            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 1, dictionary } });
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction { Exception = (int)BonusException.Failed });

            _source.Begin(new List<string>());
            nameValues.ForEach(p => _source.SetMemberValue(p.Key, p.Value));
            _source.Commit();

            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction());
            _source.Commit();
        }

        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            //Call dispose twice - should only unsubscribe/deregister from events once
            _source.Dispose();
            _source.Dispose();

            _eventBus.Verify(v => v.UnsubscribeAll(It.IsAny<object>()), Times.Once);
        }

        private async Task ProcessCreditTransfer(CreditTransferDataSource dataSource)
        {
            var nameValues = GetMemberValues(1);

            var dictionary = new Dictionary<string, object>();
            nameValues.ForEach(p => _transaction.SetupSet(r => r[p.Key] = It.IsAny<object>()).Callback<string, object>((key, value) => dictionary[key] = value));

            _storageAccessor.Setup(m => m.GetAll()).Returns(new Dictionary<int, Dictionary<string, object>> { { 1, dictionary } });
            _bonusHandler.Setup(m => m.Award(It.IsAny<IBonusRequest>())).Returns(new BonusTransaction());

            dataSource.Begin(new List<string>());
            nameValues.ForEach(p => dataSource.SetMemberValue(p.Key, p.Value));
            dataSource.Commit();

            await Task.Delay(MaxDCOMCommunicationTimeout);
            nameValues.ForEach(p => Assert.AreEqual(dataSource.GetMemberValue(p.Key), p.Value));
            nameValues.ForEach(p => Assert.AreEqual(dictionary[p.Key], p.Value));
        }
    }
}
