namespace Aristocrat.Monaco.Sas.Tests.EftTransferProvider
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Sas.Contracts.Eft;
    using Aristocrat.Sas.Client.Eft;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using EftTransferProvider = Sas.EftTransferProvider.EftTransferProvider;

    /// <summary>
    ///     Contains the unit tests for the AftTransferProvider class
    /// </summary>
    [TestClass]
    public class EftTransferProviderTest
    {
        private IEftTransferProvider _target;
        private readonly Mock<IEftOffTransferProvider> _eftOff = new Mock<IEftOffTransferProvider>(MockBehavior.Strict);
        private readonly Mock<IEftOnTransferProvider> _eftOn = new Mock<IEftOnTransferProvider>(MockBehavior.Strict);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Strict);
        private readonly Mock<IMeterManager> _meterManager = new Mock<IMeterManager>(MockBehavior.Strict);
        private Mock<ITransactionHistory> _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);

        private Mock<IMeter> _onCashableAmountMeter = new Mock<IMeter>(MockBehavior.Strict);
        private Mock<IMeter> _onPromoAmountMeter = new Mock<IMeter>(MockBehavior.Strict);
        private Mock<IMeter> _onNonCashAmountMeter = new Mock<IMeter>(MockBehavior.Strict);

        private Mock<IMeter> _offCashableAmountMeter = new Mock<IMeter>(MockBehavior.Strict);
        private Mock<IMeter> _offPromoAmountMeter = new Mock<IMeter>(MockBehavior.Strict);
        private Mock<IMeter> _offNonCashAmountMeter = new Mock<IMeter>(MockBehavior.Strict);

        private const ulong TransferLimit = 1000UL;
        private const long bankLimit = 100L;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _eftOff.Setup(m => m.EftOffRequest(It.IsAny<string>(), It.IsAny<AccountType[]>(), It.IsAny<ulong>())).Returns(true);
            _eftOff.SetupGet(m => m.CanTransfer).Returns(true);

            _eftOn.Setup(m => m.EftOnRequest(It.IsAny<string>(), It.IsAny<AccountType>(), It.IsAny<ulong>())).Returns(true);
            _eftOn.SetupGet(m => m.CanTransfer).Returns(true);

            _bank.Setup(m => m.Limit).Returns(bankLimit);

            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOnNonCashableAmount)).Returns(_onNonCashAmountMeter.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOnCashableAmount)).Returns(_onCashableAmountMeter.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOnCashablePromoAmount)).Returns(_onPromoAmountMeter.Object);

            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOffNonCashableAmount)).Returns(_offNonCashAmountMeter.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOffCashableAmount)).Returns(_offCashableAmountMeter.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOffCashablePromoAmount)).Returns(_offPromoAmountMeter.Object);
            _target = CreateProvider();
        }

        private EftTransferProvider CreateProvider(
            bool nullBank = false,
            bool nullMeter = false,
            bool nullEftOff = false,
            bool nullEftOn = false)
        {
            return new EftTransferProvider(
                nullEftOff ? null : _eftOff.Object,
                nullEftOn ? null : _eftOn.Object,
                _transactionHistory.Object,
                nullBank ? null : _bank.Object,
                nullMeter ? null : _meterManager.Object);
        }

        [DataTestMethod]
        [DataRow(false, true, true, true)]
        [DataRow(true, true, true, true)]
        [DataRow(true, false, true, true)]
        [DataRow(true, true, false, true)]
        [DataRow(true, true, true, false)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullBank = false,
            bool nullMeter = false,
            bool nullEftOff = false,
            bool nullEftOn = false)
        {
            CreateProvider(
                nullBank,
                nullMeter,
                nullEftOff,
                nullEftOn);
        }

        [DataTestMethod]
        [DataRow(AccountType.Cashable)]
        [DataRow(AccountType.Promo)]
        [DataRow(AccountType.NonCash)]
        public void ProcessEftOffRequestTest(AccountType accountType)
        {
            var r = _target.DoEftOff("TRANSACTIONID", accountType, 20);
            Assert.IsTrue(r);
        }

        [DataTestMethod]
        [DataRow(AccountType.Cashable)]
        [DataRow(AccountType.Promo)]
        [DataRow(AccountType.NonCash)]
        public void ProcessEftOnRequestTest(AccountType accountType)
        {
            var r = _target.DoEftOn("TRANSACTIONID", accountType, 20);
            Assert.IsTrue(r);
        }

        [TestMethod]
        public void RestartCashoutTimerTest()
        {
            _eftOff.Setup(x => x.RestartCashoutTimer());
            _target.RestartCashoutTimer();
            _eftOff.Verify(x => x.RestartCashoutTimer(), Times.Once);
        }

        [DataTestMethod]
        [DataRow(90ul, false)]
        [DataRow(1000ul, true)]
        public void GetAcceptedTransferInAmountTest(ulong amount, bool limitExceeded)
        {
            _eftOn.Setup(x => x.GetAcceptedTransferInAmount(It.IsAny<ulong>())).Returns((amount, limitExceeded));
            (var r, var e) = _target.GetAcceptedTransferInAmount(amount);
            Assert.AreEqual(r, amount);
            Assert.AreEqual(e, limitExceeded);
        }

        [DataTestMethod]
        [DataRow(AccountType.Cashable, 20UL)]
        [DataRow(AccountType.Promo, 80UL)]
        [DataRow(AccountType.NonCash, 110UL)]
        [DataRow(AccountType.Cashable, 20UL, false)]
        [DataRow(AccountType.Promo, 80UL, false)]
        [DataRow(AccountType.NonCash, 110UL, false)]
        public void GetAcceptedTransferOutAmountTest(AccountType accountType, ulong amount, bool limitExceeded = true)
        {
            _eftOff.Setup(x => x.GetAcceptedTransferOutAmount(It.IsAny<AccountType[]>())).Returns((amount, limitExceeded));
            var r = _target.GetAcceptedTransferOutAmount(accountType);
            Assert.AreEqual(r, (amount, limitExceeded));
        }

        [TestMethod]
        public void CheckIfProcessedEmptyWatOnTransactionListTest()
        {
            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>()).Returns(new WatOnTransaction[] { });
            _transactionHistory.Setup(x => x.RecallTransactions<WatTransaction>()).Returns(new WatTransaction[] { });
            const string TransactionNumber = "TRANSACTION-NUMBER";
            var r = _target.CheckIfProcessed(TransactionNumber, EftTransferType.In);
            Assert.AreEqual(r, false);
        }

        [DataTestMethod]
        [DataRow("TRANSACTION-NUMBER", "REQUEST-ID", WatStatus.Complete, false)]
        [DataRow("TRANSACTION-NUMBER", "TRANSACTION-NUMBER", WatStatus.Complete, true)]
        [DataRow("TRANSACTION-NUMBER", "REQUEST-ID", WatStatus.RequestReceived, false)]
        [DataRow("TRANSACTION-NUMBER", "TRANSACTION-NUMBER", WatStatus.RequestReceived, false)]
        public void CheckIfProcessedWatOnTransactionTest(string transactionNumber, string requestID, WatStatus status, bool result)
        {
            var watOnTransaction = new WatOnTransaction(0, DateTime.Now, 0, 0, 0, false, requestID) { Status = status };
            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>()).Returns(new[] { watOnTransaction });
            _transactionHistory.Setup(x => x.RecallTransactions<WatTransaction>()).Returns(new WatTransaction[] { });
            var r = _target.CheckIfProcessed(transactionNumber, EftTransferType.In);
            Assert.AreEqual(r, result);
        }

        [TestMethod]
        public void CheckIfProcessedEmptyWatTransactionListTest()
        {
            _transactionHistory.Setup(x => x.RecallTransactions<WatTransaction>()).Returns(new WatTransaction[] { });
            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>()).Returns(new WatOnTransaction[] { });
            const string TransactionNumber = "TRANSACTION-NUMBER";
            var r = _target.CheckIfProcessed(TransactionNumber, EftTransferType.In);
            Assert.AreEqual(r, false);
        }

        [DataTestMethod]
        [DataRow("TRANSACTION-NUMBER", "REQUEST-ID", WatStatus.Complete, false)]
        [DataRow("TRANSACTION-NUMBER", "TRANSACTION-NUMBER", WatStatus.Complete, true)]
        [DataRow("TRANSACTION-NUMBER", "REQUEST-ID", WatStatus.RequestReceived, false)]
        [DataRow("TRANSACTION-NUMBER", "TRANSACTION-NUMBER", WatStatus.RequestReceived, false)]
        public void CheckIfProcessedWatTransactionTest(string transactionNumber, string requestID, WatStatus status, bool result)
        {
            var watTransaction = new WatTransaction(0, DateTime.Now, 0, 0, 0, false, requestID) { Status = status };
            _transactionHistory.Setup(x => x.RecallTransactions<WatTransaction>()).Returns(new[] { watTransaction });
            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>()).Returns(new WatOnTransaction[] { });
            var r = _target.CheckIfProcessed(transactionNumber, EftTransferType.Out);
            Assert.AreEqual(result, r);
        }

        [DataTestMethod]
        [DataRow(-2, false)]
        [DataRow(2, true)]
        public void CheckIfProcessedWatOnandOffTransactionTest(int dayOffset, bool result)
        {
            var watOnTransaction1 = new WatOnTransaction(0, DateTime.Now, 0, 0, 0, false, "Transaction-Number") { Status = WatStatus.Complete };
            var watOnTransaction2 = new WatOnTransaction(0, DateTime.Now.AddDays(dayOffset), 0, 0, 0, false, "REQUEST-ID") { Status = WatStatus.Complete };
            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>()).Returns(new[] { watOnTransaction1, watOnTransaction2 });
            var r = _target.CheckIfProcessed("REQUEST-ID", EftTransferType.In);
            Assert.AreEqual(r, result);
        }

        [TestMethod]
        public void QueryBalanceAmountTest()
        {
            var nonCashableCredits = 101l;
            _onNonCashAmountMeter.Setup(x => x.Lifetime).Returns(nonCashableCredits);
            var cashableCredits = 102l;
            _onCashableAmountMeter.Setup(x => x.Lifetime).Returns(cashableCredits);
            var promotionalCredits = 103l;
            _onPromoAmountMeter.Setup(x => x.Lifetime).Returns(promotionalCredits);

            var offNonCashAmount = 104l;
            _offNonCashAmountMeter.Setup(x => x.Lifetime).Returns(offNonCashAmount);
            var offCashableAmount = 105l;
            _offCashableAmountMeter.Setup(x => x.Lifetime).Returns(offCashableAmount);
            var offPromoAmount = 106l;
            _offPromoAmountMeter.Setup(x => x.Lifetime).Returns(offPromoAmount);

            var r = _target.QueryBalanceAmount();
            Assert.AreEqual(r.NonCashableCredits, (ulong)promotionalCredits);
            Assert.AreEqual(r.CashableCredits, (ulong)cashableCredits);
            Assert.AreEqual(r.PromotionalCredits, (ulong)nonCashableCredits);
            Assert.AreEqual(r.TransferredCredits, (ulong)(offNonCashAmount + offCashableAmount + offPromoAmount));
        }

        [TestMethod]
        public void GetCurrentPromotionalCreditsTest()
        {
            const long result = 100;
            _bank.Setup(x => x.QueryBalance(AccountType.NonCash)).Returns(result.CentsToMillicents());
            var r = _target.GetCurrentPromotionalCredits();
            Assert.AreEqual(r, result);
        }

        [DataTestMethod]
        [DataRow(false, false)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        [DataRow(false, true)]
        public void GetSupportedTransferTypesTest(bool canOff, bool canOn)
        {
            _eftOff.SetupGet(m => m.CanTransfer).Returns(canOff);
            _eftOn.SetupGet(m => m.CanTransfer).Returns(canOn);
            var (on, off) = _target.GetSupportedTransferTypes();

            Assert.AreEqual(canOn, on);
            Assert.AreEqual(canOff, off);
        }
    }
}
