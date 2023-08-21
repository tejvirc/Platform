namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Monaco.Common;
    using Constants = Aristocrat.G2S.Client.Constants;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{TDevice,TCommand}" />
    /// </summary>
    public class CabinetStatusCommandBuilder : ICommandBuilder<ICabinetDevice, cabinetStatus>
    {
        private readonly IDisplayService _displayService;
        private readonly ITowerLight _light;
        private readonly ICabinetService _cabinetService;
        private readonly IDoorService _doors;
        private readonly IGameProvider _gameProvider;
        private readonly IHardMeter _hardMeter;
        private readonly IGameHistory _history;
        private readonly ILocalization _locale;
        private readonly IPropertiesManager _properties;

        private static IReelController ReelController => ServiceManager.GetInstance().TryGetService<IReelController>();

        public CabinetStatusCommandBuilder(
            ICabinetService cabinetService,
            IDoorService doors,
            IHardMeter hardMeter,
            ILocalization locale,
            IGameProvider gameProvider,
            IGameHistory history,
            IPropertiesManager properties,
            IDisplayService displayService,
            ITowerLight light)
        {
            _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
            _doors = doors ?? throw new ArgumentNullException(nameof(doors));
            _hardMeter = hardMeter ?? throw new ArgumentNullException(nameof(hardMeter));
            _locale = locale ?? throw new ArgumentNullException(nameof(locale));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
            _light = light ?? throw new ArgumentNullException(nameof(light));
        }

        /// <inheritdoc />
        public async Task Build(ICabinetDevice device, cabinetStatus command)
        {
            const string defaultDeviceClass = @"G2S_gamePlay";
            const int defaultDeviceId = 1;
            const int defaultMaximumWagerCredits = 1;

            IGameProfile committedGame = null;

            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var defaultGame = _gameProvider.GetGames().OrderBy(g => g.Id).FirstOrDefault();

            command.configurationId = device.ConfigurationId;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;
            command.enableGamePlay = device.GamePlayEnabled;
            command.enableMoneyIn = _properties.GetValue(AccountingConstants.MoneyInEnabled, true);
            command.enableMoneyOut = device.MoneyOutEnabled;
            command.hostLocked = device.HostLocked;

            command.egmState = ToEgmState(device.State);
            command.deviceClass = device.Device?.PrefixedDeviceClass() ??
                                  (defaultGame != null ? defaultDeviceClass : Constants.None);

            var currentLog = _history.CurrentLog;
            if (currentLog != null)
            {
                // GetGames only returns active games, which is required for this since we need data that we can only get from an active game
                committedGame = _gameProvider.GetGames().SingleOrDefault(g => g.Id == currentLog.GameId);
            }

            command.deviceId = device.Device?.Id ??
                               (currentLog?.GameId != defaultDeviceId ? currentLog?.GameId : defaultGame?.Id) ??
                               defaultDeviceId;

            command.gamePlayId = currentLog?.GameId ?? 0;
            command.themeId = committedGame?.ThemeId ?? Constants.None;
            command.paytableId = committedGame?.PaytableId ?? Constants.None;
            command.denomId = currentLog?.DenomId ?? 0;
            command.maxWagerCredits = committedGame?.MaximumWagerCredits ?? defaultMaximumWagerCredits;

            command.serviceLampOn = _light.IsLit;

            command.logicDoorOpen = !_doors.GetDoorClosed((int)DoorLogicalId.Logic);
            command.logicDoorDateTime = _doors.GetDoorLastOpened((int)DoorLogicalId.Logic);
            command.logicDoorDateTimeSpecified = DateTimeSpecified(command.logicDoorDateTime);

            command.auxDoorOpen = !_doors.GetDoorClosed((int)DoorLogicalId.TopBox);
            command.auxDoorDateTime = _doors.GetDoorLastOpened((int)DoorLogicalId.TopBox);
            command.auxDoorDateTimeSpecified = DateTimeSpecified(command.auxDoorDateTime);

            command.cabinetDoorOpen = !_doors.GetDoorClosed((int)DoorLogicalId.Main);
            command.cabinetDoorDateTime = _doors.GetDoorLastOpened((int)DoorLogicalId.Main);
            command.cabinetDoorDateTimeSpecified = DateTimeSpecified(command.cabinetDoorDateTime);

            command.hardMetersDisconnected = _hardMeter.LogicalState == HardMeterLogicalState.Disabled;

            command.enableMoneyOut = true;
            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;
            command.egmIdle = _cabinetService.Idle;

            command.generalFault =
                device.HasCondition((d, s, f) => d is ICabinetDevice && s == EgmState.EgmDisabled && Faults.IsGeneral(f));
            if (ReelController != null)
            {
                command.reelTilt = ReelController.LogicalState == ReelControllerState.Tilted;
                command.reelsTilted = command.reelTilt ? string.Join(",", ReelController.Faults.Select(x => x.Key).ToArray()) : string.Empty;
            }

            command.videoDisplayFault = _displayService.IsFaulted;
            command.nvStorageFault =
                device.HasCondition((d, s, f) => d is ICabinetDevice && s == EgmState.EgmDisabled && f == (int)CabinetFaults.StorageFault);
            command.generalMemoryFault = false;

            command.localeId = device.LocaleId(_locale.CurrentCulture);
            command.timeZoneOffset =
                _properties.GetValue(ApplicationConstants.TimeZoneOffsetKey, TimeSpan.Zero).GetFormattedOffset();

            await Task.CompletedTask;
        }

        private static bool DateTimeSpecified(DateTime dateTime)
        {
            return dateTime != DateTime.MinValue;
        }

        private static t_egmStates ToEgmState(EgmState state)
        {
            switch (state)
            {
                case EgmState.Enabled:
                    return t_egmStates.G2S_enabled;
                case EgmState.OperatorMode:
                    return t_egmStates.G2S_operatorMode;
                case EgmState.AuditMode:
                    return t_egmStates.G2S_auditMode;
                case EgmState.OperatorDisabled:
                    return t_egmStates.G2S_operatorDisabled;
                case EgmState.OperatorLocked:
                    return t_egmStates.G2S_operatorLocked;
                case EgmState.TransportDisabled:
                    return t_egmStates.G2S_transportDisabled;
                case EgmState.HostDisabled:
                    return t_egmStates.G2S_hostDisabled;
                case EgmState.EgmDisabled:
                    return t_egmStates.G2S_egmDisabled;
                case EgmState.EgmLocked:
                    return t_egmStates.G2S_egmLocked;
                case EgmState.HostLocked:
                    return t_egmStates.G2S_hostLocked;
                case EgmState.DemoMode:
                    return t_egmStates.G2S_demoMode;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}