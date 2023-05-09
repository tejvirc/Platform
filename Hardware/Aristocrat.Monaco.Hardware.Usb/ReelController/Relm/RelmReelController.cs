namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Gds.Reel;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Contracts.SharedDevice;

    /// <summary>
    ///     The Relm Reel Controller control class
    /// </summary>
    public class RelmReelController : IReelControllerImplementation
    {
        private readonly Dictionary<Type, IReelImplementationCapability> _supportedCapabilities = new();

        private bool _disposed;
        private IRelmCommunicator _communicator;
        private bool _isInitialized;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Initialized;

        /// <inheritdoc />
        public event EventHandler<EventArgs> InitializationFailed;

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
        public event EventHandler<ReelEventArgs> ReelConnected;

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
        public IReadOnlyDictionary<int, ReelStatus> ReelsStatus { get; }

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
                await _communicator.Initialize();
                IsInitialized = _communicator.IsOpen;
                
                _supportedCapabilities.Add(typeof(IAnimationImplementation), new RelmAnimation(_communicator));
                _supportedCapabilities.Add(typeof(IReelBrightnessImplementation), new RelmBrightness(_communicator));
                _supportedCapabilities.Add(typeof(ISynchronizationImplementation), new RelmSynchronization(_communicator));

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

                var usbCommunicator = _communicator;
                _communicator = null;
                usbCommunicator.Dispose();
            }

            _disposed = true;
        }
    }
}
