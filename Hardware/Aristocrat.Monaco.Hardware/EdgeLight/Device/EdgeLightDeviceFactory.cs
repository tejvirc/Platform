namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using NativeUsb.Hid;

    public class EdgeLightDeviceFactory : IEdgeLightDeviceFactory
    {
        private readonly IReadOnlyCollection<IEdgeLightDevice> _existingDevices;

        public EdgeLightDeviceFactory()
            : this(
                ServiceManager.GetInstance().GetService<IBeagleBoneController>() as IEdgeLightDevice,
                new ReelLightDevice())
        {
        }

        public EdgeLightDeviceFactory(params IEdgeLightDevice[] existingDevices)
        {
            _existingDevices = existingDevices ?? throw new ArgumentNullException(nameof(existingDevices));
        }

        public IEnumerable<IEdgeLightDevice> GetDevices()
        {
            return HidDeviceFactory.Enumerate(EdgeLightConstants.VendorId, EdgeLightConstants.ProductId)
                .Select(x => new EdgeLightDevice(x) as IEdgeLightDevice).Union(_existingDevices);
        }

        public string Name => nameof(EdgeLightDeviceFactory);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(IEdgeLightDeviceFactory) };

        public void Initialize()
        {
        }
    }
}