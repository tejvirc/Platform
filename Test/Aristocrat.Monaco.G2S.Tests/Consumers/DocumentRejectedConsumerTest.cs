namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using Aristocrat.G2S;
    using G2S.Consumers;
    using Hardware.Contracts.NoteAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DocumentRejectedConsumerTest : NoteAcceptorConsumerTestBase
    {
        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var consumer = new DocumentRejectedConsumer(
                EgmMock.Object,
                CommandBuilderMock.Object,
                EventLiftMock.Object);

            Assert.IsNotNull(consumer);

            AssertConsumeEvent(consumer, EventCode.G2S_NAE110, () => new DocumentRejectedEvent());
        }
    }
}