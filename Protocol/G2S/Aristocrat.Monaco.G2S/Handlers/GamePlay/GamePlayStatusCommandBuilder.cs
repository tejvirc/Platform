namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{TDevice,TCommand}" />
    /// </summary>
    public class GamePlayStatusCommandBuilder : ICommandBuilder<IGamePlayDevice, gamePlayStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _provider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayStatusCommandBuilder" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="provider">An <see cref="IGameProvider" /> instance.</param>
        public GamePlayStatusCommandBuilder(IG2SEgm egm, IGameProvider provider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <inheritdoc />
        public async Task Build(IGamePlayDevice device, gamePlayStatus command)
        {
            var game = _provider.GetGame(device.Id);

            var cabinet = _egm.GetDevice<ICabinetDevice>();

            command.configurationId = device.ConfigurationId;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;

            command.themeId = game.ThemeId;
            command.paytableId = game.PaytableId;
            command.generalTilt = false;
            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;
            command.egmLocked = cabinet.Device == device;

            await Task.CompletedTask;
        }
    }
}