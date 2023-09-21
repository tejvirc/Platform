namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
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
    using MonacoLightStatus = Contracts.Reel.LightStatus;
    using ReelStatus = Contracts.Reel.ReelStatus;

    /// <summary>
    ///     The Relm Reel Controller control class
    /// </summary>
    public class RelmReelController : IReelControllerImplementation
    {
        private const string SampleAnimationsPath = @"ReelController\Relm\SampleAnimations";
        private const string LightShowExtenstion = ".lightshow";
        private const string StepperCurveExtenstion = ".stepper";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly (Func<ReelStatus, bool> Condition, ReelFaults Fault)[] ReelFaultPredicates =
        {
            (static x => x.ReelTampered, ReelFaults.ReelTamper),
            (static x => x.IdleUnknown, ReelFaults.IdleUnknown),
            (static x => x.ReelStall, ReelFaults.ReelStall),
            (static x => x.UnknownStop, ReelFaults.UnknownStop),
            (static x => x.OpticSequenceError, ReelFaults.ReelOpticSequenceError),
            (static x => x.LowVoltage, ReelFaults.LowVoltage)
        };

        private readonly Dictionary<Type, IReelImplementationCapability> _supportedCapabilities = new();
        private readonly ConcurrentDictionary<int, ReelStatus> _reelStatuses = new();
        private readonly ConcurrentDictionary<int, MonacoLightStatus> _lightStatuses = new();

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
        public event EventHandler<EventArgs> ResetSucceeded;

        /// <inheritdoc />
        public event EventHandler<EventArgs> ResetFailed;

        /// <inheritdoc />
        public event EventHandler HardwareInitialized;
#pragma warning restore 67

        /// <inheritdoc />
        public event EventHandler<EventArgs> Connected;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Disconnected;


        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultOccurred;

        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultCleared;

        /// <inheritdoc />
        public event EventHandler<ReelFaultedEventArgs> FaultOccurred;

        /// <inheritdoc />
        public event EventHandler<ReelFaultedEventArgs> FaultCleared;

        /// <inheritdoc />
        public event EventHandler<ReelStoppingEventArgs> ReelStopping;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelStopped;

        /// <inheritdoc />
        public event EventHandler<ReelSpinningEventArgs> ReelSpinning;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelSlowSpinning;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelDisconnected;

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

        // TODO: Wire this up
        /// <inheritdoc />
        public bool IsEnabled { get; }

        /// <inheritdoc />
        public int Crc => _communicator?.FirmwareCrc ?? 0;

        /// <inheritdoc />
        public IReadOnlyCollection<int> ReelIds => Faults.Keys.ToList().AsReadOnly();

        /// <inheritdoc />
        public ReelControllerFaults ReelControllerFaults { get; private set; }

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelFaults> Faults =>
            _reelStatuses.ToDictionary(x => x.Key, x => x.Value.GetFaults());

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelStatus> ReelStatuses => _reelStatuses;

        /// <inheritdoc />
        public int DefaultHomeStep => _communicator?.DefaultHomeStep ?? 0;

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
            if (communicator is not IRelmCommunicator relmCommunicator)
            {
                return false;
            }

            try
            {
                _communicator = relmCommunicator;
                RegisterEventListeners();
                await _communicator.Initialize();

                if (_communicator.IsOpen)
                {
                    _supportedCapabilities.Add(typeof(IAnimationImplementation), new RelmAnimation(_communicator));
                    _supportedCapabilities.Add(typeof(IReelBrightnessImplementation), new RelmBrightness(_communicator));
                    _supportedCapabilities.Add(typeof(ISynchronizationImplementation), new RelmSynchronization(_communicator));
                    _supportedCapabilities.Add(typeof(IStepperRuleImplementation), new RelmStepperRule(_communicator));
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
            // HarkeyProtocol.SelfTest does not test anything, it just clears errors.
            // this needs to do the same.  this is because the ReelControllerMonitor needs
            // these faults to be cleared before it can home the reels.

            OnControllerFaultCleared(this,
                new ReelControllerFaultedEventArgs(ReelControllerFaults.RequestError |
                                                   ReelControllerFaults.CommunicationError));

            var connectedReels = _reelStatuses.Values.Where(x => x.Connected).ToArray();
            foreach (var status in connectedReels)
            {
                ClearFaults(status.ReelId);
            }
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
        public Task<bool> HaltReels()
        {
            return _communicator.HaltReels();
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

        private void OnReelStatusesReceived(object sender, ReelStatusReceivedEventArgs args)
        {
            foreach (var status in args.Statuses)
            {
                _reelStatuses.AddOrUpdate(
                    status.ReelId,
                    addValueFactory: _ =>
                    {
                        ReelConnected?.Invoke(this, new ReelEventArgs(status.ReelId));
                        return status;
                    },
                    updateValueFactory: (_, currentStatus) =>
                    {
                        if (currentStatus.Connected && !status.Connected)
                        {
                            currentStatus.Connected = false;
                            ReelDisconnected?.Invoke(this, new ReelEventArgs(status.ReelId));
                        }
                        else if (!currentStatus.Connected && status.Connected)
                        {
                            currentStatus.Connected = true;
                            ReelConnected?.Invoke(this, new ReelEventArgs(status.ReelId));
                        }

                        var newFaults = ReelFaults.None;

                        foreach (var (condition, fault) in ReelFaultPredicates)
                        {
                            var oldFaulted = condition(currentStatus);
                            var newFaulted = condition(status);

                            if (!oldFaulted && newFaulted)
                            {
                                newFaults |= fault;
                                currentStatus.SetFault(fault);
                            }
                        }

                        if (newFaults != ReelFaults.None)
                        {
                            FaultOccurred?.Invoke(this, new ReelFaultedEventArgs(newFaults, status.ReelId));
                        }

                        return currentStatus;
                    });
            }
        }

        private void ClearFaults(int reelId)
        {
            if (!_reelStatuses.TryGetValue(reelId, out var status))
            {
                return;
            }

            var faultsToClear = ReelFaults.None;
            foreach (var (condition, fault) in ReelFaultPredicates)
            {
                if (condition(status))
                {
                    faultsToClear |= fault;
                }
            }

            var newStatus = new ReelStatus { ReelId = reelId, Connected = true };
            if (faultsToClear != ReelFaults.None)
            {
                _reelStatuses.AddOrUpdate(reelId, _ => newStatus, (_, _) => newStatus);
                FaultCleared?.Invoke(this, new ReelFaultedEventArgs(faultsToClear, status.ReelId));
            }
        }

        private void OnLightStatusReceived(object sender, LightEventArgs args)
        {
            foreach (var status in args.Statuses)
            {
                _lightStatuses.AddOrUpdate(status.LightId, status, (_, _) => status);
            }

            var faultArgs = new ReelControllerFaultedEventArgs(ReelControllerFaults.LightError);
            if (_lightStatuses.Values.Any(x => x.Faulted))
            {
                OnControllerFaultOccurred(this, faultArgs);
            }
            else
            {
                OnControllerFaultCleared(this, faultArgs);
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
                foreach (var key in _supportedCapabilities.Keys.ToArray())
                {
                    var capability = _supportedCapabilities[key];
                    capability.Dispose();
                    _supportedCapabilities[key] = null;
                }

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

            _communicator.ReelStatusReceived += OnReelStatusesReceived;
            _communicator.LightStatusReceived += OnLightStatusReceived;
            _communicator.ControllerFaultOccurred += OnControllerFaultOccurred;
            _communicator.ControllerFaultCleared += OnControllerFaultCleared;
            _communicator.ReelSpinningStatusReceived += OnReelSpinningStatusReceived;
            _communicator.ReelStopping += OnReelStoppingReceived;
            _communicator.DeviceAttached += OnDeviceAttached;
            _communicator.DeviceDetached += OnDeviceDetached;
        }

        private void UnregisterEventListeners()
        {
            if (_communicator is null)
            {
                return;
            }

            _communicator.ReelStatusReceived -= OnReelStatusesReceived;
            _communicator.LightStatusReceived -= OnLightStatusReceived;
            _communicator.ControllerFaultOccurred -= OnControllerFaultOccurred;
            _communicator.ControllerFaultCleared -= OnControllerFaultCleared;
            _communicator.ReelSpinningStatusReceived -= OnReelSpinningStatusReceived;
            _communicator.ReelStopping -= OnReelStoppingReceived;
            _communicator.DeviceAttached -= OnDeviceAttached;
            _communicator.DeviceDetached -= OnDeviceDetached;
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
                await _communicator.LoadAnimationFiles(animationFiles, new Progress<LoadingAnimationFileModel>());
            }
        }

        private IEnumerable<string> GetSampleAnimationFilePaths()
        {
            return Directory.EnumerateFiles(SampleAnimationsPath).Where(x =>
                x.EndsWith(LightShowExtenstion, true, CultureInfo.InvariantCulture) ||
                x.EndsWith(StepperCurveExtenstion, true, CultureInfo.InvariantCulture));
        }

        private void OnReelSpinningStatusReceived(object sender, ReelSpinningEventArgs evt)
        {
            if (evt.IdleAtStep)
            {
                ReelStopped?.Invoke(sender, new ReelEventArgs(evt.ReelId, evt.Step));
                return;
            }

            if (evt.SlowSpinning)
            {
                ReelSlowSpinning?.Invoke(this, new ReelEventArgs(evt.ReelId));
                return;
            }

            if (evt.SpinVelocity != SpinVelocity.None)
            {
                ReelSpinning?.Invoke(this, new ReelSpinningEventArgs(evt.ReelId, evt.SpinVelocity));
            }
        }

        private void OnReelStoppingReceived(object sender, ReelStoppingEventArgs args)
        {
            ReelStopping?.Invoke(sender, args);
        }

        private void OnControllerFaultOccurred(object sender, ReelControllerFaultedEventArgs e)
        {
            if ((e.Faults & ReelControllerFaults.CommunicationError) != 0)
            {
                ReelControllerFaults |= ReelControllerFaults.CommunicationError;
            }

            if ((e.Faults & ReelControllerFaults.HardwareError) != 0)
            {
                ReelControllerFaults |= ReelControllerFaults.HardwareError;
            }

            if ((e.Faults & ReelControllerFaults.LightError) != 0)
            {
                ReelControllerFaults |= ReelControllerFaults.LightError;
            }

            ControllerFaultOccurred?.Invoke(this, e);
        }

        private void OnControllerFaultCleared(object sender, ReelControllerFaultedEventArgs e)
        {
            if ((e.Faults & ReelControllerFaults.CommunicationError) != 0)
            {
                ReelControllerFaults &= ~ReelControllerFaults.CommunicationError;
            }

            if ((e.Faults & ReelControllerFaults.HardwareError) != 0)
            {
                ReelControllerFaults &= ~ReelControllerFaults.HardwareError;
            }

            if ((e.Faults & ReelControllerFaults.LightError) != 0)
            {
                ReelControllerFaults &= ~ReelControllerFaults.LightError;
            }

            ControllerFaultCleared?.Invoke(this, e);
        }

        private void OnDeviceAttached(object sender, EventArgs e)
        {
            if (IsInitialized)
            {
                Connected?.Invoke(sender, e);
                OnInitialized();
            }
        }

        private void OnDeviceDetached(object sender, EventArgs e)
        {
            foreach (var status in _reelStatuses.Values)
            {
                status.Connected = false;
            }
            Disconnected?.Invoke(sender, e);
        }
    }
}
