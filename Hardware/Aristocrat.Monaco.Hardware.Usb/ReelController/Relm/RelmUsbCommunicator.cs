﻿namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using Contracts.Communicator;
    using Contracts.Reel.ControlData;
    using Contracts.SharedDevice;
    using log4net;
    using RelmReels;
    using RelmReels.Communicator;
    using RelmReels.Messages.Commands;
    using RelmReels.Messages.Queries;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using DeviceConfiguration = RelmReels.Messages.Queries.DeviceConfiguration;
    using IRelmCommunicator = Contracts.Communicator.IRelmCommunicator;

    internal class RelmUsbCommunicator : IRelmCommunicator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private bool _disposed;
        private RelmCommunicator _relmCommunicator;
        private uint _firmwareSize;

        public RelmUsbCommunicator()
        {
            _relmCommunicator = new RelmCommunicator(new VerificationFactory());
        }
        
#pragma warning disable 67
        public event EventHandler<EventArgs> DeviceAttached;

        public event EventHandler<EventArgs> DeviceDetached;

        public event EventHandler<ProgressEventArgs> DownloadProgressed;
#pragma warning restore 67

        public int ReelCount => _relmCommunicator?.Configuration.NumReels ?? 0;

        public string Manufacturer => _relmCommunicator?.Manufacturer;

        public string Model => _relmCommunicator?.DeviceDescription;

        public string Firmware => string.Empty;

        public string SerialNumber => _relmCommunicator?.SerialNumber;

        public bool IsOpen => _relmCommunicator?.IsOpen ?? false;

        public int VendorId { get; }

        public int ProductId { get; }

        public int ProductIdDfu { get; }

        public string Protocol { get; private set; }

        public DeviceType DeviceType { get; set; } = DeviceType.ReelController;

        public IDevice Device { get; set; }

        public string FirmwareVersion =>
            $"{_relmCommunicator?.ControllerVersion.Software.Major}.{_relmCommunicator?.ControllerVersion.Software.Minor}.{_relmCommunicator?.ControllerVersion.Software.Mini}";

        public int FirmwareCrc => 0xFFFF; // TODO: Calculate actual CRC

        public bool IsDfuCapable => true;

        public bool InDfuMode { get; }

        public bool CanDownload { get; }

        public bool IsDownloadInProgress { get; }

        public int DefaultReelBrightness { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Initialize()
        {
            if (!IsOpen && !Open())
            {
                return;
            }

            await _relmCommunicator?.SendCommandAsync(new Reset())!;
            await _relmCommunicator?.SendQueryAsync<RelmVersionInfo>()!;
            await _relmCommunicator?.SendQueryAsync<DeviceConfiguration>()!;
            await HomeReels();

            var configuration = _relmCommunicator?.Configuration ?? new DeviceConfiguration();
            Logger.Debug($"Reel controller connected with {configuration.NumReels} reel and {configuration.NumLights} lights. {configuration}");

            var firmwareSize = await _relmCommunicator?.SendQueryAsync<FirmwareSize>()!;
            _firmwareSize = firmwareSize.Size;
            Logger.Debug($"Reel controller firmware size is {_firmwareSize}");
        }

        public bool Close()
        {
            if (_relmCommunicator is null || !_relmCommunicator.IsOpen)
            {
                return false;
            }
            
            _relmCommunicator.Close();
            return !_relmCommunicator.IsOpen;
        }

        public bool Open()
        {
            if (_relmCommunicator is null || _relmCommunicator.IsOpen)
            {
                return false;
            }
            
            _relmCommunicator.Open(RelmConstants.ReelsAddress);
            return _relmCommunicator.IsOpen;
        }

        public bool Configure(IComConfiguration comConfiguration)
        {
            // TODO: Implement DFU & Connect/Disconnect
            Protocol = comConfiguration.Protocol;
            return true;
        }

        public void ResetConnection()
        {
            // Implement resetting connection
            throw new NotImplementedException();
        }

        public Task<bool> LoadControllerAnimationFile(AnimationFile file, CancellationToken token)
        {
            // TODO: Implement loading of animation file in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> LoadControllerAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token)
        {
            // TODO: Implement loading of animation files in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimation(LightShowFile file, CancellationToken token)
        {
            // TODO: Implement prepare light show animation in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            // TODO: Implement prepare light show animations in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimation(ReelCurveData curveData, CancellationToken token)
        {
            // TODO: Implement prepare reel curve animation in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimations(IEnumerable<ReelCurveData> files, CancellationToken token)
        {
            // TODO: Implement prepare reel curve animations in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> PlayControllerAnimations(CancellationToken token)
        {
            // TODO: Implement play animations in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> StopControllerLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            // TODO: Implement stop light show animations in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> StopAllControllerLightShows(CancellationToken token)
        {
            // TODO: Implement stop all light show animations in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token)
        {
            // TODO: Implement prepare stop reels in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token)
        {
            // TODO: Implement prepare nudge reels in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> Synchronize(ReelSynchronizationData data, CancellationToken token)
        {
            // TODO: Implement synchronize in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            // TODO: Implement set brightness in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> SetBrightness(int brightness)
        {
            // TODO: Implement set brightness in driver and wire up here
            throw new NotImplementedException();
        }

        public Task<bool> EnterDfuMode()
        {
            // TODO: Implement enter DFU in driver and wire up
            throw new NotImplementedException();
        }

        public Task<bool> ExitDfuMode()
        {
            // TODO: Implement exit DFU in driver and wire up
            throw new NotImplementedException();
        }

        public Task<DfuStatus> Download(Stream firmware)
        {
            // TODO: Implement DFU download in driver and wire up
            throw new NotImplementedException();
        }

        public void AbortDownload()
        {
            // TODO: Implement abort DFU download in driver and wire up
            throw new NotImplementedException();
        }

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

        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            if (_relmCommunicator is null)
            {
                return Task.FromResult(false);
            }

            _relmCommunicator?.SendCommandAsync(new HomeReels(new List<ReelStepInfo> { new ((byte)reelId, (short)stop) }));
            return Task.FromResult(true);
        }

        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            // TODO: Implement this
            return Task.FromResult(true);
        }

        public Task<bool> TiltReels()
        {
            // TODO: Implement tilt reels in driver and wire up
            throw new NotImplementedException();
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
    }
}
