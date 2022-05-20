namespace Aristocrat.G2S.Emdi.Handlers.Cabinet
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Client;
    using Client.Devices;
    using Emdi.Host;
    using Extensions;
    using log4net;
    using Monaco.Application.Contracts.Localization;
    using Monaco.Gaming.Contracts;
    using Protocol.v21ext1b1;

    /// <summary>
    ///     Handles the <see cref="getCabinetStatus"/> command
    /// </summary>
    public class GetCabinetStatus : CommandHandler<getCabinetStatus>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly IGameProvider _game;
        private readonly ILocalization _localization;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCabinetStatus"/> class.
        /// </summary>
        /// <param name="egm"></param>
        /// <param name="game"></param>
        /// <param name="localization"></param>
        public GetCabinetStatus(
            IG2SEgm egm,
            IGameProvider game,
            ILocalization localization)
        {
            _egm = egm;
            _game = game;
            _localization = localization;
        }

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(getCabinetStatus command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            const string defaultDeviceClass = @"G2S_gamePlay";

            var defaultGame = _game.GetGames().OrderBy(g => g.Id).FirstOrDefault();

            var device = _egm.GetDevice<ICabinetDevice>();

            return Task.FromResult(Success(new cabinetStatus
            {
                deviceClass = device.Device?.PrefixedDeviceClass() ??
                    (defaultGame != null ? defaultDeviceClass : Constants.None),
                egmState = device.State.ToG2S(),
                localeId = device.LocaleId(_localization.CurrentCulture)
            }));
        }
    }
}
