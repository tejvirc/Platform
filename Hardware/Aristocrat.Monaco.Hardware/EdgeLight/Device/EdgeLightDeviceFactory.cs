namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Vgt.Client12.Hardware.HidLibrary;

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
            NativeMethods.HidD_GetHidGuid(out _); // This is needed to initialize hid library.
        }

        public IEnumerable<IEdgeLightDevice> GetDevices()
        {
            return HidDevices.Enumerate(EdgeLightConstants.VendorId, EdgeLightConstants.ProductId)
                .Select(x => new EdgeLightDevice(x) as IEdgeLightDevice).Union(_existingDevices);
        }

        public string Name => nameof(EdgeLightDeviceFactory);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(IEdgeLightDeviceFactory) };

        public void Initialize()
        {
        }

        private static class NativeMethods
        {
            [DllImport("hid.dll", SetLastError = true)]
            public static extern void HidD_GetHidGuid(out Guid hidGuid);
        }
    }
}