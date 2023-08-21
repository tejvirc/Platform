namespace Aristocrat.Monaco.Application.Handlers
{
    using System;
    using Cabinet.Contracts;
    using Contracts.Handlers;

    public class DefaultDeviceStatusHandler : IDeviceStatusHandler
    {
        private DeviceStatus _status = DeviceStatus.Unknown;

        public DeviceStatus Status
        {
            get => _status;
            set
            {
                if (value == _status)
                {
                    return;
                }

                _status = value;
                OnStatusChanged();
            }
        }

        private void OnStatusChanged()
        {
            switch (Status)
            {
                case DeviceStatus.Connected:
                    ConnectAction?.Invoke(this);
                    break;
                case DeviceStatus.Disconnected:
                    DisconnectAction?.Invoke(this);
                    break;
                case DeviceStatus.Unknown:
                    break;
                case DeviceStatus.Unexpected:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Status));
            }
        }

        public string Meter { get; set; }

        public IDevice Device { get; set; }

        public Action<IDeviceStatusHandler> ConnectAction { get; set; }

        public Action<IDeviceStatusHandler> DisconnectAction { get; set; }

        public virtual void Refresh()
        {
            Status = Device.Status;
        }
    }
}
