namespace Aristocrat.Monaco.Hardware.EdgeLight.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Common;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using log4net;
    using Serial.EdgeLighting.BeagleBone;
    using Strips;

    internal sealed class BeagleBoneControllerService : BaseRunnable, IBeagleBoneController, IEdgeLightDevice
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly IDictionary<string, LightShows> LightShowsMap = Enum.GetValues(typeof(LightShows))
            .Cast<LightShows>().Select(x => (Attribute: x.GetAttribute<ShowNameAttribute>(), LightShow: x))
            .Where(x => !string.IsNullOrEmpty(x.Attribute?.ShowName))
            .ToDictionary(x => x.Attribute.ShowName, x => x.LightShow);

        private readonly ICabinetDetectionService _cabinetService;
        private BeagleBoneProtocol _beagleBoneProtocol;
        private bool _disposed;

        /// <summary>
        ///     Creates an instance of <see cref="BeagleBoneControllerService"/>
        /// </summary>
        public BeagleBoneControllerService()
            : this(ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        /// <summary>
        ///     Creates an instance of <see cref="BeagleBoneControllerService"/>
        /// </summary>
        /// <param name="cabinetService">An instance of <see cref="ICabinetDetectionService"/></param>
        public BeagleBoneControllerService(ICabinetDetectionService cabinetService)
        {
            _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
        }

        public bool Initialized { get; private set; }

        public string LastError => string.Empty;

        public string Name => nameof(BeagleBoneControllerService);

        public ICollection<Type> ServiceTypes { get; } = new[] { typeof(IBeagleBoneController) };

        public IReadOnlyList<IStrip> PhysicalStrips { get; private set; } = new List<IStrip>();

        BoardIds IEdgeLightDevice.BoardId => BoardIds.InvalidBoardId;

        public ICollection<EdgeLightDeviceInfo> DevicesInfo { get; private set; } = new List<EdgeLightDeviceInfo>();

        event EventHandler<EventArgs> IEdgeLightDevice.ConnectionChanged
        {
            add => ConnectionChanged += value;
            remove => ConnectionChanged -= value;
        }

        event EventHandler<EventArgs> IEdgeLightDevice.StripsChanged
        {
            add => StripsChanged += value;
            remove => StripsChanged -= value;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global  Required by the interface
        public bool LowPowerMode { get; set; }

        public bool IsOpen => Initialized && PhysicalStrips.Any();

        public void SendShow(LightShows lightShow)
        {
            _beagleBoneProtocol?.SendShow(lightShow);
        }

        public void RenderAllStripData()
        {
            if (!IsOpen)
            {
                return;
            }

            var showName = Encoding.ASCII.GetString(
                PhysicalStrips.FirstOrDefault()?.ColorBuffer.ArgbBytes ?? Array.Empty<byte>());
            if (LightShowsMap.TryGetValue(showName, out var show))
            {
                _beagleBoneProtocol?.SendShow(show);
            }
        }

        public void Close()
        {
            PhysicalStrips = new List<IStrip>();
        }

        public bool CheckForConnection()
        {
            if (!Initialized)
            {
                return false;
            }

            if (!IsOpen)
            {
                OpenDevice();
            }

            return IsOpen;
        }

        public void SetSystemBrightness(int brightness)
        {
        }

        protected override void OnInitialize()
        {
            if (Initialized)
            {
                return;
            }

            if (_cabinetService != null && _cabinetService.IsCabinetType(HardwareConstants.CabinetTypeRegexLs))
            {
                _beagleBoneProtocol = new BeagleBoneProtocol();
                _beagleBoneProtocol.Enable(true);
                Initialized = true;
            }

            Logger.Info($"{Name} initialized {Initialized}");
        }

        protected override void OnRun()
        {
            Logger.Info(Name + " started");
        }

        protected override void OnStop()
        {
            Logger.Info(Name + " stopped");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Close();

                if (_beagleBoneProtocol != null)
                {
                    _beagleBoneProtocol.Dispose();
                    _beagleBoneProtocol = null;
                }

                Initialized = false;
            }

            _disposed = true;
        }

        private event EventHandler<EventArgs> StripsChanged;

        private event EventHandler<EventArgs> ConnectionChanged;

        private void OpenDevice()
        {
            if (!Initialized || IsOpen)
            {
                return;
            }

            DevicesInfo = new List<EdgeLightDeviceInfo>
            {
                new EdgeLightDeviceInfo
                {
                    DeviceType = ElDeviceType.Cabinet,
                    Manufacturer = "Aristocrat",
                    Product = nameof(BeagleBoneProtocol),
                    Version = 1,
                    SerialNumber = string.Empty
                }
            };

            PhysicalStrips = new List<IStrip> { new PhysicalStrip((int)StripIDs.MainCabinetLeft, 1) };
            ConnectionChanged?.Invoke(this, EventArgs.Empty);
            StripsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}