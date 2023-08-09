namespace Aristocrat.Monaco.Hardware.Gds.NoteAcceptor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.Gds;
    using Contracts.Gds.NoteAcceptor;
    using Contracts.NoteAcceptor;
    using Contracts.SharedDevice;
    using log4net;

    /// <summary>A GDS note acceptor.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.GdsDeviceBase" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor.INoteAcceptorImplementation" />
    [HardwareDevice("GDS", DeviceType.NoteAcceptor)]
    [HardwareDevice("EBDS", DeviceType.NoteAcceptor)]
    [HardwareDevice("ID003", DeviceType.NoteAcceptor)]
    public class NoteAcceptorGds : GdsDeviceBase,
        INoteAcceptorImplementation
    {
        private const string JcmName = "JCM";
        private const string MeiName = "MEI";
        private const string GdsName = "GDS";
        private const string EbdsName = "EBDS";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ConcurrentDictionary<int, Note> _noteTable = new();
        private readonly object _stackerStatusLock = new();
        private bool _utf;
        private Validator _validator;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.NoteAcceptor.NoteAcceptorGds class.
        /// </summary>
        public NoteAcceptorGds(IGdsCommunicator communicator)
        {
            _communicator = communicator ?? throw new ArgumentNullException(nameof(communicator));
            DeviceType = DeviceType.NoteAcceptor;
            RegisterCallback<FailureStatus>(FailureReported);
            RegisterCallback<NumberOfNoteDataEntries>(NumberOfNoteDataEntriesReported);
            RegisterCallback<ReadNoteTable>(NoteTableRead);
            RegisterCallback<NoteValidated>(NoteValidatedReported);
            RegisterCallback<TicketValidated>(TicketValidatedReported);
            RegisterCallback<NoteOrTicketStatus>(NoteOrTicketStatusReported);
            RegisterCallback<StackerStatus>(StackerStatusReported);
            RegisterCallback<Metrics>(MetricsReported);
            RegisterCallback<UtfTicketValidated>(UtfTicketValidatedReported);
        }

        /// <summary>Gets the printer metrics.</summary>
        /// <value>The printer metrics.</value>
        public string NoteAcceptorMetrics { get; protected set; }

        /// <summary>Gets a value indicating if currently validating a ticket or note.</summary>
        /// <value>True if validating a ticket or note, false if not.</value>
        public bool IsValidating => _validator != null;

        /// <inheritdoc />
        public NoteAcceptorFaultTypes Faults { get; protected set; }

        /// <inheritdoc />
        public IEnumerable<INote> SupportedNotes => _noteTable.Values.ToArray();

        /// <inheritdoc />
        public IComConfiguration LastComConfiguration { get; protected set; }

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultCleared;

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultOccurred;

        /// <inheritdoc />
        public event EventHandler<NoteEventArgs> NoteAccepted;

        /// <inheritdoc />
        public event EventHandler<NoteEventArgs> NoteReturned;

        /// <inheritdoc />
        public event EventHandler<NoteEventArgs> NoteValidated;

        /// <inheritdoc />
        public event EventHandler NoteOrTicketStacking;

        /// <inheritdoc />
        public event EventHandler<TicketEventArgs> TicketAccepted;

        /// <inheritdoc />
        public event EventHandler<TicketEventArgs> TicketReturned;

        /// <inheritdoc />
        public event EventHandler<TicketEventArgs> TicketValidated;

        /// <inheritdoc />
        public event EventHandler<EventArgs> NoteOrTicketRejected;

        /// <inheritdoc />
        public event EventHandler<EventArgs> NoteOrTicketRemoved;

        /// <inheritdoc />
        public event EventHandler<EventArgs> UnknownDocumentReturned;

        /// <inheritdoc cref="IHardwareDevice.SelfTest" />
        public override async Task<bool> SelfTest(bool nvm)
        {
            SendCommand(
                new SelfTest { Nvm = nvm ? 1 : 0 });
            var report = await WaitForReport<FailureStatus>();
            if (report == null)
            {
                return false;
            }

            if (report.FirmwareError || report.MechanicalError || report.OpticalError || report.ComponentError ||
                report.DiagnosticCode)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc cref="IHardwareDevice.UpdateConfiguration" />
        public override void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
            if (internalConfiguration.Manufacturer.Equals(JcmName) && internalConfiguration.Protocol.Contains(GdsName))
            {
                SetJcmConfigurations(internalConfiguration);
            }
            else if (internalConfiguration.Manufacturer.Equals(MeiName) &&
                     !internalConfiguration.Protocol.Equals(EbdsName))
            {
                SetMeiConfigurations(internalConfiguration);
            }
            else
            {
                base.UpdateConfiguration(internalConfiguration);
            }
        }

        /// <inheritdoc />
        public virtual async Task<string> ReadMetrics()
        {
            NoteAcceptorMetrics = string.Empty;
            SendCommand<Metrics>(new GdsSerializableMessage(GdsConstants.ReportId.NoteAcceptorReadMetrics));
            NoteAcceptorMetrics = await WaitForDataReport<Metrics>();
            return NoteAcceptorMetrics;
        }

        /// <inheritdoc />
        public virtual async Task<bool> AcceptNote()
        {
            Note note = _validator;
            if (note == null)
            {
                return false;
            }

            return await AcceptNoteOrTicket();
        }

        /// <inheritdoc />
        public virtual async Task<bool> AcceptTicket()
        {
            string barcode = _validator;
            if (barcode == null)
            {
                return false;
            }

            return await AcceptNoteOrTicket();
        }

        /// <inheritdoc />
        public virtual async Task<bool> Return()
        {
            return await ReturnNoteOrTicket();
        }

        /// <summary>Reads note table.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        public virtual async Task<bool> ReadNoteTable()
        {
            _noteTable.Clear();
            var count = await GetNoteTableEntriesCount();
            if (count <= 0)
            {
                return false;
            }

            SendCommand<ReadNoteTable>(new GdsSerializableMessage(GdsConstants.ReportId.NoteAcceptorReadNoteTable));
            while (true)
            {
                var report = await WaitForReport<ReadNoteTable>();
                if (report == null)
                {
                    Logger.Warn("ReadNoteTable: could not read note table");
                    return false;
                }

                if (report.NoteId >= count)
                {
                    break;
                }
            }

            return true;
        }

        /// <summary>Gets note table entries count.</summary>
        /// <returns>An asynchronous result that yields the note table entries count.</returns>
        public virtual async Task<int> GetNoteTableEntriesCount()
        {
            SendCommand<NumberOfNoteDataEntries>(
                new GdsSerializableMessage(GdsConstants.ReportId.NoteAcceptorNumberOfNoteDataEntries));
            var report = await WaitForReport<NumberOfNoteDataEntries>();
            return report?.Number ?? 0;
        }

        /// <summary>Extend timeout.</summary>
        public virtual void ExtendTimeout()
        {
            SendCommand(new GdsSerializableMessage(GdsConstants.ReportId.NoteAcceptorExtendTimeout));
        }

        /// <summary>Accept note or ticket.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        public virtual async Task<bool> AcceptNoteOrTicket()
        {
            NoteOrTicketStacking?.Invoke(this, null);
            SendCommand<NoteOrTicketStatus>(
                new GdsSerializableMessage(GdsConstants.ReportId.NoteAcceptorAcceptNoteOrTicket));
            var report = await WaitForReport<NoteOrTicketStatus>();
            return report?.Accepted ?? false;
        }

        /// <summary>Returns note or ticket.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        public virtual async Task<bool> ReturnNoteOrTicket()
        {
            SendCommand<NoteOrTicketStatus>(
                new GdsSerializableMessage(GdsConstants.ReportId.NoteAcceptorReturnNoteOrTicket));
            var report = await WaitForReport<NoteOrTicketStatus>();
            return report?.Returned ?? false;
        }

        /// <summary>Executes the note or ticket accepted action.</summary>
        /// <returns>An asynchronous result.</returns>
        protected virtual Task OnNoteOrTicketAccepted()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        if (!IsValidating)
                        {
                            return;
                        }

                        string barcode = _validator;
                        if (barcode != null)
                        {
                            OnTicketAccepted(
                                new TicketEventArgs { Barcode = barcode });
                        }

                        Note note = _validator;
                        if (note != null)
                        {
                            OnNoteAccepted(
                                new NoteEventArgs { Note = note });
                        }
                    }
                    finally
                    {
                        KillValidator();
                    }
                });
        }

        /// <summary>Executes the note or ticket returned action.</summary>
        /// <returns>An asynchronous result.</returns>
        protected virtual Task OnNoteOrTicketReturned()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        if (!IsValidating)
                        {
                            // unknown document type returned - known documents are handled through other handlers
                            OnUnknownDocumentReturned();
                        }

                        string barcode = _validator;
                        if (barcode != null)
                        {
                            OnTicketReturned(
                                new TicketEventArgs { Barcode = barcode });
                        }

                        Note note = _validator;
                        if (note != null)
                        {
                            OnNoteReturned(
                                new NoteEventArgs { Note = note });
                        }
                    }
                    finally
                    {
                        KillValidator();
                    }
                });
        }

        /// <summary>
        ///     Updates the fault flags for this device.
        /// </summary>
        /// <param name="fault">The fault.</param>
        /// <param name="set">True to set; otherwise fault will be cleared.</param>
        protected virtual void SetFault(NoteAcceptorFaultTypes fault, bool set)
        {
            if (!set)
            {
                var cleared = Faults & fault;
                if (cleared == NoteAcceptorFaultTypes.None)
                {
                    // no updates
                    return;
                }

                Faults &= ~fault;
                Logger.Info($"SetFault: fault cleared {cleared}");
                OnFaultCleared(
                    new FaultEventArgs { Fault = cleared });
            }
            else
            {
                var toggle = Faults ^ fault;
                if (toggle == NoteAcceptorFaultTypes.None)
                {
                    // no updates
                    return;
                }

                Faults |= fault;
                Logger.Warn($"SetFault: fault set {toggle}");
                OnFaultOccurred(
                    new FaultEventArgs { Fault = toggle });
            }
        }

        /// <summary>Raises the <see cref="FaultCleared" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnFaultCleared(FaultEventArgs e)
        {
            FaultCleared?.Invoke(this, e);

            if (e.Fault == NoteAcceptorFaultTypes.StackerDisconnected
                && Faults.HasFlag(NoteAcceptorFaultTypes.CheatDetected))
            {
                SetFault(NoteAcceptorFaultTypes.CheatDetected, false);
            }
        }

        /// <summary>Raises the <see cref="FaultOccurred" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnFaultOccurred(FaultEventArgs e)
        {
            FaultOccurred?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="NoteAccepted" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnNoteAccepted(NoteEventArgs e)
        {
            NoteAccepted?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="NoteReturned" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnNoteReturned(NoteEventArgs e)
        {
            NoteReturned?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="NoteValidated" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnNoteValidated(NoteEventArgs e)
        {
            CreateValidator(e.Note as Note);
            Task.Run(() => { NoteValidated?.Invoke(this, e); }).FireAndForget(
                ex => { Logger.Error($"OnNoteValidated: Exception occurred {ex}"); });
        }

        /// <summary>Raises the <see cref="TicketAccepted" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnTicketAccepted(TicketEventArgs e)
        {
            TicketAccepted?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="TicketReturned" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnTicketReturned(TicketEventArgs e)
        {
            TicketReturned?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="TicketValidated" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnTicketValidated(TicketEventArgs e)
        {
            CreateValidator(e.Barcode);

            Task.Run(() => { TicketValidated?.Invoke(this, e); }).FireAndForget(
                ex => { Logger.Error($"OnTicketValidated: Exception occurred {ex}"); });
        }

        /// <summary>Raises the <see cref="NoteOrTicketRejected" /> event.</summary>
        protected virtual void OnNoteOrTicketRejected()
        {
            NoteOrTicketRejected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="NoteOrTicketRemoved" /> event.</summary>
        protected virtual void OnNoteOrTicketRemoved()
        {
            NoteOrTicketRemoved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="UnknownDocumentReturned" /> event.</summary>
        protected virtual void OnUnknownDocumentReturned()
        {
            UnknownDocumentReturned?.Invoke(this, EventArgs.Empty);
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

            if (report.MechanicalError)
            {
                Logger.Error("FailureReported: Mechanical components have failed");
            }

            if (report.OpticalError)
            {
                Logger.Error("FailureReported: Optical components have failed");
            }

            if (report.ComponentError)
            {
                Logger.Error("FailureReported: One or more components have failed");
            }

            if (report.NvmError)
            {
                Logger.Error("FailureReported: NVM failed");
            }

            if (report.DiagnosticCode)
            {
                Logger.Error($"FailureReported: Diagnostic code {report.ErrorCode}");
            }

            SetFault(NoteAcceptorFaultTypes.FirmwareFault, report.FirmwareError);
            SetFault(NoteAcceptorFaultTypes.MechanicalFault, report.MechanicalError);
            SetFault(NoteAcceptorFaultTypes.OpticalFault, report.OpticalError);
            SetFault(NoteAcceptorFaultTypes.ComponentFault, report.ComponentError);
            SetFault(NoteAcceptorFaultTypes.NvmFault, report.NvmError);
            SetFault(NoteAcceptorFaultTypes.OtherFault, report.DiagnosticCode);

            PublishReport(report);
        }

        /// <summary>Number of note data entries reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void NumberOfNoteDataEntriesReported(NumberOfNoteDataEntries report)
        {
            Logger.Debug($"NumberOfNoteDataEntriesReported: {report}");
            PublishReport(report);
        }

        /// <summary>Note descriptor reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void NoteTableRead(ReadNoteTable report)
        {
            Logger.Debug($"NoteTableRead: {report}");

            var scale = 1;
            for (var i = 0; i < report.Scalar; i++)
            {
                scale *= 10;
            }

            var denom = report.Value * scale * (!report.Sign ? -1 : 1);
            var note = new Note
            {
                NoteId = report.NoteId, ISOCurrencySymbol = report.Currency, Value = denom, Version = report.Version
            };

            _noteTable[note.NoteId] = note;
            PublishReport(report);
        }

        /// <summary>Note validated reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void NoteValidatedReported(NoteValidated report)
        {
            Logger.Debug($"NoteValidatedReported: {report}");
            Ack(report.TransactionId, false);
            if (!_noteTable.TryGetValue(report.NoteId, out var note))
            {
                return;
            }

            var e = new NoteEventArgs { Note = note };
            OnNoteValidated(e);
        }

        /// <summary>Ticket validated reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void TicketValidatedReported(TicketValidated report)
        {
            Logger.Debug($"TicketValidatedReported: {report}");
            Ack(report.TransactionId, false);

            OnTicketValidated(
                new TicketEventArgs { Barcode = report.Code });
        }

        /// <summary>Note or ticket status reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void NoteOrTicketStatusReported(NoteOrTicketStatus report)
        {
            Logger.Debug($"NoteOrTicketStatusReported: {report}");
            Ack(report.TransactionId, false);
            PublishReport(report);

            if (report.Accepted)
            {
                OnNoteOrTicketAccepted().FireAndForget(
                    e => { Logger.Error($"OnNoteOrTicketAccepted: Exception occurred {e}"); });
            }

            if (report.Returned)
            {
                OnNoteOrTicketReturned().FireAndForget(
                    e => { Logger.Error($"OnNoteOrTicketReturned: Exception occurred {e}"); });
            }

            if (report.Rejected)
            {
                OnNoteOrTicketRejected();
            }

            if (report.Removed)
            {
                OnNoteOrTicketRemoved();
            }

            if (report.Cheat)
            {
                Logger.Warn("NoteOrTicketStatusReported: cheat detected");
            }

            SetFault(NoteAcceptorFaultTypes.CheatDetected, report.Cheat);
            SetFault(NoteAcceptorFaultTypes.NoteJammed, report.Jam);
        }

        /// <summary>Stacker status reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void StackerStatusReported(StackerStatus report)
        {
            Logger.Debug($"StackerStatusReported: {report}");
            Ack(report.TransactionId, false);

            lock (_stackerStatusLock)
            {
                if (report.Disconnect)
                {
                    Logger.Error("StackerStatusReported: Stacker disconnected");
                }

                if (report.Full)
                {
                    Logger.Error("StackerStatusReported: Stacker full");
                }

                if (report.Jam)
                {
                    Logger.Error("StackerStatusReported: Mechanical components have failed");
                }

                if (report.Fault)
                {
                    Logger.Error("StackerStatusReported: Stacker is faulted");
                }

                SetFault(NoteAcceptorFaultTypes.StackerDisconnected, report.Disconnect);
                SetFault(NoteAcceptorFaultTypes.StackerFull, report.Full);
                SetFault(NoteAcceptorFaultTypes.StackerJammed, report.Jam);
                SetFault(NoteAcceptorFaultTypes.StackerFault, report.Fault);
            }

            PublishReport(report);
        }

        /// <summary>Metrics reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void MetricsReported(Metrics report)
        {
            Logger.Debug($"MetricsReported: {report}");
            PublishReport(report);
        }

        /// <summary>UTF ticket validated reported.</summary>
        /// <param name="report">The report.</param>
        protected virtual void UtfTicketValidatedReported(UtfTicketValidated report)
        {
            Logger.Debug($"UtfTicketValidatedReported: {report}");
            PublishReport(report);
            ReadValidationId().FireAndForget(e => { Logger.Error($"ReadValidationId: Exception occurred {e}"); });
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_validator != null)
                {
                    _validator.Dispose();
                    _validator = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override async Task<bool> Reset()
        {
            if (!await Disable())
            {
                return false; // first disable the device
            }

            if (!await ReadNoteTable())
            {
                return false;
            }

            if (await CalculateCrc(GdsConstants.DefaultSeed) == 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(await RequestGatReport()))
            {
                return false;
            }

            return true;
        }

        private static bool Exchange(ref bool location1, bool location2)
        {
            const int true32 = 1;
            const int false32 = 0;
            var state1 = location1 ? true32 : false32;
            var result = Interlocked.Exchange(ref state1, location2 ? true32 : false32) == true32;
            location1 = state1 == true32;
            return result;
        }

        private static (string major, string minor) GetMeiVersionNumber(string partNumber)
        {
            const int versionLength = 3;
            const int minorVersionLength = 2;
            if (string.IsNullOrEmpty(partNumber) || partNumber.Length < versionLength)
            {
                return (string.Empty, string.Empty);
            }

            var majorVersion = partNumber.Substring(partNumber.Length - versionLength, 1);
            var minorVersion = partNumber.Substring(partNumber.Length - minorVersionLength);
            return (majorVersion, minorVersion);
        }

        private static void SetFirmwareValues(IDeviceConfiguration internalConfiguration, string firmwareIdAndRevision)
        {
            // Parse the fourth item in the interface descriptor string as the firmware Id and
            // variant version.
            var firmwareIdAndFirmwareRevisionArray = firmwareIdAndRevision.Split('-');
            if (firmwareIdAndFirmwareRevisionArray.Length >= 2)
            {
                internalConfiguration.FirmwareId = firmwareIdAndFirmwareRevisionArray[0];
                internalConfiguration.FirmwareRevision = firmwareIdAndFirmwareRevisionArray[1];
                Logger.DebugFormat(
                    $"FirmwareId: {internalConfiguration.FirmwareId}, FirmwareRevision: {internalConfiguration.FirmwareRevision}");
            }
        }

        private void CreateValidator(string barcode)
        {
            KillValidator();

            _validator = new Validator(barcode);
            _validator.TimerElapsed += (source, e) => { ExtendTimeout(); };
        }

        private void CreateValidator(Note note)
        {
            KillValidator();

            _validator = new Validator(note);
            _validator.TimerElapsed += (source, e) => { ExtendTimeout(); };
        }

        private void KillValidator()
        {
            _validator?.Dispose();
            _validator = null;
        }

        private async Task ReadValidationId()
        {
            if (Exchange(ref _utf, true))
            {
                return; // already reading
            }

            var barcode = await WaitForDataReport<UtfTicketValidated>();
            Exchange(ref _utf, false);

            OnTicketValidated(
                new TicketEventArgs { Barcode = barcode });
        }

        private void SetMeiConfigurations(IDeviceConfiguration internalConfiguration)
        {
            // Yes, parse firmware id and variant from GAT report.
            var firmwareId = string.Empty;
            var firmwareRevision = string.Empty;
            var variantVersion = string.Empty;

            var gatDataReportArray = GatReport.Split('_');

            // Do we have a valid MEI GAT data report?
            if (gatDataReportArray.Length >= 5)
            {
                // Yes, set the model, firmware Id, variant name, and  variant version.
                internalConfiguration.Model = gatDataReportArray[1];
                (firmwareId, firmwareRevision) = GetMeiVersionNumber(gatDataReportArray[2]);
                internalConfiguration.VariantName = gatDataReportArray[3];
                var (major, minor) = GetMeiVersionNumber(gatDataReportArray[4]);
                variantVersion = $"{major}.{minor}";
            }
            else
            {
                Logger.Warn($"Malformed MEI GAT data report: {GatReport}");
            }

            // Set the protocol as GDS.
            internalConfiguration.Protocol = GdsName;
            Logger.Debug($"Model: {internalConfiguration.Model}, Protocol: {internalConfiguration.Protocol}");

            internalConfiguration.FirmwareId = firmwareId;
            internalConfiguration.VariantVersion = variantVersion;
            internalConfiguration.FirmwareRevision = firmwareRevision;
            Logger.Debug(
                $"FirmwareId: {internalConfiguration.FirmwareId}, FirmwareRevision: {internalConfiguration.FirmwareRevision}, VariantVersion: {internalConfiguration.VariantVersion}");
        }

        private void SetJcmConfigurations(IDeviceConfiguration internalConfiguration)
        {
            // Yes, parse rom Id (model and protocol), firmware id and revision from GAT report.
            var romId = string.Empty;
            var firmwareIdAndRevision = string.Empty;

            var gatDataReportArray = GatReport.Split(':');

            // Do we have a valid JCM GAT data report?
            if (gatDataReportArray.Length >= 3)
            {
                var romIdArray = gatDataReportArray[1].Split('\r');
                if (romIdArray.Length >= 2)
                {
                    romId = romIdArray[0];
                }
                else
                {
                    Logger.Warn($"Malformed JCM GAT data report[1]: {gatDataReportArray[1]}");
                }

                var firmwareIdAndRevisionArray = gatDataReportArray[2].Split(' ');
                if (firmwareIdAndRevisionArray.Length >= 2)
                {
                    firmwareIdAndRevision = firmwareIdAndRevisionArray[0];
                }
                else
                {
                    Logger.Warn($"Malformed JCM GAT data report[2]: {gatDataReportArray[2]}");
                }
            }
            else
            {
                Logger.Warn(
                    $"Malformed JCM GAT data report: {GatReport} using communicator values{internalConfiguration.Model}");

                var communicatorValues = internalConfiguration.Model?.Split(' ', ',');
                if (communicatorValues?.Length > 3)
                {
                    internalConfiguration.Model = communicatorValues[2];
                    Logger.DebugFormat(
                        $"Model: {internalConfiguration.Model}, Protocol: {internalConfiguration.Protocol}");
                }

                if (communicatorValues?.Length > 5)
                {
                    SetFirmwareValues(internalConfiguration, communicatorValues[4]);
                }

                return;
            }

            // Yes, parse the third item in the interface descriptor string as the model and
            // protocol.
            var modelAndProtocolArray = romId.Split(' ');
            if (modelAndProtocolArray.Length >= 2)
            {
                var model = modelAndProtocolArray[0];

                // *NOTE* The JCM iVizion model string is slightly inconsistent between the InterfaceDescriptorString and the GAT report, it will
                // be i(USA)100-SS in the InterfaceDescriptorString and i(USA)-100-SS in the GAT report.  We add the '-' after the ')' if it is not
                // there so that it will pass minimum requirements during discovery evaluation.
                if (model.Contains(")-") == false)
                {
                    model = model.Replace(")", ")-");
                }

                internalConfiguration.Model = model;

                Logger.DebugFormat($"Model: {internalConfiguration.Model}, Protocol: {internalConfiguration.Protocol}");
            }

            SetFirmwareValues(internalConfiguration, firmwareIdAndRevision);
        }
    }
}