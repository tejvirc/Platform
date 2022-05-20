namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;

    internal class DeviceMultiplexer : IEdgeLightDevice
    {
        private readonly IEdgeLightDeviceFactory _edgeLightDeviceFactory;
        private List<IEdgeLightDevice> _devices = new List<IEdgeLightDevice>();

        public DeviceMultiplexer(IEdgeLightDeviceFactory edgeLightDeviceFactory)
        {
            _edgeLightDeviceFactory = edgeLightDeviceFactory ?? throw new ArgumentNullException(nameof(edgeLightDeviceFactory));
        }

        public IReadOnlyList<IStrip> PhysicalStrips { get; set; } = new List<IStrip>();

        BoardIds IEdgeLightDevice.BoardId => _devices.FirstOrDefault()?.BoardId ?? BoardIds.InvalidBoardId;

        public ICollection<EdgeLightDeviceInfo> DevicesInfo => _devices.SelectMany(x => x.DevicesInfo).ToList();

        public void RenderAllStripData()
        {
            _devices.ForEach(x => x.RenderAllStripData());
        }

        public bool LowPowerMode
        {
            set => SetPowerMode(value);
        }

        public event EventHandler<EventArgs> StripsChanged;

        public bool IsOpen => _devices.Any(x => x.IsOpen);

        public event EventHandler<EventArgs> ConnectionChanged;

        public void Close()
        {
            _devices.ForEach(x => x.Close());
            UnSubscribeEvents();
            _devices.Clear();
        }

        public void Dispose()
        {
            Close();
            _devices.ForEach(x => x.Dispose());
        }

        public bool CheckForConnection()
        {
            if (_edgeLightDeviceFactory.GetDevices().Count() != _devices.Count)
            {
                Open();
            }
            else
            {
                _devices.ForEach(x => x.CheckForConnection());
            }

            return IsOpen;
        }

        public void SetPowerMode(bool lowPowerMode)
        {
            foreach (var device in _devices)
            {
                device.LowPowerMode = lowPowerMode;
            }
        }

        public void SetSystemBrightness(int brightness)
        {
            _devices.ForEach(x => x.SetSystemBrightness(brightness));
        }

        private void Open()
        {
            Close();
            _devices = _edgeLightDeviceFactory.GetDevices().ToList();
            SubscribeEvents();
            _devices.ForEach(x => x.CheckForConnection());
            PhysicalStrips = _devices.SelectMany(x => x.PhysicalStrips).ToList();
        }

        private void EdgeLightDevice_StripsChanged(object sender, EventArgs e)
        {
            PhysicalStrips = _devices.SelectMany(x => x.PhysicalStrips).ToList();
            StripsChanged?.Invoke(this, e);
        }

        private void EdgeLightDevice_ConnectionChanged(object sender, EventArgs e)
        {
            ConnectionChanged?.Invoke(this, e);
        }

        private void SubscribeEvents()
        {
            foreach (var edgeLightDevice in _devices)
            {
                edgeLightDevice.ConnectionChanged += EdgeLightDevice_ConnectionChanged;
                edgeLightDevice.StripsChanged += EdgeLightDevice_StripsChanged;
            }
        }

        private void UnSubscribeEvents()
        {
            foreach (var edgeLightDevice in _devices)
            {
                edgeLightDevice.ConnectionChanged -= EdgeLightDevice_ConnectionChanged;
                edgeLightDevice.StripsChanged -= EdgeLightDevice_StripsChanged;
            }
        }
    }
}