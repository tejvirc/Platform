namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Consumers;
    using G2S.Handlers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    public class NoteAcceptorConsumerTestBase
    {
        protected Mock<ICabinetDevice> CabinetDeviceMock;

        protected Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>> CommandBuilderMock;
        protected Mock<IG2SEgm> EgmMock;

        protected Mock<IEventLift> EventLiftMock;

        protected Mock<INoteAcceptorDevice> NoteAcceptorDeviceMock;

        [TestInitialize]
        public void Initialize()
        {
            EgmMock = new Mock<IG2SEgm>();
            CommandBuilderMock = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            EventLiftMock = new Mock<IEventLift>();

            CabinetDeviceMock = new Mock<ICabinetDevice>();
            NoteAcceptorDeviceMock = new Mock<INoteAcceptorDevice>();

            CabinetDeviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
            NoteAcceptorDeviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_noteAcceptor);

            EgmMock.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(CabinetDeviceMock.Object);
            EgmMock.Setup(m => m.GetDevice<INoteAcceptorDevice>()).Returns(NoteAcceptorDeviceMock.Object);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
        }

        protected void AssertConsumeEvent<TEvent>(NoteAcceptorConsumerBase<TEvent> consumer, string eventCode, Func<TEvent> create)
            where TEvent : BaseEvent, new()
        {
            AssertConsumeEvent(consumer, create(), eventCode);
        }

        protected void AssertConsumeEvent<TEvent>(
            NoteAcceptorConsumerBase<TEvent> consumer,
            TEvent theEvent,
            string eventCode)
            where TEvent : BaseEvent
        {
            consumer.Consume(theEvent);

            CommandBuilderMock.Verify(m => m.Build(NoteAcceptorDeviceMock.Object, It.IsAny<noteAcceptorStatus>()));
            EventLiftMock
                .Verify(
                    m => m.Report(
                        NoteAcceptorDeviceMock.Object,
                        eventCode,
                        It.IsAny<deviceList1>(),
                        It.IsAny<long>(),
                        It.IsAny<transactionList>(),
                        It.IsAny<meterList>()));
        }
    }
}