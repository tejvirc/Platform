namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GpuInfo
    {
        public string BiosVersion { get; set; }

        public string ModelName { get; set; }

        public string GpuName { get; set; }

        public string SerialNumber { get; set; }

        public string ImageVersion { get; set; }

        public uint TotalRam { get; set; }
    }
}
