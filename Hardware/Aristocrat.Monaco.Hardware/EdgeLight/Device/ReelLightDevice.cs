namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.Reel;
    using Kernel;
    using log4net;
    using Strips;

    public sealed class ReelLightDevice : IEdgeLightDevice
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly object _lock = new object();
        private bool _disposed;
        private int _lightsPerReel;
        private int _lastReelCount;
        private int _lastLightIdentifiersCount;
        private List<LightData> LastLightData { get; set; } = new List<LightData>();
        private int _lastReelBrightness = -1;
        private IReelController _reelController;

        internal class LightData
        {
            public LightData()
            {
                IsLampOn = false;
                ArgbColor = 0;
                Brightness = -1;
            }

            public bool IsLampOn { get; set; }
            public int ArgbColor { get; set; }

            public int Brightness { get; set; }
        }

        public IReadOnlyList<IStrip> PhysicalStrips { get; private set; } = new List<IStrip>();

        public BoardIds BoardId => BoardIds.InvalidBoardId;

        public ICollection<EdgeLightDeviceInfo> DevicesInfo => new List<EdgeLightDeviceInfo>();

        // ReSharper disable once UnusedAutoPropertyAccessor.Global  Required by the interface
        public bool LowPowerMode { get; set; }

        public bool IsOpen => Controller?.Connected ?? false;

        public event EventHandler<EventArgs> StripsChanged;

        public event EventHandler<EventArgs> ConnectionChanged;

        private IReelController Controller =>
            _reelController ?? (_reelController = ServiceManager.GetInstance().TryGetService<IReelController>());

        public ReelLightDevice()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public ReelLightDevice(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<ReelConnectedEvent>(this, HandledStripChanged);
            _eventBus.Subscribe<ReelDisconnectedEvent>(this, HandledStripChanged);
            _eventBus.Subscribe<ConnectedEvent>(this, HandleConnectionChanged);
            _eventBus.Subscribe<DisconnectedEvent>(this, HandleConnectionChanged);
        }

        private async Task HandleConnectionChanged(ReelControllerBaseEvent evt, CancellationToken token)
        {
            LastLightData = new List<LightData>();
            _lastReelCount = 0;
            _lastLightIdentifiersCount = 0;
            ConnectionChanged?.Invoke(this, EventArgs.Empty);
            await HandledStripChanged(evt, token);
        }

        private async Task HandledStripChanged(ReelControllerBaseEvent evt, CancellationToken token)
        {
            var reelController = Controller;
            if (reelController is null)
            {
                lock (_lock)
                {
                    _lightsPerReel = 0;
                    PhysicalStrips = new List<IStrip>();
                    LastLightData = new List<LightData>();
                    _lastReelCount = 0;
                    _lastLightIdentifiersCount = 0;
                }
            }
            else
            {
                IList<int> lightIdentifiers = null;
                try
                {
                    lightIdentifiers = await reelController.GetReelLightIdentifiers();
                }
                catch (Exception ex)
                {
                    Logger.Error($"GetReelLightIdentifiers failed : {ex.Message}");
                }

                if (lightIdentifiers != null)
                {
                    var reels = reelController.ConnectedReels;
                    if (reels.Count > 0 && lightIdentifiers.Count > 0)
                    {
                        lock (_lock)
                        {
                            _lightsPerReel = lightIdentifiers.Count / reels.Count;

                            PhysicalStrips = reels.Select(
                                x => new PhysicalStrip((int)StripIDs.StepperReel1 + x - 1, _lightsPerReel)).ToList();

                            if (_lastReelCount != reels.Count && _lastLightIdentifiersCount != lightIdentifiers.Count)
                            {
                                _lastReelCount = reels.Count;
                                _lastLightIdentifiersCount = lightIdentifiers.Count;
                                LastLightData = new List<LightData>();
                            }
                        }
                    }
                    else
                    {
                        Logger.Debug($"ReelLightDevice invalid reel light data, reels.Count={reels.Count}, lightIdentifiers.Count={lightIdentifiers.Count}");
                    }
                }
            }

            StripsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }

        public void RenderAllStripData()
        {
            lock (_lock)
            {
                if (PhysicalStrips.Count == 0 || !IsOpen || !Controller.Enabled || !Controller.Initialized)
                {
                    return;
                }

                // Convert the strip data to lamp data
                var reelLampData = new List<ReelLampData>();
                var reelLampBrightness = new Dictionary<int, int>();
                var lampStateMatch = true;
                var colorsMatch = true;
                var brightnessMatch = true;
                for (var stripIndex = 0; stripIndex < PhysicalStrips.Count; ++stripIndex)
                {
                    var strip = PhysicalStrips[stripIndex];
                    for (var ledIndex = 0; ledIndex < _lightsPerReel; ++ledIndex)
                    {
                        var ledColorBuffer = strip.ColorBuffer[ledIndex];
                        var isLampOn = ledColorBuffer.R != 0 || ledColorBuffer.G != 0 || ledColorBuffer.B != 0;
                        var lightId = (stripIndex * _lightsPerReel) + ledIndex + 1;
                        reelLampBrightness.Add(lightId, strip.Brightness);
                        var lastDataIndex = lightId - 1;

                        if (LastLightData.Count <= lastDataIndex)
                        {
                            LastLightData.Add(new LightData { IsLampOn = isLampOn, ArgbColor = ledColorBuffer.ToArgb(), Brightness = strip.Brightness });
                            lampStateMatch = false;
                            colorsMatch = false;
                            brightnessMatch = false;
                        }
                        else
                        {
                            if (isLampOn != LastLightData[lastDataIndex].IsLampOn)
                            {
                                lampStateMatch = false;
                                LastLightData[lastDataIndex].IsLampOn = isLampOn;
                            }

                            var isBlackColor = ledColorBuffer.R == 0 && ledColorBuffer.G == 0 && ledColorBuffer.B == 0;
                            if (!isBlackColor && ledColorBuffer.ToArgb() != LastLightData[lastDataIndex].ArgbColor)
                            {
                                colorsMatch = false;
                                LastLightData[lastDataIndex].ArgbColor = ledColorBuffer.ToArgb();
                            }

                            if (strip.Brightness != LastLightData[lastDataIndex].Brightness)
                            {
                                brightnessMatch = false;
                                LastLightData[lastDataIndex].Brightness = strip.Brightness;
                            }
                        }

                        reelLampData.Add(new ReelLampData(
                            Color.FromArgb(LastLightData[lastDataIndex].ArgbColor),
                            LastLightData[lastDataIndex].IsLampOn,
                            lightId));
                    }
                }

                if (!lampStateMatch || !colorsMatch)
                {
                    Controller.SetLights(reelLampData.ToArray()).WaitForCompletion();
                }

                if (!brightnessMatch)
                {
                    Controller.SetReelBrightness(reelLampBrightness).WaitForCompletion();
                }
            }
        }

        public void Close()
        {
        }

        public bool CheckForConnection() => IsOpen;

        public void SetSystemBrightness(int brightness)
        {
            lock (_lock)
            {
                if (!IsOpen || !Controller.Enabled || !Controller.Initialized || _lastReelBrightness == brightness)
                {
                    return;
                }

                Controller.SetReelBrightness(brightness).WaitForCompletion();
                _lastReelBrightness = brightness;
            }
        }
    }
}