namespace Aristocrat.Monaco.Hardware.Printer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.Persistence;
    using Contracts.Printer;
    using Contracts.SharedDevice;
    using Contracts.Ticket;
    using Contracts.TicketContent;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Serial.Printer;
    using Stateless;
    using Tickets;
    using TaskExtensions = Common.TaskExtensions;

    /// <summary>A printer adapter.</summary>
    /// <seealso
    ///     cref="T:Aristocrat.Monaco.Hardware.Contracts.SharedDevice.DeviceAdapter{Aristocrat.Monaco.Hardware.Contracts.Printer.IPrinterImplementation}" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Printer.IPrinter" />
    public class PrinterAdapter : DeviceAdapter<IPrinterImplementation>,
        IPrinter,
        IStorageAccessor<PrinterOptions>
    {
        private const string PrintableRegionsExtensionPath = "/Hardware/PrintableRegions";
        private const string PrintableTemplatesExtensionPath = "/Hardware/PrintableTemplates";
        private const string PrinterOverridesExtensionPath = "/Hardware/PrinterOverrides";
        private const string DeviceImplementationsExtensionPath = "/Hardware/Printer/PrinterImplementations";
        private const string RendererImplementationsExtensionPath = "/Hardware/Printer/Renderers";

        private const string OptionsBlock = "Aristocrat.Monaco.Hardware.Printer.PrinterAdapter.Options";
        private const string RenderTargetOption = "RenderTarget";
        private const string Nanoptix = "Nanoptix";
        private const string ActivationTimeText = "ActivationTime";
        private const string Printer = "Printer";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private ReaderWriterLockSlim _stateLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private IPrinterImplementation _printer;
        private Resolver _resolver;
        private DateTime _activationTime = DateTime.MinValue;
        private StateMachine<PrinterLogicalState, PrinterLogicalStateTrigger> _state;

        private bool _formFeedPending;
        private string _renderTarget = "PDL";

        /// <inheritdoc />
        protected override IPrinterImplementation Implementation => _printer;

        /// <inheritdoc />
        protected override string Description => Printer;

        /// <inheritdoc />
        protected override string Path => Constants.PrinterPath;

        /// <summary>Gets the full pathname of the printable region file.</summary>
        /// <value>The full pathname of the printable region file.</value>
        private string PrintableRegionPath { get; set; }

        /// <summary>Gets the full pathname of the printable template file.</summary>
        /// <value>The full pathname of the printable template file.</value>
        private string PrintableTemplatePath { get; set; }

        /// <summary>Gets or sets the renderer.</summary>
        /// <value>The renderer.</value>
        private ITemplateRenderer Renderer { get; set; }
        
        private bool IsSerial => DeviceConfiguration?.Mode?.Contains("RS232") ?? false;

        private bool IsJcm => DeviceConfiguration?.Manufacturer?.Contains("JCM") ?? false;

        private bool IsFutureLogic => DeviceConfiguration?.Manufacturer?.Contains("FutureLogic") ?? false;

        /// <inheritdoc />
        public int PrinterId { get; set; } = 1; // Default to deviceId 1 since 0 isn't valid in G2S

        /// <inheritdoc />
        public bool CanPrint => CanFire(PrinterLogicalStateTrigger.Printing);

        /// <inheritdoc />
        public override DeviceType DeviceType => DeviceType.Printer;

        /// <inheritdoc />
        public override bool Connected => !(_state?.State == PrinterLogicalState.Uninitialized ||
                                            _state?.State == PrinterLogicalState.Disconnected ||
                                            (!Implementation?.IsConnected ?? false));

#if !(RETAIL)
        /// <summary>
        /// Get printer implementation for automation.
        /// </summary>
        public IPrinterImplementation PrinterImplementation => Implementation;
#endif

        /// <inheritdoc />
        public PrinterLogicalState LogicalState => _state?.State ?? PrinterLogicalState.Uninitialized;

        /// <inheritdoc />
        public DateTime ActivationTime
        {
            get => _activationTime;
            set
            {
                if (_activationTime != value)
                {
                    _activationTime = value;
                    this.ModifyBlock(
                        OptionsBlock,
                        (transaction, index) =>
                        {
                            transaction[index, ActivationTimeText] = _activationTime;
                            return true;
                        },
                        PrinterId - 1);
                }
            }
        }

        /// <inheritdoc />
        public string RenderTarget
        {
            get => _renderTarget;
            set
            {
                if (_renderTarget != value)
                {
                    _renderTarget = value;
                    this.ModifyBlock(
                        OptionsBlock,
                        (transaction, index) =>
                        {
                            transaction[index, RenderTargetOption] = _renderTarget;
                            return true;
                        },
                        PrinterId - 1);

                    Renderer = null;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<PrintableRegion> Regions =>
            _resolver?.PrintableRegions?.Values.ToArray() ?? new PrintableRegion[] { };

        /// <inheritdoc />
        public IEnumerable<PrintableTemplate> Templates =>
            _resolver?.PrintableTemplates?.Values.ToArray() ?? new PrintableTemplate[] { };

        /// <inheritdoc />
        public PrinterFaultTypes Faults => Implementation?.Faults ?? PrinterFaultTypes.None;

        /// <inheritdoc />
        public PrinterWarningTypes Warnings => Implementation?.Warnings ?? PrinterWarningTypes.None;

        /// <inheritdoc />
        public PaperStates PaperState
        {
            get
            {
                // TODO: This isn't ideal, but it prevents us from having to maintain multiple states
                if (Faults.HasFlag(PrinterFaultTypes.PaperJam))
                {
                    return PaperStates.Jammed;
                }

                if (Faults.HasFlag(PrinterFaultTypes.PaperEmpty))
                {
                    return PaperStates.Empty;
                }

                if (Warnings.HasFlag(PrinterWarningTypes.PaperLow))
                {
                    return PaperStates.Low;
                }

                return PaperStates.Full;
            }
        }

        /// <inheritdoc />
        public bool UseLargeFont => DeviceConfiguration?.Manufacturer?.Contains(Nanoptix) ?? false;

        public override string Name => string.IsNullOrEmpty(ServiceProtocol) == false
            ? $"{ServiceProtocol} Printer Service"
            : "Unknown Printer Service";

        public override ICollection<Type> ServiceTypes => new[] { typeof(IPrinter) };

        public override async Task<bool> SelfTest(bool clear)
        {
            var result = await base.SelfTest(clear);
            if (result)
            {
                Logger.Debug("Self test passed");
                PostEvent(new SelfTestPassedEvent(PrinterId));
            }
            else
            {
                Logger.Error("Self test failed");
                PostEvent(new SelfTestFailedEvent(PrinterId));
            }

            return result;
        }

        /// <inheritdoc />
        public int GetCharactersPerLine(bool isLandscape, int fontIndex)
        {
            // TODO: calculate from metrics
            const int landscape = 80;
            const int portrait = 39;

            return isLandscape ? landscape : portrait;
        }

        /// <inheritdoc />
        public virtual async Task<bool> FormFeed()
        {
            Logger.Info("Form Feed");
            if (!await Implementation.FormFeed())
            {
                PostEvent(new ErrorWhilePrintingEvent(PrinterId));
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public virtual async Task<bool> Print(Ticket ticket)
        {
            return await Print(ticket, null);
        }

        /// <inheritdoc />
        public virtual async Task<bool> Print(Ticket ticket, Func<Task> onFieldOfInterest)
        {
            if (!PrinterReady())
            {
                return false;
            }

            var ticketType = ticket["ticket type"];
            Logger.Info($"Print: Ticket {ticketType}");
            if (!_resolver.PrintableTemplates.TryGetValue(ticketType, out var template))
            {
                Logger.Warn($"Print: Unable to find template {ticketType}");
                return false;
            }

            PostEvent(new PrintRequestedEvent(PrinterId, template?.Id ?? 0));

            var adjustTextTicketTitle = ticketType.Equals("text") && IsSerial && (IsJcm || IsFutureLogic);

            // Render the ticket command.
            if (!Renderer.RenderTicket(ticket, _resolver, out var ticketCommand, adjustTextTicketTitle))
            {
                Logger.Warn($"Renderer failed to render ticket:{Environment.NewLine}{ticket.AllFields()}");
                Disable(DisabledReasons.Error);
                return false;
            }

            if (!await Implementation.PrintTicket(ticketCommand, onFieldOfInterest))
            {
                // reset the printer state
                if (LogicalState == PrinterLogicalState.Printing)
                {
                    if (!InitializePrinter(false))
                    {
                        ImplementationFaultOccurred(
                            new object(),
                            new FaultEventArgs { Fault = PrinterFaultTypes.OtherFault });
                    }
                    else
                    {
                        Fire(PrinterLogicalStateTrigger.Printed);
                    }
                }

                PostEvent(new ErrorWhilePrintingEvent(PrinterId));
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public void AddPrintableRegion(PrintableRegion region)
        {
            _resolver?.AddPrintableRegion(region);
        }

        /// <inheritdoc />
        public void AddPrintableTemplate(PrintableTemplate template)
        {
            _resolver?.AddPrintableTemplate(template);
        }

        /// <inheritdoc />
        public bool TryAddBlock(IPersistentStorageAccessor accessor, int blockIndex, out PrinterOptions block)
        {
            block = new PrinterOptions { RenderTarget = RenderTarget, ActivationTime = ActivationTime };

            using (var transaction = accessor.StartTransaction())
            {
                transaction[blockIndex, RenderTargetOption] = block.RenderTarget;
                transaction[blockIndex, ActivationTimeText] = block.ActivationTime;

                transaction.Commit();
                return true;
            }
        }

        /// <inheritdoc />
        public bool TryGetBlock(IPersistentStorageAccessor accessor, int blockIndex, out PrinterOptions block)
        {
            block = new PrinterOptions
            {
                RenderTarget = (string)accessor[blockIndex, RenderTargetOption],
                ActivationTime = (DateTime)accessor[blockIndex, ActivationTimeText]
            };

            return true;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                Disable(DisabledReasons.Service);
                if (Implementation != null)
                {
                    Implementation.Connected -= ImplementationConnected;
                    Implementation.Disconnected -= ImplementationDisconnected;
                    Implementation.Initialized -= ImplementationInitialized;
                    Implementation.InitializationFailed -= ImplementationInitializationFailed;
                    Implementation.Disabled -= ImplementationDisabled;
                    Implementation.Enabled -= ImplementationEnabled;
                    Implementation.FaultCleared -= ImplementationFaultCleared;
                    Implementation.FaultOccurred -= ImplementationFaultOccurred;
                    Implementation.WarningCleared -= ImplementationWarningCleared;
                    Implementation.WarningOccurred -= ImplementationWarningOccurred;
                    Implementation.PrintInProgress -= ImplementationPrintInProgress;
                    Implementation.FieldOfInterestPrinted -= ImplementationFieldOfInterestPrinted;
                    Implementation.PrintCompleted -= ImplementationPrintCompleted;
                    Implementation.PrintIncomplete -= ImplementationPrintIncomplete;
                    Implementation.Dispose();
                    _printer = null;
                }

                if (_stateLock != null)
                {
                    _stateLock.Dispose();
                    _stateLock = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void DisabledDetected()
        {
            PostEvent(new DisabledEvent(PrinterId, ReasonDisabled));
        }

        /// <inheritdoc />
        protected override void Disabling(DisabledReasons reason)
        {
            if (Fire(PrinterLogicalStateTrigger.Disable, new DisabledEvent(PrinterId, ReasonDisabled), true))
            {
                Implementation.Disable();
            }
        }

        /// <inheritdoc />
        protected override void Enabling(EnabledReasons reason, DisabledReasons remedied)
        {
            CheckActivationTime();

            if (Enabled)
            {
                if (Fire(PrinterLogicalStateTrigger.Enable, new EnabledEvent(PrinterId, reason), true))
                {
                    Implementation.Enable();
                }
                else
                {
                    PostEvent(new EnabledEvent(PrinterId, reason));
                }
            }
            else
            {
                PostEvent(new EnabledEvent(PrinterId, reason));

                DisabledDetected();
            }
        }

        /// <inheritdoc />
        protected override void Initializing()
        {
            _state = ConfigureStateMachine();

            // Resolve regions and templates now since this will not be done at print time.
            _resolver = new Resolver();
            PrintableRegionPath = AddinFactory.FindFirstFilePath(PrintableRegionsExtensionPath);
            PrintableTemplatePath = AddinFactory.FindFirstFilePath(PrintableTemplatesExtensionPath);
            _resolver.LoadRegions(PrintableRegionPath);
            _resolver.LoadTemplates(PrintableTemplatePath);

            // Load an instance of the given protocol implementation.
            _printer = AddinFactory.CreateAddin<IPrinterImplementation>(
                DeviceImplementationsExtensionPath,
                ServiceProtocol);
            if (Implementation == null)
            {
                var errorMessage = $"Cannot load {Name}";
                Logger.Fatal(errorMessage);
                throw new ServiceException(errorMessage);
            }

            ReadOrCreateOptions();

            Implementation.Connected += ImplementationConnected;
            Implementation.Disconnected += ImplementationDisconnected;
            Implementation.Initialized += ImplementationInitialized;
            Implementation.InitializationFailed += ImplementationInitializationFailed;
            Implementation.Disabled += ImplementationDisabled;
            Implementation.Enabled += ImplementationEnabled;
            Implementation.FaultCleared += ImplementationFaultCleared;
            Implementation.FaultOccurred += ImplementationFaultOccurred;
            Implementation.WarningCleared += ImplementationWarningCleared;
            Implementation.WarningOccurred += ImplementationWarningOccurred;
            Implementation.PrintInProgress += ImplementationPrintInProgress;
            Implementation.FieldOfInterestPrinted += ImplementationFieldOfInterestPrinted;
            Implementation.PrintCompleted += ImplementationPrintCompleted;
            Implementation.PrintIncomplete += ImplementationPrintIncomplete;
        }

        /// <inheritdoc />
        protected override void Inspecting(IComConfiguration comConfiguration, int timeout)
        {
            Fire(PrinterLogicalStateTrigger.Inspecting);
        }

        /// <inheritdoc />
        protected override void SubscribeToEvents(IEventBus eventBus)
        {
            eventBus.Subscribe<ResolverErrorEvent>(this, ResolverError);
        }

        private void CheckActivationTime()
        {
            if (ActivationTime == DateTime.MinValue)
            {
                ActivationTime = DateTime.UtcNow;
            }
        }

        private async Task<bool> LoadRegionsAndTemplates()
        {
            // Post an even to indicate we are loading the regions and templates (used to to remove any previous transfer status errors).
            Logger.Debug("Loading regions and templates...");
            PostEvent(new LoadingRegionsAndTemplatesEvent());

            var printerOverridesPath = AddinFactory.FindFirstFilePath(PrinterOverridesExtensionPath);
            PrinterOverrideParser.LoadOverrides(printerOverridesPath);

            var printerOverride = PrinterOverrideParser.GetPrinterSpecificOverride($"{DeviceConfiguration.Manufacturer} {DeviceConfiguration.Protocol}", DeviceConfiguration.FirmwareId);
            

            // Load the printable regions.
            foreach (var item in Regions)
            {
                item.Format = SetFontOverride();

                var region = item.ToPDL(UseLargeFont);
                Logger.Debug($"Loading region {item.Id} : {region} Implementation is null {Implementation is null} ");
                if (Implementation == null || !await Implementation.DefineRegion(region))
                {
                    Logger.Error($"Error loading print region - ID:{item.Id}; Name:{item.Name}");
                    return false;
                }

                string SetFontOverride()
                {
                    // Format -> T=XXX   where T is a letter F, G, B, L, or O and XXX is a 3-digit number. See GDS PDL documentation
                    var originalFontNumber = item.Format.Substring(2);
                    var fontOverride = printerOverride?.GetSpecificFont(originalFontNumber, item.Id.ToString()) ?? string.Empty;

                    if (!string.IsNullOrEmpty(fontOverride) && short.TryParse(fontOverride, out var newFont))
                    {
                        return $"F={newFont:000}";
                    }
                    return item.Format;
                }
            }

            // wait for all the region reports to come in before starting templates
            Logger.Debug("LoadRegionsAndTemplates: waiting before loading templates");
            await Task.Delay(100);

            // Load the printable templates.
            foreach (var item in Templates)
            {
                var template = item.ToPDL();
                Logger.Debug($"Loading template {item.Id} : {template}");
                if (Implementation == null || !await Implementation.DefineTemplate(template))
                {
                    Logger.Error($"Error loading print template - ID:{item.Id}; Name:{item.Name}");
                    return false;
                }
            }

            return true;
        }

        private bool PrinterReady()
        {
            if (!CanPrint)
            {
                Logger.Warn($"Invalid state {LogicalState} for printing");
                return false;
            }

            if (Renderer == null)
            {
                Renderer = AddinFactory.CreateAddin<ITemplateRenderer>(
                    RendererImplementationsExtensionPath,
                    RenderTarget);
                if (Renderer == null)
                {
                    Logger.Warn($"Print failed to create renderer for {RenderTarget}");
                    Disable(DisabledReasons.Error);
                    return false;
                }

                OverrideTemplates();
            }

            return true;
        }

        private void ReadOrCreateOptions()
        {
            if (!this.GetOrAddBlock(OptionsBlock, out var options, PrinterId - 1))
            {
                Logger.Error($"Could not access block {OptionsBlock} {PrinterId - 1}");
                return;
            }

            _renderTarget = options.RenderTarget;
            _activationTime = options.ActivationTime;
            Logger.Debug($"Block successfully read {OptionsBlock} {PrinterId - 1}");
        }

        private void Reset()
        {
            Enable(EnabledReasons.Reset);
        }

        private void ResolverError(ResolverErrorEvent error)
        {
            if (AddError(typeof(ResolverErrorEvent)))
            {
                Logger.Info($"ResolverError: ADDED {error} to the error list.");
                Disable(DisabledReasons.Error);
            }
        }

        private void ImplementationConnected(object sender, EventArgs e)
        {
            Logger.Info("ImplementationConnected: device connected");
            Fire(PrinterLogicalStateTrigger.Connected, new ConnectedEvent(PrinterId));
        }

        private void OverrideTemplates()
        {
            var printableRegionPath = AddinFactory.FindFirstFilePath(PrinterOverridesExtensionPath);
            PrinterOverrideParser.LoadOverrides(printableRegionPath);

            // get the overrides for this protocol/firmware
            Logger.Debug($"Getting template overrides for protocol '{DeviceConfiguration?.Protocol}' and firmware '{DeviceConfiguration?.FirmwareId}' revision '{DeviceConfiguration?.FirmwareRevision}' boot version '{DeviceConfiguration?.FirmwareBootVersion}'");
            var templateOverrides = PrinterOverrideParser.GetTemplateOverrides(DeviceConfiguration?.Protocol, DeviceConfiguration?.FirmwareId);
            if (templateOverrides is null)
            {
                return;
            }

            foreach (var change in templateOverrides)
            {
                Logger.Debug($"Overriding template '{change.Name}' '{change.PlatformTemplateId}' with regions '{change.Regions}'");
                var regions = change.Regions.Split(' ').Select(int.Parse).ToList();
                AddPrintableTemplate(new PrintableTemplate(change.Name, change.PlatformTemplateId, 0, 0, regions));
            }
        }

        private bool InitializePrinter(bool loadRegions)
        {
            // load regions and templates now to expedite initial print (slow sending regions and templates prior to every print for GDS).
            var regionsResult = true;
            if (loadRegions)
            {
                regionsResult = LoadRegionsAndTemplates().Result;
            }

            if (!regionsResult || !SelfTest(false).Result || CalculateCrc(0).Result == 0)
            {
                Logger.Debug("Failure while loading regions, doing self test, or getting crc");
                return false;
            }

            if (Enable(EnabledReasons.Device))
            {
                return true;
            }

            if (ReasonDisabled != DisabledReasons.Error)
            {
                return true;
            }

            if (ContainsError(PrinterFaultTypes.OtherFault))
            {
                ImplementationFaultCleared(
                    new object(),
                    new FaultEventArgs { Fault = PrinterFaultTypes.OtherFault });
            }

            if (!Faults.HasFlag(PrinterFaultTypes.PaperEmpty)
                && !Faults.HasFlag(PrinterFaultTypes.PaperJam))
            {
                Reset();
            }

            return true;
        }

        private void ImplementationDisconnected(object sender, EventArgs e)
        {
            Logger.Warn("ImplementationDisconnected: device disconnected");
            if (LogicalState == PrinterLogicalState.Printing)
            {
                _formFeedPending = true;
            }

            Disable(DisabledReasons.Device);
            Fire(PrinterLogicalStateTrigger.Disconnected, new DisconnectedEvent(PrinterId));
        }

        private void ImplementationInitialized(object sender, EventArgs e)
        {
            if (!Fire(PrinterLogicalStateTrigger.Initializing, true))
            {
                Logger.Error("ImplementationInitialized: invalid state for device initializing");

                return;
            }

            Logger.Info("ImplementationInitialized: device initializing");
            if (ContainsWarning(PrinterWarningTypes.PaperInChute))
            {
                _formFeedPending = true;
            }

            SetInternalConfiguration();
            Implementation?.UpdateConfiguration(InternalConfiguration);
            RegisterComponent();

            InitializePrinter(true);

            Fire(PrinterLogicalStateTrigger.Initialized, new InspectedEvent(PrinterId));
            Initialized = true;
            if (!Enabled && !AnyErrors && ReasonDisabled == 0)
            {
                Reset();
            }

            if (Enabled)
            {
                Implementation?.Enable();
            }
            else
            {
                DisabledDetected();
                Implementation?.Disable();
            }
        }

        private void ImplementationInitializationFailed(object sender, EventArgs e)
        {
            Logger.Warn("ImplementationInitializationFailed: device initialization failed");
            Fire(PrinterLogicalStateTrigger.InspectionFailed, new InspectionFailedEvent(PrinterId));
            PostEvent(new ResetEvent(PrinterId));
        }

        private void ImplementationDisabled(object sender, EventArgs e)
        {
            Logger.Warn("ImplementationDisabled: device disabled");
        }

        private void ImplementationEnabled(object sender, EventArgs e)
        {
            Logger.Info("ImplementationEnabled: device enabled");
        }

        private void ImplementationFaultCleared(object sender, FaultEventArgs e)
        {
            foreach (PrinterFaultTypes value in Enum.GetValues(typeof(PrinterFaultTypes)))
            {
                if (!e.Fault.HasFlag(value))
                {
                    continue;
                }

                switch (value)
                {
                    case PrinterFaultTypes.TemperatureFault:
                    case PrinterFaultTypes.PrintHeadDamaged:
                    case PrinterFaultTypes.NvmFault:
                    case PrinterFaultTypes.FirmwareFault:
                    case PrinterFaultTypes.OtherFault:
                    case PrinterFaultTypes.PaperJam:
                    case PrinterFaultTypes.PaperEmpty:
                    case PrinterFaultTypes.PaperNotTopOfForm:
                    case PrinterFaultTypes.PrintHeadOpen:
                    case PrinterFaultTypes.ChassisOpen:
                        if (ClearError(value))
                        {
                            Logger.Info($"ImplementationFaultCleared: REMOVED {value} from the error list.");
                            if (!AnyErrors)
                            {
                                Reset();
                            }
                        }

                        break;
                    default:
                        continue;
                }

                PostEvent(new HardwareFaultClearEvent(PrinterId, value));

                if (ContainsError(PrinterFaultTypes.OtherFault))
                {
                    ImplementationFaultCleared(
                        new object(),
                        new FaultEventArgs { Fault = PrinterFaultTypes.OtherFault });
                }
            }
        }

        private void ImplementationFaultOccurred(object sender, FaultEventArgs e)
        {
            foreach (PrinterFaultTypes value in Enum.GetValues(typeof(PrinterFaultTypes)))
            {
                if (!e.Fault.HasFlag(value))
                {
                    continue;
                }

                var postFaultEvent = !ContainsError(value.ToString());

                switch (value)
                {
                    case PrinterFaultTypes.PaperNotTopOfForm:
                        if (LogicalState == PrinterLogicalState.Printing)
                        {
                            return;
                        }

                        HandleFault(value);
                        break;

                    case PrinterFaultTypes.TemperatureFault:
                    case PrinterFaultTypes.PrintHeadDamaged:
                    case PrinterFaultTypes.NvmFault:
                    case PrinterFaultTypes.FirmwareFault:
                    case PrinterFaultTypes.OtherFault:
                    case PrinterFaultTypes.PaperJam:
                    case PrinterFaultTypes.PaperEmpty:
                    case PrinterFaultTypes.PrintHeadOpen:
                    case PrinterFaultTypes.ChassisOpen:
                        HandleFault(value);

                        break;
                    default:
                        continue;
                }

                if (postFaultEvent)
                {
                    PostEvent(new HardwareFaultEvent(PrinterId, value));
                }
            }
        }

        private void HandleFault(PrinterFaultTypes value)
        {
            if (AddError(value))
            {
                Logger.Info($"ImplementationFaultOccurred: ADDED {value} to the error list.");
                Disable(DisabledReasons.Error);
            }
            else
            {
                Logger.Debug($"ImplementationFaultOccurred: DUPLICATE ERROR EVENT {value}");
            }
        }

        private void ImplementationWarningCleared(object sender, WarningEventArgs e)
        {
            foreach (PrinterWarningTypes value in Enum.GetValues(typeof(PrinterWarningTypes)))
            {
                if (!e.Warning.HasFlag(value))
                {
                    continue;
                }

                switch (value)
                {
                    case PrinterWarningTypes.PaperLow:
                    case PrinterWarningTypes.PaperInChute:
                        if (ClearWarning(value))
                        {
                            Logger.Info($"ImplementationWarningCleared: REMOVED {value} from the warning list.");
                        }

                        break;
                    default:
                        continue;
                }

                PostEvent(new HardwareWarningClearEvent(PrinterId, value));
            }
        }

        private void ImplementationWarningOccurred(object sender, WarningEventArgs e)
        {
            foreach (PrinterWarningTypes value in Enum.GetValues(typeof(PrinterWarningTypes)))
            {
                if (!e.Warning.HasFlag(value))
                {
                    continue;
                }

                switch (value)
                {
                    case PrinterWarningTypes.PaperLow:
                    case PrinterWarningTypes.PaperInChute:
                        if (AddWarning(value))
                        {
                            Logger.Info($"ImplementationWarningOccurred: ADDED {value} to the warning list.");
                        }
                        else
                        {
                            Logger.Debug($"ImplementationWarningOccurred: DUPLICATE WARNING EVENT {value}");
                        }

                        break;
                    default:
                        continue;
                }

                PostEvent(new HardwareWarningEvent(PrinterId, value));
            }
        }

        private void ImplementationPrintInProgress(object sender, EventArgs e)
        {
            if (CanFire(PrinterLogicalStateTrigger.Printing))
            {
                Logger.Info("Printing");
                Fire(PrinterLogicalStateTrigger.Printing, new PrintStartedEvent(PrinterId));
            }
        }

        private void ImplementationFieldOfInterestPrinted(object sender, FieldOfInterestEventArgs e)
        {
            PostEvent(new FieldOfInterestEvent(PrinterId, e.FieldOfInterest));
        }

        private void ImplementationPrintCompleted(object sender, EventArgs e)
        {
            Fire(PrinterLogicalStateTrigger.Printed, new PrintCompletedEvent(PrinterId));
        }

        private void ImplementationPrintIncomplete(object sender, EventArgs e)
        {
            _formFeedPending = true;
        }

        private StateMachine<PrinterLogicalState, PrinterLogicalStateTrigger> ConfigureStateMachine()
        {
            var stateMachine =
                new StateMachine<PrinterLogicalState, PrinterLogicalStateTrigger>(PrinterLogicalState.Uninitialized);
            // Uninitialized and Inspecting are only used when configuring the reader
            stateMachine.Configure(PrinterLogicalState.Uninitialized)
                .Permit(PrinterLogicalStateTrigger.Inspecting, PrinterLogicalState.Inspecting)
                .Permit(PrinterLogicalStateTrigger.Initializing, PrinterLogicalState.Initializing);

            stateMachine.Configure(PrinterLogicalState.Inspecting)
                .PermitReentry(PrinterLogicalStateTrigger.Enable)
                .Permit(PrinterLogicalStateTrigger.InspectionFailed, PrinterLogicalState.Uninitialized)
                .Permit(PrinterLogicalStateTrigger.Initializing, PrinterLogicalState.Initializing)
                .Permit(PrinterLogicalStateTrigger.Disconnected, PrinterLogicalState.Disconnected);

            stateMachine.Configure(PrinterLogicalState.Initializing)
                .PermitReentry(PrinterLogicalStateTrigger.Enable)
                .PermitDynamic(
                    PrinterLogicalStateTrigger.Initialized,
                    () => Enabled ? PrinterLogicalState.Idle : PrinterLogicalState.Disabled)
                .Permit(PrinterLogicalStateTrigger.Disconnected, PrinterLogicalState.Disconnected);

            stateMachine.Configure(PrinterLogicalState.Disconnected)
                .Permit(PrinterLogicalStateTrigger.Connected, PrinterLogicalState.Inspecting);

            stateMachine.Configure(PrinterLogicalState.Idle)
                .OnEntry(
                    () =>
                    {
                        if (!_formFeedPending)
                        {
                            return;
                        }

                        _formFeedPending = false;
                        TaskExtensions.FireAndForget(FormFeed());
                    })
                .Permit(PrinterLogicalStateTrigger.Initializing, PrinterLogicalState.Initializing)
                .Permit(PrinterLogicalStateTrigger.Printing, PrinterLogicalState.Printing)
                .Permit(PrinterLogicalStateTrigger.Disable, PrinterLogicalState.Disabled)
                .Permit(PrinterLogicalStateTrigger.Disconnected, PrinterLogicalState.Disconnected);

            stateMachine.Configure(PrinterLogicalState.Printing)
                .Permit(PrinterLogicalStateTrigger.Printed, PrinterLogicalState.Idle)
                .Permit(PrinterLogicalStateTrigger.Disable, PrinterLogicalState.Disabled)
                .Permit(PrinterLogicalStateTrigger.Disconnected, PrinterLogicalState.Disconnected);

            stateMachine.Configure(PrinterLogicalState.Disabled)
                .Permit(PrinterLogicalStateTrigger.Enable, PrinterLogicalState.Idle)
                .PermitReentry(PrinterLogicalStateTrigger.Disable)
                .Permit(PrinterLogicalStateTrigger.Connected, PrinterLogicalState.Inspecting)
                .Permit(PrinterLogicalStateTrigger.Disconnected, PrinterLogicalState.Disconnected);

            stateMachine.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error($"Invalid Printer State Transition. State : {state} Trigger : {trigger}");
                });

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
                });

            return stateMachine;
        }

        private bool CanFire(PrinterLogicalStateTrigger trigger)
        {
            if (!_state?.CanFire(trigger) ?? true)
            {
                Logger.Warn($"Cannot transition {_state} with trigger {trigger}");
                return false;
            }

            return true;
        }

        private bool Fire(PrinterLogicalStateTrigger trigger, bool verify = false)
        {
            if (verify && !CanFire(trigger))
            {
                return false;
            }

            _stateLock?.EnterWriteLock();
            Logger.Debug($"Transitioning {_state.State} with trigger {trigger}");
            try
            {
                _state.Fire(trigger);
            }
            finally
            {
                _stateLock?.ExitWriteLock();
            }

            return true;
        }

        private bool Fire<TEvent>(PrinterLogicalStateTrigger trigger, TEvent @event, bool verify = false)
            where TEvent : IEvent
        {
            if (!Fire(trigger, verify))
            {
                return false;
            }

            if (@event != null)
            {
                PostEvent(@event);
            }

            return true;
        }
    }
}