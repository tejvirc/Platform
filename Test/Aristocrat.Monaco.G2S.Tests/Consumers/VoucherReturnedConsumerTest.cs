using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using Aristocrat.G2S;
    using G2S.Consumers;
    using Hardware.Contracts.NoteAcceptor;

    /// <summary>(Unit Test Class) a voucher returned consumer test.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.G2S.Tests.Consumers.NoteAcceptorConsumerTestBase"/>
    [TestClass]
    public class VoucherReturnedConsumerTest : NoteAcceptorConsumerTestBase
    {
        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var consumer = new VoucherReturnedConsumer(
                EgmMock.Object,
                CommandBuilderMock.Object,
                EventLiftMock.Object);

            Assert.IsNotNull(consumer);

            AssertConsumeEvent(consumer, new VoucherReturnedEvent(""), EventCode.G2S_NAE109);
        }
    }

}
