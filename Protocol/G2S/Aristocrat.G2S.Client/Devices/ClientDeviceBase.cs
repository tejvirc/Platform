namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using v21;

    /// <summary>
    ///     Defines a G2S client device.
    /// </summary>
    public abstract class ClientDeviceBase : IDevice, IDisposable
    {
        private readonly IDeviceObserver _deviceStateObserver;
        private bool _enabled;
        private List<int> _guests;
        private bool _hostEnabled;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientDeviceBase" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="hostEnabled">The initial host enabled attribute</param>
        protected ClientDeviceBase(int deviceId, IDeviceObserver deviceStateObserver, bool hostEnabled)
        {
            Id = deviceId;

            Active = false;
            Owner = Constants.EgmHostId;
            _guests = new List<int>();
            Configurator = Constants.EgmHostId;

            RequiredForPlay = false;

            Enabled = true;
            HostEnabled = hostEnabled;

            Locked = false;
            HostLocked = false;

            ConfigurationId = 0;
            ConfigComplete = true;

            _deviceStateObserver = deviceStateObserver;
        }

        /// <inheritdoc />
        public bool UseDefaultConfig { get; protected set; }

        /// <inheritdoc />
        public abstract string DeviceClass { get; }

        /// <inheritdoc />
        public virtual string DevicePrefix => Constants.DefaultPrefix;

        /// <inheritdoc />
        public DateTime ConfigDateTime { get; protected set; }

        /// <inheritdoc />
        public bool ConfigComplete { get; protected set; }

        /// <inheritdoc />
        public DateTime? ListStateDateTime { get; protected set; }

        /// <inheritdoc />
        public bool Existing { get; private set; }

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public long ConfigurationId { get; protected set; }

        /// <inheritdoc />
        [JsonIgnore]
        public bool Locked { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public bool HostLocked { get; set; }

        /// <inheritdoc />
        public int Owner { get; protected internal set; }

        /// <inheritdoc />
        public int Configurator { get; protected set; }

        /// <inheritdoc />
        public IEnumerable<int> Guests
        {
            get => _guests;
            protected set => _guests = value.ToList();
        }

        /// <inheritdoc />
        [JsonIgnore]
        public bool Enabled
        {
            get => _enabled;

            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    NotifyStateChanged();
                }
            }
        }

        /// <inheritdoc />
        [JsonProperty(Order = int.MaxValue)]
        public bool HostEnabled
        {
            get => _hostEnabled;

            set
            {
                if (_hostEnabled != value)
                {
                    _hostEnabled = value;
                    NotifyStateChanged();
                }
            }
        }

        /// <inheritdoc />
        public bool RequiredForPlay { get; protected set; }

        /// <inheritdoc />
        public string DisableText { get; set; }

        /// <inheritdoc />
        public bool Active { get; internal set; }

        /// <inheritdoc />
        [JsonIgnore]
        public ICommandQueue Queue { get; internal set; }

        /// <summary>
        ///     When overriden in a derived class, signifies that communication has just opened.
        /// </summary>
        /// <param name="context">Startup data provided to the device.</param>
        public abstract void Open(IStartupContext context);

        /// <summary>
        ///     When overriden in a derived class, signifies that communication is about to be closed.
        /// </summary>
        public abstract void Close();

        /// <inheritdoc />
        public virtual void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            ConfigDateTime = DateTime.UtcNow;
            ConfigComplete = true;
            ConfigurationId = optionConfigValues.ConfigurationId;
        }

        /// <inheritdoc />
        public virtual void RegisterEvents()
        {
        }

        /// <inheritdoc />
        public virtual void UnregisterEvents()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Configures the owner Id.
        /// </summary>
        /// <param name="hostId">The host Id.</param>
        /// <param name="active">Indicating whether the device is active (true) or inactive.</param>
        /// <returns>true if the device was changed.</returns>
        public bool HasOwner(int hostId, bool active)
        {
            var modified = false;

            if (Owner != hostId)
            {
                Owner = hostId;
                modified = true;
            }

            if (Active != active)
            {
                Active = active;
                modified = true;
            }

            return modified;
        }

        /// <summary>
        ///     Configures the configurator Id.
        /// </summary>
        /// <param name="hostId">The owner Id.</param>
        /// <returns>true if the device was changed.</returns>
        public bool HasConfigurator(int hostId)
        {
            if (Configurator == hostId)
            {
                return false;
            }

            Configurator = hostId;
            return true;
        }

        /// <summary>
        ///     Adds the specified id to the guest list.
        /// </summary>
        /// <param name="hostId">The host Id.</param>
        /// <returns>true if the device was changed.</returns>
        public bool AddGuest(int hostId)
        {
            if (Guests.Contains(hostId))
            {
                return false;
            }

            _guests.Add(hostId);
            return true;
        }

        /// <summary>
        ///     Removes the specified from the guest list.
        /// </summary>
        /// <param name="hostId">The host Id.</param>
        /// <returns>true if the device was changed.</returns>
        public bool RemoveGuest(int hostId)
        {
            if (!Guests.Contains(hostId))
            {
                return false;
            }

            _guests.Remove(hostId);
            return true;
        }

        /// <summary>
        ///     Creates class.
        /// </summary>
        /// <returns>An instance of IClass.</returns>
        internal abstract IClass InternalCreateClass();

        /// <summary>
        ///     Allows the device to configure it's default values.
        /// </summary>
        protected virtual void ConfigureDefaults()
        {
            UseDefaultConfig = false;
            RequiredForPlay = false;
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        ///     Sends a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        protected void SendNotification(IClass request)
        {
            Queue.SendNotification(request);
        }

        /// <summary>
        ///     Sends a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="sessionTimeout">
        ///     The <see cref="TimeSpan" /> that specifies how long the a session time to
        ///     live will be set for.
        /// </param>
        protected void SendNotification(IClass request, TimeSpan sessionTimeout)
        {
            Queue.SendNotification(request, sessionTimeout);
        }

        /// <summary>
        ///     Sends a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:DefaultSessionTimeout" />.</remarks>
        protected Session SendRequest(IClass request)
        {
            return Queue.SendRequest(request);
        }

        /// <summary>
        ///     Sends a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="sessionTimeout">
        ///     The <see cref="TimeSpan" /> that specifies how long the a session time to
        ///     live will be set for.
        /// </param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:DefaultSessionTimeout" />.</remarks>
        protected Session SendRequest(IClass request, TimeSpan sessionTimeout)
        {
            return Queue.SendRequest(request, sessionTimeout);
        }

        /// <summary>
        ///     Sends a request to a host bypassing any queue related checks.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="alwaysSend">
        ///     Bypasses the allow send on the host queue. This is reserved for communications class
        ///     (commsOnline and commsDisabled).
        /// </param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:DefaultSessionTimeout" />.</remarks>
        protected Session SendRequest(IClass request, bool alwaysSend)
        {
            return Queue.SendRequest(request, alwaysSend);
        }

        /// <summary>
        ///     Sends a request to a host bypassing any queue related checks.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="retryCount">The number of times to resend a command if the session expires.</param>
        /// <param name="alwaysSend">
        ///     Bypasses the allow send on the host queue. This is reserved for communications class
        ///     (commsOnline and commsDisabled).
        /// </param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:DefaultSessionTimeout" />.</remarks>
        protected Session SendRequest(IClass request, int retryCount, bool alwaysSend)
        {
            return Queue.SendRequest(request, retryCount, alwaysSend);
        }

        /// <summary>
        ///     Sends a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="callback">The <see cref="SessionCallback" /> delegate.</param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:DefaultSessionTimeout" />.</remarks>
        protected Session SendRequest(IClass request, SessionCallback callback)
        {
            return Queue.SendRequest(request, callback);
        }

        /// <summary>
        ///     Sends a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="callback">The <see cref="SessionCallback" /> delegate.</param>
        /// <param name="retryCount">The number of times to resend a command if the session expires.</param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:.DefaultSessionTimeout" />.</remarks>
        protected Session SendRequest(IClass request, SessionCallback callback, int retryCount)
        {
            return Queue.SendRequest(request, callback, retryCount);
        }

        /// <summary>
        ///     Sends a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="callback">The <see cref="T:SessionCallback" /> delegate.</param>
        /// <param name="retryCount">The number of times to resend a command if the session expires.</param>
        /// <returns>The session created for the request.</returns>
        /// <param name="sessionTimeout">
        ///     The value that specifies how long the a session time to live will be set for in
        ///     milliseconds.
        /// </param>
        protected virtual Session SendRequest(
            IClass request,
            SessionCallback callback,
            int retryCount,
            TimeSpan sessionTimeout)
        {
            return Queue.SendRequest(request, callback, retryCount, sessionTimeout);
        }

        /// <summary>
        ///     Queue any responses to the <i>request</i> command.
        /// </summary>
        /// <param name="request">The <see cref="T:Aristocrat.G2S.ClassCommand" /> request to respond to.</param>
        protected void SendResponse(ClassCommand request)
        {
            Queue.SendResponse(request);
        }

        /// <summary>
        ///     Notifies a subscriber that a property changed.
        /// </summary>
        /// <param name="propertyName">The property name that changed.</param>
        protected void NotifyStateChanged([CallerMemberName] string propertyName = null)
        {
            _deviceStateObserver?.Notify(this, propertyName);
        }

        /// <summary>
        ///     Sets the device value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <param name="deviceOptionConfigValues">The device option configuration values.</param>
        /// <param name="func">The function.</param>
        protected void SetDeviceValue(
            string parameterId,
            DeviceOptionConfigValues deviceOptionConfigValues,
            Action<string> func)
        {
            if (deviceOptionConfigValues.HasValue(parameterId) || deviceOptionConfigValues.HasTableValue(parameterId))
            {
                func.Invoke(parameterId);
            }
        }

        /// <summary>
        ///     Invoked when the device is deserialized
        /// </summary>
        /// <param name="context">The streaming context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Existing = true;

            if (UseDefaultConfig)
            {
                ConfigureDefaults();
            }
        }
    }

    /// <summary>
    ///     Defines a generic G2S client device.
    /// </summary>
    /// <typeparam name="TClass">The class this device implements.</typeparam>
    public abstract class ClientDeviceBase<TClass> : ClientDeviceBase
        where TClass : IClass, new()
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientDeviceBase{TClass}" /> class.
        /// </summary>
        /// <param name="deviceId">device id</param>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        protected ClientDeviceBase(int deviceId, IDeviceObserver deviceStateObserver)
            : this(deviceId, deviceStateObserver, true)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientDeviceBase{TClass}" /> class.
        /// </summary>
        /// <param name="deviceId">device id</param>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="hostEnabled">Sets the initial host enabled attribute.</param>
        protected ClientDeviceBase(int deviceId, IDeviceObserver deviceStateObserver, bool hostEnabled)
            : base(deviceId, deviceStateObserver, hostEnabled)
        {
        }

        /// <inheritdoc />
        public sealed override string DeviceClass => typeof(TClass).Name;

        /// <inheritdoc />
        internal override IClass InternalCreateClass()
        {
            return CreateClass(Id);
        }

        /// <summary>
        ///     Create a new G2S <i>TClass</i> that holds a command.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>A newly created instance of <i>TClass</i>.</returns>
        /// <remarks>
        ///     Any command generated here will have the <see cref="IClass.deviceId" /> and
        ///     <see cref="IClass.dateTime" /> set to the appropriate values.
        /// </remarks>
        protected static TClass CreateClass(int deviceId)
        {
            // Create a copy of the same type
            var newClass = new TClass
            {
                deviceId = deviceId,
                dateTime = DateTime.UtcNow
            };

            return newClass;
        }
    }
}
