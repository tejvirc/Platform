namespace Aristocrat.Monaco.G2S.Tests.Handlers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Moq;

    internal static class HandlerUtilities
    {
        public static IG2SEgm CreateMockEgm<T>() where T : class, IDevice
        {
            return CreateMockEgm(new Mock<T>());
        }

        public static IG2SEgm CreateMockEgm<T>(Mock<T> device) where T : class, IDevice
        {
            var egm = new Mock<IG2SEgm>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(comms => comms.Queue).Returns(queue.Object);
            egm.Setup(e => e.GetDevice<T>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            return egm.Object;
        }
    }
}