namespace Aristocrat.Monaco.G2S.Tests.Handlers.Events
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Monaco.Common.Storage;
    using Moq;

    internal static class EventsUtiliites
    {
        public static IG2SEgm CreateMockEgm()
        {
            return CreateMockEgm(new Mock<IEventHandlerDevice>());
        }

        public static IMonacoContextFactory CreateMonacoContextFactory()
        {
            var contextFactory = new Mock<IMonacoContextFactory>();
            var context = new MonacoContext("fake");
            contextFactory.Setup(a => a.CreateDbContext()).Returns(context);

            return contextFactory.Object;
        }

        public static IG2SEgm CreateMockEgm(Mock<IEventHandlerDevice> eventsDevice)
        {
            var egm = new Mock<IG2SEgm>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            eventsDevice.SetupGet(evt => evt.Queue).Returns(queue.Object);
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(eventsDevice.Object);

            return egm.Object;
        }
    }
}