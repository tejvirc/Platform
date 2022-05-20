namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP94RemoteHandPayResetHandlerTests
    {
        private LP94RemoteHandPayResetHandler _target;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager>  _properties;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
            _target = new LP94RemoteHandPayResetHandler(_transactionHistory.Object, _eventBus.Object, _properties.Object);           
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTransactionHistoryTest()
        {
            _target = new LP94RemoteHandPayResetHandler(null, _eventBus.Object, _properties.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEventBusTest()
        {
            _target = new LP94RemoteHandPayResetHandler(_transactionHistory.Object, null, _properties.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.RemoteHandpayReset));
        }

        [TestMethod]
        public void SuccessfulRemoteKeyOffTest()
        {
            const long cashableAmount = 100;
            const long promoAmount = 200;
            const long nonCashAmount = 300;
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>()).Returns(
                new List<HandpayTransaction>
                {
                    new HandpayTransaction(
                        0,
                        DateTime.Now,
                        cashableAmount,
                        promoAmount,
                        nonCashAmount,
                        HandpayType.CancelCredit,
                        false,
                        Guid.Empty)
                    {
                        State = HandpayState.Pending
                    }
                }).Verifiable();

            _eventBus.Setup(
                x => x.Publish(
                    It.Is<RemoteKeyOffEvent>(
                        keyEvent => keyEvent.PromoAmount == promoAmount &&
                                    keyEvent.CashableAmount == cashableAmount &&
                                    keyEvent.NonCashAmount == 0))).Verifiable();

            _properties.Setup(m => m.GetProperty(AccountingConstants.RemoteHandpayResetAllowed, true)).Returns(true);
            _properties.Setup(m => m.GetProperty(AccountingConstants.MenuSelectionHandpayInProgress, false)).Returns(false);

            var response = _target.Handle(null);
            Assert.AreEqual(HandPayResetCode.HandpayWasReset, response.Data);
            _transactionHistory.Verify();
            _eventBus.Verify();
        }

        [TestMethod]
        public void FailedRemoteKeyOffTest()
        {
            const long cashableAmount = 100;
            const long promoAmount = 200;
            const long nonCashAmount = 300;
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>()).Returns(
                new List<HandpayTransaction>
                {
                    new HandpayTransaction(
                        0,
                        DateTime.Now,
                        cashableAmount,
                        promoAmount,
                        nonCashAmount,
                        HandpayType.CancelCredit,
                        false,
                        Guid.Empty)
                    {
                        State = HandpayState.Committed
                    }
                }).Verifiable();

            _eventBus.Setup(x => x.Publish(It.IsAny<RemoteKeyOffEvent>()))
                .Throws(new Exception("RemoteKeyOffEvent should not be raised"));

            var response = _target.Handle(null);
            Assert.AreEqual(HandPayResetCode.NotInHandpay, response.Data);
            _transactionHistory.Verify();
            _eventBus.Verify();
        }
    }
}