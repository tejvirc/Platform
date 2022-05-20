using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using Aristocrat.G2S;
    using G2S.Consumers;
    using Hardware.Contracts.NoteAcceptor;

    [TestClass]
    public class CurrencyEscrowedConsumerTest : NoteAcceptorConsumerBaseTest
    {
        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var consumer = new CurrencyEscrowedConsumer(
                EgmMock.Object,
                CommandBuilderMock.Object,
                EventLiftMock.Object);

            Assert.IsNotNull(consumer);

            AssertConsumeEvent(consumer,
                new CurrencyEscrowedEvent(new Note { ISOCurrencySymbol = "USD", Value = 0}),
                EventCode.G2S_NAE116);
        }
    }
}
