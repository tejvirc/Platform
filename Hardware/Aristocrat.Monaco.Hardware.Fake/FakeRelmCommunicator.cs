namespace Aristocrat.Monaco.Hardware.Fake
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Contracts.Communicator;
    using Contracts.Reel.ControlData;
    using Contracts.SharedDevice;
    using GopherReels.Controls;
    using Kernel;
    using log4net;
    using MVVM;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class FakeRelmCommunicator : IRelmCommunicator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly IPathMapper _pathMapper;
        private int[] _reelOffsets;
        private const string SimWindowNamePartial = "ReelLayout_";
        private const string GamesDirectory = "/Games";
        private const string PackagesDirectory = "/Packages";
        private readonly string _baseName = "FakeRelm";

        private string GamesPath => _pathMapper.GetDirectory(GamesDirectory).FullName;
        private string PackagesPath => _pathMapper.GetDirectory(PackagesDirectory).FullName;
        private string KnownReels
        {
            get
            {
                var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
                if (reelController == null)
                {
                    return string.Empty;
                }

                var knownReels = reelController.ConnectedReels.Count;
                return $"{knownReels}";
            }
        }

        private int _id;
        private bool _disposed;

        public ReelSetWindowGS ReelSimWindow { get; private set; }
        public int ReelCount => ReelSimWindow?.ReelCount ?? 0;
        public int DefaultReelBrightness { get; set; }
        public string Manufacturer => _baseName + DeviceType;
        public string Model => _baseName + DeviceType;
        public string Firmware => _baseName;
        public string SerialNumber => _baseName;
        public bool IsOpen { get; set; }
        public int VendorId { get; set; }
        public int ProductId { get; set; }
        public int ProductIdDfu { get; set; }
        public string Protocol => "FakeRelm";
        public DeviceType DeviceType { get; set; }
        public IDevice Device { get; set; }
        public string FirmwareVersion => $"{_baseName}1.0";
        public string FirmwareRevision => $"{_baseName}001";
        public int FirmwareCrc => -1;
        public string BootVersion => string.Empty;
        public string VariantName => string.Empty;
        public string VariantVersion => string.Empty;
        public bool IsDfuCapable => true;

        public bool InDfuMode { get; private set; }

        public bool CanDownload { get; }

        public bool IsDownloadInProgress { get; }

        public event EventHandler<EventArgs> DeviceAttached;
        public event EventHandler<EventArgs> DeviceDetached;
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <summary>
        ///     Construct a <see cref="FakeCommunicator"/>
        /// </summary>
        public FakeRelmCommunicator()
            : this(
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
            
        }

        public FakeRelmCommunicator(IPathMapper pathMapper, IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));

            Logger.Debug("Constructed");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            _eventBus?.UnsubscribeAll(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Close();
            }

            _disposed = true;
        }

        /// <inheritdoc/>
        public bool Close()
        {
            Logger.Debug($"Closing Simulator.");
            MvvmHelper.ExecuteOnUI(() =>
            {
                ReelSimWindow?.Close();
            });

            IsOpen = false;
            return true;
        }

        /// <inheritdoc/>
        public bool Open()
        {
            var knownReels = KnownReels;

            if (knownReels == string.Empty)
            {
                Logger.Error("Cannot open reel simulator; there is no reel controller.");
                return false;
            }

            Logger.Debug($"Known reels: {knownReels}");

            // Avoid using same ID as any other already-running simulators
            var usedIds = new List<int> { 0 };
            var usedTitles = Process.GetProcesses()
                .Where(process => process.MainWindowTitle.Contains(SimWindowNamePartial))
                .Select(process => process.MainWindowTitle.Substring(process.MainWindowTitle.IndexOf('_') + 1));
            usedIds.AddRange(usedTitles.ToList().Select(int.Parse).ToList());
            _id = 1 + usedIds.Max();

            MvvmHelper.ExecuteOnUI(
            () =>
            {
                ReelSimWindow = new ReelSetWindowGS(_id, GamesPath, $"{knownReels} Reel Layout", PackagesPath, new UtilitiesLib.ExtLogger());
                ReelSimWindow.Show();

                Logger.Debug($"Game says: {ReelCount} reels");
            });

            while (ReelCount == 0)
            {
                Task.Delay(50);
            }

            Logger.Debug("FakeRelmReels is connected");
            IsOpen = true;
            return true;
        }

        /// <inheritdoc/>
        public bool Configure(IComConfiguration comConfiguration)
        {
            return true;
        }

        /// <summary>Raises the <see cref="DeviceAttached"/> event.</summary>
        private void OnDeviceAttached()
        {
            var invoker = DeviceAttached;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="DeviceDetached"/> event.</summary>
        private void OnDeviceDetached()
        {
            var invoker = DeviceDetached;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="DownloadProgressed"/> event.</summary>
        private void OnDownloadProgressed()
        {
            var invoker = DownloadProgressed;
            invoker?.Invoke(this, new ProgressEventArgs(0));
        }

        /// <inheritdoc/>
        public void ResetConnection()
        {
            Logger.Debug($"Resetting connection for {VendorId:X}...");
            OnDeviceDetached();
            Task.Delay(500);
            OnDeviceAttached();
        }

        /// <inheritdoc/>
        public Task<bool> HomeReels()
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            Logger.Debug($"Homing reels");

            for (int i = 1; i <= ReelCount; i++)
            {
                ReelSimWindow.HomeReel(i, 0);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            Logger.Debug($"Homing reel {reelId} to step {stop}");
            ReelSimWindow.SetReelBrightness(reelId, stop);

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> LoadControllerAnimationFile(AnimationFile file, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public Task<bool> LoadControllerAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PrepareControllerAnimation(LightShowFile file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PrepareControllerAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PrepareControllerAnimation(ReelCurveData file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PrepareControllerAnimations(IEnumerable<ReelCurveData> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PlayControllerAnimations(CancellationToken token)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            Logger.Debug($"Playing animations");
            ReelSimWindow.StartAnimations();

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> StopControllerLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> StopAllControllerLightShows(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PrepareControllerStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PrepareControllerNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            foreach (var nudge in nudgeData)
            {
                ReelSimWindow.NudgeReel(nudge.ReelId, nudge.Direction == SpinDirection.Backwards, nudge.Step);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            foreach (var brightnessSetting in brightness)
            {
                Logger.Debug($"Set reel brightness {brightnessSetting.Key}/{brightnessSetting.Value}");
                ReelSimWindow.SetReelBrightness(brightnessSetting.Key, brightnessSetting.Value);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SetBrightness(int brightness)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            Logger.Debug($"Setting all reels brightness to brightness");
            for (int i = 0; i < ReelCount; i++)
            {
                ReelSimWindow.SetReelBrightness(i, brightness);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> Synchronize(ReelSynchronizationData data, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task Initialize()
        {
            //prevent a double subscription for the FakeDeviceConnectedEvent in the case of Open->Close->Open
            _eventBus?.UnsubscribeAll(this);

            if (!Open() && !IsOpen)
            {
                return Task.CompletedTask;
            }

            Logger.Debug($"There are {ReelCount} reels");

            _reelOffsets = new int[ReelCount];

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            if (_reelOffsets.Length != offsets.Length)
            {
                return Task.FromResult(false);
            }

            Array.Copy(offsets, _reelOffsets, offsets.Length);

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> TiltReels()
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            for (int i = 1; i <= ReelCount; i++)
            {
                ReelSimWindow.TiltReel(i);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> EnterDfuMode()
        {
            Logger.Debug($"Entering Dfu Mode");
            InDfuMode = true;
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> ExitDfuMode()
        {
            Logger.Debug($"Exiting Dfu Mode");
            InDfuMode = false;
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<DfuStatus> Download(Stream firmware)
        {
            Logger.Debug($"Downloading firmware to fake device");
            return Task.FromResult(DfuStatus.Ok);
        }

        /// <inheritdoc/>
        public void AbortDownload()
        {
            Logger.Debug("Aborting Download");
        }
    }
}
