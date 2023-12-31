﻿namespace Aristocrat.Monaco.Hardware.Gds.Reel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Gds;
    using Contracts.Gds.Reel;
    using Contracts.Reel;
    using Contracts.SharedDevice;
    using log4net;
    using HomeReel = Contracts.Gds.Reel.HomeReel;
    using Nudge = Contracts.Gds.Reel.Nudge;

    /// <summary>
    ///     The reel controller gds device
    /// </summary>
    public class ReelControllerGds : GdsDeviceBase, IReelControllerImplementation
    {
        private const int WaitForReportTime = 30000;

        private const int InitializationWaitTimeout = 30000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ConcurrentDictionary<int, ReelFaults> _faults = new();

        private readonly ConcurrentDictionary<int, ReelStatus> _reelsStatus = new();

        /// <summary>
        /// </summary>
        public ReelControllerGds()
        {
            DeviceType = DeviceType.ReelController;
            RegisterCallback<FailureStatus>(FailureReported);
            RegisterCallback<FailureStatusClear>(FailureClear);
            RegisterCallback<ReelStatus>(ReelStatusReceived);
            RegisterCallback<ReelSpinningStatus>(ReelSpinningStatusReceived);
            RegisterCallback<ReelLightIdentifiersResponse>(ReelLightsIdentifiersReceived);
            RegisterCallback<ReelLightResponse>(ReelLightsResponseReceived);
            RegisterCallback<TiltReelsResponse>(TiltReelsResponseReceived);
            RegisterCallback<ControllerInitializedStatus>(OnHardwareInitializedReceived);
        }

        /// <inheritdoc />
        public event EventHandler<ReelFaultedEventArgs> FaultOccurred;

        /// <inheritdoc />
        public event EventHandler<ReelFaultedEventArgs> FaultCleared;

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

        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultOccurred;

        /// <inheritdoc />
        public event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultCleared;

        /// <inheritdoc />
        public ReelControllerFaults ReelControllerFaults { get; private set; }

        /// <inheritdoc />
        public IReadOnlyCollection<int> ReelIds => Faults.Keys.ToList().AsReadOnly();

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelFaults> Faults => _faults;

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ReelStatus> ReelsStatus => _reelsStatus;

        /// <inheritdoc />
        public override async Task<bool> SelfTest(bool nvm)
        {
            SendCommand(new SelfTest { Nvm = nvm ? 1 : 0 });
            var report = await WaitForReport<FailureStatus>();
            if (report is null)
            {
                return false;
            }

            return !report.FirmwareError && !report.MechanicalError && !report.ComponentError && !report.DiagnosticCode;
        }

        /// <inheritdoc />
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            ClearFaults(reelId);
            if (resetStatus)
            {
                ResetConnectedReelStatus(reelId);
            }

            SendCommand(new HomeReel { ReelId = reelId, Stop = stop });
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<bool> HomeReels()
        {
            for (var reelId = 1; reelId < ReelConstants.MaxSupportedReels; reelId++)
            {
                ResetConnectedReelStatus(reelId);
            }

            var results = Faults.Where(x => !x.Value.HasFlag(ReelFaults.Disconnected) && x.Key > 0).Select(x => x.Key)
                .Select(reelId => HomeReel(reelId, -1, false));
            var homed = await Task.WhenAll(results);
            return homed.All(x => x);
        }

        /// <inheritdoc />
        public Task<bool> SpinReels(params ReelSpinData[] spinData)
        {
            var gdsData = spinData.Select(x => new GdsReelSpinData(x)).ToArray();
            SendCommand(new SpinReels { ReelCount = (byte)gdsData.Length, ReelSpinData = gdsData });
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> NudgeReels(params NudgeReelData[] nudgeReelData)
        {
            var gdsData = nudgeReelData.Select(x => new GdsNudgeReelData(x)).ToArray();
            SendCommand(new Nudge { ReelCount = (byte)gdsData.Length, NudgeReelData = gdsData });
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            foreach (var pair in brightness)
            {
                SendCommand(new SetBrightness { ReelId = pair.Key, Brightness = pair.Value });
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<bool> SetBrightness(int brightness)
        {
            ClearStaleReports<ReelLightResponse>();
            SendCommand(new SetBrightness { ReelId = 0, Brightness = brightness });
            var result = await WaitForReport<ReelLightResponse>();
            return result?.LightsUpdated ?? false;
        }

        /// <inheritdoc />
        public Task<bool> SetReelSpeed(params ReelSpeedData[] speedData)
        {
            var gdsData = speedData.Select(x => new GdsReelSpeedData(x)).ToArray();
            SendCommand(new SetSpeed { ReelCount = (byte)speedData.Length, ReelSpeedData = gdsData });
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<bool> SetLights(params ReelLampData[] lampData)
        {
            ClearStaleReports<ReelLightResponse>();
            var gdsData = lampData.Select(x => new GdsReelLampData(x)).ToArray();
            SendCommand(new SetLamps { LampCount = (byte)lampData.Length, ReelLampData = gdsData });
            var result = await WaitForReport<ReelLightResponse>();
            return result?.LightsUpdated ?? false;
        }

        /// <inheritdoc />
        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            SendCommand(new SetOffsets { ReelOffsets = offsets });
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<bool> TiltReels()
        {
            SendCommand(new TiltReels());
            await WaitForReport<TiltReelsResponse>();
            return true;
        }

        /// <inheritdoc />
        public async Task<IList<int>> GetReelLightIdentifiers()
        {
            SendCommand(new GetReelLightIdentifiers());
            ReelLightIdentifiersResponse status = null;
            try
            {
                status = await WaitForReport<ReelLightIdentifiersResponse>(WaitForReportTime);
            }
            catch (Exception ex)
            {
                Logger.Error($"GetReelLightIdentifiers failed : {ex.Message}");
            }

            return (status is null || status.StartId > status.EndId
                ? Enumerable.Empty<int>()
                : Enumerable.Range(status.StartId, status.EndId - status.StartId + 1)).ToList();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        ///     Called when a reel is connected
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnReelConnected(ReelEventArgs e)
        {
            var reelsStatus = new ReelStatus { ReelId = e.ReelId, Connected = true };
            _reelsStatus.AddOrUpdate(e.ReelId, reelsStatus, (i, s) => reelsStatus);
            ReelConnected?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a reel is disconnected
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnReelDisconnected(ReelEventArgs e)
        {
            var reelsStatus = new ReelStatus { ReelId = e.ReelId, Connected = false };
            _reelsStatus.AddOrUpdate(e.ReelId, reelsStatus, (i, s) => reelsStatus);
            ReelDisconnected?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a reel is slow spinning
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnReelSlowSpinning(ReelEventArgs e)
        {
            ReelSlowSpinning?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a reel is spinning
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnReelSpinning(ReelEventArgs e)
        {
            ReelSpinning?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a reel is stopped
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnReelStopped(ReelEventArgs e)
        {
            var reelsStatus = new ReelStatus
            {
                ReelId = e.ReelId,
                ReelStall = false,
                ReelTampered = false,
                Connected = true,
                LowVoltage = false,
                FailedHome = false
            };
            _reelsStatus.AddOrUpdate(e.ReelId, reelsStatus, (i, s) => reelsStatus);
            ReelStopped?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a reel fault is cleared
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnFaultCleared(ReelFaultedEventArgs e)
        {
            Faults.TryGetValue(e.ReelId, out var faults);
            if ((e.Faults & ReelFaults.ReelStall) != 0)
            {
                faults &= ~ReelFaults.ReelStall;
                _faults.AddOrUpdate(e.ReelId, faults, (i, reelFaults) => faults);
            }

            if ((e.Faults & ReelFaults.ReelTamper) != 0)
            {
                faults &= ~ReelFaults.ReelTamper;
                _faults.AddOrUpdate(e.ReelId, faults, (i, reelFaults) => faults);
            }

            if ((e.Faults & ReelFaults.LowVoltage) != 0)
            {
                faults &= ~ReelFaults.LowVoltage;
                _faults.AddOrUpdate(e.ReelId, faults, (i, reelFaults) => faults);
            }

            FaultCleared?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a reel controller fault occurs
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnControllerFaultOccurred(ReelControllerFaultedEventArgs e)
        {
            if ((e.Faults & ReelControllerFaults.CommunicationError) != 0)
            {
                ReelControllerFaults |= ReelControllerFaults.CommunicationError;
            }

            if ((e.Faults & ReelControllerFaults.HardwareError) != 0)
            {
                ReelControllerFaults |= ReelControllerFaults.HardwareError;
            }

            if ((e.Faults & ReelControllerFaults.RequestError) != 0)
            {
                ReelControllerFaults |= ReelControllerFaults.RequestError;
            }

            if ((e.Faults & ReelControllerFaults.FirmwareFault) != 0)
            {
                ReelControllerFaults |= ReelControllerFaults.FirmwareFault;
            }

            ControllerFaultOccurred?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a reel controller fault is cleared
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnControllerFaultCleared(ReelControllerFaultedEventArgs e)
        {
            if ((e.Faults & ReelControllerFaults.CommunicationError) != 0)
            {
                ReelControllerFaults &= ~ReelControllerFaults.CommunicationError;
            }

            if ((e.Faults & ReelControllerFaults.HardwareError) != 0)
            {
                ReelControllerFaults &= ~ReelControllerFaults.HardwareError;
            }

            if ((e.Faults & ReelControllerFaults.RequestError) != 0)
            {
                ReelControllerFaults &= ~ReelControllerFaults.RequestError;
            }

            if ((e.Faults & ReelControllerFaults.RequestError) != 0)
            {
                ReelControllerFaults &= ~ReelControllerFaults.RequestError;
            }

            ControllerFaultCleared?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a reel fault occurs
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnFaultOccurred(ReelFaultedEventArgs e)
        {
            Faults.TryGetValue(e.ReelId, out var faults);
            if ((e.Faults & ReelFaults.ReelStall) != 0)
            {
                faults |= ReelFaults.ReelStall;
                _faults.AddOrUpdate(e.ReelId, faults, (i, reelFaults) => faults);
            }

            if ((e.Faults & ReelFaults.ReelTamper) != 0)
            {
                faults |= ReelFaults.ReelTamper;
                _faults.AddOrUpdate(e.ReelId, faults, (i, reelFaults) => faults);
            }

            if ((e.Faults & ReelFaults.LowVoltage) != 0)
            {
                faults |= ReelFaults.LowVoltage;
                _faults.AddOrUpdate(e.ReelId, faults, (i, reelFaults) => faults);
            }

            FaultOccurred?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when the controller hardware is initialized
        /// </summary>
        protected virtual void OnHardwareInitializedReceived(ControllerInitializedStatus status)
        {
            PublishReport(status);
            HardwareInitialized?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        protected override async Task<bool> Reset()
        {
            if (!await Disable())
            {
                Logger.Warn("Reset - Disable failed");
                return false;
            }

            if (await CalculateCrc(GdsConstants.DefaultSeed) == 0)
            {
                Logger.Warn("Reset - CalculateCrc failed");
                return false;
            }

            if (string.IsNullOrEmpty(await RequestGatReport()))
            {
                Logger.Warn("Reset - RequestGatReport failed");
                return false;
            }

            var result = await WaitForReport<ControllerInitializedStatus>(InitializationWaitTimeout);
            if (result is null)
            {
                Logger.Warn("Reel Controller Failed to Initialize");
            }

            return result is not null;
        }

        private void FailureClear(FailureStatusClear status)
        {
            if (status.MechanicalError)
            {
                var reelsStatus = new ReelStatus { ReelId = status.ReelId, ReelStall = false };
                _reelsStatus.AddOrUpdate(status.ReelId, reelsStatus, (i, s) => reelsStatus);
                OnFaultCleared(new ReelFaultedEventArgs(ReelFaults.ReelStall, status.ReelId));
                Logger.Debug($"FailureCleared - Mechanical Error, reel={status.ReelId}");
            }

            if (status.TamperDetected)
            {
                var reelsStatus = new ReelStatus { ReelId = status.ReelId, ReelTampered = false };
                _reelsStatus.AddOrUpdate(status.ReelId, reelsStatus, (i, s) => reelsStatus);
                OnFaultCleared(new ReelFaultedEventArgs(ReelFaults.ReelTamper, status.ReelId));
                Logger.Debug($"FailureCleared - Tamper Detected, reel={status.ReelId}");
            }

            if (status.StallDetected)
            {
                var reelsStatus = new ReelStatus { ReelId = status.ReelId, ReelStall = false };
                _reelsStatus.AddOrUpdate(status.ReelId, reelsStatus, (i, s) => reelsStatus);
                OnFaultCleared(new ReelFaultedEventArgs(ReelFaults.ReelStall, status.ReelId));
                Logger.Debug($"FailureCleared - Stall Detected, reel={status.ReelId}");
            }

            if (status.LowVoltageDetected)
            {
                var reelsStatus = new ReelStatus { ReelId = status.ReelId, LowVoltage = false };
                _reelsStatus.AddOrUpdate(status.ReelId, reelsStatus, (i, s) => reelsStatus);
                OnFaultCleared(new ReelFaultedEventArgs(ReelFaults.LowVoltage, status.ReelId));
                Logger.Debug($"FailureCleared - Low Voltage Detected, reel={status.ReelId}");
            }

            if (status.FirmwareError)
            {
                OnControllerFaultCleared(new ReelControllerFaultedEventArgs(ReelControllerFaults.FirmwareFault));
                Logger.Debug($"FailureCleared - Firmware Error, reel={status.ReelId}");
            }

            if (status.ComponentError)
            {
                OnControllerFaultCleared(new ReelControllerFaultedEventArgs(ReelControllerFaults.RequestError));
                Logger.Debug($"FailureCleared - Component Error, reel={status.ReelId}");
            }

            if (status.CommunicationError)
            {
                OnControllerFaultCleared(new ReelControllerFaultedEventArgs(ReelControllerFaults.CommunicationError));
                Logger.Debug("FailureCleared - Communication Error");
            }

            if (status.HardwareError)
            {
                OnControllerFaultCleared(new ReelControllerFaultedEventArgs(ReelControllerFaults.HardwareError));
                Logger.Debug("FailureCleared - Hardware Error");
            }
        }

        private void FailureReported(FailureStatus status)
        {
            if (status.ErrorCode != 0)
            {
                if (status.MechanicalError)
                {
                    var reelsStatus = new ReelStatus { ReelId = status.ReelId, ReelStall = true };
                    _reelsStatus.AddOrUpdate(status.ReelId, reelsStatus, (i, s) => reelsStatus);
                    OnFaultOccurred(new ReelFaultedEventArgs(ReelFaults.ReelStall, status.ReelId));
                    Logger.Debug($"FailureReported - Mechanical Error, reel={status.ReelId}, errorCode=0x{status.ErrorCode:X}");
                }

                if (status.TamperDetected)
                {
                    var reelsStatus = new ReelStatus { ReelId = status.ReelId, ReelTampered = true };
                    _reelsStatus.AddOrUpdate(status.ReelId, reelsStatus, (i, s) => reelsStatus);
                    OnFaultOccurred(new ReelFaultedEventArgs(ReelFaults.ReelTamper, status.ReelId));
                    Logger.Debug($"FailureReported - Tamper Detected, reel={status.ReelId}, errorCode=0x{status.ErrorCode:X}");
                }

                if (status.StallDetected)
                {
                    var reelsStatus = new ReelStatus { ReelId = status.ReelId, ReelStall = true };
                    _reelsStatus.AddOrUpdate(status.ReelId, reelsStatus, (i, s) => reelsStatus);
                    OnFaultOccurred(new ReelFaultedEventArgs(ReelFaults.ReelStall, status.ReelId));
                    Logger.Debug($"FailureReported - Stall Detected, reel={status.ReelId}, errorCode=0x{status.ErrorCode:X}");
                }

                if (status.LowVoltageDetected)
                {
                    var reelsStatus = new ReelStatus { ReelId = status.ReelId, LowVoltage = true };
                    _reelsStatus.AddOrUpdate(status.ReelId, reelsStatus, (i, s) => reelsStatus);
                    OnFaultOccurred(new ReelFaultedEventArgs(ReelFaults.LowVoltage, status.ReelId));
                    Logger.Debug($"FailureReported - Low Voltage Detected, reel={status.ReelId}, errorCode=0x{status.ErrorCode:X}");
                }

                if (status.FirmwareError)
                {
                    ReelControllerFaults |= ReelControllerFaults.RequestError;
                    OnControllerFaultOccurred(new ReelControllerFaultedEventArgs(ReelControllerFaults));
                    Logger.Debug($"FailureReported - Firmware Error, errorCode=0x{status.ErrorCode:X}");
                }

                if (status.ComponentError)
                {
                    ReelControllerFaults |= ReelControllerFaults.RequestError;
                    OnControllerFaultOccurred(new ReelControllerFaultedEventArgs(ReelControllerFaults));
                    Logger.Debug($"FailureReported - Component Error, reel={status.ReelId}, errorCode= 0x{status.ErrorCode:X}");
                }

                if (status.CommunicationError)
                {
                    ReelControllerFaults |= ReelControllerFaults.CommunicationError;
                    OnControllerFaultOccurred(new ReelControllerFaultedEventArgs(ReelControllerFaults));
                    Logger.Debug($"FailureReported - Communication Error 0x{status.ErrorCode:X}");
                }

                if (status.HardwareError)
                {
                    ReelControllerFaults |= ReelControllerFaults.HardwareError;
                    OnControllerFaultOccurred(new ReelControllerFaultedEventArgs(ReelControllerFaults));
                    Logger.Debug($"FailureReported - Hardware Error 0x{status.ErrorCode:X}");
                }

                if (status.FailedHome)
                {
                    var reelsStatus = new ReelStatus { ReelId = status.ReelId, FailedHome = true };
                    _reelsStatus.AddOrUpdate(status.ReelId, reelsStatus, (i, s) => reelsStatus);
                    OnFaultOccurred(new ReelFaultedEventArgs(ReelFaults.ReelStall, status.ReelId));
                    Logger.Debug($"FailureReported - Failed Homing, reel={status.ReelId}");
                }
            }

            PublishReport(status);
        }

        private void ReelSpinningStatusReceived(ReelSpinningStatus status)
        {
            if (status.SlowSpinning)
            {
                OnReelSlowSpinning(new ReelEventArgs(status.ReelId));
            }

            if (status.IdleAtStop)
            {
                OnReelStopped(new ReelEventArgs(status.ReelId, status.Step));
            }

            if (status.Spinning)
            {
                OnReelSpinning(new ReelEventArgs(status.ReelId));
            }
        }

        private void ClearFaults(int reelId)
        {
            if (Faults.Any(x => x.Value.HasFlag(ReelFaults.ReelStall)))
            {
                var reelIds = Faults.Where(x => x.Value.HasFlag(ReelFaults.ReelStall)).Where(x => x.Key == reelId).Select(x => x.Key).ToList();
                foreach (var r in reelIds)
                {
                    OnFaultCleared(new ReelFaultedEventArgs(ReelFaults.ReelStall, r));
                }
            }

            if (Faults.Any(x => x.Value.HasFlag(ReelFaults.ReelTamper)))
            {
                var reelIds = Faults.Where(x => x.Value.HasFlag(ReelFaults.ReelTamper)).Where(x => x.Key == reelId).Select(x => x.Key).ToList();
                foreach (var r in reelIds)
                {
                    OnFaultCleared(new ReelFaultedEventArgs(ReelFaults.ReelTamper, r));
                }
            }

            if (Faults.Any(x => x.Value.HasFlag(ReelFaults.LowVoltage)))
            {
                var reelIds = Faults.Where(x => x.Value.HasFlag(ReelFaults.LowVoltage)).Where(x => x.Key == reelId).Select(x => x.Key).ToList();
                foreach (var r in reelIds)
                {
                    OnFaultCleared(new ReelFaultedEventArgs(ReelFaults.LowVoltage, r));
                }
            }

            if (ReelControllerFaults.HasFlag(ReelControllerFaults.RequestError))
            {
                OnControllerFaultCleared(new ReelControllerFaultedEventArgs(ReelControllerFaults.CommunicationError));
            }

            if (ReelControllerFaults.HasFlag(ReelControllerFaults.CommunicationError))
            {
                OnControllerFaultCleared(new ReelControllerFaultedEventArgs(ReelControllerFaults.CommunicationError));
            }

            if (ReelControllerFaults.HasFlag(ReelControllerFaults.HardwareError))
            {
                OnControllerFaultCleared(new ReelControllerFaultedEventArgs(ReelControllerFaults.HardwareError));
            }
        }

        private void ReelStatusReceived(ReelStatus status)
        {
            var firstFault = !Faults.TryGetValue(status.ReelId, out var faults);
            switch (status.Connected)
            {
                case true when (firstFault || (faults & ReelFaults.Disconnected) != 0):
                    faults &= ~ReelFaults.Disconnected;
                    OnReelConnected(new ReelEventArgs(status.ReelId));
                    break;
                case false when (firstFault || (faults & ReelFaults.Disconnected) == 0):
                    faults |= ReelFaults.Disconnected;

                    if (!firstFault)
                    {
                        OnReelDisconnected(new ReelEventArgs(status.ReelId));
                    }

                    break;
            }

            _faults.AddOrUpdate(status.ReelId, faults, (i, reelFaults) => faults);
        }

        private void ReelLightsIdentifiersReceived(ReelLightIdentifiersResponse response)
        {
            PublishReport(response);
        }

        private void ReelLightsResponseReceived(ReelLightResponse response)
        {
            PublishReport(response);
        }

        private void TiltReelsResponseReceived(TiltReelsResponse response)
        {
            PublishReport(response);
        }

        private void ResetConnectedReelStatus(int reelId)
        {
            if (_reelsStatus.ContainsKey(reelId))
            {
                _reelsStatus.TryGetValue(reelId, out var reelStatus);
                if (reelStatus?.Connected == true)
                {
                    reelStatus.ReelStall = false;
                    reelStatus.ReelTampered = false;
                    reelStatus.LowVoltage = false;
                    reelStatus.FailedHome = false;
                    _reelsStatus.AddOrUpdate(reelId, reelStatus, (i, s) => reelStatus);
                }
            }
            else
            {
                var reelsStatus = new ReelStatus
                {
                    ReelId = reelId,
                    ReelStall = false,
                    ReelTampered = false,
                    Connected = true,
                    LowVoltage = false,
                    FailedHome = false
                };
                _reelsStatus.AddOrUpdate(reelId, reelsStatus, (i, s) => reelsStatus);
            }
        }
    }
}