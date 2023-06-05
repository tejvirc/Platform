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

                    var status = new progressiveStatus();
                    _commandBuilder.Build(device, status).Wait();

                    _eventLift.Report(device, EventCode.G2S_PGE002, device.DeviceList(status));
                }

                if (_egm.GetDevices<IProgressiveDevice>().All(d => d.HostEnabled))
                {
                    Task.Run(OnTransportUp);
                    _disableProvider.Enable(G2SDisableStates.ProgressiveStateDisabledByHost);
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
                    reason = G2SDisableStates.ProgressiveStateDisabledByHost;
                }
                _disableProvider.Disable(SystemDisablePriority.Immediate, reason);
            }
        }
    }
}
