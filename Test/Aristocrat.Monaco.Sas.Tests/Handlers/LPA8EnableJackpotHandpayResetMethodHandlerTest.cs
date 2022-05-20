namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Sas.Handlers;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Tests for the LPA4SendCashOutLimitHandlerTest class
    /// </summary>
    [TestClass]
    public class LPA8EnableJackpotHandpayResetMethodHandlerTest
    {
        private LPa8EnableJackpotHandpayResetMethodHandler _target;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IBank> _bank;

        private readonly ResetMethod _badResetMethod = (ResetMethod)99;
        private readonly long _largeWinLimit = 1200000;
        private readonly long _handpayLimit = 2000000;
        private static readonly long _eligibleAmount = 1500000;
        private static readonly long _ineligibleAmount = 3000000;
        private static readonly long _bankLimit = 10000000;

        private readonly List<HandpayTransaction> _validTransactions = new List<HandpayTransaction>()
        {
            new HandpayTransaction(
                    1,
                    DateTime.Now,
                    _eligibleAmount,
                    0,
                    0,
                    HandpayType.GameWin,
                    true,
                    Guid.NewGuid())
        };

        private readonly List<HandpayTransaction> _validIneligibleTransactions = new List<HandpayTransaction>()
        {
            new HandpayTransaction(
                    1,
                    DateTime.Now,
                    _ineligibleAmount,
                    0,
                    0,
                    HandpayType.GameWin,
                    true,
                    Guid.NewGuid())
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Strict);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _bank = new Mock<IBank>(MockBehavior.Strict);

            _propertiesManager.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(_largeWinLimit);
            _propertiesManager.Setup(mock => mock.GetProperty(AccountingConstants.HandpayLimit, It.IsAny<long>()))
                .Returns(_handpayLimit);
            _propertiesManager.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinHandpayResetMethod, It.IsAny<int>()))
                .Returns(LargeWinHandpayResetMethod.PayBy1HostSystem);
            _propertiesManager.Setup(mock => mock.GetProperty(AccountingConstants.MenuSelectionHandpayInProgress, false))
                .Returns(false);

            _eventBus.Setup(mock => mock.Publish(It.IsAny<RemoteKeyOffEvent>()));

            _bank.Setup(mock => mock.QueryBalance()).Returns(0);
            _bank.Setup(mock => mock.Limit).Returns(_bankLimit);
            _propertiesManager.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinHandpayResetMethod, It.IsAny<LargeWinHandpayResetMethod>()))
                .Returns(LargeWinHandpayResetMethod.PayBy1HostSystem);

            _target = new LPa8EnableJackpotHandpayResetMethodHandler(_transactionHistory.Object, _propertiesManager.Object, _eventBus.Object, _bank.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.EnableJackpotHandpayResetMethod));
        }

        [TestMethod]
        public void HandleNullTest()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());

            var data = new EnableJackpotHandpayResetMethodData() { Method = ResetMethod.StandardHandpay };
            var actual = _target.Handle(data);
            Assert.AreEqual(AckCode.NotCurrentlyInAHandpayCondition, actual.Code);
        }

        [TestMethod]
        public void HandleValidAsStandardTest()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>())
                .Returns(_validTransactions);

            var data = new EnableJackpotHandpayResetMethodData() { Method = ResetMethod.StandardHandpay };
            var actual = _target.Handle(data);
            Assert.AreEqual(AckCode.ResetMethodEnabled, actual.Code);
        }

        [TestMethod]
        public void HandleValidAsResetTest()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>())
                .Returns(_validTransactions);

            var data = new EnableJackpotHandpayResetMethodData() { Method = ResetMethod.ResetToTheCreditMeter };
            var actual = _target.Handle(data);
            Assert.AreEqual(AckCode.ResetMethodEnabled, actual.Code);
        }

        [TestMethod]
        public void HandleValidAsBadMethodTest()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>())
                .Returns(_validTransactions);

            var data = new EnableJackpotHandpayResetMethodData() { Method = _badResetMethod };
            var actual = _target.Handle(data);
            Assert.AreEqual(AckCode.UnableToEnableResetMethod, actual.Code);
        }

        [TestMethod]
        public void HandleValidAsResetCreditsTooHighTest()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>())
                .Returns(_validTransactions);
            _bank.Setup(mock => mock.QueryBalance()).Returns(_bankLimit);

            var data = new EnableJackpotHandpayResetMethodData() { Method = ResetMethod.ResetToTheCreditMeter };
            var actual = _target.Handle(data);
            Assert.AreEqual(AckCode.UnableToEnableResetMethod, actual.Code);
        }

        [TestMethod]
        public void HandleValidIneligibleAsStandardTest()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>())
                .Returns(_validIneligibleTransactions);

            _propertiesManager.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinHandpayResetMethod, It.IsAny<int>()))
                .Returns(LargeWinHandpayResetMethod.PayByHand);

            var data = new EnableJackpotHandpayResetMethodData() { Method = ResetMethod.StandardHandpay };
            var actual = _target.Handle(data);
            Assert.AreEqual(AckCode.UnableToEnableResetMethod, actual.Code);
        }

        [TestMethod]
        public void HandleValidIneligibleWithPayByHandAsResetTest()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>())
                .Returns(_validIneligibleTransactions);

            _propertiesManager.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinHandpayResetMethod, It.IsAny<int>()))
                .Returns(LargeWinHandpayResetMethod.PayByHand);

            var data = new EnableJackpotHandpayResetMethodData() { Method = ResetMethod.ResetToTheCreditMeter };
            var actual = _target.Handle(data);
            Assert.AreEqual(AckCode.UnableToEnableResetMethod, actual.Code);
        }

        [TestMethod]
        public void HandleValidInvalidHandpayLimitWithPayBy1HostSystemAsResetTest()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<HandpayTransaction>())
                .Returns(_validTransactions);

            _propertiesManager.Setup(mock => mock.GetProperty(AccountingConstants.HandpayLimit, It.IsAny<long>()))
                .Returns(AccountingConstants.DefaultHandpayLimit);

            var data = new EnableJackpotHandpayResetMethodData() { Method = ResetMethod.ResetToTheCreditMeter };
            var actual = _target.Handle(data);
            Assert.AreEqual(AckCode.ResetMethodEnabled, actual.Code);
        }
    }
}
