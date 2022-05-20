namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using Aristocrat.G2S;
    using G2S.Consumers;
    using Hardware.Contracts.NoteAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NoteAcceptorConnectedConsumerTest : NoteAcceptorConsumerTestBase
    {
        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var consumer = new NoteAcceptorConnectedConsumer(
                EgmMock.Object,
                CommandBuilderMock.Object,
                EventLiftMock.Object);

            Assert.IsNotNull(consumer);

            AssertConsumeEvent(consumer, EventCode.G2S_NAE902, () => new ConnectedEvent());
        }
    }
}