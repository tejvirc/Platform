namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    public class SimEdgeLightDeviceFactory : IEdgeLightDeviceFactory
    {
        private readonly IReadOnlyCollection<IEdgeLightDevice> _existingDevices;

        public SimEdgeLightDeviceFactory(IEnumerable<IEdgeLightDevice> existingDevices)
        {
            _existingDevices = existingDevices?.ToArray() ?? throw new ArgumentNullException(nameof(existingDevices));

            var simEdgeLightDevice = new SimEdgeLightDevice();
            SimulatedEdgeLightDevice = simEdgeLightDevice;
        }

        public SimEdgeLightDevice SimulatedEdgeLightDevice { get; }

        public IEnumerable<IEdgeLightDevice> GetDevices()
        {
            return _existingDevices.Append(SimulatedEdgeLightDevice);
        }

        public string Name => nameof(SimEdgeLightDeviceFactory);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(IEdgeLightDeviceFactory) };

        public void Initialize()
        {
        }
    }
}