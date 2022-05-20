namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;
    using Contracts.Cabinet;
    using Contracts.Display;
    using Kernel;

    public class DisplayService : IService, IDisplayService
    {
        private readonly ICabinetDetectionService _cabinetDetection;

        private readonly IReadOnlyDictionary<string, int> _frameRates = new Dictionary<string, int>
        {
            { "GT630", 30 }
        };

        public DisplayService()
            : this(ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        public DisplayService(ICabinetDetectionService cabinetDetection)
        {
            _cabinetDetection = cabinetDetection ?? throw new ArgumentNullException(nameof(cabinetDetection));
        }

        public bool IsFaulted => ConnectedCount < ExpectedCount;

        public int ConnectedCount => _cabinetDetection.NumberOfDisplaysConnected;

        public int ExpectedCount { get; private set; }

        public string GraphicsCard => GetGraphicsCard();

        public int MaximumFrameRate
        {
            get
            {
                var card = GraphicsCard;
                return _frameRates.Where(x => card.Replace(" ", string.Empty).Contains(x.Key))
                    .Select(x => (int?)x.Value)
                    .FirstOrDefault() ?? -1;
            }
        }

        public string Name => nameof(IDisplayService);

        public ICollection<Type> ServiceTypes => new[] { typeof(IDisplayService) };

        public void Initialize()
        {
            ExpectedCount = _cabinetDetection.NumberOfDisplaysConnectedDuringInitialization;
        }

        private static string GetGraphicsCard()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (var result in searcher.Get())
                {
                    var mo = (ManagementObject)result;

                    var currentBitsPerPixel = mo.Properties["CurrentBitsPerPixel"];
                    var description = mo.Properties["Description"];

                    if (currentBitsPerPixel.Value != null)
                    {
                        return description.Value as string;
                    }
                }

                return string.Empty;
            }
        }
    }
}