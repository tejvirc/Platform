namespace Aristocrat.Monaco.Application.Handlers
{
    using System;
    using Cabinet.Contracts;
    using Hardware.Contracts.Touch;
    using Kernel;

    public class SerialTouchDeviceStatusHandler : DefaultDeviceStatusHandler
    {
        private readonly ISerialTouchService _serialTouchService;

        public SerialTouchDeviceStatusHandler()
            : this(ServiceManager.GetInstance().TryGetService<ISerialTouchService>())
        {
        }

        private SerialTouchDeviceStatusHandler(ISerialTouchService serialTouchService)
        {
            _serialTouchService = serialTouchService ?? throw new ArgumentNullException(nameof(serialTouchService));
        }

        public override void Refresh()
        {
            if (_serialTouchService.IsDisconnected)
            {
                Device.Status = DeviceStatus.Disconnected;
            }
            else if (_serialTouchService.HasReceivedData)
            {
                Device.Status = DeviceStatus.Connected;
            }
            else
            {
                Device.Status = DeviceStatus.Unknown;
            }

            Status = Device.Status;
        }
    }
}
