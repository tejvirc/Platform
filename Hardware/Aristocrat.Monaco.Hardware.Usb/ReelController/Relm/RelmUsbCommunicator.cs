namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Common.Cryptography;
    using Contracts;
    using Contracts.Cabinet;
    using Contracts.Communicator;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using RelmReels;
    using RelmReels.Communicator;
    using RelmReels.Communicator.InterruptHandling;
    using RelmReels.Messages;
    using RelmReels.Messages.Commands;
    using RelmReels.Messages.Interrupts;
    using RelmReels.Messages.Queries;
    using AnimationFile = Contracts.Reel.ControlData.AnimationFile;
    using DeviceConfiguration = RelmReels.Messages.Queries.DeviceConfiguration;
    using IRelmCommunicator = Contracts.Communicator.IRelmCommunicator;
    using MonacoLightStatus = Contracts.Reel.LightStatus;
    using MonacoReelStatus = Contracts.Reel.ReelStatus;

    internal class RelmUsbCommunicator : IRelmCommunicator
    {
        private const ReelControllerFaults PingTimeoutFault = ReelControllerFaults.CommunicationError;
        private const int DefaultHomeStepValue = 5;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPropertiesManager _propertiesManager;
        private readonly IEventBus _eventBus;
        private readonly HashSet<AnimationFile> _animationFiles = new();
        private readonly ConcurrentDictionary<string, uint> _tags = new();

        private RelmReels.Communicator.IRelmCommunicator _relmCommunicator;
        private bool _disposed;
        private uint _firmwareSize;
        private int[] _reelOffsets = { 0, 0, 0, 0, 0, 0 };
        private bool _initialized;
        private bool _resetting;

        /// <summary>
        ///     Instantiates a new instance of the RelmUsbCommunicator class
        /// </summary>
        public RelmUsbCommunicator()
            : this(new RelmCommunicator(new VerificationFactory(),
                RelmConstants.DefaultKeepAlive),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
            _relmCommunicator = new RelmCommunicator(new VerificationFactory(), RelmConstants.DefaultKeepAlive);
        }

        /// <summary>
        ///     Instantiates a new instance of the RelmUsbCommunicator class
        /// </summary>
        /// <param name="communicator">The Relm communicator</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="eventBus">The event bus</param>
        public RelmUsbCommunicator(RelmReels.Communicator.IRelmCommunicator communicator, IPropertiesManager propertiesManager, IEventBus eventBus)
        {
            _relmCommunicator = communicator;
            _propertiesManager = propertiesManager;
            _eventBus = eventBus;
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceAttached;

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceDetached;

#pragma warning disable 67
        // TODO: Wire up DFU events
        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;
#pragma warning restore 67

        /// <inheritdoc />
        public event EventHandler<LightEventArgs> LightStatusReceived;

        /// <inheritdoc />
        public event EventHandler<ReelStatusReceivedEventArgs> ReelStatusReceived;

        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultOccurred;

        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultCleared;

        /// <inheritdoc />
        public event EventHandler<ReelSpinningEventArgs> ReelSpinningStatusReceived;

        /// <inheritdoc />
        public event EventHandler<ReelStoppingEventArgs> ReelStopping;

        /// <inheritdoc />
        public event EventHandler<StepperRuleTriggeredEventArgs> StepperRuleTriggered;

        /// <inheritdoc />
        public event EventHandler<ReelSynchronizationEventArgs> SynchronizationStarted;

        /// <inheritdoc />
        public event EventHandler<ReelSynchronizationEventArgs> SynchronizationCompleted;

        /// <inheritdoc />
        public event EventHandler AllLightAnimationsCleared;

        /// <inheritdoc />
        public event EventHandler<LightAnimationEventArgs> LightAnimationRemoved;

        /// <inheritdoc />
        public event EventHandler<LightAnimationEventArgs> LightAnimationStarted;

        /// <inheritdoc />
        public event EventHandler<LightAnimationEventArgs> LightAnimationStopped;

        /// <inheritdoc />
        public event EventHandler<LightAnimationEventArgs> LightAnimationPrepared;

        /// <inheritdoc />
        public event EventHandler<ReelAnimationEventArgs> ReelAnimationStarted;

        /// <inheritdoc />
        public event EventHandler<ReelAnimationEventArgs> ReelAnimationStopped;

        /// <inheritdoc />
        public event EventHandler<ReelAnimationEventArgs> ReelAnimationPrepared;

        /// <inheritdoc />
        public IReadOnlyCollection<AnimationFile> AnimationFiles => _animationFiles.ToList();

        /// <summary>
        ///     The reel count
        /// </summary>
        public int ReelCount => _relmCommunicator?.Configuration.NumReels ?? 0;

        /// <inheritdoc />
        public string Manufacturer => _relmCommunicator?.Manufacturer;

        /// <inheritdoc />
        public string Model => _relmCommunicator?.DeviceDescription;

        /// <inheritdoc />
        public string Firmware => string.Empty;

        /// <inheritdoc />
        public string SerialNumber => _relmCommunicator?.SerialNumber;

        /// <inheritdoc />
        public bool IsOpen => _relmCommunicator?.IsOpen ?? false;

        // TODO: Wire this up
        /// <inheritdoc />
        public int VendorId => 0;

        // TODO: Wire this up
        /// <inheritdoc />
        public int ProductId => 0;

        // TODO: Wire this up
        /// <inheritdoc />
        public int ProductIdDfu => 0;

        /// <inheritdoc />
        public string Protocol { get; private set; }

        /// <inheritdoc />
        public DeviceType DeviceType { get; set; } = DeviceType.ReelController;

        /// <inheritdoc />
        public IDevice Device { get; set; }

        /// <inheritdoc />
        public string FirmwareVersion =>
            $"{_relmCommunicator?.ControllerVersion.Software.Major}.{_relmCommunicator?.ControllerVersion.Software.Minor}.{_relmCommunicator?.ControllerVersion.Software.Mini}";

        /// <inheritdoc />
        public int FirmwareCrc => 0xFFFF; // TODO: Calculate actual CRC

        /// <inheritdoc />
        public bool IsDfuCapable => true;

        /// <inheritdoc />
        public bool InDfuMode => false;

        /// <inheritdoc />
        public bool CanDownload => false;

        /// <inheritdoc />
        public bool IsDownloadInProgress => false;

        /// <inheritdoc />
        public int DefaultReelBrightness { get; set; }

        /// <inheritdoc />
        public int DefaultHomeStep => DefaultHomeStepValue;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task Initialize()
        {
            _initialized = false;
            if (_relmCommunicator is not null)
            {
                _relmCommunicator.InterruptReceived += OnInterruptReceived;
                _relmCommunicator.PingTimeoutCleared += OnPingTimeoutCleared;
                _relmCommunicator.DeviceDetached += OnDeviceDetached;

                _eventBus.Subscribe<DeviceConnectedEvent>(this, HandleDeviceConnected);

                await InnerOpen(true);
            }

            _initialized = true;
        }

        /// <inheritdoc />
        public bool Close()
        {
            if (_relmCommunicator is null || !_relmCommunicator.IsOpen)
            {
                return false;
            }

            _relmCommunicator.Close();
            return !_relmCommunicator.IsOpen;
        }

        /// <inheritdoc />
        public bool Open()
        {
            if (_relmCommunicator is null || _relmCommunicator.IsOpen)
            {
                return false;
            }

            _relmCommunicator.Open(RelmConstants.ReelsAddress);
            return _relmCommunicator.IsOpen;
        }

        /// <inheritdoc />
        public bool Configure(IComConfiguration comConfiguration)
        {
            // TODO: Implement DFU & Connect/Disconnect
            Protocol = comConfiguration.Protocol;
            return true;
        }

        /// <inheritdoc />
        public void ResetConnection()
        {
            // TODO: Implement resetting connection
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token = default)
        {
            return LoadAnimationFiles(new[] { file }, null, token);
        }

        /// <inheritdoc />
        public async Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, IProgress<LoadingAnimationFileModel> progress, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return false;
            }

            var filesToLoad = new List<AnimationFile>();
            var animationFiles = files as AnimationFile[] ?? files.ToArray();
            var loadingAnimationFileModel = new LoadingAnimationFileModel();
            var (animationIdsSuccess, animationIds) = await _relmCommunicator.SendQueryAsync<StoredAnimationIds>(token);

            Logger.Debug($"Checking {animationFiles.Length} animation files to download");

            loadingAnimationFileModel.Count = 1;
            loadingAnimationFileModel.Total = animationFiles.Length;
            foreach (var file in animationFiles)
            {
                var fileName = Path.GetFileName(file.Path);
                var fileNameHash = fileName.HashDjb2();
                var fileNeedsLoaded = true;

                if (animationIdsSuccess && (animationIds?.AnimationIds.Contains(fileNameHash) ?? false))
                {
                    fileNeedsLoaded = await CheckFileIsModified(file.Path, fileNameHash, token);
                }

                loadingAnimationFileModel.Filename = fileName;

                if (!fileNeedsLoaded)
                {
                    if (!_animationFiles.Contains(file))
                    {
                        file.AnimationId = fileNameHash;
                        _animationFiles.Add(file);
                    }
                    
                    Logger.Debug(
                        $"Animation file is already present on controller. {loadingAnimationFileModel.Count} / {loadingAnimationFileModel.Total}: [{file.Path}], Name: {file.FriendlyName}, AnimationId: {file.AnimationId}");
                    loadingAnimationFileModel.Count++;
                    continue;
                }

                if (!filesToLoad.Contains(file))
                {
                    filesToLoad.Add(file);
                }
            }

            var loadSuccess = await LoadAnimationFilesInternal(filesToLoad, loadingAnimationFileModel, progress, token);

            return loadSuccess;
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimation(LightShowData showData, CancellationToken token = default)
        {
            return PrepareAnimations(new[] { showData }, token);
        }

        /// <inheritdoc />
        public async Task<bool> PrepareAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return false;
            }

            var success = true;
            var command = new PrepareLightShowAnimations();

            foreach (var show in showData)
            {
                var animationId = GetAnimationId(show.AnimationName);
                if (animationId == 0)
                {
                    Logger.Debug($"Can not find animation file with name {show.AnimationName}");
                    success = false;
                    continue;
                }

                _tags.TryAdd(show.Tag, show.Tag.HashDjb2());

                command.Animations.Add(new PrepareLightShowAnimationData
                {
                    AnimationId = animationId,
                    LoopCount = show.LoopCount,
                    ReelIndex = show.ReelIndex,
                    Tag = _tags[show.Tag],
                    Step = show.Step
                });
            }

            if (command.Animations.Count == 0)
            {
                return success;
            }

            Logger.Debug($"Preparing {command.Animations.Count} light shows");
            var result = await _relmCommunicator.SendCommandAsync(command, token);
            return result && success;
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimation(ReelCurveData curveData, CancellationToken token = default)
        {
            return PrepareAnimations(new[] { curveData }, token);
        }

        /// <inheritdoc />
        public async Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> curveData, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return false;
            }

            var success = true;
            var command = new PrepareStepperCurves();

            foreach (var curve in curveData)
            {
                var animationId = GetAnimationId(curve.AnimationName);
                if (animationId == 0)
                {
                    Logger.Debug($"Can not find animation file with name {curve.AnimationName}");
                    success = false;
                    continue;
                }

                command.Animations.Add(new PrepareStepperCurveData
                {
                    AnimationId = animationId,
                    ReelIndex = curve.ReelIndex
                });
            }

            if (command.Animations.Count == 0)
            {
                return success;
            }

            Logger.Debug($"Preparing {command.Animations.Count} curves");
            var result = await _relmCommunicator.SendCommandAsync(command, token);
            return success && result;
        }

        /// <inheritdoc />
        public Task<bool> PlayAnimations(CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            Logger.Debug("Playing prepared animations");
            return _relmCommunicator.SendCommandAsync(new StartAnimations(), token);
        }

        /// <inheritdoc />
        public Task<bool> RemoveAllControllerAnimations(CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            if (_propertiesManager is not null &&
                _propertiesManager.GetValue(HardwareConstants.DoNotResetRelmController, false))
            {
                return Task.FromResult(true);
            }

            Logger.Debug("Removing all animation files from controller");
            _animationFiles.Clear();
            return _relmCommunicator.SendCommandAsync(new RemoveAllAnimationFiles(), token);
        }

        /// <inheritdoc />
        public Task<bool> StopAllAnimationTags(string animationName, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            var animationId = GetAnimationId(animationName);
            if (animationId == 0)
            {
                Logger.Debug($"Can not find animation file with name {animationName}");
                return Task.FromResult(false);
            }

            Logger.Debug($"Stopping all animations for {animationName} ({animationId})");
            return _relmCommunicator.SendCommandAsync(new StopAllAnimationTags
            {
                AnimationId = animationId,
            }, token);
        }

        /// <inheritdoc />
        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            var showDataArray = showData.ToArray();
            var command = new StopLightShowAnimation
            {
                Count = (short)showDataArray.Length,
                AnimationData = showDataArray.Select(x => new StopAnimationData
                {
                    AnimationId = GetAnimationId(x.AnimationName),
                    TagId = x.Tag.HashDjb2()
                }).ToList()
            };

            return _relmCommunicator.SendCommandAsync(command, token);
        }

        /// <inheritdoc />
        public Task<bool> StopAllLightShows(CancellationToken token = default)
        {
            _relmCommunicator.SendCommandAsync(new StopAllLightShowAnimations(), token);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token = default)
        {
            foreach (var data in stopData)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                byte reel = data.ReelIndex;

                var command = new PrepareStopReel
                {
                    ReelIndex = reel,
                    Duration = data.Duration,
                    Step = (short)(data.Step + _reelOffsets[reel])
                };

                _relmCommunicator.SendCommandAsync(command, token);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return false;
            }

            Logger.Debug("Nudging reels");
            foreach (var nudge in nudgeData)
            {
                await _relmCommunicator.SendCommandAsync(new PrepareNudgeReel
                {
                    ReelIndex = (byte)nudge.ReelId,
                    Speed = (short)(nudge.Rpm * (nudge.Direction == SpinDirection.Forward ? 1 : -1)),
                    StepCount = (short)nudge.Step
                }, token);
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> Synchronize(ReelSynchronizationData syncData, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return false;
            }

            if (syncData.SyncType == SynchronizeType.Regular)
            {
                var syncDataList = syncData.ReelSyncStepData.Select(
                    x => new ReelSyncData { ReelIndex = x.ReelIndex, SynchronizationStep = x.SyncStep });

                await _relmCommunicator.SendCommandAsync(new PrepareSynchronizeReels(syncData.Duration, syncDataList), token);
            }
            else if (syncData.SyncType == SynchronizeType.Enhanced)
            {
                var syncDataList = syncData.ReelSyncStepData.Select(
                    x => new EnhancedReelSyncData { ReelIndex = x.ReelIndex, SyncStep = x.SyncStep, Duration = x.Duration });

                await _relmCommunicator.SendCommandAsync(
                    new PrepareEnhancedSynchronizeReels(
                        syncData.MasterReelIndex,
                        syncData.MasterReelStep,
                        syncDataList),
                    token);
            }

            return true;
        }

        /// <inheritdoc />
        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            return brightness.Count < 1 ? Task.FromResult(false) : SetBrightness(brightness[0]);
        }

        /// <inheritdoc />
        public Task<bool> SetBrightness(int brightness)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            _relmCommunicator?.SendCommandAsync(new SetBrightness { Brightness = (byte)brightness });
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> EnterDfuMode()
        {
            // TODO: Implement enter DFU in driver and wire up
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> ExitDfuMode()
        {
            // TODO: Implement exit DFU in driver and wire up
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<DfuStatus> Download(Stream firmware)
        {
            // TODO: Implement DFU download in driver and wire up
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void AbortDownload()
        {
            // TODO: Implement abort DFU download in driver and wire up
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> HaltReels()
        {
            return _relmCommunicator.SendCommandAsync(new HaltReels());
        }

        /// <inheritdoc />
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            byte reel = (byte)(reelId - 1);
            var reelStepInfo = new ReelStepInfo(reel, (short)(stop + _reelOffsets[reel]));

            _relmCommunicator?.SendCommandAsync(new HomeReels(new List<ReelStepInfo> { reelStepInfo }));
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            _reelOffsets = offsets;

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> TiltReels()
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            _relmCommunicator.SendCommandAsync(new TiltReelController());
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task RequestDeviceStatuses()
        {
            if (_relmCommunicator is null)
            {
                return;
            }

            var (success, deviceStatuses) = await _relmCommunicator.SendQueryAsync<DeviceStatuses>();
            if (!success)
            {
                return;
            }

            if (deviceStatuses!.ReelStatuses.Any())
            {
                var reelStatuses = deviceStatuses.ReelStatuses.Select(x => x.ToReelStatus());
                ReelStatusReceived?.Invoke(this, new ReelStatusReceivedEventArgs(reelStatuses));
            }

            if (deviceStatuses.LightStatuses.Any())
            {
                var lightStatuses = deviceStatuses.LightStatuses.Select(x => x.ToLightStatus());
                LightStatusReceived?.Invoke(this, new LightEventArgs(lightStatuses));
            }
        }

        /// <inheritdoc/>
        public Task<bool> PrepareStepperRule(StepperRuleData ruleData, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            RelmCommand command = ruleData.RuleType switch
            {
                StepperRuleType.AnticipationRule => new PrepareStepperAnticipationRule
                {
                    ReelIndex = ruleData.ReelIndex,
                    StepToFollow = ruleData.StepToFollow,
                    ReferenceStep = ruleData.ReferenceStep,
                    Cycle = ruleData.Cycle,
                    Delta = ruleData.Delta,
                    EventId = ruleData.EventId
                },
                StepperRuleType.FollowRule => new PrepareStepperFollowRule
                {
                    ReelIndex = ruleData.ReelIndex,
                    StepToFollow = ruleData.StepToFollow,
                    ReferenceStep = ruleData.ReferenceStep,
                    Cycle = ruleData.Cycle,
                    EventId = ruleData.EventId
                },
                _ => null
            };

            return command == null
                ? Task.FromResult(false)
                : _relmCommunicator.SendCommandAsync(command, token);
        }

        internal virtual byte[] HashAnimationFile(string filePath)
        {
            using var streamReader = new StreamReader(filePath);
            using var crc32 = new Crc32();
            var hash = crc32.ComputeHash(streamReader.BaseStream).Reverse().ToArray();

            Logger.Info($"Calculated File Hash for {filePath}: {string.Join(", ", hash.Take(hash.Length))}");

            return hash;
        }

        private async Task<bool> LoadAnimationFilesInternal(
            IEnumerable<AnimationFile> animationFiles,
            LoadingAnimationFileModel loadingAnimationFileModel,
            IProgress<LoadingAnimationFileModel> progress,
            CancellationToken token = default)
        {
            foreach (var file in animationFiles)
            {
                loadingAnimationFileModel.Filename = Path.GetFileName(file.Path);
                ReportLoadingAnimationProgress(LoadingAnimationState.Loading);
                var loadProgressText = $"{loadingAnimationFileModel.Count} / {loadingAnimationFileModel.Total}";
                Logger.Debug($"Downloading animation file {loadProgressText} {file.Path}");
                try
                {
                    var storedFile = await _relmCommunicator.Download(file.Path, BitmapVerification.CRC32, token);

                    if (!storedFile.Success)
                    {
                        Logger.Error($"Failed to load animation file {loadProgressText} {loadingAnimationFileModel.Filename}");
                        ReportLoadingAnimationProgress(LoadingAnimationState.Error);
                        return false;
                    }

                    Logger.Debug($"Finished downloading animation file {loadProgressText}: [{file.Path}], Name: {file.FriendlyName}, AnimationId: {file.AnimationId}");
                    file.AnimationId = storedFile.FileId;
                    loadingAnimationFileModel.Count++;
                    _animationFiles.Add(file);
                }
                catch (Exception e)
                {
                    Logger.Error($"Error while loading animation file {loadProgressText} {loadingAnimationFileModel.Filename} : {e}");
                    ReportLoadingAnimationProgress(LoadingAnimationState.Error);
                    return false;
                }
            }

            ReportLoadingAnimationProgress(LoadingAnimationState.Completed);

            return true;

            void ReportLoadingAnimationProgress(LoadingAnimationState state)
            {
                loadingAnimationFileModel.State = state;
                progress?.Report(loadingAnimationFileModel);
            }
        }

        private async Task<bool> CheckFileIsModified(string filePath, uint fileNameHash, CancellationToken token = default)
        {
            var animationHashCompleted = await CalculateAnimationHashOnDevice(fileNameHash, token);
            if (!animationHashCompleted.Success)
            {
                //If we fail to hash for some reason assume the files are not the same
                return false;
            }

            var hashFromController = animationHashCompleted.Response?.Hash ?? Array.Empty<byte>();
            Logger.Info($"File hash from controller {filePath}: {string.Join(", ", hashFromController.Take(hashFromController.Length))}");
            var areFilesDifferent = !HashAnimationFile(filePath).SequenceEqual(hashFromController);

            Logger.Debug($"Animation file {filePath} was modified: {areFilesDifferent}");

            return areFilesDifferent;
        }

        private async Task<RelmResponse<AnimationHashCompleted>> CalculateAnimationHashOnDevice(uint animationId, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return null;
            }

            var animationHashCompleted = await _relmCommunicator.SendCommandWithResponseAsync(new CalculateAnimationHash
            {
                AnimationId = animationId,
                Verification = BitmapVerification.CRC32
            }, token);

            return animationHashCompleted;
        }

        private void OnInterruptReceived(object sender, RelmInterruptEventArgs e)
        {
            switch (e.Interrupt)
            {
                case IReelInterrupt reelInterrupt:
                    HandleReelInterrupt(reelInterrupt);
                    break;

                case ILightInterrupt lightInterrupt:
                    var lightStatus = new MonacoLightStatus((int)lightInterrupt.LightId, lightInterrupt is LightMalfunction);
                    var args = new LightEventArgs(lightStatus);
                    LightStatusReceived?.Invoke(this, args);
                    break;

                case PingTimeout:
                    ControllerFaultOccurred?.Invoke(
                        this,
                        new ReelControllerFaultedEventArgs(PingTimeoutFault));
                    break;

                case LightShowAnimationStarted started:
                    LightAnimationStarted?.Invoke(
                        this,
                        new LightAnimationEventArgs(GetAnimationName(started.AnimationId), GetTag(started.TagId)));
                    break;

                case LightShowAnimationStopped stopped:
                    LightAnimationStopped?.Invoke(
                        this,
                        new LightAnimationEventArgs(
                            GetAnimationName(stopped.AnimationId),
                            GetTag(stopped.TagId),
                            stopped.LightShowAnimationQueueLocation.ToAnimationQueueType()));
                    break;

                case LightShowAnimationsPrepared lightsPrepared:
                    if (lightsPrepared.Animations is null)
                    {
                        break;
                    }

                    foreach (var prepared in lightsPrepared.Animations)
                    {
                        LightAnimationPrepared?.Invoke(
                            this,
                            new LightAnimationEventArgs(
                                GetAnimationName(prepared.AnimationId),
                                GetTag(prepared.TagId),
                                prepared.PreparedStatus.ToAnimationPreparedStatus()));
                    }
                    break;

                case AllLightShowsCleared:
                    AllLightAnimationsCleared?.Invoke(this, EventArgs.Empty);
                    break;

                case LightShowAnimationRemoved showRemoved:
                    LightAnimationRemoved?.Invoke(
                        this,
                        new LightAnimationEventArgs(
                            GetAnimationName(showRemoved.AnimationId),
                            showRemoved.QueueLocation.ToAnimationQueueType()));
                    break;

                case ReelAnimationsPrepared reelsPrepared:
                    if (reelsPrepared.Animations is null)
                    {
                        break;
                    }

                    foreach (var prepared in reelsPrepared.Animations)
                    {
                        ReelAnimationPrepared?.Invoke(
                            this,
                            new ReelAnimationEventArgs(
                                GetAnimationName(prepared.AnimationId),
                                prepared.PreparedStatus.ToAnimationPreparedStatus()));
                    }
                    break;
            }
        }

        private void HandleReelInterrupt(IReelInterrupt interrupt)
        {
            var reelIndex = interrupt.ReelIndex + 1;
            switch (interrupt)
            {
                case ReelIdle idle:
                    RaiseReelSpinningStatusUpdated(
                        ReelSpinningEventArgs.CreateForIdleAtStep(reelIndex, idle.Step));
                    break;

                case ReelAccelerating:
                    RaiseReelSpinningStatusUpdated(
                        new ReelSpinningEventArgs(reelIndex, SpinVelocity.Accelerating));
                    break;

                case ReelDecelerating:
                    RaiseReelSpinningStatusUpdated(
                        new ReelSpinningEventArgs(reelIndex, SpinVelocity.Decelerating));
                    break;

                case ReelSpinningConstant:
                    RaiseReelSpinningStatusUpdated(
                        new ReelSpinningEventArgs(reelIndex, SpinVelocity.Constant));
                    break;

                case ReelSlowSpin:
                    RaiseReelSpinningStatusUpdated(ReelSpinningEventArgs.CreateForSlowSpinning(reelIndex));
                    break;

                case ReelTamperingDetected:
                    RaiseStatus(x => x.ReelTampered = true);
                    break;

                case ReelStalled:
                    RaiseStatus(x => x.ReelStall = true);
                    break;

                case ReelOpticSequenceError:
                    RaiseStatus(x => x.OpticSequenceError = true);
                    break;

                case ReelDisconnected:
                    RaiseStatus(x => x.Connected = false);
                    break;

                case ReelIdleUnknown:
                    RaiseStatus(x => x.IdleUnknown = true);
                    break;

                case ReelUnknownStopReceived:
                    RaiseStatus(x => x.UnknownStop = true);
                    break;

                case ReelPlayingAnimation playing:
                    ReelAnimationStarted?.Invoke(
                        this,
                        new ReelAnimationEventArgs(playing.ReelIndex, GetAnimationName(playing.AnimationId)));
                    break;

                case ReelFinishedAnimation finished:
                    ReelAnimationStopped?.Invoke(
                        this,
                        new ReelAnimationEventArgs(finished.ReelIndex, GetAnimationName(finished.AnimationId)));
                    break;

                case UserSpecifiedInterrupt userInterrupt:
                    StepperRuleTriggered?.Invoke(this, new StepperRuleTriggeredEventArgs(userInterrupt.ReelIndex, userInterrupt.EventId));
                    break;

                case ReelSyncStarted syncStarted:
                    SynchronizationStarted?.Invoke(this, new ReelSynchronizationEventArgs(syncStarted.ReelIndex));
                    break;

                case ReelSynchronized synchronized:
                    SynchronizationCompleted?.Invoke(this, new ReelSynchronizationEventArgs(synchronized.ReelIndex));
                    break;

                case ReelIdleTimeCalculated idleTime:
                    ReelStopping?.Invoke(this, new ReelStoppingEventArgs(idleTime.ReelIndex, idleTime.StopTime));
                    break;
            }

            void RaiseStatus(Action<MonacoReelStatus> configure)
            {
                var status = new MonacoReelStatus
                {
                    ReelId = interrupt.ReelIndex + 1,
                    Connected = interrupt is not ReelDisconnected
                };

                configure(status);
                ReelStatusReceived?.Invoke(this, new ReelStatusReceivedEventArgs(status));
            }

            void RaiseReelSpinningStatusUpdated(ReelSpinningEventArgs eventArgs)
            {
                RaiseStatus(x => x.Connected = true);   // receiving a reel spin status means the reel must be connected
                ReelSpinningStatusReceived?.Invoke(this, eventArgs);
            }
        }

        private void OnPingTimeoutCleared(object sender, EventArgs e)
        {
            ControllerFaultCleared?.Invoke(this, new ReelControllerFaultedEventArgs(PingTimeoutFault));
        }

        private void Dispose(bool disposing)
        {
            if (_disposed || !disposing)
            {
                return;
            }

            var communicator = _relmCommunicator;
            if (communicator is not null)
            {
                communicator.InterruptReceived -= OnInterruptReceived;
                communicator.PingTimeoutCleared -= OnPingTimeoutCleared;
                communicator.DeviceDetached -= OnDeviceDetached;

                _relmCommunicator = null;
                communicator.Close();
                communicator.Dispose();
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }

        private uint GetAnimationId(string animationName)
        {
            return AnimationFiles.FirstOrDefault(x => x.FriendlyName == animationName)?.AnimationId ?? 0;
        }

        private string GetAnimationName(uint animationId)
        {
            return AnimationFiles.FirstOrDefault(x => x.AnimationId == animationId)?.FriendlyName ?? string.Empty;
        }

        private string GetTag(uint tagId)
        {
            return _tags.FirstOrDefault(x => x.Value == tagId).Key ?? string.Empty;
        }

        private void HandleDeviceConnected(DeviceConnectedEvent evt)
        {
            if (_initialized && !_resetting &&
                evt.IsForVidPid(RelmConstants.VendorId, RelmConstants.ProductId))
            {
                InnerOpen(false).FireAndForget(Logger.Error);
            }
        }

        private async Task InnerOpen(bool initializing)
        {
            if (!IsOpen && !Open())
            {
                return;
            }

            if (initializing &&
                !_propertiesManager.GetValue(HardwareConstants.DoNotResetRelmController, false))
            {
                _resetting = true;
                await _relmCommunicator?.SendCommandAsync(new Reset())!;
                _resetting = false;

                // the reset failed and the connection could not be reopened
                if (!_relmCommunicator.IsOpen)
                {
                    OnDeviceDetached(this, EventArgs.Empty);
                }
            }

            _relmCommunicator!.KeepAliveEnabled = true;

            await _relmCommunicator?.SendQueryAsync<RelmVersionInfo>()!;
            var (success, configuration) = await _relmCommunicator?.SendQueryAsync<DeviceConfiguration>()!;
            if (success)
            {
                Logger.Debug($"Reel controller connected with {configuration!.NumReels} reel and {configuration.NumLights} lights. {configuration}");
            }

            var (firmwareSuccess, firmwareSize) = await _relmCommunicator?.SendQueryAsync<FirmwareSize>()!;
            if (firmwareSuccess)
            {
                _firmwareSize = firmwareSize!.Size;
                Logger.Debug($"Reel controller firmware size is {_firmwareSize}");
            }

            await RequestDeviceStatuses();
            DeviceAttached?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeviceDetached(object sender, EventArgs e)
        {
            if (!_resetting)
            {
                DeviceDetached?.Invoke(sender, e);
            }
        }
    }
}
