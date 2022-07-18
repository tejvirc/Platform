namespace Aristocrat.Monaco.Sas.Tests.AftTransferProvider
{
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.AftTransferProvider;
    using Storage.Models;
    using Storage.Repository;

    [TestClass]
    public class AftHostCashOutProviderTests
    {
        private const int WaitTimeOut = 1000;

        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IHardCashOutLock> _hardCashOutLock;
        private Mock<IBank> _bank;
        private Mock<IStorageDataProvider<AftTransferOptions>> _transferOptionsProvider;
        private AftHostCashOutProvider _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _transferOptionsProvider = new Mock<IStorageDataProvider<AftTransferOptions>>(MockBehavior.Default);
            _hardCashOutLock = new Mock<IHardCashOutLock>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);
            _target = new AftHostCashOutProvider(
                _exceptionHandler.Object,
                _transferOptionsProvider.Object,
                _bank.Object,
                _hardCashOutLock.Object);
        }

        [DataRow(100, 200, 300, 100, GeneralExceptionCode.AftRequestForHostCashOut, DisplayName = "All cashable amounts amounts being requested is a host cashout")]
        [DataRow(100, 200, 300, 20, GeneralExceptionCode.AftRequestForHostToCashOutWin, DisplayName = "Not removing all cash is a host cashout win")]
        [DataTestMethod]
        public void CashOutAcceptedTest(
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            long availableBalance,
            GeneralExceptionCode expectedExceptionType)
        {
            _bank.Setup(x => x.QueryBalance()).Returns(availableBalance);
            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransferFlags = AftTransferFlags.HostCashOutEnableControl | AftTransferFlags.HostCashOutEnable
            });
            _exceptionHandler.Setup(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == expectedExceptionType)));
            var transaction = new WatTransaction { CashableAmount = cashableAmount, PromoAmount = promoAmount, NonCashAmount = nonCashAmount };
            var handleHostCashOut = _target.HandleHostCashOut(transaction);
            Assert.IsTrue(_target.HostCashOutPending);
            _target.CashOutAccepted();
            Assert.IsTrue(handleHostCashOut.Wait(WaitTimeOut));
            Assert.IsTrue(handleHostCashOut.Result);
        }

        [DataRow(100, 200, 300, 100, GeneralExceptionCode.AftRequestForHostCashOut, DisplayName = "All cashable amounts amounts being requested is a host cashout")]
        [DataRow(100, 200, 300, 20, GeneralExceptionCode.AftRequestForHostToCashOutWin, DisplayName = "Not removing all cash is a host cashout win")]
        [DataTestMethod]
        public void CashOutDeniedTest(
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            long availableBalance,
            GeneralExceptionCode expectedExceptionType)
        {
            _bank.Setup(x => x.QueryBalance()).Returns(availableBalance);
            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransferFlags = AftTransferFlags.HostCashOutEnableControl | AftTransferFlags.HostCashOutEnable
            });
            _exceptionHandler.Setup(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == expectedExceptionType)));
            var transaction = new WatTransaction { CashableAmount = cashableAmount, PromoAmount = promoAmount, NonCashAmount = nonCashAmount };
            var handleHostCashOut = _target.HandleHostCashOut(transaction);
            Assert.IsTrue(_target.HostCashOutPending);
            _target.CashOutDenied();
            Assert.IsTrue(handleHostCashOut.Wait(WaitTimeOut));
            Assert.IsFalse(handleHostCashOut.Result);
        }

        [DataRow(100, 200, 300, 100, GeneralExceptionCode.AftRequestForHostCashOut, DisplayName = "All cashable amounts amounts being requested is a host cashout")]
        [DataRow(100, 200, 300, 20, GeneralExceptionCode.AftRequestForHostToCashOutWin, DisplayName = "Not removing all cash is a host cashout win")]
        [DataTestMethod]
        public void HardCashOutModeFailureThenAcceptTest(
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            long availableBalance,
            GeneralExceptionCode expectedExceptionType)
        {
            _bank.Setup(x => x.QueryBalance()).Returns(availableBalance);
            _hardCashOutLock.Setup(x => x.PresentLockup()).Callback(() => _target.CashOutAccepted());
            _hardCashOutLock.Setup(x => x.RemoveLockupPresentation());
            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransferFlags = AftTransferFlags.HostCashOutEnableControl | AftTransferFlags.HostCashOutEnable | AftTransferFlags.HostCashOutMode
            });
            _exceptionHandler.Setup(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == expectedExceptionType)));
            var transaction = new WatTransaction { CashableAmount = cashableAmount, PromoAmount = promoAmount, NonCashAmount = nonCashAmount };
            var handleHostCashOut = _target.HandleHostCashOut(transaction);
            Assert.IsTrue(_target.HostCashOutPending);
            _target.CashOutDenied();

            Assert.IsTrue(handleHostCashOut.Wait(WaitTimeOut));

            // We accepted the transfer after being locked up in a host cashout failure
            Assert.IsTrue(handleHostCashOut.Result);
        }

        [TestMethod]
        public void NotInHostCashOutTest()
        {
            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransferFlags = AftTransferFlags.None
            });
            var transaction = new WatTransaction { CashableAmount = 100, PromoAmount = 100, NonCashAmount = 100 };
            var handleHostCashOut = _target.HandleHostCashOut(transaction);

            Assert.IsTrue(handleHostCashOut.Wait(WaitTimeOut));
            Assert.IsFalse(handleHostCashOut.Result);
        }
    }
}