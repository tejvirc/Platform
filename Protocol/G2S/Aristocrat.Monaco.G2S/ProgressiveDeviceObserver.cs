namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Data.Profile;
    using Aristocrat.Monaco.G2S.DisableProvider;
    using Aristocrat.Monaco.G2S.Handlers;
    using Aristocrat.Monaco.Kernel;
    using Gaming.Contracts.Progressives;
    using Services.Progressive;

    public class ProgressiveDeviceObserver : DeviceObserver, IProgressiveDeviceObserver
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IG2SDisableProvider _disableProvider;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _progressiveStatusBuilder;

        public ProgressiveDeviceObserver(
            IProfileService profiles,
            IEgmStateManager egmState,
            IHostStatusHandlerFactory hostStatusHandlerFactory,
            IG2SDisableProvider disableProvider,
            IG2SEgm egm,
            IEventLift eventLift,
            ICommandBuilder<IProgressiveDevice, progressiveStatus> progressiveStatusBuilder)
            : base(profiles, egmState, hostStatusHandlerFactory)
        {
            _egm = egm;
            _disableProvider = disableProvider;
            _eventLift = eventLift;
            _progressiveStatusBuilder = progressiveStatusBuilder;
        }

        public override void Notify(IDevice device, string propertyName)
        {
            if (device is not IProgressiveDevice progDevice)
            {
                throw new ArgumentException(@"Invalid device type provided", nameof(device));
            }

            switch (propertyName)
            {
                case nameof(progDevice.Enabled):
                    HandleStatusChange(progDevice);
                    break;
                case nameof(progDevice.HostEnabled):
                    HandleHostStatusChange(progDevice);
                    break;
            }

            base.Notify(device, propertyName);
        }

        private void HandleStatusChange(IProgressiveDevice device)
        {
            if (device.Enabled)
            {
                //Only lift the disablement if ALL devices have been updated with good progressive data
                if (_egm.GetDevices<IProgressiveDevice>().All(d => d.ProgInfoValid))
                {
                    _disableProvider.Enable(G2SDisableStates.ProgressiveValueNotReceived);
                }
                RaiseEvent(device, EventCode.G2S_PGE002);
            }
            else
            {
                _disableProvider.Disable(
                    SystemDisablePriority.Immediate,
                    G2SDisableStates.ProgressiveValueNotReceived);
                RaiseEvent(device, EventCode.G2S_PGE001);
            }

            SetGamePlayDeviceState(device);
        }

        private void HandleHostStatusChange(IProgressiveDevice device)
        {
            if (device.HostEnabled)
            {
                //Only lift the disablement if ALL devices have been turned back on
                if (_egm.GetDevices<IProgressiveDevice>().All(d => d.HostEnabled))
                {
                    _disableProvider.Enable(G2SDisableStates.ProgressiveStateDisabledByHost);
                }
                RaiseEvent(device, EventCode.G2S_PGE004);
            }
            else
            {
                //meter rollbacks are extreme, nonrecoverable errors requiring manual intervention,
                //followed by EGM reboot to clear.
                //There is no way to clear this disable once set other than reboot
                var reason = device.DisableText?.ToLower().Contains("meter rollback") == true
                    ? G2SDisableStates.ProgressiveMeterRollback
                    : G2SDisableStates.ProgressiveStateDisabledByHost;
                _disableProvider.Disable(SystemDisablePriority.Immediate, reason);
                RaiseEvent(device, EventCode.G2S_PGE003);
            }

            SetGamePlayDeviceState(device);
        }
        private void SetGamePlayDeviceState(IProgressiveDevice device)
        {
            var levelProvider = ServiceManager.GetInstance().GetService<IProgressiveLevelProvider>();
            var gamePlayDeviceIds = levelProvider.GetProgressiveLevels()
                .Where(l => l.ProgressiveId == device.ProgressiveId && l.DeviceId == device.Id)
                .Select(l => l.GameId);

            //first loop over all game play devices connected to the progressive device that changed
            foreach (var gamePlayDeviceId in gamePlayDeviceIds)
            {
                var gamePlayDevice = _egm.GetDevice<IGamePlayDevice>(gamePlayDeviceId);
                if (gamePlayDevice == null) continue;

                var levelDevices = levelProvider.GetProgressiveLevels().Where(l => l.GameId == gamePlayDeviceId).Select(l => l.DeviceId).ToList();
                var progDevices = _egm.GetDevices<IProgressiveDevice>().Where(d => levelDevices.Contains(d.Id));

                //game play device can only be re-enabled once all associated  progressive devices agree
                gamePlayDevice.Enabled = progDevices.All(d => d.Enabled && d.HostEnabled);

            }
        }

        private void RaiseEvent(IProgressiveDevice device, string eventCode)
        {
            var status = new progressiveStatus();
            _progressiveStatusBuilder.Build(device, status).Wait();
            _eventLift.Report(device, eventCode, device.DeviceList(status));
        }
    }
}
