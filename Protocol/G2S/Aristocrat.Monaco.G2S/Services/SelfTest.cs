namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Media;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;

    /// <inheritdoc />
    public class SelfTest : ISelfTest
    {
        private readonly IDeviceRegistryService _deviceRegistryService;
        private readonly IDisableConditionSaga _disableCondition;
        private readonly IDisableByOperatorManager _disabledByOperator;
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTest" /> class.
        /// </summary>
        public SelfTest(
            IG2SEgm egm,
            IPropertiesManager properties,
            IDisableByOperatorManager disabledByOperator,
            IDisableConditionSaga disableCondition,
            IDeviceRegistryService deviceRegistryService,
            IGameProvider gameProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _disabledByOperator = disabledByOperator ?? throw new ArgumentNullException(nameof(disabledByOperator));
            _disableCondition = disableCondition ?? throw new ArgumentNullException(nameof(disableCondition));
            _deviceRegistryService = deviceRegistryService ?? throw new ArgumentNullException(nameof(deviceRegistryService));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public void Execute()
        {
            // TODO: Refactor this.  Each type belongs in it's own implementation

            if (!_gameProvider.GetEnabledGames().Any())
            {
                var device = _egm.GetDevice<ICabinetDevice>();
                device.AddCondition(device, EgmState.EgmDisabled, (int)CabinetFaults.NoGamesEnabled);
            }

            if (_disabledByOperator.DisabledByOperator)
            {
                var device = _egm.GetDevice<ICabinetDevice>();
                device.AddCondition(device, EgmState.OperatorLocked);
            }

            var noteAcceptor = _deviceRegistryService.GetDevice<INoteAcceptor>();
            if (noteAcceptor != null)
            {
                var deviceService = (IDeviceService)noteAcceptor;

                var device = _egm.GetDevice<INoteAcceptorDevice>();
                device.Enabled = deviceService.Enabled || !deviceService.Enabled &&
                                 ((deviceService.ReasonDisabled & DisabledReasons.Service) == DisabledReasons.Service ||
                                  (deviceService.ReasonDisabled & DisabledReasons.Error) == DisabledReasons.Error ||
                                  (deviceService.ReasonDisabled & DisabledReasons.FirmwareUpdate) == DisabledReasons.FirmwareUpdate ||
                                  (deviceService.ReasonDisabled & DisabledReasons.Device) == DisabledReasons.Device);
            }

            var printer = _deviceRegistryService.GetDevice<IPrinter>();
            if (printer != null)
            {
                var deviceService = (IDeviceService)printer;

                var device = _egm.GetDevice<IPrinterDevice>();
                device.Enabled = deviceService.Enabled || !deviceService.Enabled &&
                                 ((deviceService.ReasonDisabled & DisabledReasons.Service) == DisabledReasons.Service ||
                                  (deviceService.ReasonDisabled & DisabledReasons.Error) == DisabledReasons.Error ||
                                  (deviceService.ReasonDisabled & DisabledReasons.FirmwareUpdate) == DisabledReasons.FirmwareUpdate ||
                                  (deviceService.ReasonDisabled & DisabledReasons.Device) == DisabledReasons.Device);
            }

            foreach (var game in _properties.GetValues<IGameDetail>(GamingConstants.Games))
            {
                var device = _egm.GetDevice<IGamePlayDevice>(game.Id);
                device.Enabled = (game.Status & GameStatus.DisabledBySystem) != GameStatus.DisabledBySystem && game.ActiveDenominations.Any();
            }

            foreach (var player in _properties.GetValues<IMediaPlayer>(ApplicationConstants.MediaPlayers))
            {
                var device = _egm.GetDevice<IMediaDisplay>(player.Id);
                device.Enabled = (player.Status & MediaPlayerStatus.DisabledBySystem) != MediaPlayerStatus.DisabledBySystem;
            }

            _disableCondition.Reenter();
        }
    }
}
