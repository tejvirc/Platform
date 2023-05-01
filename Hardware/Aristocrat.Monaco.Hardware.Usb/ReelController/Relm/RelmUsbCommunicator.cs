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

        public RelmUsbCommunicator()
        {
            _relmCommunicator = new RelmCommunicator(new VerificationFactory());

            if (!IsOpen)
            {
                Open();
            }
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
            await _relmCommunicator?.SendCommandAsync(new Reset())!;
            await _relmCommunicator?.SendQueryAsync<RelmVersionInfo>()!;
            await _relmCommunicator?.SendQueryAsync<DeviceConfiguration>()!;

            var configuration = _relmCommunicator?.Configuration ?? new DeviceConfiguration();
            Logger.Debug($"Reel controller connected with {configuration.NumReels} reel and {configuration.NumLights} lights. {configuration}");
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
            throw new NotImplementedException();
        }

        public Task<bool> LoadControllerAnimationFile(AnimationFile file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LoadControllerAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimation(LightShowFile file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimation(ReelCurveData file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimations(IEnumerable<ReelCurveData> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PlayControllerAnimations(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopControllerLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopAllControllerLightShows(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Synchronize(ReelSynchronizationData data, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetBrightness(int brightness)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EnterDfuMode()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExitDfuMode()
        {
            throw new NotImplementedException();
        }

        public Task<DfuStatus> Download(Stream firmware)
        {
            throw new NotImplementedException();
        }

        public void AbortDownload()
        {
            throw new NotImplementedException();
        }

        public Task<bool> HomeReels()
        {
            // TODO: Use proper home positions
            _relmCommunicator?.SendCommandAsync(new HomeReels(new List<short>{0, 0, 0, 0, 0}));
            return Task.FromResult(true);
        }

        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            // TODO: Implement this
            return Task.FromResult(true);
        }

        public Task<bool> TiltReels()
        {
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
