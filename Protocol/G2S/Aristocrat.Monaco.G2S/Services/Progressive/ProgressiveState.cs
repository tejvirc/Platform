namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S;
    using Aristocrat.Monaco.G2S.DisableProvider;
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.G2S.Client.Devices.v21;
    using System.Timers;

    public partial class ProgressiveService : IProgressiveState
    {
        /// <inheritdoc />
        public DateTime LastProgressiveUpdateTime { get; set; }

        /// <inheritdoc />
        public void ReceiveProgressiveValueUpdate()
        {
            if (_progressiveHostOfflineTimer == null)
            {
                return;
            }

            _progressiveHostOfflineTimer.Stop();

            var progressiveHost = _egm.Hosts.FirstOrDefault(h => h.IsProgressiveHost);
            if (progressiveHost == null)
            {
                return;
            }

            var interval = progressiveHost.OfflineTimerInterval.TotalMilliseconds > 0 ? progressiveHost.OfflineTimerInterval.TotalMilliseconds : 100;
            _progressiveHostOfflineTimer.Interval = interval;

            var systemDisableManager = ServiceManager.GetInstance().TryGetService<ISystemDisableManager>();

            _progressiveHostOfflineTimer.Start();

            _disableProvider.Enable(G2SDisableStates.CommsOffline);
            _disableProvider.Enable(G2SDisableStates.ProgressiveValueNotReceived);
            LastProgressiveUpdateTime = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public void SetProgressiveDeviceState(bool state, IProgressiveDevice device, string hostReason = null)
        {
            if (state)
            {
                SetGamePlayDeviceState(true, device);
                if (!device.HostEnabled)
                {
                    Logger.Debug($"Progressive service is enabling the Progressive device");

                    device.DisableText = string.Empty;
                    device.HostEnabled = true;
                    device.Enabled = true;

                    if (_egm.GetDevices<IProgressiveDevice>().All(d => d.HostEnabled))
                    {
                        Task.Run(OnTransportUp);
                        _disableProvider.Enable(G2SDisableStates.ProgressiveState);
                    }

                    var status = new progressiveStatus();
                    _commandBuilder.Build(device, status).Wait();

                    _eventLift.Report(device, EventCode.G2S_PGE002, device.DeviceList(status));
                }
            }
            else
            {
                device.DisableText = hostReason;
                SetGamePlayDeviceState(false, device);
                device.HostEnabled = false;

                Logger.Debug($"Progressive service is disabling a Progressive device: {hostReason}");

                var status = new progressiveStatus();
                _commandBuilder.Build(device, status).Wait();

                _eventLift.Report(device, EventCode.G2S_PGE001, device.DeviceList(status));

                G2SDisableStates reason;
                if (hostReason?.ToLower()?.Contains("meter rollback") == true)
                {
                    reason = G2SDisableStates.ProgressiveMeterRollback;
                }
                else
                {
                    reason = G2SDisableStates.ProgressiveState;
                }
                _disableProvider.Disable(
                    SystemDisablePriority.Immediate,
                    reason,
                    true);
            }
        }

        private void ProgressiveHostOfflineTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _disableProvider.Disable(SystemDisablePriority.Immediate, G2SDisableStates.CommsOffline, true);
        }

        private void EventUpdateTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _progressiveValueUpdateTimer?.Stop();

            var devices = _egm.GetDevices<IProgressiveDevice>();

            foreach (var device in devices)
            {
                var noProgressiveInfo = device.NoProgressiveInfo;

                if (noProgressiveInfo <= 0)
                {
                    noProgressiveInfo = DefaultNoProgInfo;
                }

                var timeout = TimeSpan.FromMilliseconds(noProgressiveInfo);

                if (DateTime.UtcNow - LastProgressiveUpdateTime > timeout)
                {
                    _disableProvider.Disable(
                        SystemDisablePriority.Immediate,
                        G2SDisableStates.ProgressiveValueNotReceived,
                        true);
                }
                else if (!device.Enabled || !device.HostEnabled)
                {
                    _disableProvider.Enable(G2SDisableStates.ProgressiveValueNotReceived);
                }

                if (_progressiveValueUpdateTimer != null && timeout.TotalMilliseconds > 0)
                {
                    _progressiveValueUpdateTimer.Interval = timeout.TotalMilliseconds;
                }

                _progressiveValueUpdateTimer?.Start();
            }
        }
    }
}
