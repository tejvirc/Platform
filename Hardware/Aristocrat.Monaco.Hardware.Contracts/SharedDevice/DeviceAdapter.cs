namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Communicator;
    using Dfu;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;

    /// <summary>A device adapter base.</summary>
    /// <typeparam name="TImplementation">Type of the implementation.</typeparam>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.IDeviceAdapter" />
    /// <seealso cref="T:System.IDisposable" />
    public abstract class DeviceAdapter<TImplementation> : IDeviceAdapter,
        IDisposable
        where TImplementation : IHardwareDevice, IDfuDevice
    {
        private const string CommunicatorsExtensionPath = "/Hardware/CommunicatorDrivers";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<string> _errorList = new List<string>();
        private readonly List<string> _warningList = new List<string>();

        private ICommunicator _communicator;
        private Component _component;
        private IEventBus _eventBus;
        private IDfuProvider _dfuProvider;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.Contracts.SharedDevice.DeviceAdapter class.
        /// </summary>
        protected DeviceAdapter()
        {
            //ReasonDisabled = DisabledReasons.Service | DisabledReasons.Device;
            InternalConfiguration = new DeviceConfiguration();
        }

        /// <summary>Gets the device type.</summary>
        /// <value>The device type.</value>
        public abstract DeviceType DeviceType { get; }

        /// <summary>Object used for locking to protect concurrent access to member variables.</summary>
        /// <value>The synchronise.</value>
        protected object Sync { get; } = new object();

        /// <summary>Gets a value indicating whether the object is disposed.</summary>
        /// <value>True if disposed, false if not.</value>
        protected bool Disposed { get; private set; }

        /// <summary>Gets a value indicating whether any errors.</summary>
        /// <value>True if any errors, false if not.</value>
        protected bool AnyErrors
        {
            get
            {
                lock (_errorList)
                {
                    return _errorList.Any();
                }
            }
        }

        /// <summary>Gets a value indicating whether any warnings.</summary>
        /// <value>True if any warnings, false if not.</value>
        protected bool AnyWarnings
        {
            get
            {
                lock (_warningList)
                {
                    return _warningList.Any();
                }
            }
        }

        /// <summary>Gets the addin factory.</summary>
        /// <value>The addin factory.</value>
        protected IAddinFactory AddinFactory { get; set; } = new DeviceAddinHelper();

        /// <summary>Gets the device implementation.</summary>
        /// <value>The device implementation.</value>
        protected abstract TImplementation Implementation { get; }

        /// <summary>
        ///     Gets or sets the implemented device configuration for internal use only.
        ///     Just directly returns the _deviceConfiguration for inherited class use
        ///     No copying done as in DeviceConfiguration property.
        ///     Had to name this way because we already have DeviceConfiguration property and it
        ///     does something different.
        /// </summary>
        /// <returns>Device configuration.</returns>
        protected IDeviceConfiguration InternalConfiguration { get; }

        /// <summary>
        ///     Gets or sets the device description.
        /// </summary>
        /// <returns>Device Description.</returns>
        protected abstract string Description { get; }

        /// <summary>
        ///     Gets or sets the device path.
        /// </summary>
        /// <returns>Device path.</returns>
        protected abstract string Path { get; }

        /// <summary>Gets the last com configuration.</summary>
        /// <value>The last com configuration.</value>
        protected IComConfiguration LastComConfiguration { get; private set; }

        /// <inheritdoc />
        public virtual bool Enabled { get; protected set; }

        /// <inheritdoc />
        public virtual bool Initialized { get; protected set; }

        /// <inheritdoc />
        public virtual string LastError
        {
            get
            {
                var lastError = string.Empty;

                lock (_errorList)
                {
                    if (_errorList.Any())
                    {
                        lastError = _errorList.Aggregate(lastError, (current, error) => current + error);
                    }
                }

                lock (_warningList)
                {
                    if (_warningList.Any())
                    {
                        lastError = _warningList.Aggregate(lastError, (current, warning) => current + warning);
                    }
                }

                return lastError;
            }
        }

        /// <inheritdoc />
        public DisabledReasons ReasonDisabled { get; private set; }

        /// <inheritdoc />
        public string ServiceProtocol { get; set; }

        /// <summary>Gets the name.</summary>
        /// <value>The name.</value>
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract bool Connected { get; }

        /// <inheritdoc />
        public abstract ICollection<Type> ServiceTypes { get; }

        /// <inheritdoc />
        public virtual IDevice DeviceConfiguration => new Device(InternalConfiguration, LastComConfiguration);

        /// <inheritdoc />
        public int ProductId => _communicator?.ProductId ?? 0;

        /// <inheritdoc />
        public int VendorId => _communicator?.VendorId ?? 0;

        /// <inheritdoc />
        public virtual int Crc => Implementation?.Crc ?? 0;

        /// <inheritdoc />
        public virtual bool DisabledByError => ReasonDisabled.HasFlag(DisabledReasons.Error) ||
                                               ReasonDisabled.HasFlag(DisabledReasons.FirmwareUpdate) ||
                                               ReasonDisabled.HasFlag(DisabledReasons.Device);

        /// <summary>
        ///     Initializes the runnable.
        /// </summary>
        /// <exception cref="ServiceException">Thrown when the runnable fails initialization.</exception>
        public void Initialize()
        {
            lock (Sync)
            {
                ClearErrors();
                ClearWarnings();

                // Subscribe to all events.
                var eventBus = ServiceManager.GetInstance().TryGetService<IEventBus>();
                SubscribeToEvents(eventBus);

                // Set service initialized.
                Initialized = true;

                Initializing();

                Enable(EnabledReasons.Service);

                Logger.Debug($"{Name} initialized");
            }
        }

        /// <inheritdoc />
        public void Disable(DisabledReasons reason)
        {
            DisabledReasons condition = 0;
            foreach (DisabledReasons value in Enum.GetValues(typeof(DisabledReasons)))
            {
                if ((reason & value) != value)
                {
                    continue;
                }

                if ((ReasonDisabled & reason) > 0)
                {
                    continue;
                }

                condition |= value;
            }

            if (condition == 0)
            {
                return;
            }

            ReasonDisabled |= condition;
            Logger.Debug($"{Name} disabled by {reason}");
            Enabled = false;
            Disabling(condition);
        }

        /// <inheritdoc />
        public bool Enable(EnabledReasons reason)
        {
            if (!Initialized)
            {
                Logger.Warn($"{Name} can not be enabled by {reason} because service is not initialized");
                Disable(ReasonDisabled);
                return false;
            }

            if (!reason.HasFlag(EnabledReasons.Backend) &&
                !reason.HasFlag(EnabledReasons.GamePlay) &&
                !reason.HasFlag(EnabledReasons.Reset) &&
                (!Implementation?.IsConnected ?? true))
            {
                // This check will be for the case where the BNA was disconnected in the audit menu,
                // and the audit menu was exited before reconnecting the BNA. The System disable
                // should be removed from the disabled reasons.
                if (reason.HasFlag(EnabledReasons.System))
                {
                    UpdateDisabledReasons(EnabledReasons.System);
                }

                Logger.Warn($"{Name} can not be enabled by {reason} because implementation is not connected");
                Disable(ReasonDisabled);
                return false;
            }

            if (Enabled)
            {
                Logger.Debug($"{Name} enabled by {reason} already Enabled");
                Enabling(reason, 0);
                return true;
            }

            var updated = UpdateDisabledReasons(reason);

            Enabled = ReasonDisabled == 0;
            Enabling(reason, updated);
            if (Enabled)
            {
                Logger.Debug($"{Name} enabled by {reason}");
            }
            else
            {
                Logger.Warn($"{Name} can not be enabled by {reason} because disabled by {ReasonDisabled}");
            }

            return Enabled;
        }

        /// <inheritdoc />
        public async void Inspect(IComConfiguration config, int timeout)
        {
            if (!Initialized)
            {
                return;
            }

            if (config == null)
            {
                Logger.Error("comConfiguration object is null");
                return;
            }

            _communicator?.Dispose();

            var protocolName = config.Mode == ComConfiguration.RS232CommunicationMode ||
                               config.Protocol == ComConfiguration.RelmProtocol
                ? config.Protocol
                : config.Mode;
            _communicator = AddinFactory.CreateAddin<ICommunicator>(CommunicatorsExtensionPath, protocolName);

            if (_communicator == null)
            {
                var errorMessage = $"Cannot load {config.Mode} communicator";
                Logger.Fatal(errorMessage);
                throw new ServiceException(errorMessage);
            }

            _communicator.Device = DeviceConfiguration;
            _communicator.DeviceType = DeviceType;

            LastComConfiguration = config;
            if (!_communicator.Configure(config))
            {
                var errorMessage = $"Error in configuring {config.Mode} communicator";
                Logger.Fatal(errorMessage);
                throw new ServiceException(errorMessage);
            }

            SetInternalConfiguration();
            Inspecting(config, timeout);

            await (Implementation?.Initialize(_communicator) ?? Task.FromResult(false));
            RegisterDfuAdapter();

            if (Implementation?.Crc == 0)
            {
                await CalculateCrc(0);
            }
            else
            {
                InternalConfiguration.FirmwareCyclicRedundancyCheck = $"0x{Implementation?.Crc:X}";
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> SelfTest(bool clear)
        {
            var result = false;
            await ExecuteDisabled(
                async () =>
                {
                    Logger.Debug($"SelfTest: {(clear ? "With NVM Clear" : string.Empty)}");
                    result = await (Implementation?.SelfTest(clear) ?? Task.FromResult(false));
                    return result;
                });

            return result;
        }

        /// <inheritdoc />
        public async Task<int> CalculateCrc(int seed)
        {
            var crc = 0;
            await ExecuteDisabled(
                async () =>
                {
                    Logger.Debug($"CalculateCrc With Seed {seed} Request");
                    crc = await (Implementation?.CalculateCrc(seed) ?? Task.FromResult(0));
                    InternalConfiguration.FirmwareCyclicRedundancyCheck = $"0x{Implementation?.Crc ?? 0:X}";
                    return crc != 0;
                });
            return crc;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the
        ///     Aristocrat.Monaco.Hardware.Contracts.SharedDevice.DeviceAdapter and optionally releases the managed
        ///     resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            Disposed = disposing;
            if (disposing)
            {
                ServiceManager.GetInstance().TryGetService<IEventBus>()?.UnsubscribeAll(this);
                _communicator?.Dispose();
                _communicator = null;
                UnregisterDfuAdapter();
            }

            Logger.Debug($"{Name} disposing");
        }

        /// <summary>Determine if we can proceed.</summary>
        /// <param name="checkEnabled">True to enable, false to disable the check.</param>
        /// <returns>True if we can proceed, false if not.</returns>
        protected bool CanProceed(bool checkEnabled)
        {
            // Is the service initialized?
            if (Initialized == false)
            {
                // No, log error, and return false to disallow procession.
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(1);
                var methodBase = stackFrame.GetMethod();
                Logger.Error($"{methodBase.Name} cannot proceed, must initialize {Name} first");
                return false;
            }

            // Is the device connected?
            if (!(Implementation?.IsConnected ?? false))
            {
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(1);
                var methodBase = stackFrame.GetMethod();
                Logger.Error($"{methodBase.Name} cannot proceed, must connect {Name} first");
                return false;
            }

            // Are we checking enabled and is the service enabled?
            if (checkEnabled && Enabled == false)
            {
                // No, log error, post disabled event, and return false to disallow procession.
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(1);
                var methodBase = stackFrame.GetMethod();
                Logger.Error($"{methodBase.Name} cannot proceed, must enable {Name} first");
                DisabledDetected();
                return false;
            }

            return true;
        }

        /// <summary>Executes the specified method while disabling the device.</summary>
        /// <param name="method">The method return a result of success or failed.</param>
        /// <returns>An asynchronous result.</returns>
        protected virtual async Task ExecuteDisabled(Func<Task<bool>> method)
        {
            if (!CanProceed(false))
            {
                return;
            }

            var reenable = false;
            if (Implementation?.IsEnabled ?? false)
            {
                reenable = true;
                if (!await Implementation?.Disable())
                {
                    Logger.Error("ExecuteDisabled: failed to disable device");
                    return;
                }
            }

            if (!await method())
            {
                Logger.Error("ExecuteDisabled: method failed");
                return;
            }

            if (reenable && !await Implementation?.Enable())
            {
                Logger.Error("ExecuteDisabled: failed to re-enable device");
            }
        }

        /// <summary>Disabled detected.</summary>
        protected abstract void DisabledDetected();

        /// <summary>Called when the device adapter is disabling the device.</summary>
        /// <param name="reason">The reason.</param>
        protected abstract void Disabling(DisabledReasons reason);

        /// <summary>Called when the device adapter is enabling the device.</summary>
        /// <param name="reason">The reason.</param>
        /// <param name="remedied">The remedied.</param>
        protected abstract void Enabling(EnabledReasons reason, DisabledReasons remedied);

        /// <summary>Called when the device adapter is initializing.</summary>
        protected abstract void Initializing();

        /// <summary>Called when the device adapter is inspecting.</summary>
        /// <param name="comConfiguration">The com configuration.</param>
        /// <param name="timeout">The timeout.</param>
        protected abstract void Inspecting(IComConfiguration comConfiguration, int timeout);

        /// <summary>
        ///     The method used to post an event to the Event Bus
        /// </summary>
        /// <typeparam name="T">The type of event to publish</typeparam>
        /// <param name="event">The event to be published</param>
        protected virtual void PostEvent<T>(T @event)
            where T : IEvent
        {
            if (_eventBus == null)
            {
                _eventBus = ServiceManager.GetInstance().TryGetService<IEventBus>();
            }

            _eventBus?.Publish(@event);
        }

        /// <summary>Registers the dfu adapter.</summary>
        protected virtual void RegisterDfuAdapter()
        {
            if (_dfuProvider == null)
            {
                _dfuProvider = ServiceManager.GetInstance().TryGetService<IDfuProvider>();
            }

            _dfuProvider?.Register(Implementation);
        }

        /// <summary>Un register dfu adapter.</summary>
        protected virtual void UnregisterDfuAdapter()
        {
            var provider = ServiceManager.GetInstance().TryGetService<IDfuProvider>();

            provider?.Unregister(Implementation);
        }

        /// <summary>Subscribe to events.</summary>
        /// <param name="eventBus">The event bus.</param>
        protected abstract void SubscribeToEvents(IEventBus eventBus);

        /// <summary>Adds an error.</summary>
        /// <param name="error">The error.</param>
        protected bool AddError(string error)
        {
            lock (_errorList)
            {
                if (_errorList.Contains(error))
                {
                    return false;
                }

                _errorList.Add(error);
                _component?.OnFaultOccurred();
                return true;
            }
        }

        /// <summary>Adds an error.</summary>
        /// <param name="value">The error object.</param>
        protected bool AddError(object value)
        {
            return AddError(value.ToString());
        }

        /// <summary>Adds a warning.</summary>
        /// <param name="warning">The warning.</param>
        protected bool AddWarning(string warning)
        {
            lock (_warningList)
            {
                if (_warningList.Contains(warning))
                {
                    return false;
                }

                _warningList.Add(warning);
                return true;
            }
        }

        /// <summary>Adds a warning.</summary>
        /// <param name="value">The warning object.</param>
        protected bool AddWarning(object value)
        {
            return AddWarning(value.ToString());
        }

        /// <summary>Clears the specified error.</summary>
        /// <param name="error">The error.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected bool ClearError(string error)
        {
            lock (_errorList)
            {
                var removed = _errorList.Remove(error);

                if (!AnyErrors)
                {
                    _component?.OnAllFaultsCleared();
                }

                return removed;
            }
        }

        /// <summary>Clears the specified error.</summary>
        /// <param name="value">The error object.</param>
        protected bool ClearError(object value)
        {
            return ClearError(value.ToString());
        }

        /// <summary>Clears the warning described by warning.</summary>
        /// <param name="warning">The warning.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected bool ClearWarning(string warning)
        {
            lock (_warningList)
            {
                return _warningList.Remove(warning);
            }
        }

        /// <summary>Clears the warning described by warning.</summary>
        /// <param name="value">The warning object.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected bool ClearWarning(object value)
        {
            return ClearWarning(value.ToString());
        }

        /// <summary>Clears the errors described by omitted.</summary>
        /// <param name="omitted">A variable-length parameters list containing omitted errors.</param>
        protected void ClearErrors(params string[] omitted)
        {
            lock (_errorList)
            {
                // must call something to create a concrete iterator ie ToArray - we are clearing _errorList in the next line
                var updated = omitted.Intersect(_errorList).ToArray();
                _errorList.Clear();
                _errorList.AddRange(updated);
            }
        }

        /// <summary>Clears the warnings described by omitted.</summary>
        /// <param name="omitted">A variable-length parameters list containing omitted warnings.</param>
        protected void ClearWarnings(params string[] omitted)
        {
            lock (_warningList)
            {
                // must call something to create a concrete iterator ie ToArray - we are clearing _warningList in the next line
                var updated = omitted.Intersect(_warningList).ToArray();
                _warningList.Clear();
                _warningList.AddRange(updated);
            }
        }

        /// <summary>Query if 'error' contains error.</summary>
        /// <param name="error">The error.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected bool ContainsError(string error)
        {
            lock (_errorList)
            {
                return _errorList.Contains(error);
            }
        }

        /// <summary>Query if 'error' contains error.</summary>
        /// <param name="value">The error object.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected bool ContainsError(object value)
        {
            return ContainsError(value.ToString());
        }

        /// <summary>Query if 'warning' contains warning.</summary>
        /// <param name="warning">The warning.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected bool ContainsWarning(string warning)
        {
            lock (_warningList)
            {
                return _warningList.Contains(warning);
            }
        }

        /// <summary>Query if 'warning' contains error.</summary>
        /// <param name="value">The warning object.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected bool ContainsWarning(object value)
        {
            return ContainsWarning(value.ToString());
        }

        /// <summary>Set Internal Configuration.</summary>
        protected void SetInternalConfiguration()
        {
            InternalConfiguration.Protocol = Implementation?.Protocol ?? string.Empty;
            InternalConfiguration.Manufacturer = _communicator?.Manufacturer ?? string.Empty;
            InternalConfiguration.Model = _communicator?.Model ?? string.Empty;
            InternalConfiguration.FirmwareId = _communicator?.FirmwareVersion ?? string.Empty;
            InternalConfiguration.FirmwareRevision = _communicator?.Firmware ?? string.Empty;
            InternalConfiguration.SerialNumber = _communicator?.SerialNumber ?? string.Empty;
            //InternalConfiguration.PollingFrequency = config.BaudRate;
        }

        /// <summary>Register Component.</summary>
        protected void RegisterComponent()
        {
            lock (Sync)
            {
                if (string.IsNullOrEmpty(Description) || string.IsNullOrEmpty(Path) || Implementation == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(InternalConfiguration.Manufacturer))
                {
                    InternalConfiguration.Manufacturer = Description;
                }

                var componentRegistry = ServiceManager.GetInstance().TryGetService<IComponentRegistry>();

                var cycling = _component != null && componentRegistry?.Get(_component.ComponentId) != null;

                if (!UnregisterComponent(cycling))
                {
                    return;
                }

                var manufacturer = GetManufacturerInformation();
                var firmwareId = string.IsNullOrEmpty(InternalConfiguration.FirmwareId)
                    ? string.Empty : $"_{InternalConfiguration.FirmwareId}";
                var version = string.IsNullOrEmpty(InternalConfiguration.VariantVersion) && string.IsNullOrEmpty(InternalConfiguration.FirmwareRevision)
                    ? string.Empty : string.IsNullOrEmpty(InternalConfiguration.VariantVersion)
                    ? $"_{InternalConfiguration.FirmwareRevision}" : $"_{InternalConfiguration.VariantVersion}";

                var componentId = $"ATI{manufacturer}{firmwareId}{version}".Replace(" ", "_");

                _component = new Component
                {
                    ComponentId = componentId,
                    Description = Description,
                    Path = Path,
                    Size = 0,
                    Type = ComponentType.Hardware,
                    FileSystemType = FileSystemType.Stream,
                    Available = Implementation.IsConnected,
                    HasFault = AnyErrors
                };

                Implementation.Connected += _component.OnAvailable;
                Implementation.Disconnected += _component.OnUnavailable;

                componentRegistry?.Register(_component, cycling);
            }
        }

        /// <summary>Unregister Component.</summary>
        protected bool UnregisterComponent(bool cycling = false)
        {
            if (_component != null)
            {
                if (InternalConfiguration.Manufacturer.Equals(Description))
                {
                    return false;
                }

                if (Implementation != null)
                {
                    Implementation.Connected -= _component.OnAvailable;
                    Implementation.Disconnected -= _component.OnUnavailable;
                }

                ServiceManager.GetInstance().TryGetService<IComponentRegistry>()?.UnRegister(_component.ComponentId, cycling);

                _component = null;

                return true;
            }

            return true;
        }

        private DisabledReasons UpdateDisabledReasons(EnabledReasons reason)
        {
            DisabledReasons remedy = 0;

            switch (reason)
            {
                case EnabledReasons.Service:
                    remedy |= DisabledReasons.Service;
                    break;
                case EnabledReasons.Configuration:
                    remedy |= DisabledReasons.Configuration;
                    break;
                case EnabledReasons.System:
                    remedy |= DisabledReasons.System;
                    break;
                case EnabledReasons.Operator:
                    remedy |= DisabledReasons.Operator | DisabledReasons.Error | DisabledReasons.FirmwareUpdate;
                    break;
                case EnabledReasons.Reset:
                    remedy |= DisabledReasons.Error | DisabledReasons.FirmwareUpdate;
                    break;
                case EnabledReasons.Backend:
                    remedy |= DisabledReasons.Backend;
                    break;
                case EnabledReasons.Device:
                    remedy |= DisabledReasons.Device;
                    break;
                case EnabledReasons.GamePlay:
                    remedy |= DisabledReasons.GamePlay;
                    break;
            }

            var updated = ReasonDisabled & remedy;
            ReasonDisabled &= ~updated;
            return updated;
        }

        private string GetManufacturerInformation()
        {
            const string aristocrat = "Aristocrat";
            var isAristocratProduct = string.Equals(
                InternalConfiguration.Manufacturer,
                aristocrat,
                StringComparison.InvariantCultureIgnoreCase);
            if (isAristocratProduct)
            {
                return $"_{InternalConfiguration.Model}";
            }

            return string.IsNullOrEmpty(InternalConfiguration.Manufacturer)
                ? string.Empty
                : $"_{InternalConfiguration.Manufacturer}";
        }
    }
}