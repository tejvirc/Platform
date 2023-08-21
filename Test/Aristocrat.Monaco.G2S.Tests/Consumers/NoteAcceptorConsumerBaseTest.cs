namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Consumers;
    using G2S.Handlers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public abstract class NoteAcceptorConsumerBaseTest : NoteAcceptorConsumerTestBase
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var consumer = new NoteAcceptorConsumer(
                null,
                CommandBuilderMock.Object,
                EventLiftMock.Object,
                string.Empty);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var consumer = new NoteAcceptorConsumer(
                EgmMock.Object,
                null,
                EventLiftMock.Object,
                string.Empty);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var consumer = new NoteAcceptorConsumer(
                EgmMock.Object,
                CommandBuilderMock.Object,
                null,
                string.Empty);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeWithEgmHasNoNoteAcceptorDeviceExpectNoActions()
        {
            var egmMock = new Mock<IG2SEgm>();

            var consumer = new NoteAcceptorConsumer(
                egmMock.Object,
                CommandBuilderMock.Object,
                EventLiftMock.Object,
                string.Empty);

            consumer.Consume(new Event());
        }

        private class NoteAcceptorConsumer : NoteAcceptorConsumerBase<BaseEvent>
        {
            public NoteAcceptorConsumer(
                IG2SEgm egm,
                ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder,
                IEventLift eventLift,
                string eventCode)
                : base(egm, commandBuilder, eventLift, eventCode)
            {
            }
        }

        private class Event : BaseEvent
        {
        }
    }
}