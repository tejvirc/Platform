namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System;
    using System.Linq;
    using Application.Contracts;
    using Common;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Reel;
    using Kernel;
    using Vgt.Client12.Application.OperatorMenu;

    public class EgmStatusHandler : IEgmStatusService
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IDoorMonitor _doorMonitor;
        private readonly IOperatorMenuLauncher _menuLauncher;
        private readonly ISystemDisableManager _systemDisable;
        private readonly IReportTransactionQueueService _transactionService;
        private readonly IGamePlayState _playState;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EgmStatusHandler" /> class.
        /// </summary>
        /// <param name="propertiesManager">the properties manager</param>
        /// <param name="doorMonitor">the monitor for EGM doors</param>
        /// <param name="menuLauncher">interface to the Operator Menu</param>
        /// <param name="systemDisable">provides access to system disabled reasons</param>
        /// <param name="playState">interface to the game state</param>
        /// <param name="transactionService"></param>
        public EgmStatusHandler(
            IPropertiesManager propertiesManager,
            IDoorMonitor doorMonitor,
            IGamePlayState playState,
            IOperatorMenuLauncher menuLauncher,
            ISystemDisableManager systemDisable,
            IReportTransactionQueueService transactionService)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _doorMonitor = doorMonitor ?? throw new ArgumentNullException(nameof(doorMonitor));
            _menuLauncher = menuLauncher ?? throw new ArgumentNullException(nameof(menuLauncher));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _playState = playState ?? throw new ArgumentNullException(nameof(playState));
        }

        /// <inheritdoc />
        public EgmStatusFlag GetCurrentEgmStatus()
        {
            return GetSerialNumberStatus() |
                   GetPrinterStatus() |
                   GetDoorStatus() |
                   GetOperatorMenuStatus() |
                   GetSystemDisableStatus() |
                   GetNoteAcceptorStatus() |
                   GetReelControllerStatus() |
                   GetPlayStateStatus() |
                   GetBatteryStatus();
        }

        private static EgmStatusFlag GetPrinterStatus()
        {
            var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
            if (printer == null)
            {
                return EgmStatusFlag.None;
            }

            var flags = EgmStatusFlag.None;
            if (printer.PaperState == PaperStates.Empty)
            {
                flags |= EgmStatusFlag.PrnNoPaper;
            }

            if (printer.Faults != PrinterFaultTypes.None)
            {
                flags |= EgmStatusFlag.PrinterError;
            }

            if (printer.LogicalState == PrinterLogicalState.Printing)
            {
                flags |= EgmStatusFlag.Printing;
            }

            return flags;
        }

        private static EgmStatusFlag GetNoteAcceptorStatus()
        {
            var noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            return noteAcceptor is null || noteAcceptor.Faults == NoteAcceptorFaultTypes.None
                ? EgmStatusFlag.None
                : EgmStatusFlag.DbaError;
        }

        private static EgmStatusFlag GetReelControllerStatus()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
            if (reelController?.ConnectedReels == null)
            {
                return EgmStatusFlag.None;
            }

            var reelFaults = reelController.Faults;
            foreach (var reelId in reelController.ConnectedReels)
            {
                if (reelFaults.TryGetValue(reelId, out var fault) && fault != ReelFaults.None)
                {
                    return EgmStatusFlag.ReelMalfunction;
                }
            }

            return EgmStatusFlag.None;
        }

        private EgmStatusFlag GetBatteryStatus()
        {
            return !_propertiesManager.GetValue(HardwareConstants.Battery1Low, true) ||
                !_propertiesManager.GetValue(HardwareConstants.Battery2Low, true)
                ? EgmStatusFlag.NvramBatteryLow : EgmStatusFlag.None;
        }

        private EgmStatusFlag GetSerialNumberStatus()
        {
            return string.IsNullOrEmpty(_propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty)) ? EgmStatusFlag.NotEnrolled : EgmStatusFlag.None;
        }

        private EgmStatusFlag GetDoorStatus() {
            return _doorMonitor.GetLogicalDoors().ContainsValue(true) ? EgmStatusFlag.DoorOpen : EgmStatusFlag.None;
        }

        private EgmStatusFlag GetOperatorMenuStatus()
        {
            return _menuLauncher.IsShowing ? EgmStatusFlag.InOperatorMenu : EgmStatusFlag.None;
        }

        private EgmStatusFlag GetSystemDisableStatus()
        {
            var flags = EgmStatusFlag.None;
            var currentDisableKeys = _systemDisable.CurrentDisableKeys;
            if (currentDisableKeys.Contains(ApplicationConstants.OperatingHoursDisableGuid))
            {
                flags |= EgmStatusFlag.Operator;
            }

            if (currentDisableKeys.Any(
                    guid => guid == ApplicationConstants.DisabledByHost0Key ||
                            guid == ApplicationConstants.DisabledByHost1Key))
            {
                flags |= EgmStatusFlag.DisabledByCmsBackend;
            }

            if (_transactionService.IsFull)
            {
                flags |= EgmStatusFlag.TxLogFull;
            }

            return flags;
        }

        private EgmStatusFlag GetPlayStateStatus()
        {
            return _playState.Enabled ? EgmStatusFlag.None : EgmStatusFlag.MachineDisabled;
        }
    }
}