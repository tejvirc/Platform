namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    ///     Tests for the LP1BSendHandpayInfoHandler class
    /// </summary>
    [TestClass]
    public class LP1BSendHandpayInfoHandlerTest
    {
        private const byte ClientNumber = 52;
        private LP1BSendHandpayInfoHandler _target;
        private readonly Mock<ISasHandPayCommittedHandler> _sasHandPayCommittedHandler = new Mock<ISasHandPayCommittedHandler>(MockBehavior.Default);
        private readonly Mock<ITransactionHistory> _history = new Mock<ITransactionHistory>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTest(
            bool nullHandpayProvider,
            bool nullHistory,
            bool nullProperties,
            bool nullBank)
        {
            _target = CreateTarget(nullHandpayProvider, nullHistory, nullProperties, nullBank);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendHandpayInformation));
        }

        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            true,
            HandpayType.GameWin,
            HandpayState.Pending,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.HandpayResetToTheCreditMeterIsAvailable,
            DisplayName = "Game Win handpay in pending can reset to the credit meter")]
        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            true,
            HandpayType.BonusPay,
            HandpayState.Pending,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.HandpayResetToTheCreditMeterIsAvailable,
            DisplayName = "Bonus Win handpay in pending can reset to the credit meter")]
        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            true,
            HandpayType.GameWin,
            HandpayState.Requested,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.HandpayResetToTheCreditMeterIsAvailable,
            DisplayName = "Game Win handpay in request can reset to the credit meter")]
        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            true,
            HandpayType.BonusPay,
            HandpayState.Requested,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.HandpayResetToTheCreditMeterIsAvailable,
            DisplayName = "Bonus Win handpay in request can reset to the credit meter")]
        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            true,
            HandpayType.CancelCredit,
            HandpayState.Requested,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.OnlyStandardHandpayResetIsAvailable,
            DisplayName = "Cancelled credits can never reset to the credit meter")]
        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            true,
            HandpayType.GameWin,
            HandpayState.Requested,
            LargeWinHandpayResetMethod.PayByMenuSelection,
            ResetId.OnlyStandardHandpayResetIsAvailable,
            DisplayName = "Not set to pay by 1 key can never reset to the credit meter")]
        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            true,
            HandpayType.GameWin,
            HandpayState.Requested,
            LargeWinHandpayResetMethod.PayByHand,
            ResetId.OnlyStandardHandpayResetIsAvailable,
            DisplayName = "Not set to pay by 1 key can never reset to the credit meter")]
        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            true,
            HandpayType.GameWin,
            HandpayState.Committed,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.OnlyStandardHandpayResetIsAvailable,
            DisplayName = "Not in request or pending can't reset to the credit meter")]
        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            true,
            HandpayType.GameWin,
            HandpayState.Acknowledged,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.OnlyStandardHandpayResetIsAvailable,
            DisplayName = "Not in request or pending can't reset to the credit meter")]
        [DataRow(
            100,
            200,
            100000,
            1000000000,
            1000000000,
            false,
            HandpayType.GameWin,
            HandpayState.Requested,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.OnlyStandardHandpayResetIsAvailable,
            DisplayName = "No transaction can not reset t the credit meter")]
        [DataRow(
            1000,
            200,
            100000,
            1000000000,
            1000,
            true,
            HandpayType.GameWin,
            HandpayState.Pending,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.OnlyStandardHandpayResetIsAvailable,
            DisplayName = "Over the handpay limit cannot reset to the credit meter")]
        [DataRow(
            100,
            200,
            999999900,
            1000000000,
            1000000000,
            true,
            HandpayType.GameWin,
            HandpayState.Pending,
            LargeWinHandpayResetMethod.PayBy1HostSystem,
            ResetId.OnlyStandardHandpayResetIsAvailable,
            DisplayName = "Current Balance plus win exceeds credit limit cannot reset to the credit meter")]
        [DataTestMethod]
        public void HandleUnreadHandpayWithProgressiveTest(
            long cashAmount,
            long promoAmount,
            long currentBalance,
            long limit,
            long handpayLimit,
            bool hasTransaction,
            HandpayType type,
            HandpayState state,
            LargeWinHandpayResetMethod resetMethod,
            ResetId expectedResetId)
        {
            const uint expectedGroupId = 3;
            const long transactionId = 1234;
            var expectedAmount = (cashAmount + promoAmount).MillicentsToCents();

            _sasHandPayCommittedHandler.Setup(x => x.GetNextUnreadHandpayTransaction(ClientNumber))
                .Returns(
                    new LongPollHandpayDataResponse
                    {
                        Amount = (cashAmount + promoAmount),
                        Level = LevelId.HighestProgressiveLevel,
                        PartialPayAmount = 0,
                        ProgressiveGroup = expectedGroupId,
                        ResetId = ResetId.OnlyStandardHandpayResetIsAvailable,
                        SessionGamePayAmount = (cashAmount + promoAmount),
                        SessionGameWinAmount = (cashAmount + promoAmount),
                        TransactionId = transactionId
                    });

            _history.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(
                    hasTransaction
                        ? new List<HandpayTransaction>
                        {
                            new HandpayTransaction(
                                0,
                                DateTime.UtcNow,
                                cashAmount,
                                promoAmount,
                                0,
                                100,
                                type,
                                false,
                                Guid.Empty) { TransactionId = transactionId, State = state }
                        }
                        : new List<HandpayTransaction>());

            _bank.Setup(x => x.Limit).Returns(limit);
            _bank.Setup(x => x.QueryBalance()).Returns(currentBalance);
            _properties.Setup(x => x.GetProperty(AccountingConstants.HandpayLimit, It.IsAny<long>()))
                .Returns(handpayLimit);
            _properties.Setup(
                x => x.GetProperty(
                    AccountingConstants.LargeWinHandpayResetMethod,
                    It.IsAny<LargeWinHandpayResetMethod>())).Returns(resetMethod);

            var actual = _target.Handle(new LongPollHandpayData { ClientNumber = ClientNumber, AccountingDenom = 100 });
            Assert.AreEqual(expectedAmount, actual.Amount);
            Assert.AreEqual(LevelId.HighestProgressiveLevel, actual.Level);
            Assert.AreEqual(0, actual.PartialPayAmount);
            Assert.AreEqual(expectedGroupId, actual.ProgressiveGroup);
            Assert.AreEqual(expectedResetId, actual.ResetId);
            Assert.AreEqual(expectedAmount, actual.SessionGamePayAmount);
            Assert.AreEqual(expectedAmount, actual.SessionGameWinAmount);
        }

        [TestMethod]
        public void HandleUnreadHandpayTest()
        {
            const long cashoutAmount = 1000000;
            const long promoAmount = 3000;
            var expectedAmount = (cashoutAmount + promoAmount).MillicentsToCents();

            _sasHandPayCommittedHandler.Setup(x => x.GetNextUnreadHandpayTransaction(ClientNumber))
                .Returns(new LongPollHandpayDataResponse
                {
                    Amount = (cashoutAmount + promoAmount),
                    Level = LevelId.NonProgressiveWin,
                    PartialPayAmount = 0,
                    ProgressiveGroup = 0,
                    ResetId = ResetId.OnlyStandardHandpayResetIsAvailable,
                    SessionGamePayAmount = (cashoutAmount + promoAmount),
                    SessionGameWinAmount = (cashoutAmount + promoAmount)
                });

            _history.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());

            var actual = _target.Handle(new LongPollHandpayData { ClientNumber = ClientNumber, AccountingDenom = 1 });
            Assert.AreEqual(expectedAmount, actual.Amount);
            Assert.AreEqual(LevelId.NonProgressiveWin, actual.Level);
            Assert.AreEqual(0, actual.PartialPayAmount);
            Assert.AreEqual((uint)0, actual.ProgressiveGroup);
            Assert.AreEqual(ResetId.OnlyStandardHandpayResetIsAvailable, actual.ResetId);
            Assert.AreEqual(expectedAmount, actual.SessionGamePayAmount);
            Assert.AreEqual(expectedAmount, actual.SessionGameWinAmount);
        }

        [TestMethod]
        public void HandleNoUnreadHandpayTest()
        {
            _sasHandPayCommittedHandler.Setup(x => x.GetNextUnreadHandpayTransaction(ClientNumber))
                .Returns((LongPollHandpayDataResponse)null);
            _history.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());

            var actual = _target.Handle(new LongPollHandpayData { ClientNumber = ClientNumber, AccountingDenom = 1 });
            Assert.AreEqual(0L, actual.Amount);
            Assert.AreEqual(LevelId.NonProgressiveWin, actual.Level);
            Assert.AreEqual(0L, actual.PartialPayAmount);
            Assert.AreEqual((uint)0, actual.ProgressiveGroup);
            Assert.AreEqual(ResetId.OnlyStandardHandpayResetIsAvailable, actual.ResetId);
            Assert.AreEqual(0L, actual.SessionGamePayAmount);
            Assert.AreEqual(0L, actual.SessionGameWinAmount);
        }

        private LP1BSendHandpayInfoHandler CreateTarget(
            bool nullHandpayProvider = false,
            bool nullHistory = false,
            bool nullProperties = false,
            bool nullBank = false)
        {
            return new LP1BSendHandpayInfoHandler(
                nullHandpayProvider ? null : _sasHandPayCommittedHandler.Object,
                nullHistory ? null : _history.Object,
                nullProperties ? null : _properties.Object,
                nullBank ? null : _bank.Object);
        }
    }
}
