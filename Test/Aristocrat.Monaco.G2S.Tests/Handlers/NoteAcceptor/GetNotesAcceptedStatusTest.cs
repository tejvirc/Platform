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
    using FizzWare.NBuilder;
    using G2S.Handlers.NoteAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ITransaction = Accounting.Contracts.ITransaction;

    [TestClass]
    public class GetNotesAcceptedStatusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetNotesAcceptedStatus(null, null);
            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullTransactionHistoryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetNotesAcceptedStatus(egm.Object, null);
            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var history = new Mock<ITransactionHistory>();
            var handler = new GetNotesAcceptedStatus(egm.Object, history.Object);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var history = new Mock<ITransactionHistory>();
            var handler = new GetNotesAcceptedStatus(egm.Object, history.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenWithNotEmptyTransactionLogExpectSuccesfullResponse()
        {
            var transactionHistory = new Mock<ITransactionHistory>();
            var log = CreateTransactionLog();
            transactionHistory.Setup(m => m.RecallTransactions<BillTransaction>())
                .Returns(log.ToArray());

            var device = new Mock<INoteAcceptorDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            var handler = new GetNotesAcceptedStatus(egm, transactionHistory.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNotesAcceptedStatus>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, notesAcceptedStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(log.Count, response.Command.totalEntries);
            Assert.AreEqual(log.Max(x => x.LogSequence), response.Command.lastSequence);
        }

        [TestMethod]
        public async Task WhenWithNoTransactionsLogExpectNoError()
        {
            var transactionHistory = new Mock<ITransactionHistory>();
            transactionHistory.Setup(m => m.RecallTransactions<BillTransaction>())
                .Returns(new List<BillTransaction>());

            var device = new Mock<INoteAcceptorDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            var handler = new GetNotesAcceptedStatus(egm, transactionHistory.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNotesAcceptedStatus>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, notesAcceptedStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Command.totalEntries);
            Assert.AreEqual(0, response.Command.lastSequence);
        }

        private static ICollection<BillTransaction> CreateTransactionLog()
        {
            var logSequence = 1;
            var log = Builder<BillTransaction>.CreateListOfSize(10).All()
                .With(x => x.LogSequence = (logSequence++ + 5) % 10 + 1)
                .Build();

            return log;
        }
    }
}