namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Test.Common;

    [TestClass]
    public class LP8ALegacyBonusAwardsHandlerTest
    {
        private const byte RespondIgnore = 0;
        private const byte RespondAck = 1;
        private const byte RespondBusy = 2;
        private readonly LegacyBonusAwardsData _data = new LegacyBonusAwardsData
            { BonusAmount = 123456, TaxStatus = TaxStatus.Nondeductible };
        private LP8ALegacyBonusAwardsHandler _target;
        private Mock<ISasBonusCallback> _bonusCallback;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IFundsTransferDisable> _fundsTransferDisable;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _bonusCallback = MoqServiceManager.CreateAndAddService<ISasBonusCallback>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);
            _transactionHistory = MoqServiceManager.CreateAndAddService<ITransactionHistory>(MockBehavior.Default);
            _fundsTransferDisable = MoqServiceManager.CreateAndAddService<IFundsTransferDisable>(MockBehavior.Default);

            _target = new LP8ALegacyBonusAwardsHandler(_bonusCallback.Object, _propertiesManager.Object,
                                                       _disableManager.Object, _transactionHistory.Object,
                                                       _fundsTransferDisable.Object);

            // mocks for Legacy Bonusing Supported property
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { LegacyBonusAllowed = true });

            // mocks default behavior that we assume we're in a game
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);

            // mocks for AwardLegacyBonus
            _bonusCallback.Setup(m => m.AwardLegacyBonus(_data.BonusAmount, _data.TaxStatus));
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenArgumentIsNullExpectException(bool nullBonusCallback, bool nullPropertiesManager,
                                                      bool nullDisableManager, bool nullTransactionHistory,
                                                      bool nullTransferDisable)
        {
            _target = new LP8ALegacyBonusAwardsHandler(
                nullBonusCallback ? null : _bonusCallback.Object,
                nullPropertiesManager ? null : _propertiesManager.Object,
                nullDisableManager ? null : _disableManager.Object,
                nullTransactionHistory ? null : _transactionHistory.Object,
                nullTransferDisable ? null : _fundsTransferDisable.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.InitiateLegacyBonusPay));
        }

        [TestMethod]
        public void HandleLegacyBonusAwardPassTest()
        {
            _disableManager.Setup(m => m.IsDisabled).Returns(false);
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(new List<Guid>());

            var transactions = new List<HandpayTransaction>();
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>()).Returns(transactions);

            var expected = new LongPollReadSingleValueResponse<byte>(RespondAck);

            var actual = _target.Handle(_data);

            Assert.AreEqual(expected.Data, actual.Data);
        }

        [TestMethod]
        public void HandleLegacyBonusAwardInLobbyTest()
        {
            _disableManager.Setup(m => m.IsDisabled).Returns(false);
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(new List<Guid>());
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(false);

            var transactions = new List<HandpayTransaction>();
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>()).Returns(transactions);

            var expected = new LongPollReadSingleValueResponse<byte>(RespondAck);

            var actual = _target.Handle(_data);

            Assert.AreEqual(expected.Data, actual.Data);
        }

        [TestMethod]
        public void HandleLegacyBonusAwardNotEnabledTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { LegacyBonusAllowed = false });

            _disableManager.Setup(m => m.IsDisabled).Returns(false);
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(new List<Guid>());

            var transactions = new List<HandpayTransaction>();
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>()).Returns(transactions);

            var expected = new LongPollReadSingleValueResponse<byte>(RespondIgnore);

            var actual = _target.Handle(_data);

            Assert.AreEqual(expected.Data, actual.Data);
        }

        [TestMethod]
        public void HandleSystemDisabledTest()
        {
            _disableManager.Setup(m => m.IsDisabled).Returns(true);
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(new List<Guid>());

            var transactions = new List<HandpayTransaction>();
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>()).Returns(transactions);

            var expected = new LongPollReadSingleValueResponse<byte>(RespondBusy);

            var actual = _target.Handle(_data);

            Assert.AreEqual(expected.Data, actual.Data);
        }

        [TestMethod]
        public void HandleSystemDisabledWithHandpayOnlyTest()
        {
            _disableManager.Setup(m => m.IsDisabled).Returns(true);
            var guids = new List<Guid> { ApplicationConstants.HandpayPendingDisableKey };
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(guids);

            var transactions = new List<HandpayTransaction>();
                        var trans = new HandpayTransaction(0, new DateTime(2020, 01, 24), 1000000, 100, 200000, 100, HandpayType.BonusPay, false, new Guid("11111111111111111111111111111111"));
            transactions.Add(trans);
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>()).Returns(transactions);

            var expected = new LongPollReadSingleValueResponse<byte>(RespondAck);

            var actual = _target.Handle(_data);

            Assert.AreEqual(expected.Data, actual.Data);
        }

        [TestMethod]
        public void HandleSystemDisabledWithHandpayAndDoorOpenTest()
        {
            _disableManager.Setup(m => m.IsDisabled).Returns(true);
            var guids = new List<Guid>
            {
                ApplicationConstants.HandpayPendingDisableKey,
                new Guid("{3ACE2A2C-2E01-4d67-8C96-D8330B54E1BE}")
            };
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(guids);

            var transactions = new List<HandpayTransaction>();
            var trans = new HandpayTransaction(0, new DateTime(2020, 01, 24), 1000000, 100, 200000, 100, HandpayType.BonusPay, false, new Guid("11111111111111111111111111111111"));
            transactions.Add(trans);
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>()).Returns(transactions);

            var expected = new LongPollReadSingleValueResponse<byte>(RespondBusy);

            var actual = _target.Handle(_data);

            Assert.AreEqual(expected.Data, actual.Data);
        }


        [TestMethod]
        public void HandleSystemDisabledWithHandpayAndLiveAuthenticationTest()
        {
            _disableManager.Setup(m => m.IsDisabled).Returns(true);
            var guids = new List<Guid>
            {
                ApplicationConstants.HandpayPendingDisableKey,
                ApplicationConstants.LiveAuthenticationDisableKey
            };
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(guids);

            var transactions = new List<HandpayTransaction>();
            var trans = new HandpayTransaction(0, new DateTime(2020, 01, 24), 1000000, 100, 200000, 100, HandpayType.BonusPay, false, new Guid("11111111111111111111111111111111"));
            transactions.Add(trans);
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>()).Returns(transactions);

            // This should still work because live authentication currently always happens when a handpay is active.
            var expected = new LongPollReadSingleValueResponse<byte>(RespondAck);

            var actual = _target.Handle(_data);

            Assert.AreEqual(expected.Data, actual.Data);
        }
    }
}