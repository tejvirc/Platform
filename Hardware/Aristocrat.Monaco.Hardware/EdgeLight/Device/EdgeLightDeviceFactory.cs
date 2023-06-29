namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using NativeUsb.Hid;

    public class EdgeLightDeviceFactory : IEdgeLightDeviceFactory
    {
        private readonly IReadOnlyCollection<IEdgeLightDevice> _existingDevices;

        public EdgeLightDeviceFactory(IEnumerable<IEdgeLightDevice> existingDevices)
        {
            _existingDevices = existingDevices?.ToArray() ?? throw new ArgumentNullException(nameof(existingDevices));
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