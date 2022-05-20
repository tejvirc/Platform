namespace Aristocrat.Monaco.G2S.Tests.Handlers.Downloads
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Moq;

    internal static class DownloadsUtilities
    {
        public static IG2SEgm CreateMockEgm()
        {
            return CreateMockEgm(new Mock<IDownloadDevice>());
        }

        public static IG2SEgm CreateMockEgm(Mock<IDownloadDevice> downloadsDevice)
        {
            var egm = new Mock<IG2SEgm>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            downloadsDevice.SetupGet(evt => evt.Queue).Returns(queue.Object);
            downloadsDevice.SetupGet(m => m.DownloadEnabled).Returns(true);
            egm.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(downloadsDevice.Object);

            return egm.Object;
        }
    }
}