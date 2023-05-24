namespace Aristocrat.Monaco.G2S.Consumers
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
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.G2S.Handlers;
    using Aristocrat.Monaco.G2S.Services.Progressive;
    using Aristocrat.Monaco.Kernel;

    public class OperatorMenuSettingsChangedConsumer : Consumes<OperatorMenuSettingsChangedEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _cabinetCommandBuilder;
        private readonly ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus> _optionConfigCommandBuilder;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _progressiveCommandBuilder;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuSettingsChangedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        public OperatorMenuSettingsChangedConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> cabinetCommandBuilder,
            ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus> optionConfigCommandBuilder,
            ICommandBuilder<IProgressiveDevice, progressiveStatus> progressiveCommandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _cabinetCommandBuilder = cabinetCommandBuilder ?? throw new ArgumentNullException(nameof(cabinetCommandBuilder));
            _optionConfigCommandBuilder = optionConfigCommandBuilder ?? throw new ArgumentNullException(nameof(optionConfigCommandBuilder));
            _progressiveCommandBuilder = progressiveCommandBuilder ?? throw new ArgumentNullException(nameof(progressiveCommandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <summary>
        ///     Consumes the <see cref="OperatorMenuSettingsChangedEvent" />.
        /// </summary>
        /// <param name="theEvent">The <see cref="OperatorMenuSettingsChangedEvent" /> being consumed.</param>
        public override void Consume(OperatorMenuSettingsChangedEvent theEvent)
        {
            var cabinetDevices = _egm.GetDevices<ICabinetDevice>();
            var optionConfigDevices = _egm.GetDevices<IOptionConfigDevice>();
            var progressiveDevices = _egm.GetDevices<IProgressiveDevice>();
            if (cabinetDevices == null || optionConfigDevices == null || progressiveDevices == null)
            {
                return;
            }

            foreach(var cabinetDevice in cabinetDevices)
            {
                var cabinetStatus = new cabinetStatus();
                _cabinetCommandBuilder.Build(cabinetDevice, cabinetStatus);
                _eventLift.Report(cabinetDevice, EventCode.G2S_CBE006, cabinetDevice.DeviceList(cabinetStatus));
            }

            foreach(var optionConfigDevice in optionConfigDevices)
            {
                var optionConfigStatus = new optionConfigModeStatus();
                _optionConfigCommandBuilder.Build(optionConfigDevice, optionConfigStatus);
                _eventLift.Report(optionConfigDevice, EventCode.G2S_OCE004, optionConfigDevice.DeviceList(optionConfigStatus));
            }

            var progressiveService = ServiceManager.GetInstance().TryGetService<IProgressiveService>();
            if (progressiveService == null) return;
            progressiveService.OnConfiguredProgressives(true);
            foreach (var progressiveDevice in progressiveDevices)
            {
                var progressiveStatus = new progressiveStatus();
                _progressiveCommandBuilder.Build(progressiveDevice, progressiveStatus);
                _eventLift.Report(progressiveDevice, EventCode.G2S_PGE006, progressiveDevice.DeviceList(progressiveStatus));
            }
        }
    }
}
