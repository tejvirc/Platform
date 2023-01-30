namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;

    public class SimEdgeLightDeviceFactory : IEdgeLightDeviceFactory
    {
        private readonly IReadOnlyCollection<IEdgeLightDevice> _existingDevices;

        public SimEdgeLightDeviceFactory()
            : this(
                new[]{
                    ServiceManager.GetInstance().GetService<IBeagleBoneController>() as IEdgeLightDevice,
                    new ReelLightDevice()})
        {
        }

        public SimEdgeLightDeviceFactory(IEdgeLightDevice[] existingDevices)
        {
            _existingDevices = existingDevices ?? throw new ArgumentNullException(nameof(existingDevices));

            var simEdgeLightDevice = new SimEdgeLightDevice();
            SimulatedEdgeLightDevice = simEdgeLightDevice;
        }

        public IEnumerable<IEdgeLightDevice> GetDevices() => _existingDevices.Append(SimulatedEdgeLightDevice);

        public SimEdgeLightDevice SimulatedEdgeLightDevice { get; }

        public string Name => nameof(SimEdgeLightDeviceFactory);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(IEdgeLightDeviceFactory) };

        public void Initialize()
        {
        }
    }
}