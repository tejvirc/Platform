namespace Aristocrat.Monaco.Hardware.Fake
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Contracts.SharedDevice;
    using log4net;

    public class FakeHarkeyReelController :
        IReelControllerImplementation,
        IReelSpinImplementation,
        IReelBrightnessImplementation,
        IReelLightingImplementation
    {
        private const string DefaultBaseName = "Fake";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly Type[] SupportedCapabilities =
        {
            typeof(IReelSpinImplementation),
            typeof(IReelBrightnessImplementation),
            typeof(IReelLightingImplementation)
        };

        private readonly Dictionary<int, ReelFaults> _faults = new();
        private readonly Dictionary<int, ReelStatus> _reelStatuses = new();

        private bool _disposed;
        private IHarkeyCommunicator _communicator;
        private bool _isInitialized;

        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            return _communicator?.SetBrightness(brightness);
        }

        /// <inheritdoc />
        public Task<bool> SetBrightness(int brightness)
        {
            return _communicator?.SetBrightness(brightness);
        }

        /// <inheritdoc />
        public bool IsConnected => _communicator?.IsOpen ?? false;

        /// <inheritdoc />
        public bool IsInitialized
        {
            get => _isInitialized;
            private set
            {
                _isInitialized = value;
                if (value)
                {
                    OnInitialized();
                }
            }
        }

        /// <inheritdoc />
        public bool IsEnabled { get; set; }

        /// <inheritdoc />
        public int Crc => _communicator?.FirmwareCrc ?? 0;

        /// <inheritdoc />
        public string Protocol => _communicator?.Protocol ?? string.Empty;

        /// <inheritdoc />
        public string Manufacturer => "Fake";

        /// <inheritdoc />
        public string Model => "FakeModel";

        /// <inheritdoc />
        public string FirmwareId => "1";

        /// <inheritdoc />
        public string FirmwareRevision => "2";

        /// <inheritdoc />
        public string SerialNumber => "3";

        /// <inheritdoc />
        public event EventHandler<EventArgs> Initialized;

        /// <inheritdoc />
        public event EventHandler<EventArgs> InitializationFailed;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Enabled;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Disabled;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Connected;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Disconnected;

        /// <inheritdoc />
        public event EventHandler<EventArgs> ResetSucceeded;

        /// <inheritdoc />
        public event EventHandler<EventArgs> ResetFailed;

        /// <inheritdoc />
        public bool Open()
        {
            return true;
        }

        /// <inheritdoc />
        public bool Close()
        {
            return true;
        }

        /// <inheritdoc />
        public Task<bool> Enable()
        {
            IsEnabled = true;
            Logger.Info("Enabled fake harkey reel controller adapter");
            OnEnabled(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Disable()
        {
            IsEnabled = false;
            Logger.Warn("Disabled fake harkey reel controller adapter.");
            OnDisable(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SelfTest(bool nvm)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<int> CalculateCrc(int seed)
        {
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
        }

        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultOccurred;

        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultCleared;

        /// <inheritdoc />
        public event EventHandler<ReelFaultedEventArgs> FaultOccurred;

        /// <inheritdoc />
        public event EventHandler<ReelFaultedEventArgs> FaultCleared;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelStopping;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelStopped;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelSpinning;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelSlowSpinning;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelDisconnected;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelConnected;

        /// <inheritdoc />
        public event EventHandler HardwareInitialized;

        /// <inheritdoc />
        public IReadOnlyCollection<int> ReelIds => Faults.Keys.ToList().AsReadOnly();

        /// <inheritdoc />
        public ReelControllerFaults ReelControllerFaults { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelFaults> Faults { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelStatus> ReelStatuses => _reelStatuses;

        /// <inheritdoc />
        public Task<bool> HomeReels()
        {
            return _communicator?.HomeReels();
        }

        /// <inheritdoc />
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            return _communicator?.HomeReel(reelId, stop, resetStatus);
        }

        /// <inheritdoc />
        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            return _communicator?.SetReelOffsets(offsets);
        }

        /// <inheritdoc />
        public Task<bool> TiltReels()
        {
            return _communicator?.TiltReels();
        }

        /// <inheritdoc />
        public IEnumerable<Type> GetCapabilities()
        {
            return SupportedCapabilities;
        }

        /// <inheritdoc />
        public T GetCapability<T>() where T : class, IReelImplementationCapability
        {
            if (!HasCapability<T>())
            {
                throw new InvalidOperationException("capability not available");
            }

            return this as T;
        }

        /// <inheritdoc />
        public bool HasCapability<T>() where T : class, IReelImplementationCapability
        {
            return SupportedCapabilities.Contains(typeof(T));
        }

        /// <inheritdoc />
        public int VendorId => _communicator?.VendorId ?? 0;

        /// <inheritdoc />
        public int ProductId => _communicator?.ProductId ?? 0;

        /// <inheritdoc />
        public bool IsDfuCapable => _communicator?.IsDfuCapable ?? false;

        /// <inheritdoc />
        public bool IsDfuInProgress => false;

        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <inheritdoc />
        public async Task<bool> Initialize()
        {
            return await Initialize(null);
        }

        /// <inheritdoc />
        public async Task<bool> Initialize(ICommunicator communicator)
        {
            IsInitialized = false;
            try
            {
                if (communicator is not IHarkeyCommunicator harkeyCommunicator)
                {
                    return false;
                }

                _communicator = harkeyCommunicator;
                RegisterEventListeners();
                await _communicator.Initialize();

                if (_communicator.IsOpen)
                {
                    IsInitialized = true;
                    Logger.Info("Initialized fake harkey reel controller adapter");
                }

                return IsInitialized;
            }
            finally
            {
                if (!IsInitialized)
                {
                    OnInitializationFailed();
                }
            }
        }

        /// <inheritdoc />
        public Task<bool> Detach()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> Reconnect()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<DfuStatus> Download(Stream firmware)
        {
            return Task.FromResult(DfuStatus.ErrUnknown);
        }

        /// <inheritdoc />
        public Task<DfuStatus> Upload(Stream firmware)
        {
            return Task.FromResult(DfuStatus.ErrUnknown);
        }

        /// <inheritdoc />
        public void Abort()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public Task<IList<int>> GetReelLightIdentifiers()
        {
            IList<int> numberOfLights = Enumerable.Range(1, 3).ToList();
            return Task.FromResult(numberOfLights);
        }

        /// <inheritdoc />
        public Task<bool> SetLights(params ReelLampData[] lampData)
        {
            return _communicator?.SetLights(lampData);
        }

        /// <inheritdoc />
        public int DefaultSpinSpeed => 1;

        /// <inheritdoc />
        public Task<bool> NudgeReels(params NudgeReelData[] nudgeReelData)
        {
            return _communicator?.NudgeReels(nudgeReelData);
        }

        /// <inheritdoc />
        public Task<bool> SpinReels(params ReelSpinData[] spinReelData)
        {
            return _communicator?.SpinReels(spinReelData);
        }

        /// <inheritdoc />
        public Task<bool> SetReelSpeed(params ReelSpeedData[] speedData)
        {
            return Task.FromResult(true);
        }

        public event EventHandler<ReelEventArgs> ReelTilted;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                UnregisterEventListeners();

                var fakeCommunicator = _communicator;
                _communicator = null;
                fakeCommunicator.Dispose();
            }

            _disposed = true;
        }

        /// <summary>Executes the <see cref="Initialized" /> action.</summary>
        protected void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="InitializationFailed" /> action.</summary>
        protected void OnInitializationFailed()
        {
            InitializationFailed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="Enabled" /> action.</summary>
        protected void OnEnabled(EventArgs e)
        {
            Enabled?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="Disable" /> action.</summary>
        protected void OnDisable(EventArgs e)
        {
            Disabled?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="Connected" /> action.</summary>
        protected void OnConnected(EventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="Disconnected" /> action.</summary>
        protected void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="ResetSucceeded" /> action.</summary>
        protected void OnResetSucceeded()
        {
            ResetSucceeded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="ResetFailed" /> action.</summary>
        protected void OnResetFailed()
        {
            ResetFailed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="DownloadProgressed" /> action.</summary>
        protected void OnDownloadProgressed(ProgressEventArgs e)
        {
            DownloadProgressed?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="ControllerFaultCleared" /> action.</summary>
        protected void OnControllerFaultCleared(ReelControllerFaultedEventArgs e)
        {
            ControllerFaultCleared?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnFaultOccurred" /> action.</summary>
        protected void OnControllerFaultOccurred(ReelControllerFaultedEventArgs e)
        {
            ControllerFaultOccurred?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="FaultCleared" /> action.</summary>
        protected void OnFaultCleared(ReelFaultedEventArgs e)
        {
            FaultCleared?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnFaultOccurred" /> action.</summary>
        protected void OnFaultOccurred(ReelFaultedEventArgs e)
        {
            FaultOccurred?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="ReelStopping" /> action.</summary>
        protected void OnReelStopping(ReelEventArgs e)
        {
            ReelStopping?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="ReelTilted" /> action.</summary>
        protected void OnReelTilted(ReelEventArgs e)
        {
            ReelTilted?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="ReelStopped" /> action.</summary>
        protected void OnReelStopped(ReelEventArgs e)
        {
            ReelStopped?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="ReelSpinning" /> action.</summary>
        protected void OnReelSpinning(ReelEventArgs e)
        {
            ReelSpinning?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="ReelSlowSpinning" /> action.</summary>
        protected void OnReelSlowSpinning(ReelEventArgs e)
        {
            ReelSlowSpinning?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="ReelDisconnected" /> action.</summary>
        protected void OnReelDisconnected(ReelEventArgs e)
        {
            var reelsStatus = new ReelStatus { ReelId = e.ReelId, Connected = false };
            AddOrUpdateReelsStatuses(reelsStatus);
            ReelDisconnected?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="ReelConnected" /> action.</summary>
        protected void OnReelConnected(ReelEventArgs e)
        {
            var reelsStatus = new ReelStatus { ReelId = e.ReelId, Connected = true };
            AddOrUpdateReelsStatuses(reelsStatus);
            ReelConnected?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="HardwareInitialized" /> action.</summary>
        protected void OnHardwareInitialized(ReelEventArgs e)
        {
            HardwareInitialized?.Invoke(this, e);
        }

        private void RegisterEventListeners()
        {
            if (_communicator is null)
            {
                return;
            }

            _communicator.StatusesReceived += OnReelStatusesReceived;
            _communicator.ReelTilted += OnReelTiltedReceived;
            _communicator.ReelSpinning += OnReelSpinningReceived;
            _communicator.ReelStopped += OnReelStoppedReceived;
        }

        private void UnregisterEventListeners()
        {
            if (_communicator is null)
            {
                return;
            }

            _communicator.StatusesReceived -= OnReelStatusesReceived;
            _communicator.ReelTilted -= OnReelTiltedReceived;
            _communicator.ReelSpinning -= OnReelSpinningReceived;
            _communicator.ReelStopped -= OnReelStoppedReceived;
        }

        private void AddOrUpdateReelsStatuses(ReelStatus reelsStatus)
        {
            if (_reelStatuses.ContainsKey(reelsStatus.ReelId))
            {
                _reelStatuses[reelsStatus.ReelId] = reelsStatus;
            }
            else
            {
                _reelStatuses.Add(reelsStatus.ReelId, reelsStatus);
            }
        }

        private void OnReelTiltedReceived(object sender, ReelEventArgs reelStatus)
        {
            OnReelTilted(reelStatus);
        }

        private void OnReelSpinningReceived(object sender, ReelEventArgs reelStatus)
        {
            OnReelSpinning(reelStatus);
        }

        private void OnReelStoppedReceived(object sender, ReelEventArgs reelStatus)
        {
            OnReelStopped(reelStatus);
        }

        private void OnReelStatusesReceived(object sender, ReelStatusReceivedEventArgs args)
        {
            foreach (var status in args.Statuses)
            {
                if (status.Connected)
                {
                    OnReelConnected(new ReelEventArgs(status.ReelId));
                }
            }
        }
    }
}