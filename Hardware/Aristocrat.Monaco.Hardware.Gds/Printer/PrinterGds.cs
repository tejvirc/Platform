namespace Aristocrat.Monaco.Hardware.Gds.Printer
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Gds;
    using Contracts.Gds.Printer;
    using Contracts.Printer;
    using Contracts.SharedDevice;
    using log4net;

    /// <summary>A printer gds.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.GdsDeviceBase" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Printer.IPrinterImplementation" />
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "False positive.  IDisposable is inherited via IFunctionality.  See http://stackoverflow.com/questions/8925925/code-analysis-ca1063-fires-when-deriving-from-idisposable-and-providing-implemen for details.")]
    public class PrinterGds : GdsDeviceBase,
        IPrinterImplementation
    {
        private const byte TransferSuccessCode = 0x01;
        private const string Transact = "TransAct";
        private const string NanoptixPrinter = "NEXTGEN PAYCHECK";
        private const string FutureLogic = "FutureLogic";
        private const string Jcm = "JCM";
        private const string FutureLogicInc = "FUTURELOGICinc";
        private const string Gen2 = "gen2";
        private const string Gen5 = "gen5";
        private const string Gen2Universal = "Gen2 Universal";
        private const string Gen5Universal = "Gen5 Universal";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private bool _disposed;
        private bool _printing;
        private CancellationTokenSource _printerCancellationTokenSource;

        /// <summary>Initializes a new instance of the Aristocrat.Monaco.Hardware.Printer.PrinterGds class.</summary>
        public PrinterGds()
        {
            DeviceType = DeviceType.Printer;
            RegisterCallback<FailureStatus>(FailureReported);
            RegisterCallback<TicketPrintStatus>(TicketPrintStatusReported);
            RegisterCallback<TransferStatus>(TransferStatusReported);
            RegisterCallback<Metrics>(MetricsReported);
            RegisterCallback<PrinterStatus>(PrinterStatusReported);
            Disconnected += PrinterGdsDisconnected;
        }

        /// <summary>
        ///     Gets a value indicating whether this Aristocrat.Monaco.Hardware.Printer.PrinterGds is print in progress.
        /// </summary>
        /// <value>True if this Aristocrat.Monaco.Hardware.Printer.PrinterGds is print in progress, false if not.</value>
        public bool IsPrinting
        {
            get => _printing;
            protected set
            {
                if (_printing != value)
                {
                    _printing = value;
                    if (_printing)
                    {
                        OnPrintInProgress();
                    }
                }
            }
        }

        /// <summary>Gets the printer metrics.</summary>
        /// <value>The printer metrics.</value>
        public string PrinterMetrics { get; protected set; }

        /// <inheritdoc />
        public bool CanRetract { get; protected set; }

        /// <inheritdoc />
        public PrinterFaultTypes Faults { get; protected set; }

        /// <inheritdoc />
        public PrinterWarningTypes Warnings { get; protected set; }

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultCleared;

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultOccurred;

        /// <inheritdoc />
        public event EventHandler<FieldOfInterestEventArgs> FieldOfInterestPrinted;

        /// <inheritdoc />
        public event EventHandler<EventArgs> PrintCompleted;

        /// <inheritdoc />
        public event EventHandler<EventArgs> PrintIncomplete;

        /// <inheritdoc />
        public event EventHandler<EventArgs> PrintInProgress;

        /// <inheritdoc />
        public event EventHandler<WarningEventArgs> WarningCleared;

        /// <inheritdoc />
        public event EventHandler<WarningEventArgs> WarningOccurred;

        /// <inheritdoc />
        public override async Task<bool> SelfTest(bool nvm)
        {
            if (IsEnabled) // this command is only allowed when the device is disabled
            {
                return false;
            }

            IsPrinting = false;

            SendCommand(new SelfTest { Nvm = nvm ? 1 : 0 });
            var report = await WaitForReport<FailureStatus>(ExtendedResponseTimeout);
            if (report == null)
            {
                return false;
            }

            if (report.FirmwareError || report.NvmError || report.PrintHeadDamaged || report.TemperatureError ||
                report.DiagnosticCode)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
            Logger.Debug($"Parsing string {internalConfiguration.Model}");
            var firmwareArray = internalConfiguration.Model.Split(',');
            if(internalConfiguration.Manufacturer.Equals(Transact))
            {
                if(firmwareArray.Length > 3)
                {
                    internalConfiguration.Model = firmwareArray[1];
                    internalConfiguration.FirmwareId = firmwareArray[0];
                    internalConfiguration.FirmwareRevision = firmwareArray[2];
                }
            }
            else if (internalConfiguration.Model.Contains(NanoptixPrinter))
            {
                var manufacturer = internalConfiguration.Manufacturer.Split(' ');
                if(manufacturer.Length > 0)
                {
                    internalConfiguration.Manufacturer = manufacturer[0];
                }

                internalConfiguration.Model = NanoptixPrinter;

                if (firmwareArray.Length >= 4)
                {
                    internalConfiguration.FirmwareId = firmwareArray[3];
                    internalConfiguration.FirmwareRevision = firmwareArray[2];
                }
            }
            else if (internalConfiguration.Manufacturer.Equals(FutureLogicInc) || firmwareArray.Length >= 3 &&
                     (internalConfiguration.Model.ToLower().Contains(Gen2) || internalConfiguration.Model.ToLower().Contains(Gen5)))
            {
                if (firmwareArray.Length >= 3)
                {
                    if (internalConfiguration.Model.ToLower().Contains(Gen2))
                    {
                        internalConfiguration.Manufacturer = FutureLogic;
                        internalConfiguration.Model = Gen2Universal;
                    }
                    else
                    {
                        internalConfiguration.Manufacturer = Jcm;
                        internalConfiguration.Model = Gen5Universal;
                    }

                    // Parse the firmware Id from the third element in the array.
                    internalConfiguration.FirmwareId = firmwareArray[2];

                    Logger.Debug(
                        $"Manufacturer: {internalConfiguration.Manufacturer} Model: {internalConfiguration.Model} FirmwareId: {internalConfiguration.FirmwareId} Protocol: {internalConfiguration.Protocol}");
                }
                else
                {
                    Logger.Warn($"Malformed firmware string: {internalConfiguration.Model}");
                }
            }
            else
            {
                base.UpdateConfiguration(internalConfiguration);
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> DefineRegion(string region)
        {
            Logger.Debug($"DefineRegion: sending {region}");
            if (region == null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            SendCommand<TransferStatus>(new DefineRegion { Data = region });
            var report = await WaitForReport<TransferStatus>();
            if (report == null || !report.RegionCode)
            {
                Logger.Debug($"DefineRegion: report null is {report is null} report is {report}");
                return false;
            }

            Logger.Debug($"region report status code is {report.StatusCode}");
            return report.StatusCode == TransferSuccessCode;
        }

        /// <inheritdoc />
        public virtual async Task<bool> DefineTemplate(string template)
        {
            Logger.Debug($"DefineTemplate: sending {template}");
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            SendCommand<TransferStatus>(new DefineTemplate { Data = template });
            var report = await WaitForReport<TransferStatus>();
            if (report == null || !report.TemplateCode)
            {
                Logger.Debug($"DefineTemplate: report null is {report is null} report is {report}");
                return false;
            }

            Logger.Debug($"template report status code is {report.StatusCode}");
            return report.StatusCode == TransferSuccessCode;
        }

        /// <inheritdoc />
        public virtual async Task<bool> PrintTicket(string ticket)
        {
            return await PrintTicket(ticket, null);
        }

        /// <inheritdoc />
        public virtual async Task<bool> PrintTicket(string ticket, Func<Task> onFieldOfInterest)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            if (!IsEnabled) // this command is only allowed when the device is enabled
            {
                if (!await Enable())
                {
                    return false;
                }
            }

            IsPrinting = true;

            using (var tokenSource = _printerCancellationTokenSource = new CancellationTokenSource())
            {
                SendCommand<TicketPrintStatus>(new PrintTicket { Data = ticket }, tokenSource.Token);
                var result = await WaitForPrintComplete(onFieldOfInterest, tokenSource.Token);

                IsPrinting = false;

                _printerCancellationTokenSource = null;
                return result;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> FormFeed()
        {
            if (IsPrinting) // this command cannot be sent while printing
            {
                return false;
            }

            IsPrinting = true;

            using (var tokenSource = _printerCancellationTokenSource = new CancellationTokenSource())
            {
                SendCommand<TicketPrintStatus>(new GdsSerializableMessage(GdsConstants.ReportId.PrinterFormFeed), tokenSource.Token);
                await WaitForPrintComplete(null, tokenSource.Token);
                _printerCancellationTokenSource = null;
            }

            IsPrinting = false;

            OnPrintCompleted();
            return true;
        }

        /// <summary>Request printer metrics.</summary>
        /// <returns>An asynchronous result that yields a string.</returns>
        public virtual async Task<string> ReadMetrics()
        {
            PrinterMetrics = string.Empty;

            SendCommand<Metrics>(new GdsSerializableMessage(GdsConstants.ReportId.PrinterRequestMetrics));
            PrinterMetrics = await WaitForDataReport<Metrics>();

            return PrinterMetrics;
        }

        /// <inheritdoc />
        public virtual async Task<bool> RetractTicket()
        {
            if (!CanRetract)
            {
                return false;
            }

            SendCommand<PrinterStatus>(new GdsSerializableMessage(GdsConstants.ReportId.PrinterTicketRetract));
            var report = await WaitForReport<PrinterStatus>();

            return report != null && report.TopOfForm;
        }

        /// <inheritdoc />
        public virtual async Task<bool> TransferFile(GraphicFileType graphicType, int graphicIndex, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (stream.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(stream));
            }

            if (graphicIndex > byte.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(graphicIndex));
            }

            SendCommand(
                new GraphicTransferSetup
                {
                    GraphicType = graphicType, GraphicIndex = (byte)graphicIndex, FileSize = (ushort)stream.Length
                });

            stream.Position = 0;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                SendCommand(new FileTransfer { Data = reader.ReadToEnd() });
            }

            var report = await WaitForReport<TransferStatus>();
            if (report == null || !report.GraphicCode)
            {
                return false;
            }

            return report.StatusCode == TransferSuccessCode;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Disconnected -= PrinterGdsDisconnected;
            }

            _disposed = true;
            // Because of "CA2213: Disposable fields should be disposed" error, we added dispose call
            if (_printerCancellationTokenSource != null)
            {
                _printerCancellationTokenSource.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override async Task<bool> Reset()
        {
            return await Disable() &&
                   await CalculateCrc(GdsConstants.DefaultSeed) != 0 &&
                   !string.IsNullOrEmpty(await RequestGatReport());
        }

        /// <summary>
        ///     Updates the fault flags for this device.
        /// </summary>
        /// <param name="fault">The fault.</param>
        /// <param name="set">True to set; otherwise fault will be cleared.</param>
        protected virtual void SetFault(PrinterFaultTypes fault, bool set)
        {
            if (!set)
            {
                var cleared = Faults & fault;
                if (cleared == PrinterFaultTypes.None)
                {
                    // no updates
                    return;
                }

                Faults &= ~fault;
                Logger.Info($"SetFault: fault cleared {cleared}");
                OnFaultCleared(new FaultEventArgs { Fault = cleared });
            }
            else
            {
                var toggle = Faults ^ fault;
                if (toggle == PrinterFaultTypes.None)
                {
                    // no updates
                    return;
                }

                Faults |= fault;
                Logger.Warn($"SetFault: fault set {toggle}");
                OnFaultOccurred(new FaultEventArgs { Fault = toggle });
            }
        }

        /// <summary>
        ///     Updates the warning flags for this device.
        /// </summary>
        /// <param name="warning">The warning.</param>
        /// <param name="set">True to set; otherwise warning will be cleared.</param>
        protected virtual void SetWarning(PrinterWarningTypes warning, bool set)
        {
            if (!set)
            {
                var cleared = Warnings & warning;
                if (cleared == PrinterWarningTypes.None)
                {
                    // no updates
                    return;
                }

                Warnings &= ~warning;
                Logger.Info($"SetWarning: warning cleared {cleared}");
                OnWarningCleared(new WarningEventArgs { Warning = cleared });
            }
            else
            {
                var toggle = Warnings ^ warning;
                if (toggle == PrinterWarningTypes.None)
                {
                    // no updates
                    return;
                }

                Warnings |= warning;
                Logger.Warn($"SetWarning: warning set {toggle}");
                OnWarningOccurred(new WarningEventArgs { Warning = toggle });
            }
        }

        /// <summary>Wait for print complete.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        protected async Task<bool>
            WaitForPrintComplete(Func<Task> onFieldOfInterest, CancellationToken token)
        {
            var notified = false;
            var retry = true;
            while (true)
            {
                var report = await WaitForReport<TicketPrintStatus>(token);
                if (report == null)
                {
                    CancelPrinting();
                    Logger.Error("WaitForPrintComplete: error occurred waiting for print completion");
                    return false;
                }

                if (report.FieldOfInterest1 && !notified)
                {
                    if (onFieldOfInterest != null)
                    {
                        await onFieldOfInterest();
                    }

                    notified = true;
                }

                if (report.PrintComplete)
                {
                    if (!notified && onFieldOfInterest != null)
                    {
                        await onFieldOfInterest();
                    }

                    Logger.Info("WaitForPrintComplete: print completed successfully");
                    return true;
                }

                if (report.PrintIncomplete)
                {
                    CancelPrinting();
                    Logger.Error("WaitForPrintComplete: print incomplete occurred waiting for print completion");
                    return false;
                }

                if (report.PrintInProgress)
                {
                    continue;
                }

                if (retry)
                {
                    retry = false;
                    continue;
                }

                CancelPrinting();
                Logger.Error("WaitForPrintComplete: print not inprogress occurred waiting for print completion");
                return false;
            }
        }

        /// <summary>Raises the <see cref="FaultCleared" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnFaultCleared(FaultEventArgs e)
        {
            FaultCleared?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="FaultOccurred" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnFaultOccurred(FaultEventArgs e)
        {
            FaultOccurred?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="WarningCleared" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnWarningCleared(WarningEventArgs e)
        {
            WarningCleared?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="WarningOccurred" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnWarningOccurred(WarningEventArgs e)
        {
            WarningOccurred?.Invoke(this, e);
        }

        /// <summary>Raises the fault event.</summary>
        protected virtual void OnFieldOfInterestPrinted(FieldOfInterestEventArgs e)
        {
            FieldOfInterestPrinted?.Invoke(this, e);
        }

        /// <summary>Raises the fault event.</summary>
        protected virtual void OnPrintCompleted()
        {
            PrintCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the fault event.</summary>
        protected virtual void OnPrintIncomplete()
        {
            PrintIncomplete?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the print in progress action.</summary>
        protected virtual void OnPrintInProgress()
        {
            PrintInProgress?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Called when a failure status report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void FailureReported(FailureStatus report)
        {
            Logger.Debug($"FailureReported: {report}");

            if (report.FirmwareError)
            {
                Logger.Error("FailureReported: Firmware failed");
            }

            if (report.NvmError)
            {
                Logger.Error("FailureReported: NVM failed");
            }

            if (report.PrintHeadDamaged)
            {
                Logger.Error("FailureReported: Print head damaged");
            }

            if (report.TemperatureError)
            {
                Logger.Error("FailureReported: Temperature error");
            }

            if (report.DiagnosticCode)
            {
                Logger.Error($"FailureReported: Diagnostic code {report.ErrorCode}");
            }

            SetFault(PrinterFaultTypes.FirmwareFault, report.FirmwareError);
            SetFault(PrinterFaultTypes.NvmFault, report.NvmError);
            SetFault(PrinterFaultTypes.PrintHeadDamaged, report.PrintHeadDamaged);
            SetFault(PrinterFaultTypes.TemperatureFault, report.TemperatureError);
            SetFault(PrinterFaultTypes.OtherFault, report.DiagnosticCode);

            PublishReport(report);
        }

        /// <summary>Called when a ticket print status report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void TicketPrintStatusReported(TicketPrintStatus report)
        {
            Logger.Debug($"TicketPrintStatus: {report}");
            Ack(report.TransactionId, false);

            PublishReport(report);

            IsPrinting = report.PrintInProgress;
            if (report.FieldOfInterest1)
            {
                OnFieldOfInterestPrinted(new FieldOfInterestEventArgs { FieldOfInterest = 1 });
            }

            if (report.FieldOfInterest2)
            {
                OnFieldOfInterestPrinted(new FieldOfInterestEventArgs { FieldOfInterest = 2 });
            }

            if (report.FieldOfInterest3)
            {
                OnFieldOfInterestPrinted(new FieldOfInterestEventArgs { FieldOfInterest = 3 });
            }

            if (report.PrintComplete)
            {
                OnPrintCompleted();
            }

            if (report.PrintIncomplete)
            {
                OnPrintIncomplete();
            }

            CanRetract = report.TicketRetractable && report.PrintIncomplete;
        }

        /// <summary>Called when a transfer status report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void TransferStatusReported(TransferStatus report)
        {
            Logger.Debug($"TransferStatusReported: {report}");
            PublishReport(report);
        }

        /// <summary>Metrics reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void MetricsReported(Metrics report)
        {
            Logger.Debug($"MetricsReported: {report}");
            PublishReport(report);
        }

        /// <summary>Printer status reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void PrinterStatusReported(PrinterStatus report)
        {
            Logger.Debug($"PrinterStatusReported: {report}");
            Ack(report.TransactionId, false);
            PublishReport(report);

            if (!report.PaperInChute)
            {
                Logger.Warn("PrinterStatusReported: no paper in chute");
            }

            if (report.PaperEmpty)
            {
                Logger.Error("PrinterStatusReported: paper is empty");
            }

            if (report.PaperLow)
            {
                Logger.Warn("PrinterStatusReported: paper low");
            }

            if (report.PaperJam)
            {
                Logger.Error("PrinterStatusReported: paper jam");
            }

            //  if we are printing ignore top of form
            var topOfForm = report.PaperEmpty || IsPrinting || report.TopOfForm;
            if (!topOfForm)
            {
                Logger.Error("PrinterStatusReported: paper is not top of form");
            }

            if (report.PrintHeadOpen)
            {
                Logger.Error("PrinterStatusReported: Print head open");
            }

            if (report.ChassisOpen)
            {
                Logger.Error("PrinterStatusReported: Print chassis open");
            }

            SetFault(PrinterFaultTypes.PaperEmpty, report.PaperEmpty);
            SetFault(PrinterFaultTypes.PaperJam, report.PaperJam);
            if (!IsPrinting && !report.TopOfForm)
            {
                // Setting this as a fault causes duplicate device state report requests (was a warning type before USB refactor)
                // This is getting set when re-loading paper after clearing the paper empty fault
                Logger.Info("(Platform ignores this fault) PrinterStatusReported: paper is not top of form");
                //SetFault(PrinterFaultTypes.PaperNotTopOfForm, !topOfForm);
            }

            SetFault(PrinterFaultTypes.PrintHeadOpen, report.PrintHeadOpen);
            SetFault(PrinterFaultTypes.ChassisOpen, report.ChassisOpen);

            SetWarning(PrinterWarningTypes.PaperLow, report.PaperLow);
            SetWarning(PrinterWarningTypes.PaperInChute, report.PaperInChute);
        }

        private void CancelPrinting()
        {
            _printerCancellationTokenSource?.Cancel();
        }

        private void PrinterGdsDisconnected(object sender, EventArgs e)
        {
            CancelPrinting();
        }
    }
}