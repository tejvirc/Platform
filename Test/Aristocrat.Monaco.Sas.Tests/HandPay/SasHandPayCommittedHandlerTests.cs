namespace Aristocrat.Monaco.Sas.Tests.HandPay
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.MarketConfig.Models.Accounting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Progressive;
    using Sas.HandPay;

    [TestClass]
    public class SasHandPayCommittedHandlerTests
    {
        private const int WaitTime = 1000;
        private const byte ClientNumber = 100;

        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IHandpayQueue> _handpayQueue;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IProgressiveWinDetailsProvider> _progressiveProvider;
        private Mock<IBank> _bank;
        private SasHandPayCommittedHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _handpayQueue = new Mock<IHandpayQueue>(MockBehavior.Default);
            _gameHistory = new Mock<IGameHistory>(MockBehavior.Default);
            _progressiveProvider = new Mock<IProgressiveWinDetailsProvider>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { HandpayReportingType = SasHandpayReportingType.SecureHandpayReporting });
            _propertiesManager.Setup(mock => mock.GetProperty(AccountingConstants.HandpayLimit, It.IsAny<long>()))
                .Returns(2000000L);
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());

            _target = CreateHandler();
            _target.RegisterHandpayQueue(_handpayQueue.Object, ClientNumber);
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false)]
        [DataRow(false, false, true, false, false, false)]
        [DataRow(false, false, false, true, false, false)]
        [DataRow(false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTransactionHistory(
            bool nullTransactionHistory,
            bool nullProperties,
            bool nullExceptionHandler,
            bool nullGameHistory,
            bool nullProgressiveWinDetailProvider,
            bool nullBank)
        {
            _target = CreateHandler(
                nullTransactionHistory,
                nullProperties,
                nullExceptionHandler,
                nullGameHistory,
                nullProgressiveWinDetailProvider,
                nullBank);
        }

        [TestMethod]
        public void LegacyHandpayResetAlwaysPostRest()
        {
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { HandpayReportingType = SasHandpayReportingType.LegacyHandpayReporting });
            _target = CreateHandler();
            _target.RegisterHandpayQueue(_handpayQueue.Object, ClientNumber);
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.HandPayWasReset), ClientNumber))
                .Verifiable();
            _target.HandPayReset(new HandpayTransaction { State = HandpayState.Committed, Read = false });
            _exceptionHandler.Verify();
        }

        [DataRow(KeyOffType.LocalCredit, true)]
        [DataRow(KeyOffType.LocalHandpay, false)]
        [DataRow(KeyOffType.LocalVoucher, false)]
        [DataRow(KeyOffType.LocalWat, false)]
        [DataRow(KeyOffType.RemoteCredit, true)]
        [DataRow(KeyOffType.RemoteHandpay, false)]
        [DataRow(KeyOffType.RemoteVoucher, false)]
        [DataRow(KeyOffType.RemoteWat, false)]
        [DataRow(KeyOffType.Unknown, false)]
        [DataRow(KeyOffType.Cancelled, false)]
        [DataTestMethod]
        public void SecureHandpayResetPostWhenAllAreRead(KeyOffType keyOffType, bool reportJackpotHandpayKeyedOffToMachinePay)
        {
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(
                    new List<HandpayTransaction>
                    {
                        new HandpayTransaction(
                            0,
                            DateTime.Now,
                            100,
                            200,
                            300,
                            400,
                            HandpayType.GameWin,
                            false,
                            Guid.NewGuid())
                        {
                            Read = true,
                            State = HandpayState.Committed,
                        }
                    });

            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.HandPayWasReset), ClientNumber))
                .Verifiable();
            if (reportJackpotHandpayKeyedOffToMachinePay)
            {
                _exceptionHandler.Setup(
                        x => x.ReportException(
                            It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.JackpotHandpayKeyedOffToMachinePay), ClientNumber))
                    .Verifiable();
            }

            _handpayQueue.Setup(x => x.Count).Returns(0);
            _target.HandPayReset(new HandpayTransaction { State = HandpayState.Committed, Read = false, KeyOffType = keyOffType });

            _transactionHistory.Verify();
            _exceptionHandler.Verify();
        }

        [DataRow(
            HandpayState.Requested,
            SasHandpayReportingType.SecureHandpayReporting,
            DisplayName = "Secure Handpay Reporting will report handpay pending when Requested")]
        [DataRow(
            HandpayState.Acknowledged,
            SasHandpayReportingType.SecureHandpayReporting,
            DisplayName = "Secure Handpay Reporting will report handpay pending when Acknowledged")]
        [DataRow(
            HandpayState.Committed,
            SasHandpayReportingType.SecureHandpayReporting,
            DisplayName = "Secure Handpay Reporting will report handpay pending when Committed")]
        [DataRow(
            HandpayState.Requested,
            SasHandpayReportingType.LegacyHandpayReporting,
            DisplayName = "Legacy Handpay Reporting will report handpay pending when Requested")]
        [DataTestMethod]
        public void ProcessUnreadTransactionTest(HandpayState state, SasHandpayReportingType reportingType)
        {
            const long transactionId = 100;
            var handpayTransaction = new HandpayTransaction(
                0,
                DateTime.Now,
                100,
                200,
                300,
                400,
                HandpayType.CancelCredit,
                false,
                Guid.NewGuid())
            {
                TransactionId = transactionId,
                Read = false,
                State = state
            };

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { HandpayReportingType = reportingType });
            _target = CreateHandler();
            _target.RegisterHandpayQueue(_handpayQueue.Object, ClientNumber);
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(
                    new List<HandpayTransaction>
                    {
                        handpayTransaction
                    });

            _propertiesManager
                .Setup(
                    x => x.GetProperty(
                        AccountingConstants.LargeWinHandpayResetMethod,
                        It.IsAny<LargeWinHandpayResetMethod>())).Returns(LargeWinHandpayResetMethod.PayByHand);
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.HandPayIsPending), ClientNumber))
                .Verifiable();
            if (reportingType != SasHandpayReportingType.LegacyHandpayReporting)
            {
                _handpayQueue
                    .Setup(
                        x => x.Enqueue(
                            It.Is<LongPollHandpayDataResponse>(r => ValidHandpayData(handpayTransaction, r))))
                    .Returns(Task.CompletedTask);
            }

            Assert.IsTrue(_target.HandpayPending(handpayTransaction).Wait(WaitTime));
            _exceptionHandler.Verify();
        }

        [DataRow(
            HandpayState.Requested,
            true,
            DisplayName = "Legacy Handpay Reporting will return a result when the state is requested")]
        [DataRow(
            HandpayState.Acknowledged,
            false,
            DisplayName = "Legacy Handpay Reporting will not return a result when the state is acknowledged")]
        [DataRow(
            HandpayState.Committed,
            false,
            DisplayName = "Legacy Handpay Reporting will not return a result when the state is committed")]
        [DataRow(
            HandpayState.Pending,
            true,
            DisplayName = "Legacy Handpay Reporting will return a result when the state is pending")]
        [DataTestMethod]
        public void LegacyHandpayUnreadRecordsTest(HandpayState state, bool transactionFound)
        {
            var transaction = new HandpayTransaction(
                0,
                DateTime.Now,
                ((long)100).CentsToMillicents(),
                ((long)200).CentsToMillicents(),
                0,
                100,
                HandpayType.CancelCredit,
                true,
                Guid.NewGuid())
            {
                TransactionId = 123,
                State = state
            };

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { HandpayReportingType = SasHandpayReportingType.LegacyHandpayReporting });
            _target = CreateHandler();
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction> { transaction });
            _propertiesManager
                .Setup(
                    x => x.GetProperty(
                        AccountingConstants.LargeWinHandpayResetMethod,
                        It.IsAny<LargeWinHandpayResetMethod>())).Returns(LargeWinHandpayResetMethod.PayByHand);

            var result = _target.GetNextUnreadHandpayTransaction(0);
            if (transactionFound)
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(transaction.TransactionId, result.TransactionId);
                Assert.AreEqual((transaction.CashableAmount + transaction.PromoAmount), result.Amount);
            }
            else
            {
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void RecoveryWithPendingReadTest()
        {
            const long transactionId = 100;

            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(
                    new List<HandpayTransaction>
                    {
                        new HandpayTransaction(
                            0,
                            DateTime.Now,
                            100,
                            200,
                            300,
                            400,
                            HandpayType.CancelCredit,
                            false,
                            Guid.NewGuid())
                        {
                            TransactionId = transactionId,
                            Read = false,
                            State = HandpayState.Committed
                        }
                    });

            _transactionHistory.Setup(
                x => x.UpdateTransaction(It.Is<HandpayTransaction>(t => t.TransactionId == transactionId && t.Read))).Verifiable();
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.HandPayWasReset), ClientNumber))
                .Verifiable();
            _handpayQueue.Setup(x => x.Count).Returns(0);

            _target.Recover();
            _transactionHistory.Verify();
            _exceptionHandler.Verify();
        }

        [DataRow(HandpayState.Committed)]
        [DataRow(HandpayState.Acknowledged)]
        [DataTestMethod]
        public void RecoveryWithPendingReadLegacyHandpayReportingTest(HandpayState currentState)
        {
            const long transactionId = 100;

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { HandpayReportingType = SasHandpayReportingType.LegacyHandpayReporting });
            _target = CreateHandler();
            _target.RegisterHandpayQueue(_handpayQueue.Object, ClientNumber);
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(
                    new List<HandpayTransaction>
                    {
                        new HandpayTransaction(
                            0,
                            DateTime.Now,
                            100,
                            200,
                            300,
                            400,
                            HandpayType.CancelCredit,
                            false,
                            Guid.NewGuid())
                        {
                            TransactionId = transactionId,
                            Read = false,
                            State = currentState
                        }
                    });

            _handpayQueue.Setup(x => x.Count).Returns(1);
            _transactionHistory.Setup(
                x => x.UpdateTransaction(It.Is<HandpayTransaction>(t => t.TransactionId == transactionId && t.Read))).Verifiable();
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.HandPayWasReset), ClientNumber))
                .Verifiable();

            _target.Recover();
            _transactionHistory.Verify();
            _exceptionHandler.Verify();
        }

        [DataRow(HandpayState.Committed)]
        [DataRow(HandpayState.Acknowledged)]
        [DataTestMethod]
        public void RecoveryWithAnyPendingUnread(HandpayState currentState)
        {
            const long transactionId = 100;

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { HandpayReportingType = SasHandpayReportingType.LegacyHandpayReporting });
            _target = CreateHandler();
            _target.RegisterHandpayQueue(_handpayQueue.Object, ClientNumber);
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(
                    new List<HandpayTransaction>
                    {
                        new HandpayTransaction(
                            0,
                            DateTime.Now,
                            100,
                            200,
                            300,
                            400,
                            HandpayType.CancelCredit,
                            false,
                            Guid.NewGuid())
                        {
                            TransactionId = transactionId,
                            Read = true,
                            State = currentState
                        }
                    });

            _handpayQueue.Setup(x => x.Count).Returns(1);
            _transactionHistory.Setup(
                    x => x.UpdateTransaction(
                        It.Is<HandpayTransaction>(t => t.TransactionId == transactionId && t.Read)))
                .Verifiable();

            _target.Recover();
            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<HandpayTransaction>()), Times.Never);
            _exceptionHandler.Verify(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.HandPayWasReset),
                    ClientNumber),
                Times.Never);
        }

        [DataRow(KeyOffType.LocalCredit, true)]
        [DataRow(KeyOffType.LocalHandpay, false)]
        [DataRow(KeyOffType.LocalVoucher, false)]
        [DataRow(KeyOffType.LocalWat, false)]
        [DataRow(KeyOffType.RemoteCredit, true)]
        [DataRow(KeyOffType.RemoteHandpay, false)]
        [DataRow(KeyOffType.RemoteVoucher, false)]
        [DataRow(KeyOffType.RemoteWat, false)]
        [DataRow(KeyOffType.Unknown, false)]
        [DataRow(KeyOffType.Cancelled, false)]
        [DataTestMethod]
        public void SecureHandpayReportWithMultipleUnAcknowledgeHandPayReadTest(KeyOffType keyOffType, bool reportJackpotHandpayKeyedOffToMachinePay)
        {
            const long transactionId = 100;
            var waiter = new ManualResetEvent(false);

            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.HandPayWasReset), ClientNumber)).Verifiable();
            if (reportJackpotHandpayKeyedOffToMachinePay)
            {
                _exceptionHandler.Setup(
                        x => x.ReportException(
                            It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.JackpotHandpayKeyedOffToMachinePay), ClientNumber)).Verifiable();
            }

            _exceptionHandler.Setup(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.HandPayIsPending),
                    ClientNumber)).Callback(() => waiter.Set());
            _transactionHistory.SetupSequence(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>
                {
                    new HandpayTransaction(
                        0,
                        DateTime.Now,
                        100,
                        200,
                        300,
                        400,
                        HandpayType.GameWin,
                        false,
                        Guid.NewGuid())
                    {
                        TransactionId = transactionId,
                        Read = false,
                        State = HandpayState.Committed,
                        KeyOffType = keyOffType
                    },
                    new HandpayTransaction(
                        0,
                        DateTime.Now,
                        100,
                        200,
                        300,
                        400,
                        HandpayType.CancelCredit,
                        false,
                        Guid.NewGuid())
                    {
                        TransactionId = transactionId + 1,
                        Read = false,
                        State = HandpayState.Pending
                    }
                });

            _handpayQueue.Setup(x => x.Count).Returns(1);
            _handpayQueue.Raise(e => e.OnAcknowledged += null, ClientNumber, transactionId);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            _exceptionHandler.Verify();
        }

        private bool ValidHandpayData(HandpayTransaction transaction, LongPollHandpayDataResponse actualData)
        {
            return transaction.TransactionId == actualData.TransactionId && transaction.TransactionAmount == actualData.Amount;
        }

        private SasHandPayCommittedHandler CreateHandler(
            bool nullTransactionHistory = false,
            bool nullProperties = false,
            bool nullExceptionHandler = false,
            bool nullGameHistory = false,
            bool nullProgressiveWinDetailProvider = false,
            bool nullBank = false)
        {
            return new SasHandPayCommittedHandler(
                nullTransactionHistory ? null : _transactionHistory.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullExceptionHandler ? null : _exceptionHandler.Object,
                nullGameHistory ? null : _gameHistory.Object,
                nullProgressiveWinDetailProvider ? null : _progressiveProvider.Object,
                nullBank ? null : _bank.Object);
        }
    }
}