namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using System.Collections.ObjectModel;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using G2S.Consumers;
    using G2S.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ITransaction = Accounting.Contracts.ITransaction;

    [TestClass]
    public class DocumentStackedConsumerTest : NoteAcceptorConsumerTestBase
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMetersAgregatorExpectException()
        {
            var consumer = new CurrencyInCompletedConsumer(
                EgmMock.Object,
                CommandBuilderMock.Object,
                null,
                null,
                null,
                EventLiftMock.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullTransactionHistoryExpectException()
        {
            var metersAgregatorMock = new Mock<IMeterAggregator<INoteAcceptorDevice>>();

            var consumer = new CurrencyInCompletedConsumer(
                EgmMock.Object,
                CommandBuilderMock.Object,
                metersAgregatorMock.Object,
                null,
                null,
                EventLiftMock.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var metersAgregatorMock = new Mock<IMeterAggregator<INoteAcceptorDevice>>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();
            var history = new Mock<ITransactionHistory>();

            history.Setup(m => m.RecallTransactions<BillTransaction>())
                .Returns(new Collection<BillTransaction>());

            var consumer = new CurrencyInCompletedConsumer(
                EgmMock.Object,
                CommandBuilderMock.Object,
                metersAgregatorMock.Object,
                cabinetMeters.Object,
                history.Object,
                EventLiftMock.Object);

            Assert.IsNotNull(consumer);

            AssertConsumeEvent(consumer, new CurrencyInCompletedEvent(100), EventCode.G2S_NAE114);
        }
    }
}