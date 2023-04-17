namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Events;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;

    public enum PrintButtonStatus
    {
        Print,
        Cancel,
        PrintDisabled,
        CancelDisabled
    }
    public class OperatorMenuPrintHandler : IDisposable
    {
        private enum PrinterStatus
        {
            Disabled,
            NotReady,
            Ready,
            [PrinterStatusPrinting]
            PrintingCancellable,
            [PrinterStatusPrinting]
            PrintingNotCancellable,
            [PrinterStatusPrinting]
            Cancelling
        }

        private readonly Dictionary<string, string> _errorEventText = new Dictionary<string, string>
        {
            { PrinterFaultTypes.PaperJam.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PaperJamText) },
            { PrinterFaultTypes.PrintHeadDamaged.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HeadErrorText) },
            { PrinterFaultTypes.PaperEmpty.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PaperOutText) },
            { PrinterFaultTypes.ChassisOpen.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DrawerOpenText) },
            { PrinterFaultTypes.OtherFault.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SystemErrorText) },
            { PrinterFaultTypes.NvmFault.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NVRamFailureText) },
            { PrinterFaultTypes.TemperatureFault.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TemperatureErrorText) },
            { PrinterFaultTypes.PaperNotTopOfForm.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MissingSupplyIndexText) },
            { PrinterFaultTypes.FirmwareFault.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FlashProgErrorText) },
            { PrinterFaultTypes.PrintHeadOpen.ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrinterOpenText) },
            { typeof(InspectionFailedEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionFailedText) },
            { typeof(ResolverErrorEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ResolverErrorText) },
            { typeof(DisconnectedEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OfflineText) }
        };

        private const int PrintDelay = 1000;
        private const int PrinterRetryCount = 5;
        private const string Seperators = ", ";

        private readonly IEventBus _eventBus;
        private IPrinter _printer;
        private string _printJobProcessMessage = string.Empty;
        private string _printerStatusMessage;
        private bool _cancelPrint;
        private PrinterStatus? _printerStatus;
        private PrintButtonStatus _printButtonStatus;
        private readonly object _lockObject = new object();
        private bool _isPrinting;
        private bool _printJobInterrupted;
        private bool _disposed;
        private readonly bool _initialized;
        private bool _externalPrintInProgress;

        public Action<string> PrinterStatusMessageUpdated;
        public Action<PrintButtonStatus> PrintButtonStatusUpdated;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public OperatorMenuPrintHandler()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<IPrinter>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuPrintHandler" /> class.
        /// </summary>
        public OperatorMenuPrintHandler(
            IEventBus eventBus,
            IPrinter printer)
        {
            _eventBus = eventBus;
            _printer = printer;

            SubscribeToEvents();
            if (_printer?.LogicalState == PrinterLogicalState.Printing)
            {
                PrintJobProcessMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Printing);
                UpdatePrinterStatus(PrinterStatus.PrintingNotCancellable);
            }
            else
            {
                UpdatePrinterStatus();
            }

            _initialized = true;
        }

        private PrintButtonStatus MainPrintButtonStatus
        {
            set
            {
                if (_printButtonStatus != value)
                {
                    _printButtonStatus = value;
                    PrintButtonStatusUpdated?.Invoke(_printButtonStatus);
                    Log.Debug($"Main Print Button Status Set: {_printButtonStatus}");
                }
            }
        }

        private string PrintJobProcessMessage
        {
            set
            {
                if (_printJobProcessMessage != value)
                {
                    _printJobProcessMessage = value;
                    UpdatePrinterStatusMessage();
                }
            }
        }

        public string PrinterStatusMessage
        {
            get => _printerStatusMessage;
            set
            {
                if (_printerStatusMessage != value)
                {
                    _printerStatusMessage = value;
                    PrinterStatusMessageUpdated?.Invoke(_printerStatusMessage);
                    Log.Debug($"Printer Status Message Set: {_printerStatusMessage}");
                }
            }
        }

        /// <summary>
        ///     Print a list of tickets and handle timeouts, cancelling, and errors
        /// </summary>
        public void PrintTickets(IEnumerable<Ticket> tickets)
        {
            lock (_lockObject)
            {
                if (_isPrinting)
                    return;

                _isPrinting = true;
                _cancelPrint = false;
            }
            Task.Run(() => PrintTicketsInternal(tickets));
        }

        private bool PrinterReady => (_printer?.Enabled ?? false) && (_printer?.CanPrint ?? false);

        public void CancelPrint()
        {
            lock (_lockObject)
            {
                _cancelPrint = true;
            }
            UpdatePrinterStatus(PrinterStatus.Cancelling);
            PrintJobProcessMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrintCancelling);
        }

        private void PrintTicketsInternal(IEnumerable<Ticket> ticketData)
        {
            var tickets = ticketData?.ToList();

            if (tickets == null || !tickets.Any())
            {
                UpdatePrinterStatus();
                return;
            }

            var printTotalPageCount = tickets.Count;
            var printCurrentPageNumber = 1;

            for (; printCurrentPageNumber <= printTotalPageCount; printCurrentPageNumber++)
            {
                if (_cancelPrint)
                {
                    Log.Info("Print Event Log cancelled");
                    _eventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
                    _eventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Printer));
                    break;
                }

                var printFailed = false;
                for (var retry = 1; retry <= PrinterRetryCount && !_cancelPrint; retry++)
                {
                    if (PrintTicket(tickets[printCurrentPageNumber - 1], printCurrentPageNumber, printTotalPageCount))
                    {
                        Thread.Sleep(PrintDelay);
                        break;
                    }

                    Thread.Sleep(PrintDelay);

                    if (retry == PrinterRetryCount)
                    {
                        printFailed = true;
                        Log.WarnFormat(
                            $"Unable to print page {printCurrentPageNumber} of {printTotalPageCount} after {PrinterRetryCount} attempts");
                    }
                    else
                    {
                        Log.WarnFormat(
                            $"Failed to print page {printCurrentPageNumber} of {printTotalPageCount} on attempt {retry} of {PrinterRetryCount}. Will attempt again.");
                    }
                }

                if (printFailed)
                {
                    _eventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
                    _eventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Printer));
                    break;
                }
            }

            if (!_cancelPrint && printCurrentPageNumber <= printTotalPageCount)
            {
                Log.WarnFormat("Only printed {0} of {1} pages", printCurrentPageNumber - 1, printTotalPageCount);
                _printJobInterrupted = true;
            }

            ClearPrintJobProcessMessage();

            lock (_lockObject)
            {
                _isPrinting = false;
                _cancelPrint = false;
            }

            UpdatePrinterStatus();
            _eventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
            _eventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Printer));
        }

        private bool PrintTicket(Ticket ticket, int page, int printNumberOfPages)
        {
            if (PrinterReady && ticket != null)
            {
                if (_printer?.CanPrint ?? false)
                {
                    var message = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrintMultiPageFormat), page, printNumberOfPages);
                    var printerStatus = page < printNumberOfPages
                        ? PrinterStatus.PrintingCancellable
                        : PrinterStatus.PrintingNotCancellable;

                    PrintJobProcessMessage = message;
                    UpdatePrinterStatus(printerStatus);

                    return _printer?.Print(ticket).Result ?? false;
                }
            }
            else
            {
                Log.Warn($"Printer is not available to print for type {GetType()}");
            }

            return false;
        }

        private void UpdatePrinterStatusMessage()
        {
            var lastError = GetLastPrintError();

            PrinterStatusMessage = !string.IsNullOrEmpty(lastError)
                ? lastError
                : _printer?.Enabled ?? false
                    ? _printJobInterrupted
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrintJobNotComplete)
                        : _printJobProcessMessage
                    : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Printer_Disabled);
        }

        private string GetLastPrintError()
        {
            var result = new StringBuilder();

            var errors = _printer?.LastError;
            if (!string.IsNullOrEmpty(errors))
            {
                var separator = false;
                foreach (var entry in _errorEventText)
                {
                    if (errors.Contains(entry.Key))
                    {
                        if (separator)
                        {
                            result.Append(Seperators);
                        }

                        result.Append(entry.Value);
                        separator = true;
                    }
                }
            }

            return result.ToString();
        }

        private void ClearPrintJobProcessMessage()
        {
            PrintJobProcessMessage = string.Empty;
        }

        private void UpdatePrinterStatus(PrinterStatus? status = null)
        {
            var lastStatus = _printerStatus;
            if (!_printer?.Enabled ?? false)
            {
                _printerStatus = PrinterStatus.Disabled;
            }
            else if (status.HasValue)
            {
                _printerStatus = status.Value;
            }
            else if (_printer?.CanPrint ?? false)
            {
                _printerStatus = PrinterStatus.Ready;
            }
            else
            {
                _printerStatus = PrinterStatus.NotReady;
            }

            switch (_printerStatus)
            {
                case PrinterStatus.Ready:
                    MainPrintButtonStatus = !_isPrinting ? PrintButtonStatus.Print : PrintButtonStatus.PrintDisabled;
                    break;
                case PrinterStatus.PrintingCancellable:
                    MainPrintButtonStatus = PrintButtonStatus.Cancel;
                    break;
                case PrinterStatus.Cancelling:
                    MainPrintButtonStatus = PrintButtonStatus.CancelDisabled;
                    break;
                default:
                    MainPrintButtonStatus = PrintButtonStatus.PrintDisabled;
                    break;
            }

            if (_printerStatus != lastStatus)
            {
                Log.Debug($"Operator Menu Printer Status changed to {_printerStatus} from {lastStatus}");
                UpdatePrinterStatusMessage();

                if (!_initialized && !IsPrintingState(lastStatus) && IsPrintingState(_printerStatus))
                {
                    // we only want to publish the event from here if we detect printing is in process
                    // otherwise it will be published by OperatorMenuViewModel.cs
                    _eventBus.Publish(new OperatorMenuPrintJobStartedEvent());
                    _externalPrintInProgress = true;
                }
                else if (_externalPrintInProgress && IsPrintingState(lastStatus) && !IsPrintingState(_printerStatus))
                {
                    _eventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
                }
            }
        }

        private void OnPrintJobStarted()
        {
            _printJobInterrupted = false;
            if (!_isPrinting)
            {
                //external command.  Shift us to non-cancellable print
                UpdatePrinterStatus(PrinterStatus.PrintingNotCancellable);
            }
        }

        private void OnPrintJobCompleted()
        {
            _externalPrintInProgress = false;
            ClearPrintStatus();

        }

        // Have to deal with external printing a bit differently to make sure
        // that we properly enable/disable when print events are generated
        // from outside this handler
        // _isPrinting == true indicates a print job generated by this handler 
        private void OnExternalPrintJobStarted()
        {
            if (!_isPrinting)
            {
                _externalPrintInProgress = true;
                _eventBus.Publish(new OperatorMenuPrintJobStartedEvent());
            }
        }

        private void OnExternalPrintJobCompleted()
        {
            if (!_isPrinting)
            {
                _eventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
            }
        }

        private void ClearPrintStatus()
        {
            if (!_isPrinting)
            {
                //external command.  Reset the printer state
                ClearPrintJobProcessMessage();
                UpdatePrinterStatus();
            }
        }

        private void SubscribeToEvents()
        {
            // Let individual pages handle enabled, etc. so they can determine Print button enable status
            _eventBus.Subscribe<InspectedEvent>(this, _ => UpdatePrinterStatus());
            _eventBus.Subscribe<EnabledEvent>(this, _ => UpdatePrinterStatus());
            _eventBus.Subscribe<DisabledEvent>(this, _ => UpdatePrinterStatus());
            _eventBus.Subscribe<HardwareFaultClearEvent>(this, _ => UpdatePrinterStatusMessage());
            _eventBus.Subscribe<OperatorMenuPrintJobStartedEvent>(this, _ => OnPrintJobStarted());
            _eventBus.Subscribe<OperatorMenuPrintJobCompletedEvent>(this, _ => OnPrintJobCompleted());
            _eventBus.Subscribe<PrintStartedEvent>(this, _ => OnExternalPrintJobStarted());
            _eventBus.Subscribe<PrintCompletedEvent>(this, _ => OnExternalPrintJobCompleted());
            _eventBus.Subscribe<ServiceAddedEvent>(this, UpdatePrinter);
        }

        private bool IsPrintingState(PrinterStatus? status)
        {
            if (!status.HasValue)
            {
                return false;
            }

            var flagAttribute = GetAttribute<PrinterStatusPrinting>(status);
            return flagAttribute != null;
        }

        private void UpdatePrinter(ServiceAddedEvent e)
        {
            if (e.ServiceType == typeof(IPrinter))
            {
                _printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
                UpdatePrinterStatus();
            }
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.UnsubscribeAll(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                UnsubscribeFromEvents();
            }

            _cancelPrint = true;
            _disposed = true;
        }

        private static T GetAttribute<T>(Enum en) where T : Attribute
        {
            var type = en.GetType();
            var memInfo = type.GetMember(en.ToString());
            if (memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(T), false);
                if (attrs.Length > 0)
                {
                    return (T)attrs[0];
                }
            }

            return null;
        }
    }

    public class PrinterStatusPrinting : Attribute
    {
    }
}
