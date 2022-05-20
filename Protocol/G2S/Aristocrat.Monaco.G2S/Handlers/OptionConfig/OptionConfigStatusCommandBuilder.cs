namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    /// <summary>
    ///     An implementation of ICommandBuilder&lt;ICabinet, cabinetStatus&gt;.
    /// </summary>
    public class OptionConfigStatusCommandBuilder : ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>
    {
        private readonly IDisableConditionSaga _configurationMode;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigStatusCommandBuilder" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="configurationMode">The configuration mode.</param>
        public OptionConfigStatusCommandBuilder(IG2SEgm egm, IDisableConditionSaga configurationMode)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _configurationMode = configurationMode ?? throw new ArgumentNullException(nameof(configurationMode));
        }

        /// <inheritdoc />
        public async Task Build(IOptionConfigDevice device, optionConfigModeStatus command)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            command.configurationId = device.ConfigurationId;
            command.enabled = _configurationMode.Enabled(device);
            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;
            command.egmLocked = cabinet.Device == device;

            await Task.CompletedTask;
        }
    }
}