namespace Aristocrat.Monaco.Hardware.EdgeLight.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using log4net;
    using Manager;
    using SequenceLib;
    using Vgt.Client12.Hardware.HidLibrary;

    internal class EdgeLightingControllerService : BaseRunnable, IEdgeLightingController, IService
    {
        private const int TickInterval = 33;

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Func<IEnumerable<IHidDevice>> _deviceEnumerator;
        private readonly IEventBus _eventBus;
        private readonly Func<PatternParameters, IEdgeLightRenderer> _rendererFactory;

        private readonly List<IEdgeLightRenderer> _rendererList = new List<IEdgeLightRenderer>();
        private readonly object _rendererListLock = new object();
        private readonly IEdgeLightManager _edgeLightManager;

        private readonly bool _isSimulated;

        public EdgeLightingControllerService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                new EdgeLightManager(),
                RendererFactory.CreateRenderer,
                () => HidDevices.Enumerate(EdgeLightConstants.VendorId, EdgeLightConstants.ProductId),
                new List<IEdgeLightRenderer> { new EdgeLightDataRenderer() })
        {
        }

        public EdgeLightingControllerService(
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            IEdgeLightManager edgeLightManager,
            Func<PatternParameters, IEdgeLightRenderer> rendererFactory,
            Func<IEnumerable<IHidDevice>> deviceEnumerator,
            IEnumerable<IEdgeLightRenderer> initialRendererList)
        {
            _edgeLightManager = edgeLightManager ?? throw new ArgumentNullException(nameof(edgeLightManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _rendererFactory = rendererFactory ?? throw new ArgumentNullException(nameof(rendererFactory));
            _deviceEnumerator = deviceEnumerator ?? throw new ArgumentNullException(nameof(deviceEnumerator));

            _isSimulated = propertiesManager.GetValue(HardwareConstants.SimulateEdgeLighting, false);

            DetectedOnStartup = IsDetected;
            foreach (var edgeLightRenderer in initialRendererList)
            {
                AddEdgeLightRenderer(edgeLightRenderer);
            }
        }

        public bool IsDetected => _isSimulated ? StripIds.Any() : _deviceEnumerator.Invoke().Any();

        public bool DetectedOnStartup { get; }

        public IEnumerable<EdgeLightDeviceInfo> Devices => _edgeLightManager.DevicesInfo;

        public void SetBrightnessLimits(EdgeLightingBrightnessLimits limits, StripPriority forPriority)
        {
            _edgeLightManager.SetBrightnessLimits(limits, forPriority);
        }

        public EdgeLightingBrightnessLimits GetBrightnessLimits(StripPriority forPriority)
        {
            return _edgeLightManager.GetBrightnessLimits(forPriority);
        }

        public IList<int> StripIds => _edgeLightManager.LogicalStrips.Select(x => x.StripId).ToList();

        public int GetStripLedCount(int stripId)
        {
            return _edgeLightManager.LogicalStrips.FirstOrDefault(x => x.StripId == stripId)?.LedCount ?? 0;
        }

        public void SetBrightnessForPriority(int brightness, StripPriority priority)
        {
            _edgeLightManager.SetBrightnessForPriority(brightness, priority);
        }

        public void ClearBrightnessForPriority(StripPriority priority)
        {
            _edgeLightManager.ClearBrightnessForPriority(priority);
        }

        public void SetStripBrightnessForPriority(int stripId, int brightness, StripPriority priority)
        {
            _edgeLightManager.SetStripBrightnessForPriority(stripId, brightness, priority);
        }

        public void ClearStripBrightnessForPriority(int stripId, StripPriority priority)
        {
            _edgeLightManager.ClearStripBrightnessForPriority(stripId, priority);
        }

        public IEdgeLightToken AddEdgeLightRenderer(PatternParameters forParameters)
        {
            var renderer = _rendererFactory.Invoke(forParameters) ??
                           throw new ArgumentException(nameof(forParameters));
            return AddEdgeLightRenderer(renderer);
        }

        public void RemoveEdgeLightRenderer(IEdgeLightToken token)
        {
            lock (_rendererListLock)
            {
                if (token is IEdgeLightRenderer renderer && _rendererList.Remove(renderer))
                {
                    renderer.Clear();
                }
            }
        }

        public void SetPriorityComparer(IComparer<StripPriority> comparer)
        {
            _edgeLightManager.SetPriorityComparer(comparer);
        }

        public string Name => nameof(EdgeLightingControllerService);

        public ICollection<Type> ServiceTypes { get; } = new[] { typeof(IEdgeLightingController) };

        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            base.Dispose(disposing);
            if (!disposing)
            {
                return;
            }

            _edgeLightManager.Dispose();
        }

        protected override void OnInitialize()
        {
            Logger.Debug(Name + " OnInitialize()");

            _eventBus.Subscribe<EdgeLightingStripsChangedEvent>(this, _ => { SetupRendererList(); });
            SetupRendererList();
        }

        protected override void OnRun()
        {
            Logger.Info(Name + " started");

            while (RunState == RunnableState.Running)
            {
                Update();
                _edgeLightManager.RenderAllStripData();

                Thread.Sleep(TickInterval);
            }
        }

        protected override void OnStop()
        {
            Logger.Info(Name + " stopping");
            Clear();
        }

        private void Update()
        {
            lock (_rendererListLock)
            {
                _rendererList.ForEach(x => x.Update());
            }
        }

        private void Clear()
        {
            _eventBus.UnsubscribeAll(this);
            lock (_rendererListLock)
            {
                _rendererList.ForEach(x => x.Clear());
            }
        }

        private void SetupRendererList()
        {
            lock (_rendererListLock)
            {
                _rendererList.ForEach(x => x.Setup(_edgeLightManager));
            }
        }

        private IEdgeLightToken AddEdgeLightRenderer(IEdgeLightRenderer renderer)
        {
            lock (_rendererListLock)
            {
                _rendererList.Add(renderer);
                if (_edgeLightManager != null)
                {
                    renderer.Setup(_edgeLightManager);
                }
            }

            return renderer;
        }
    }
}