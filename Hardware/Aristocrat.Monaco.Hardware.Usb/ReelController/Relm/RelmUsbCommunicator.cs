namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using Common;
    using Contracts.Communicator;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.SharedDevice;
    using log4net;
    using RelmReels;
    using RelmReels.Communicator;
    using RelmReels.Messages;
    using RelmReels.Messages.Commands;
    using RelmReels.Messages.Queries;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AnimationFile = Contracts.Reel.ControlData.AnimationFile;
    using DeviceConfiguration = RelmReels.Messages.Queries.DeviceConfiguration;
    using IRelmCommunicator = Contracts.Communicator.IRelmCommunicator;
    using ReelStatus = Contracts.Reel.ReelStatus;
    using RelmAnimationData = RelmReels.Messages.Commands.AnimationData;
    using RelmReelStatus = RelmReels.Messages.ReelStatus;

    internal class RelmUsbCommunicator : IRelmCommunicator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly HashSet<AnimationFile> _animationFiles = new();

        private RelmReels.Communicator.IRelmCommunicator _relmCommunicator;
        private bool _disposed;
        private uint _firmwareSize;

        /// <summary>
        ///     Instantiates a new instance of the RelmUsbCommunicator class
        /// </summary>
        public RelmUsbCommunicator()
            : this(new RelmCommunicator(new VerificationFactory(), RelmConstants.DefaultKeepAlive))
        {
        }

        /// <summary>
        ///     Instantiates a new instance of the RelmUsbCommunicator class
        /// </summary>
        /// <param name="communicator">The Relm communicator</param>
        public RelmUsbCommunicator(RelmReels.Communicator.IRelmCommunicator communicator)
        {
            _relmCommunicator = communicator;
        }
        
#pragma warning disable 67
        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceAttached;

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceDetached;

        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;
#pragma warning restore 67

        /// <inheritdoc />
        public event EventHandler<ReelStatusReceivedEventArgs> StatusesReceived;

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

        /// <inheritdoc />
        public int VendorId { get; }

        /// <inheritdoc />
        public int ProductId { get; }

        /// <inheritdoc />
        public int ProductIdDfu { get; }

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
        public bool InDfuMode { get; }

        /// <inheritdoc />
        public bool CanDownload { get; }

        /// <inheritdoc />
        public bool IsDownloadInProgress { get; }

        /// <inheritdoc />
        public int DefaultReelBrightness { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task Initialize()
        {
            if (!IsOpen && !Open())
            {
                return;
            }

            await _relmCommunicator?.SendCommandAsync(new Reset())!;
            _relmCommunicator!.KeepAliveEnabled = true;

            await _relmCommunicator?.SendQueryAsync<RelmVersionInfo>()!;
            await _relmCommunicator?.SendQueryAsync<DeviceConfiguration>()!;

            var configuration = _relmCommunicator?.Configuration ?? new DeviceConfiguration();
            Logger.Debug($"Reel controller connected with {configuration.NumReels} reel and {configuration.NumLights} lights. {configuration}");

            var firmwareSize = await _relmCommunicator?.SendQueryAsync<FirmwareSize>()!;
            _firmwareSize = firmwareSize.Size;
            Logger.Debug($"Reel controller firmware size is {_firmwareSize}");

            RequestDeviceStatuses().FireAndForget();
            HomeReels().FireAndForget();
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
            // Implement resetting connection
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token = default)
        {
            return await LoadAnimationFiles(new[] { file }, token);
        }

        /// <inheritdoc />
        public async Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return false;
            }

            var animationFiles = files as AnimationFile[] ?? files.ToArray();
            Logger.Debug($"Downloading {animationFiles.Length} Animation files");

            foreach (var file in animationFiles)
            {
                if (_animationFiles.Contains(file))
                {
                    Logger.Debug($"Animation file already loaded: {file.Path}");
                    continue;
                }

                Logger.Debug($"Downloading Animation file: {file.Path}");

                var storedFile = await _relmCommunicator.Download(file.Path, BitmapVerification.CRC32, token);

                file.AnimationId = storedFile.FileId;
                _animationFiles.Add(file);

                Logger.Debug($"Finished downloading animation file: [{file.Path}], Name: {file.FriendlyName}, AnimationId: {file.AnimationId}");
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> PrepareAnimation(LightShowData showData, CancellationToken token = default)
        {
            return await PrepareAnimations(new[] { showData }, token);
        }

        /// <inheritdoc />
        public async Task<bool> PrepareAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return false;
            }

            foreach (var data in showData)
            {
                var animationData = new RelmAnimationData
                {
                    AnimationId = data.Id,
                    LoopCount = data.LoopCount,
                    ReelIndex = data.ReelIndex,
                    Tag = data.Tag.HashDjb2(),
                    Step = data.Step
                };

                var prepareCommand = new PrepareLightShowAnimations();
                prepareCommand.Animations.Add(animationData);

                Logger.Debug($"Preparing animation with id: {animationData.AnimationId}");
                await _relmCommunicator.SendCommandAsync(prepareCommand, token);
            }

            return true;
        }

        /// <inheritdoc />
        public Task<bool> PrepareControllerAnimation(ReelCurveData curveData, CancellationToken token = default)
        {
            // TODO: Implement prepare reel curve animation in driver and wire up here
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> PrepareControllerAnimations(IEnumerable<ReelCurveData> curveData, CancellationToken token = default)
        {
            // TODO: Implement prepare reel curve animations in driver and wire up here
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<bool> PlayAnimations(CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return false;
            }

            StartAnimations startCommand = new StartAnimations();

            Logger.Debug($"Playing prepared animations");
            await _relmCommunicator.SendCommandAsync(startCommand, token);

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAllControllerAnimations(CancellationToken token = default)
        {
            if (_relmCommunicator is null)
            {
                return false;
            }

            Logger.Debug("Removing all animation files from controller");
            _animationFiles.Clear();
            return await _relmCommunicator.SendCommandAsync(new RemoveAllAnimationFiles(), token);
        }

        /// <inheritdoc />
        public Task<bool> StopControllerLightShowAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            // TODO: Implement stop light show animations in driver and wire up here
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> StopAllLightShows(CancellationToken token = default)
        {
            _relmCommunicator.SendCommandAsync(new StopAllLightShowAnimations(), token);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> PrepareControllerStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token = default)
        {
            // TODO: Implement prepare stop reels in driver and wire up here
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> PrepareControllerNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token = default)
        {
            // TODO: Implement prepare nudge reels in driver and wire up here
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> Synchronize(ReelSynchronizationData data, CancellationToken token = default)
        {
            // TODO: Implement synchronize in driver and wire up here
            throw new NotImplementedException();
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

            _relmCommunicator?.SendCommandAsync(new SetBrightness { Brightness = (byte)brightness});
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
        public Task<bool> HomeReels()
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            // TODO: Use proper home positions and number of reels
            var defaultHomeStep = 0;
            var homeData = new List<short>();
            
            for (int i=0; i < ReelCount; i++)
            {
                homeData.Add((short)defaultHomeStep);
            }

            _relmCommunicator?.SendCommandAsync(new HomeReels(homeData));
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            _relmCommunicator?.SendCommandAsync(new HomeReels(new List<ReelStepInfo> { new ((byte)reelId, (short)stop) }));
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            // TODO: Implement this
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> TiltReels()
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            // TODO: Add this back in when all of the statuses for enabling/disabling work correctly.
            //_relmCommunicator.SendCommandAsync(new TiltReelController());
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task RequestDeviceStatuses()
        {
            if (_relmCommunicator is null)
            {
                return;
            }

            var deviceStatuses = await _relmCommunicator.SendQueryAsync<DeviceStatuses>();
            var statuses = deviceStatuses.ReelStatuses.Select(ConvertToReelStatus).ToList();

            StatusesReceived?.Invoke(this, new ReelStatusReceivedEventArgs(statuses));
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                var communicator = _relmCommunicator;
                _relmCommunicator = null;
                communicator.Close();
                communicator.Dispose();
            }

            _disposed = true;
        }

        private ReelStatus ConvertToReelStatus(DeviceStatus<RelmReelStatus> deviceReelStatus)
        {
            return new ReelStatus
            {
                ReelId = deviceReelStatus.Id + 1,
                Connected = deviceReelStatus.Status != RelmReelStatus.Disconnected
            };
        }
    }
}
