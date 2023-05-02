namespace Aristocrat.Monaco.Hardware.Fake
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Contracts.Communicator;
    using Contracts.Gds;
    using Contracts.IO;
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
        private const string GamesPath = "/Games";
        private const string PackagesPath = "/Packages";
        private readonly string _baseName = "FakeRelm";
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

            if (!IsOpen)
            {
                Open();
            }

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
            IsOpen = true;
            return true;
        }

        private void OpenReelSimulatorWindow()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
            if (reelController == null)
            {
                Logger.Error("Cannot open reel simulator; there is no reel controller.");
                return;
            }

            var gamesPath = _pathMapper.GetDirectory(GamesPath).FullName;
            var packagesPath = _pathMapper.GetDirectory(PackagesPath).FullName;
            var knownReels = reelController.ConnectedReels.Count;
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
                ReelSimWindow = new ReelSetWindowGS(_id, gamesPath, $"{knownReels} Reel Layout", packagesPath, new UtilitiesLib.ExtLogger());
                ReelSimWindow.Show();

                Logger.Debug($"Game says: {ReelCount} reels");
            });

            while (ReelCount == 0)
            {
                Task.Delay(50);
            }
        }

        /// <summary>Handle a <see cref="FakeDeviceConnectedEvent"/>.</summary>
        /// <param name="fakeDeviceConnectedEvent">The <see cref="FakeDeviceConnectedEvent"/> to handle.</param>
        private void HandleEvent(FakeDeviceConnectedEvent fakeDeviceConnectedEvent)
        {
            Logger.Debug("FakeRelmReels is connected");
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
            Task.Run(() =>
            {
                OnDeviceDetached();
                Task.Delay(500);
                OnDeviceAttached();
            });
        }

        /// <inheritdoc/>
        public Task<bool> HomeReels()
        {
            return Task.Run(() => {
                Logger.Debug($"Setting all reels brightness to brightness");
                for (int i = 1; i <= ReelCount; i++)
                {
                    ReelSimWindow.HomeReel(i, 1);
                }

                return true;
            });
        }

        /// <inheritdoc/>
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            return Task.Run(() => {
                Logger.Debug($"Homing reel {reelId} to step {stop}");
                ReelSimWindow.SetReelBrightness(reelId, stop);

                return true;
            });
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
            return Task.Run(() => {
                Logger.Debug($"Playing animations");
                ReelSimWindow.StartAnimations();

                return true;
            }, token);
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
            return Task.Run(
                () =>
                {
                    foreach (var nudge in nudgeData)
                    {
                        ReelSimWindow.NudgeReel(nudge.ReelId, nudge.Direction == SpinDirection.Backwards, nudge.Step);
                    }

                    return true;
                }, token);
        }

        /// <inheritdoc/>
        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            return Task.Run(() => {
                foreach (var brightnessSetting in brightness)
                {
                    Logger.Debug($"Set reel brightness {brightnessSetting.Key}/{brightnessSetting.Value}");
                    ReelSimWindow.SetReelBrightness(brightnessSetting.Key, brightnessSetting.Value);
                }

                return true;
            });
        }

        /// <inheritdoc/>
        public Task<bool> SetBrightness(int brightness)
        {
            return Task.Run(() => {
                Logger.Debug($"Setting all reels brightness to brightness");
                for (int i = 0; i < ReelCount; i++)
                {
                    ReelSimWindow.SetReelBrightness(i, brightness);
                }

                return true;
            });
        }

        /// <inheritdoc/>
        public Task<bool> Synchronize(ReelSynchronizationData data, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task Initialize()
        {
            return Task.Run(
                () =>
                {
                    //prevent a double subscription for the FakeDeviceConnectedEvent in the case of Open->Close->Open
                    _eventBus?.UnsubscribeAll(this);

                    OpenReelSimulatorWindow();

                    Logger.Debug($"There are {ReelCount} reels");

                    _reelOffsets = new int[ReelCount];

                    //We assume the device will be opened by default
                    _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);
                });
        }

        /// <inheritdoc/>
        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            return Task.Run(
                () =>
                {
                    if (_reelOffsets.Length != offsets.Length)
                    {
                        return false;
                    }

                    Array.Copy(offsets, _reelOffsets, offsets.Length);

                    return true;
                });
        }

        /// <inheritdoc/>
        public Task<bool> TiltReels()
        {
            return Task.Run(() =>
                {
                    for (int i = 1; i <= ReelCount; i++)
                    {
                        ReelSimWindow.TiltReel(i);
                    }

                    return true;
                });
        }

        /// <inheritdoc/>
        public Task<bool> EnterDfuMode()
        {
            return Task.Run(()=>
            {
                Logger.Debug($"Entering Dfu Mode");
                InDfuMode = true;
                return true;
            });
        }

        /// <inheritdoc/>
        public Task<bool> ExitDfuMode()
        {
            return Task.Run(() =>
            {
                Logger.Debug($"Exiting Dfu Mode");
                InDfuMode = false;
                return true;
            });
        }

        /// <inheritdoc/>
        public Task<DfuStatus> Download(Stream firmware)
        {
            Logger.Debug($"Downloading firmware to fake device");
            return Task.Run(() => DfuStatus.Ok);
        }

        /// <inheritdoc/>
        public void AbortDownload()
        {
            Logger.Debug("Aborting Download");
        }
    }
}
