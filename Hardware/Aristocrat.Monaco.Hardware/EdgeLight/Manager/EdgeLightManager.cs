namespace Aristocrat.Monaco.Hardware.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using log4net;
    using Strips;

    /// <summary>
    ///     Manages the edge light system and associated functions. Implements  An <see cref="IEdgeLightManager" />
    ///     implementation
    /// </summary>
    internal sealed class EdgeLightManager : IEdgeLightManager
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EdgeLightBrightnessData _brightnessData;

        private readonly IEdgeLightDevice _edgeLightDevice;

        private readonly object _lock = new();

        private readonly ILogicalStripFactory _logicalStripFactory;

        private readonly List<int> _platformControlledColorStrips = new()
        {
            (int)StripIDs.LandingStripLeft,
            (int)StripIDs.LandingStripRight,
            (int)StripIDs.BarkeeperStrip1Led,
            (int)StripIDs.BarkeeperStrip4Led
        };

        private readonly PriorityComparer _priorityComparer;
        private readonly IEventBus _eventBus;

        private readonly StripDataRenderer _renderer;

        private bool _connectionStatus;
        private bool _disposed;
        private IDictionary<int, IStrip> _logicalStrips = new Dictionary<int, IStrip>();

        public EdgeLightManager(
            ILogicalStripFactory logicalStripFactory,
            IEdgeLightDevice edgeLightDevice,
            StripDataRenderer renderer,
            PriorityComparer priorityComparer,
            IEventBus eventBus)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _logicalStripFactory = logicalStripFactory ?? throw new ArgumentNullException(nameof(logicalStripFactory));
            _edgeLightDevice = edgeLightDevice ?? throw new ArgumentNullException(nameof(edgeLightDevice));
            _priorityComparer = priorityComparer ?? throw new ArgumentNullException(nameof(priorityComparer));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            SubscribeDeviceEvent<DeviceDisconnectedEvent>();
            SubscribeDeviceEvent<DeviceConnectedEvent>();

            _brightnessData = new EdgeLightBrightnessData(priorityComparer);
            SetBrightnessForPriority(EdgeLightConstants.MaxChannelBrightness, StripPriority.LowPriority);
            _edgeLightDevice.StripsChanged += DeviceStripsChanged;
            CheckForConnection();
            Logger.Debug("EdgeLight Manager started");
        }

        public void SetPriorityComparer(IComparer<StripPriority> comparer)
        {
            lock (_lock)
            {
                _priorityComparer.OverridenComparer = comparer;
                SetHighestPriorityBrightness(_logicalStrips.Values);
            }
        }

        public IReadOnlyCollection<StripData> LogicalStrips
        {
            get
            {
                lock (_lock)
                {
                    return _logicalStrips.Values
                        .Select(x => new StripData { StripId = x.StripId, LedCount = x.LedCount })
                        .ToList().AsReadOnly();
                }
            }
        }

        public IReadOnlyCollection<StripData> ExternalLogicalStrips
        {
            get
            {
                lock (_lock)
                {
                    return _logicalStrips.Values
                        .Where(x => !_platformControlledColorStrips.Contains(x.StripId))
                        .Select(x => new StripData { StripId = x.StripId, LedCount = x.LedCount })
                        .ToList().AsReadOnly();
                }
            }
        }

        public IEnumerable<EdgeLightDeviceInfo> DevicesInfo => _edgeLightDevice.DevicesInfo;

        public bool PowerMode
        {
            set => _edgeLightDevice.LowPowerMode = value;
        }

        public void SetBrightnessForPriority(int brightness, StripPriority priority)
        {
            lock (_lock)
            {
                _brightnessData.SetBrightnessForPriority(brightness, priority);
                SetHighestPriorityBrightness(_logicalStrips.Values);
            }
        }

        public void ClearBrightnessForPriority(StripPriority priority)
        {
            lock (_lock)
            {
                _brightnessData.ClearBrightnessForPriority(priority);
                SetHighestPriorityBrightness(_logicalStrips.Values);
            }
        }

        public void SetStripBrightnessForPriority(int stripId, int brightness, StripPriority priority)
        {
            lock (_lock)
            {
                if (!_logicalStrips.TryGetValue(stripId, out var strips))
                {
                    return;
                }

                _brightnessData.SetStripBrightnessForPriority(stripId, brightness, priority);
                SetHighestPriorityBrightness(strips);
            }
        }

        public void ClearStripBrightnessForPriority(int stripId, StripPriority priority)
        {
            lock (_lock)
            {
                if (!_logicalStrips.TryGetValue(stripId, out var strip))
                {
                    return;
                }

                _brightnessData.ClearStripBrightnessForPriority(stripId, priority);
                SetHighestPriorityBrightness(strip);
            }
        }

        public void SetBrightnessLimits(EdgeLightingBrightnessLimits limits, StripPriority forPriority)
        {
            lock (_lock)
            {
                _brightnessData.SetBrightnessLimits(limits, forPriority);
                SetHighestPriorityBrightness(_logicalStrips.Values);
            }
        }

        public EdgeLightingBrightnessLimits GetBrightnessLimits(StripPriority forPriority)
        {
            lock (_lock)
            {
                return _brightnessData.GetBrightnessLimits(forPriority);
            }
        }

        public bool SetStripColor(int stripId, Color color, StripPriority priority)
        {
            lock (_lock)
            {
                if (!_logicalStrips.TryGetValue(stripId, out var logicalStrip))
                {
                    return false;
                }

                SetStripColor(logicalStrip, color, priority);
                return true;
            }
        }

        public bool ClearStripForPriority(int stripId, StripPriority priority)
        {
            lock (_lock)
            {
                return _renderer.RemovePriority(stripId, priority);
            }
        }

        public bool SetStripColors(
            int stripId,
            LedColorBuffer colorBuffer,
            int destinationLedIndex,
            StripPriority priority)
        {
            lock (_lock)
            {
                if (!_logicalStrips.ContainsKey(stripId))
                {
                    return false;
                }

                _renderer.SetColorBuffer(stripId, colorBuffer, 0, destinationLedIndex, colorBuffer.Count, priority);
            }

            return true;
        }

        public void RenderAllStripData()
        {
            lock (_lock)
            {
                foreach (var strip in _logicalStrips.Values)
                {
                    strip.SetColors(_renderer.RenderedData(strip.StripId), 0, strip.LedCount, 0);
                }

                _edgeLightDevice.RenderAllStripData();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                _edgeLightDevice.StripsChanged -= DeviceStripsChanged;
                _edgeLightDevice.Dispose();
                _eventBus.UnsubscribeAll(this);
                _renderer.Dispose();
                _brightnessData.Dispose();
                _disposed = true;
            }
        }

        private void SubscribeDeviceEvent<TEventType>() where TEventType : BaseDeviceEvent
        {
            _eventBus.Subscribe<TEventType>(
                this,
                _ => CheckForConnection(),
                x =>
                    x.IsForVidPid(EdgeLightConstants.VendorId, EdgeLightConstants.ProductId) &&
                    x.DeviceCategory.ToUpper().Equals("HID"));
        }

        private void SetStripsDefaultColor()
        {
            _renderer.Clear();
            foreach (var strip in _logicalStrips.Values)
            {
                SetStripColor(
                    strip,
                    Color.Blue,
                    !_platformControlledColorStrips.Contains(strip.StripId)
                        ? StripPriority.LowPriority
                        : StripPriority.PlatformControlled);
            }
        }

        private void DeviceStripsChanged(object sender, EventArgs e)
        {
            lock (_lock)
            {
                _logicalStrips = _logicalStripFactory.GetLogicalStrips(_edgeLightDevice.PhysicalStrips)
                    .ToDictionary(x => x.StripId, x => x);
                var stripsText = _logicalStrips.Select(
                    x => $"Got stripId {(StripIDs)(x.Key & 0xFF)} with 0x{x.Value.LedCount:x} Led");
                Logger.Debug($"Read Light data: {string.Join(", ", stripsText)}");
                SetStripsDefaultColor();
                SetBrightnessForPriority(EdgeLightConstants.MaxChannelBrightness, StripPriority.LowPriority);
            }

            _eventBus.Publish(new EdgeLightingStripsChangedEvent());
        }

        private void SetHighestPriorityBrightness(IStrip strip, bool setSystemBrightness = true)
        {
            var brightness = _brightnessData.GetBrightness(strip.StripId, strip.Brightness);
            if (strip.Brightness != brightness)
            {
                strip.Brightness = brightness;
            }

            if (setSystemBrightness)
            {
                _edgeLightDevice.SetSystemBrightness(
                    _brightnessData.GetSystemBrightness(EdgeLightConstants.MaxChannelBrightness));
            }
        }

        private void SetHighestPriorityBrightness(IEnumerable<IStrip> strips)
        {
            foreach (var strip in strips)
            {
                SetHighestPriorityBrightness(strip, false);
            }

            _edgeLightDevice.SetSystemBrightness(
                _brightnessData.GetSystemBrightness(EdgeLightConstants.MaxChannelBrightness));
        }

        private void SetStripColor(IStrip strip, Color color, StripPriority priority)
        {
            _renderer.SetColor(strip.StripId, color, priority);
        }

        private void CheckForConnection()
        {
            bool currentStatus;
            lock (_lock)
            {
                currentStatus = _edgeLightDevice.CheckForConnection();
                if (currentStatus == _connectionStatus)
                {
                    return;
                }

                _connectionStatus = currentStatus;
            }

            if (!currentStatus)
            {
                _eventBus.Publish(new EdgeLightingDisconnectedEvent());
            }
            else
            {
                _eventBus.Publish(new EdgeLightingConnectedEvent());
            }
        }
    }
}