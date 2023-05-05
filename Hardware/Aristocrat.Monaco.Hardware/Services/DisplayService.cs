namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Cabinet;
    using Contracts.Display;
    using Kernel;

    public class DisplayService : IService, IDisplayService
    {
        private readonly ICabinetDetectionService _cabinetDetection;
        private readonly IGpuDetailService _gpuDetailService;

        private readonly string _graphicCardName;
        private readonly IReadOnlyDictionary<string, int> _frameRates = new Dictionary<string, int>
        {
            { "GT630", 30 }
        };

        public DisplayService()
            : this(ServiceManager.GetInstance().GetService<ICabinetDetectionService>(), ServiceManager.GetInstance().GetService<IGpuDetailService>())
        {
        }

        public DisplayService(ICabinetDetectionService cabinetDetection,
            IGpuDetailService gpuDetailService)
        {
            _cabinetDetection = cabinetDetection ?? throw new ArgumentNullException(nameof(cabinetDetection));
            _gpuDetailService = gpuDetailService ?? throw new ArgumentNullException(nameof(gpuDetailService));
            _graphicCardName = _gpuDetailService.ActiveGpuName;
        }

        public bool IsFaulted => ConnectedCount < ExpectedCount;

        public int ConnectedCount => _cabinetDetection.NumberOfDisplaysConnected;

        public int ExpectedCount { get; private set; }


        public int MaximumFrameRate
        {
            get
            {
                var card = _graphicCardName;
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
    }
}