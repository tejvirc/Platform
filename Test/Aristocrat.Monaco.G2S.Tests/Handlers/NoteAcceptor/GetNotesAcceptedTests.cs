namespace Aristocrat.Monaco.G2S.Tests.Handlers.NoteAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.NoteAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetNotesAcceptedTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetNotesAccepted(null, null);
            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullTransactionHistoryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetNotesAccepted(egm.Object, null);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var history = new Mock<ITransactionHistory>();
            var handler = new GetNotesAccepted(egm.Object, history.Object);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var history = new Mock<ITransactionHistory>();
            var handler = new GetNotesAccepted(egm.Object, history.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenWithNotEmptyTransactionLogExpectSuccesfullResponse()
        {
            var transactionHistory = new Mock<ITransactionHistory>();
            var log = CreateTransactionLog();
            transactionHistory.Setup(m => m.RecallTransactions<BillTransaction>()).Returns(log.ToList());

            var mock = CreateNoteAcceptorDevice();
            var egm = HandlerUtilities.CreateMockEgm(mock);

            var handler = new GetNotesAccepted(egm, transactionHistory.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNotesAccepted>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Command.lastSequence = 7;
            command.Command.totalEntries = 5;

            await handler.Handle(command);

            var response =
                command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, notesAcceptedList>;

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task WhenRequestLastTransactionLogExpectLastTransaction()
        {
            var transactionHistory = new Mock<ITransactionHistory>();
            var log = CreateTransactionLog();
            transactionHistory.Setup(m => m.RecallTransactions<BillTransaction>()).Returns(log.ToList());

            var mock = CreateNoteAcceptorDevice();
            var egm = HandlerUtilities.CreateMockEgm(mock);

            var handler = new GetNotesAccepted(egm, transactionHistory.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNotesAccepted>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Command.lastSequence = 0;
            command.Command.totalEntries = 5;

            await handler.Handle(command);

            var response =
                command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, notesAcceptedList>;

            Assert.IsNotNull(response);

            Assert.AreEqual(command.Command.totalEntries, response.Command.notesAcceptedLog.Length);
        }

        [TestMethod]
        public async Task WhenRequestAllTransactionLogExpectAllTransactions()
        {
            var transactionHistory = new Mock<ITransactionHistory>();
            var log = CreateTransactionLog().ToList();
            transactionHistory.Setup(m => m.RecallTransactions<BillTransaction>()).Returns(log.ToList());

            var mock = CreateNoteAcceptorDevice();
            var egm = HandlerUtilities.CreateMockEgm(mock);

            var handler = new GetNotesAccepted(egm, transactionHistory.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNotesAccepted>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Command.lastSequence = log.Max(l => l.TransactionId);
            command.Command.totalEntries = 0;

            await handler.Handle(command);

            var response =
                command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, notesAcceptedList>;

            Assert.IsNotNull(response);

            Assert.AreEqual(log.Count(), response.Command.notesAcceptedLog.Length);
        }

        [TestMethod]
        public async Task WhenWithNoTransactionsLogExpectNoError()
        {
            var transactionHistory = new Mock<ITransactionHistory>();
            transactionHistory.Setup(m => m.RecallTransactions<BillTransaction>())
                .Returns(new List<BillTransaction>());

            var device = new Mock<INoteAcceptorDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            var handler = new GetNotesAccepted(egm, transactionHistory.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNotesAccepted>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, notesAcceptedList>;
            Assert.IsNotNull(response);
        }

        private static IEnumerable<BillTransaction> CreateTransactionLog()
        {
            var transactions = new List<BillTransaction>();

            for (var i = 1; i <= 10; i++)
            {
                var logSequence = (i + 5) % 10 + 1;
                transactions.Add(CreateBillTransaction(logSequence, 1, i, DateTime.UtcNow, 250000));
            }

            return transactions;
        }

        private static BillTransaction CreateBillTransaction(
            long logSequence,
            int deviceId,
            long transactionId,
            DateTime transactionDateTime,
            long amount)
        {
            return new BillTransaction(
                new char[3],
                deviceId,
                transactionDateTime,
                amount)
            {
                LogSequence = logSequence,
                TransactionId = transactionId
            };
        }

        private static Mock<INoteAcceptorDevice> CreateNoteAcceptorDevice()
        {
            var device = new Mock<INoteAcceptorDevice>();
            device.SetupGet(m => m.Owner).Returns(123);
            device.SetupGet(m => m.Guests).Returns(new[] { TestConstants.HostId });
            device.SetupGet(m => m.Active).Returns(true);

            return device;
        }
    }
}