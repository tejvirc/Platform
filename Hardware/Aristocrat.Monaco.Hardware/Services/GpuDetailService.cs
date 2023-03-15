namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;
    using Contracts.Cabinet;
    using Contracts.Display;
    using Kernel;

    public class GpuDetailService : IService, IGpuDetailService
    {

        public GpuDetailService()
        {
        }

        // public GpuDetailService(ICabinetDetectionService cabinetDetection)
        // {
        //     // _cabinetDetection = cabinetDetection ?? throw new ArgumentNullException(nameof(cabinetDetection));
        // }

        public string Name => nameof(IGpuDetailService);

        public ICollection<Type> ServiceTypes => new[] { typeof(IGpuDetailService) };

        public void Initialize()
        {
       
        }

    }
}