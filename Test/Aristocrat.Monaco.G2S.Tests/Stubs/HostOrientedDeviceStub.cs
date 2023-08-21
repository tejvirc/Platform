namespace Aristocrat.Monaco.G2S.Tests.Stubs
{
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class HostOrientedDeviceStub : HostOrientedDevice<communications>
    {
        private bool _isClosed;

        public HostOrientedDeviceStub()
            : base(1, null)
        {
            _isClosed = false;
        }

        public HostOrientedDeviceStub(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver)
        {
        }

        public bool IsClosed
        {
            get { return _isClosed; }
        }

        public new virtual int Id
        {
            get { return 1; }
        }

        public override void Open(IStartupContext context)
        {
        }

        public override void Close()
        {
            _isClosed = true;
        }

        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
        }
    }
}