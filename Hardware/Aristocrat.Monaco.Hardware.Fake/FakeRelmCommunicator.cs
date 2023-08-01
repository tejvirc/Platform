namespace Aristocrat.Monaco.Hardware.Fake
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Contracts.Communicator;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using MonacoReelStatus = Contracts.Reel.ReelStatus;
    using MonacoLightStatus = Contracts.Reel.LightStatus;
    using Aristocrat.Toolkit.Mvvm.Extensions;

    public class FakeRelmCommunicator : IRelmCommunicator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly IPathMapper _pathMapper;
        private readonly string _baseName = "FakeRelm";

        private const string SimWindowNamePartial = "ReelLayout_";
        private const string GamesDirectory = "/Games";
        private const string PackagesDirectory = "/Packages";
        private const int DefaultHomeStepValue = 5;

        private int[] _reelOffsets;
        private int _id;
        private bool _disposed;

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
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> DeviceAttached;
        
        /// <inheritdoc/>
        public event EventHandler<EventArgs> DeviceDetached;
        
        /// <inheritdoc/>
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <inheritdoc/>
        public event EventHandler<ReelStatusReceivedEventArgs> ReelStatusReceived;

#pragma warning disable 67
        /// <inheritdoc/>
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultOccurred;

        /// <inheritdoc/>
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultCleared;

        public event EventHandler<LightEventArgs> LightStatusReceived;

        /// <inheritdoc/>
        public event EventHandler<ReelStopData> ReelIdleInterruptReceived;
#pragma warning restore 67

        /// <summary>
        ///     Get the reel count for the connected device
        /// </summary>
        public int ReelCount => 0;//ReelSimWindow?.ReelCount ?? 0;
        
        /// <inheritdoc/>
        public int DefaultReelBrightness { get; set; }

        /// <inheritdoc />
        public int DefaultHomeStep => DefaultHomeStepValue;

        /// <inheritdoc/>
        public string Manufacturer => _baseName + DeviceType;
        
        /// <inheritdoc/>
        public string Model => _baseName + DeviceType;
        
        /// <inheritdoc/>
        public string Firmware => _baseName;
        
        /// <inheritdoc/>
        public string SerialNumber => _baseName;
        
        /// <inheritdoc/>
        public bool IsOpen { get; set; }
        
        /// <inheritdoc/>
        public int VendorId { get; set; }
        
        /// <inheritdoc/>
        public int ProductId { get; set; }
        
        /// <inheritdoc/>
        public int ProductIdDfu { get; set; }
        
        /// <inheritdoc/>
        public string Protocol => "FakeRelm";
        
        /// <inheritdoc/>
        public DeviceType DeviceType { get; set; }
        
        /// <inheritdoc/>
        public IDevice Device { get; set; }
        
        /// <inheritdoc/>
        public string FirmwareVersion => $"{_baseName}1.0";
        
        /// <inheritdoc/>
        public int FirmwareCrc => -1;

        /// <inheritdoc/>
        public bool IsDfuCapable => true;
        
        /// <inheritdoc/>
        public bool InDfuMode { get; private set; }

        /// <inheritdoc/>
        public bool CanDownload { get; } = false;
        
        /// <inheritdoc/>
        public bool IsDownloadInProgress { get; } = false;
        
        /// <inheritdoc/>
        public IReadOnlyCollection<AnimationFile> AnimationFiles => new List<AnimationFile>();

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

        /// <inheritdoc/>
        public Task RequestDeviceStatuses()
        {
            var statuses = new List<ReelStatus>();

            for (var i = 1; i <= ReelCount; i++)
            {
                statuses.Add(new ReelStatus{ ReelId = i, Connected = true });
            }

            ReelStatusReceived?.Invoke(this, new ReelStatusReceivedEventArgs(statuses));
            return Task.CompletedTask;
        }

        public Task<bool> RemoveAllControllerAnimations(CancellationToken token = default)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            _eventBus?.UnsubscribeAll(this);
        }

        /// <inheritdoc/>
        public bool Close()
        {
            Logger.Debug($"Closing Simulator.");
            Execute.OnUIThread(() =>
            {
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

            Execute.OnUIThread(
            () =>
            {
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
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token)
        {
            Thread.Sleep(500);
            return Task.FromResult(true);
        }
        
        /// <inheritdoc/>
        public Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, IProgress<LoadingAnimationFileModel> progress, CancellationToken token)
        {
            Thread.Sleep(500);
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> PrepareAnimation(LightShowData file, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> PrepareAnimations(IEnumerable<LightShowData> files, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> PrepareAnimation(ReelCurveData file, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> files, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> PlayAnimations(CancellationToken token)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowData> data, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> StopAllLightShows(CancellationToken token)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> StopAllAnimationTags(string animationName, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SetBrightness(int brightness)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> Synchronize(ReelSynchronizationData syncData, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task Initialize()
        {
            //prevent a double subscription for the FakeDeviceConnectedEvent in the case of Open->Close->Open
            _eventBus?.UnsubscribeAll(this);

            // allow the injection of Relm reel errors from the Test Tool
            _eventBus?.Subscribe<TestToolRelmReelErrorEvent>(this, InjectTestToolInterrupt);

            if (!Open() && !IsOpen)
            {
                return Task.CompletedTask;
            }

            Logger.Debug($"There are {ReelCount} reels");

            _reelOffsets = new int[ReelCount];
            RequestDeviceStatuses();

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

        private void OnDeviceAttached()
        {
            var invoker = DeviceAttached;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeviceDetached()
        {
            var invoker = DeviceDetached;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        private void OnDownloadProgressed()
        {
            var invoker = DownloadProgressed;
            invoker?.Invoke(this, new ProgressEventArgs(0));
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

        private void InjectTestToolInterrupt(TestToolRelmReelErrorEvent evt)
        {
            if (evt.ReelStatus is not null)
            {
                ReelStatusReceived?.Invoke(this, new ReelStatusReceivedEventArgs(evt.ReelStatus));
            }

            if (evt.LightStatus is not null)
            {
                LightStatusReceived?.Invoke(this, new LightEventArgs(evt.LightStatus));
            }

            if (evt.PingTimeout)
            {
                ControllerFaultOccurred?.Invoke(
                    this,
                    new ReelControllerFaultedEventArgs(ReelControllerFaults.CommunicationError));
            }

            if (evt.ClearPingTimeout)
            {
                ControllerFaultCleared?.Invoke(
                    this,
                    new ReelControllerFaultedEventArgs(ReelControllerFaults.CommunicationError));
            }

            if (evt.IsEventQueueFull)
            {
                Logger.Debug("The event queue is almost full");
            }
        }
    }
}
