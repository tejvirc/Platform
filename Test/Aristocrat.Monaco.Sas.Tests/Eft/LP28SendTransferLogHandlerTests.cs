namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client.Eft;
    using Aristocrat.Monaco.Sas.Eft;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Contains unit tests for the LP63TransferPromoCreditHandler class.
    /// </summary>
    [TestClass]
    public class LP28SendTransferLogHandlerTests
    {
        private Mock<IEftHistoryLogProvider> _eftHistoricalLogsProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _eftHistoricalLogsProvider = new Mock<IEftHistoryLogProvider>(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfNoInjectionTest()
        {
            new LP28SendTransferLogHandler(null);
        }

        [TestMethod]
        public void CommandTest()
        {
            var handler = new LP28SendTransferLogHandler(_eftHistoricalLogsProvider.Object);
            var expected = new List<LongPoll> { LongPoll.EftSendTransferLogs };
            Assert.IsTrue(handler.Commands.Count == 1);
            Assert.AreEqual(expected[0], handler.Commands[0]);
        }

        [TestMethod]
        public void ShouldNotReturnLogsIfThereIsNoLogsTest()
        {
            _eftHistoricalLogsProvider.Setup(l => l.GetHistoryLogs()).Returns(new IEftHistoryLogEntry[0]);
            var lp28Handler = new LP28SendTransferLogHandler(_eftHistoricalLogsProvider.Object);
            var handlerResponse = lp28Handler.Handle(new LongPollData());
            Assert.AreEqual(0, handlerResponse.EftTransactionLogs.Length);
            _eftHistoricalLogsProvider.Verify(m => m.GetHistoryLogs(), Times.Once);
        }

        [TestMethod]
        public void ShouldReturnCorrectLogsIfThereIsLessLogsTest()
        {
            var logs = new List<IEftHistoryLogEntry>();
            var theLog1 = new Mock<IEftHistoryLogEntry>();
            theLog1.SetupGet(x => x.Command).Returns(LongPoll.EftTransferCashableCreditsToMachine);
            theLog1.SetupGet(x => x.TransactionNumber).Returns(0x01);
            theLog1.SetupGet(x => x.Acknowledgement).Returns(true);
            theLog1.SetupGet(x => x.ReportedTransactionStatus).Returns(TransactionStatus.EgmOutOfService);
            theLog1.SetupGet(x => x.RequestedTransactionAmount).Returns(10000);
            logs.Add(theLog1.Object);

            var theLog2 = new Mock<IEftHistoryLogEntry>();
            theLog2.SetupGet(x => x.Command).Returns(LongPoll.EftTransferCashableCreditsToMachine);
            theLog2.SetupGet(x => x.TransactionNumber).Returns(0x02);
            theLog2.SetupGet(x => x.Acknowledgement).Returns(true);
            theLog2.SetupGet(x => x.ReportedTransactionStatus).Returns(TransactionStatus.EgmOutOfService);
            theLog2.SetupGet(x => x.RequestedTransactionAmount).Returns(10000);
            logs.Add(theLog2.Object);

            _eftHistoricalLogsProvider.Setup(l => l.GetHistoryLogs()).Returns(logs.ToArray());
            var lp28Handler = new LP28SendTransferLogHandler(_eftHistoricalLogsProvider.Object);
            var handlerResponse = lp28Handler.Handle(new LongPollData());
            Assert.AreEqual(logs.Count, handlerResponse.EftTransactionLogs.Length);
            _eftHistoricalLogsProvider.Verify(m => m.GetHistoryLogs(), Times.Once);
        }

        [TestMethod]
        public void ShouldReturnMaximumLogsIfThereIsMoreLogsTest()
        {
            var logs = new List<IEftHistoryLogEntry>();

            //10 > SasConstants.EftHistoryLogsSize (5)
            for (int i = 0; i < 10; i++)
            {
                var theLog = new Mock<IEftHistoryLogEntry>();
                theLog.SetupGet(x => x.Command).Returns(LongPoll.EftTransferCashableCreditsToMachine);
                theLog.SetupGet(x => x.TransactionNumber).Returns(0x01);
                theLog.SetupGet(x => x.Acknowledgement).Returns(true);
                theLog.SetupGet(x => x.ReportedTransactionStatus).Returns(TransactionStatus.EgmOutOfService);
                theLog.SetupGet(x => x.RequestedTransactionAmount).Returns(10000);
                logs.Add(theLog.Object);
            }         

            _eftHistoricalLogsProvider.Setup(l => l.GetHistoryLogs()).Returns(logs.ToArray());
            var lp28Handler = new LP28SendTransferLogHandler(_eftHistoricalLogsProvider.Object);
            var handlerResponse = lp28Handler.Handle(new LongPollData());
            Assert.AreEqual(SasConstants.EftHistoryLogsSize, handlerResponse.EftTransactionLogs.Length);
            _eftHistoricalLogsProvider.Verify(m => m.GetHistoryLogs(), Times.Once);
        }
    }
}