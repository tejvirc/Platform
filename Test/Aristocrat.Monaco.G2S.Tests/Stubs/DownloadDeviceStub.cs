namespace Aristocrat.Monaco.G2S.Tests.Stubs
{
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class DownloadDeviceStub : ClientDeviceBase<download>
    {
        public DownloadDeviceStub()
            : base(1, null)
        {
            Configurator = 3;
        }

        public override void Open(IStartupContext context)
        {
        }

        public override void Close()
        {
        }

        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
        }
    }
}