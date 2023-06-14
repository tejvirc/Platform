namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using Contracts.Communicator;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Contracts.SharedDevice;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Reel.ControlData;
    using log4net;

    /// <summary>
    ///     The Relm Reel Controller control class
    /// </summary>
    public class RelmReelController : IReelControllerImplementation
    {
        private const string SampleAnimationsPath = @"ReelController\Relm\SampleAnimations";
        private const string LightShowExtenstion = ".lightshow";
        private const string StepperCurveExtenstion = ".stepper";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Dictionary<Type, IReelImplementationCapability> _supportedCapabilities = new();
        private readonly ConcurrentDictionary<int, ReelStatus> _reelStatuses = new();

        private bool _disposed;
        private IRelmCommunicator _communicator;
        private bool _isInitialized;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Initialized;

        /// <inheritdoc />
        public event EventHandler<EventArgs> InitializationFailed;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelConnected;

#pragma warning disable 67
        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

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
        public event EventHandler HardwareInitialized;
#pragma warning restore 67

        /// <inheritdoc />
        public int VendorId => _communicator?.VendorId ?? 0;

        /// <inheritdoc />
        public int ProductId => _communicator?.ProductId ?? 0;

        /// <inheritdoc />
        public bool IsDfuCapable => _communicator?.IsDfuCapable ?? false;

        /// <inheritdoc />
        public string Protocol => _communicator?.Protocol ?? string.Empty;

        /// <inheritdoc />
        public bool IsDfuInProgress => _communicator?.IsDownloadInProgress ?? false;

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
        public bool IsEnabled { get; }

        /// <inheritdoc />
        public int Crc => _communicator?.FirmwareCrc ?? 0;

        /// <inheritdoc />
        public IReadOnlyCollection<int> ReelIds { get; }

        /// <inheritdoc />
        public ReelControllerFaults ReelControllerFaults { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelFaults> Faults { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelStatus> ReelStatuses => _reelStatuses;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task<bool> Initialize(ICommunicator communicator)
        {
            IsInitialized = false;
            try
            {
                if (communicator is not IRelmCommunicator relmCommunicator)
                {
                    return false;
                }

                _communicator = relmCommunicator;
                RegisterEventListeners();
                await _communicator.Initialize();

                if (_communicator.IsOpen)
                {
                    _supportedCapabilities.Add(typeof(IAnimationImplementation), new RelmAnimation(_communicator));
                    _supportedCapabilities.Add(typeof(IReelBrightnessImplementation), new RelmBrightness(_communicator));
                    _supportedCapabilities.Add(typeof(ISynchronizationImplementation), new RelmSynchronization(_communicator));
                    await LoadPlatformSampleShowsAndCurves();

                    IsInitialized = true;
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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> Reconnect()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<DfuStatus> Download(Stream firmware)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<DfuStatus> Upload(Stream firmware)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Abort()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Open()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Close()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> Enable()
        {
            // TODO: Implement this correctly
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> Disable()
        {
            // TODO: Implement this correctly
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
            // TODO: Implement this correctly
            return Task.FromResult(Crc);
        }

        /// <inheritdoc />
        public void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
        }

        /// <inheritdoc />
        public Task<bool> HomeReels()
        {
            return _communicator.HomeReels();
        }

        /// <inheritdoc />
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            return _communicator.HomeReel(reelId, stop, resetStatus);
        }

        /// <inheritdoc />
        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            return _communicator.SetReelOffsets(offsets);
        }

        /// <inheritdoc />
        public Task<bool> TiltReels()
        {
            return _communicator.TiltReels();
        }

        /// <inheritdoc />
        public IEnumerable<Type> GetCapabilities() => _supportedCapabilities.Keys;

        /// <inheritdoc />
        public T GetCapability<T>() where T : class, IReelImplementationCapability
        {
            if (!HasCapability<T>())
            {
                throw new InvalidOperationException("capability not available");
            }

            return (T)_supportedCapabilities[typeof(T)];
        }

        /// <inheritdoc />
        public bool HasCapability<T>() where T : class, IReelImplementationCapability =>
            _supportedCapabilities.ContainsKey(typeof(T));

        /// <summary>
        ///     Called when a reel is connected
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnReelConnected(ReelEventArgs e)
        {
            // TODO: We need to keep track of the status and handle disconnects.
            // Right now the reel controller firmware does not support reel disconnects
            var reelsStatus = new ReelStatus { ReelId = e.ReelId, Connected = true };
            _reelStatuses.AddOrUpdate(e.ReelId, reelsStatus, (_, _) => reelsStatus);
            ReelConnected?.Invoke(this, e);
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

        private void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        private void OnInitializationFailed()
        {
            InitializationFailed?.Invoke(this, EventArgs.Empty);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _supportedCapabilities[typeof(IAnimationImplementation)] = null;
                _supportedCapabilities[typeof(IReelBrightnessImplementation)] = null;
                _supportedCapabilities[typeof(ISynchronizationImplementation)] = null;
                _supportedCapabilities.Clear();

                UnregisterEventListeners();

                var usbCommunicator = _communicator;
                _communicator = null;
                usbCommunicator.Dispose();
            }

            _disposed = true;
        }

        private void RegisterEventListeners()
        {
            if (_communicator is null)
            {
                return;
            }
            
            _communicator.StatusesReceived += OnReelStatusesReceived;
        }

        private void UnregisterEventListeners()
        {
            if (_communicator is null)
            {
                return;
            }

            _communicator.StatusesReceived -= OnReelStatusesReceived;
        }

        private async Task LoadPlatformSampleShowsAndCurves()
        {
            if (_communicator is null || !Directory.Exists(SampleAnimationsPath))
            {
                return;
            }

            var animationFiles = (from file in GetSampleAnimationFilePaths()
                let extension = Path.GetExtension(file)
                let type = extension == LightShowExtenstion
                    ? AnimationType.PlatformLightShow
                    : AnimationType.PlatformStepperCurve
                select new AnimationFile(file, type)).ToList();

            await _communicator.RemoveAllControllerAnimations();
            
            Logger.Debug($"Loading {animationFiles.Count} platform sample animations");
            if (animationFiles.Count > 0)
            {
                Logger.Debug($"Loading {animationFiles.Select(x => x.FriendlyName)} platform sample animations");
                await _communicator.LoadAnimationFiles(animationFiles);
            }
        }

        private IEnumerable<string> GetSampleAnimationFilePaths()
        {
            return Directory.EnumerateFiles(SampleAnimationsPath).Where(x =>
                x.EndsWith(LightShowExtenstion, true, CultureInfo.InvariantCulture) ||
                x.EndsWith(StepperCurveExtenstion, true, CultureInfo.InvariantCulture));
        }
    }
}
