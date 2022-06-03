namespace Aristocrat.Monaco.Hardware.Serial.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Ports;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.Gds;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Common serial device protocol functions.  This is the engine for every protocol,
    ///     driving a "data layer" that assembles messages to, and disassembles messages from,
    ///     the serial port controller.
    /// </summary>
    public abstract class SerialDeviceProtocol : IGdsCommunicator
    {
        public const int UnknownCrc = -1;

        /// <summary>The expected inter-bytes delay in MS</summary>
        private const int InterBytesDelay = 10;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object _lock = new object();

        private Timer _pollTimer;
        private ISerialPortController _physicalLayer;
        private bool _isAttached;
        private byte[] _messageUnderConstruction;
        private int _bytesInSoFar;
        private int _elementsSoFar;
        private int _crcIn;
        private bool _allowPolling = true;
        private int _failedPollCount;
        private byte _transactionId;
        private bool _disposed;

        /// <summary>
        ///     The default template is simply a single general-data field, which the protocol layer
        ///     will have to completely fill and parse.  Useful in protocols that are more free-form
        ///     or primitive.  See <see cref="IMessageTemplate"/> for details.
        /// </summary>
        private readonly IMessageTemplate _defaultMessageTemplate = new MessageTemplate<NullCrcEngine>
            (
                new List<MessageTemplateElement>
                {
                    new MessageTemplateElement{ ElementType = MessageTemplateElementType.VariableData }
                },
                0
            );

        private IMessageTemplate _temporaryReceiveMessageTemplate;

        private IMessageTemplate ReceiveMessageTemplate => _temporaryReceiveMessageTemplate ?? _defaultMessageTemplate;

        public virtual string Name { get; protected set; }

        /// <inheritdoc/>
        public virtual string Protocol { get; protected set; }

        /// <inheritdoc />
        public virtual DeviceType DeviceType { get; set; }

        /// <inheritdoc/>
        public virtual string Manufacturer { get; set; }

        /// <inheritdoc/>
        public virtual string Model { get; set; }

        /// <inheritdoc/>
        public virtual string Firmware { get; set; }

        /// <inheritdoc/>
        public virtual string FirmwareVersion { get; set; }

        /// <inheritdoc/>
        public virtual string FirmwareRevision { get; set; }

        /// <inheritdoc/>
        public virtual int FirmwareCrc { get; set; }

        /// <inheritdoc/>
        public virtual string BootVersion { get; set; }

        /// <inheritdoc/>
        public string VariantName { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string VariantVersion { get; set; } = string.Empty;

        /// <inheritdoc />
        public bool IsDfuCapable => false;

        /// <inheritdoc/>
        public virtual string SerialNumber { get; set; }

        /// <inheritdoc/>
        public virtual bool IsOpen { get; protected set; }

        /// <inheritdoc/>
        public int VendorId { get; set; }

        /// <inheritdoc/>
        public int ProductId { get; set; }

        /// <inheritdoc/>
        public int ProductIdDfu { get; set; }

        /// <summary>Gets or sets a value indicating whether the device is enabled.</summary>
        protected bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the GAT data (identification string)
        /// </summary>
        protected string GatData => $"{Manufacturer},{Model},{FirmwareVersion},{FirmwareRevision}";

        /// <summary>Gets or sets a value indicating whether the device is configured.</summary>
        protected bool IsConfigured { get; set; }

        /// <summary>Gets or sets value indicating whether the communication sync mode is used. </summary>
        protected bool UseSyncMode { get; set; }

        /// <summary>The minimal MS for the device to receive a request and send the response out</summary>
        protected int MinimumResponseTime { get; set; } = 500;

        /// <summary>
        ///     How many failed attempts until the device heartbeat is considered failed
        /// </summary>
        protected int MaxFailedPollCount { get; set; }

        /// <inheritdoc/>
        public IDevice Device { get; set; }

        /// <summary>
        ///     How long is the status polling interval.
        /// </summary>
        public int PollIntervalMs { get; protected set; } = Timeout.Infinite;

        /// <summary>
        ///     Max size of receive and transmit buffers
        /// </summary>
        public int MaxBufferLength { get; protected set; } = 256;

        /// <summary>
        ///     Timeout value for message reception and transmission.
        /// </summary>
        public int CommunicationTimeoutMs { get; protected set; } = 150;

        /// <inheritdoc />
        public void SendMessage(GdsSerializableMessage message, CancellationToken token)
        {
            ProcessMessage(message, token);
        }

        /// <inheritdoc />
        public event EventHandler<GdsSerializableMessage> MessageReceived;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> DeviceAttached;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> DeviceDetached;

        /// <summary>
        ///     Raised when a message is received
        /// </summary>
        protected event EventHandler<MessagedBuiltEventArgs> MessageBuilt;

        /// <summary>
        ///     Construct
        /// </summary>
        protected SerialDeviceProtocol(IMessageTemplate newDefaultTemplate = null)
        {
            if (newDefaultTemplate != null)
                _defaultMessageTemplate = newDefaultTemplate;

            _pollTimer = new Timer(PollTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
 
            // *NOTE* This override is needed when using the Device Simulator on a slower/resource starved system to avoid 
            // unintentional disconnects due to more than 3 failed polls.
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            MaxFailedPollCount = Convert.ToInt32(propertiesManager.GetProperty(HardwareConstants.MaxFailedPollCount, HardwareConstants.DefaultMaxFailedPollCount));
            Logger.Debug($"Max failed poll count {MaxFailedPollCount}");
        }

        /// <inheritdoc/>
        public virtual bool Configure(IComConfiguration comConfiguration)
        {
            Model = string.Empty;

            lock (_lock)
            {
                IsConfigured = false;
                Logger.Debug($"Configure {GetType()}");

                _physicalLayer = Device ?? throw new ArgumentNullException(nameof(Device));
                _physicalLayer.UseSyncMode = UseSyncMode;
                if (!UseSyncMode)
                {
                    _physicalLayer.ReceivedData += PhysicalLayerDataReceived;
                }

                if (comConfiguration == null)
                {
                    throw new ArgumentNullException(nameof(comConfiguration));
                }

                // Incoming data already includes Mode, PortName, BaudRate, DataBits, Parity, StopBits, Handshake;
                // we need to manage timing and buffers.
                comConfiguration.ReadBufferSize = MaxBufferLength;
                comConfiguration.WriteBufferSize = MaxBufferLength;
                comConfiguration.ReadTimeoutMs = UseSyncMode ? CommunicationTimeoutMs : SerialPort.InfiniteTimeout;
                comConfiguration.WriteTimeoutMs = CommunicationTimeoutMs;
                comConfiguration.KeepAliveTimeoutMs = SerialPort.InfiniteTimeout;

                _physicalLayer.Configure(comConfiguration);

                Protocol = comConfiguration.Protocol; // default

                Name = comConfiguration.Name;

                IsConfigured = true;
                Logger.Debug($"Configured {comConfiguration.PortName} for {GetType()}");

                // Open the port and start communications.  this is needed to detect Attached/Detached
                try
                {
                    Logger.Debug($"Opening port for {GetType()}");
                    FirmwareCrc = UnknownCrc;

                    _physicalLayer.IsEnabled = true;
                    ResetMessageIn();

                    GetDeviceInformation();
                    IsOpen = true;

                    Logger.Debug($"Opened {GetType()}");
                    return true;
                }
                catch (IOException e)
                {
                    Logger.Error($"Open: IOException {e}");
                    ResetConnection();
                    return false;
                }
                catch (ArgumentException e)
                {
                    Logger.Error($"Open: ArgumentException {e}");
                    ResetConnection();
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public virtual bool Open()
        {
            // The real opening of the port is done in Configure(), and the port remains running
            // as long as this object exists.  This is how we detect Attached/Detached.
            lock (_lock)
            {
                ResetMessageIn();
            }

            return true;
        }

        /// <inheritdoc/>
        public virtual bool Close()
        {
            // The real closing of the port is done in Dispose(), since the port remains running
            // as long as this object exists.  This is how we detect Attached/Detached.
            lock (_lock)
            {
                ResetMessageIn();
            }

            return true;
        }

        /// <inheritdoc/>
        public virtual void ResetConnection()
        {
            Logger.Debug($"Resetting connection for {GetType()} ...");
            Task.Run(() => {
                Close();
                Task.Delay(500);
                Open();
            });
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Perform self test.</summary>
        protected abstract void SelfTest();

        /// <summary>Get device identification information.</summary>
        /// <returns>Whether or not the device information was successfully read from the device</returns>
        protected abstract bool GetDeviceInformation();

        /// <summary>Calculate CRC.</summary>
        protected abstract void CalculateCrc();

        /// <summary>Enable or disable.</summary>
        /// <param name="enable">True to enable.</param>
        protected abstract void Enable(bool enable);

        /// <summary>Whether or not the device has a disabling fault.</summary>
        protected abstract bool HasDisablingFault { get; }

        protected void EnablePolling(bool enable)
        {
            _allowPolling = enable;
            ResetPolling();
        }

        protected void DisablePolling(int msDisableTime)
        {
            if (!_allowPolling)
            {
                return;
            }

            EnablePolling(false);
            Task.Delay(msDisableTime).ContinueWith(_ => EnablePolling(true));
        }

        protected void UpdatePollingRate(int pollingIntervalMs)
        {
            PollIntervalMs = pollingIntervalMs;
            ResetPolling();
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    Logger.Debug($"Dispose {GetType()}");

                    try
                    {
                        _pollTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                        _physicalLayer.ReceivedData -= PhysicalLayerDataReceived;
                        Logger.Debug($"Closing {GetType()}");

                        ResetMessageIn();
                        _physicalLayer.IsEnabled = false;
                        IsOpen = false;

                        Logger.Debug($"Closed {GetType()}");
                    }
                    catch (IOException e)
                    {
                        Logger.Error($"Close: IOException {e}");
                    }

                    if (_pollTimer != null)
                    {
                        _pollTimer.Dispose();
                        _pollTimer = null;
                    }

                    _physicalLayer.Dispose();
                    _physicalLayer = null;
                }

                _disposed = true;
            }
        }

        /// <summary>Raises the <see cref="DeviceAttached"/> event.</summary>
        protected virtual void OnDeviceAttached()
        {
            _failedPollCount = 0;
            DeviceAttached?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="DeviceDetached"/> event.</summary>
        protected virtual void OnDeviceDetached()
        {
            //Reset cached FirmwareCrc to UnknownCrc, so that the latest crc will be read on device attach again.
            FirmwareCrc = UnknownCrc;
            DeviceDetached?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="MessageReceived"/> event.</summary>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if sent.</returns>
        protected virtual bool OnMessageReceived(GdsSerializableMessage message)
        {
            Logger.Debug($"Respond with {message.ReportId}");
            var invoker = MessageReceived;
            if (null != invoker)
            {
                if (message is ITransactionSource transactionMessage)
                {
                    transactionMessage.TransactionId = _transactionId++;
                }

                invoker.Invoke(this, message);
                return true;
            }

            return false;
        }

        /// <summary>Process a GDS message into protocol calls.</summary>
        /// <param name="message">GDS message</param>
        /// <param name="token">The cancellation token</param>
        protected virtual void ProcessMessage(GdsSerializableMessage message, CancellationToken token)
        {
            Logger.Debug($"Process message {message}");
            switch (message.ReportId)
            {
                // Common to all GDS devices
                case GdsConstants.ReportId.Ack:
                    if (message is Ack ack)
                    {
                        if (ack.Resync)
                        {
                            _transactionId = ack.TransactionId;
                        }
                    }
                    break;
                case GdsConstants.ReportId.Enable:
                case GdsConstants.ReportId.Disable:
                    IsEnabled = message.ReportId == GdsConstants.ReportId.Enable && !HasDisablingFault;
                    Enable(IsEnabled);
                    OnMessageReceived(new DeviceState
                    {
                        Enabled = IsEnabled,
                        Disabled = !IsEnabled
                    });
                    break;
                case GdsConstants.ReportId.SelfTest:
                    SelfTest();
                    break;
                case GdsConstants.ReportId.RequestGatReport:
                    if (GetDeviceInformation())
                    {
                        OnMessageReceived(new GatData { Data = GatData });
                    }

                    break;
                case GdsConstants.ReportId.CalculateCrc:
                    CalculateCrc();
                    OnMessageReceived(new CrcData
                    {
                        Result = FirmwareCrc
                    });
                    break;
            }
        }

        /// <summary>
        ///     Send bytes to the port
        /// </summary>
        /// <param name="command">Command bytes</param>
        /// <param name="tempSendTemplate">If not null, this template is used during this one transmission</param>
        protected void SendCommand(byte[] command, IMessageTemplate tempSendTemplate = null)
        {
            if (command == null || command.Length == 0)
            {
                return;
            }

            lock (_lock)
            {
                SendCommandInternal(command, tempSendTemplate);
            }
        }

        protected void WaitSendComplete()
        {
            lock (_lock)
            {
                if (_physicalLayer != null)
                {
                    while (_physicalLayer.BytesToWrite > 0)
                        Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        ///     Send bytes to the port and wait for a response.
        /// </summary>
        /// <param name="command">Command bytes</param>
        /// <param name="responseCount">How many response bytes to get (-1 means indefinite)</param>
        /// <param name="tempSendTemplate">If not null, this template is used during this one transmission</param>
        /// <param name="tempGetTemplate">If not null, this template is used during this one reception</param>
        /// <returns>Response bytes</returns>
        protected byte[] SendCommandAndGetResponse(byte[] command, int responseCount, IMessageTemplate tempSendTemplate = null, IMessageTemplate tempGetTemplate = null)
        {
            lock (_lock)
            {
                if (_physicalLayer == null)
                {
                    Logger.Debug("Cannot send a command without a communication channel.");
                    return null;
                }

                SendCommandInternal(command, tempSendTemplate);
                return GetResponseInternal(responseCount, tempGetTemplate);
            }
        }

        /// <summary>
        ///     Whether or not the device is enabled and able to send and receive bytes
        /// </summary>
        protected bool IsDeviceEnabled => _physicalLayer?.IsEnabled ?? false;

        /// <summary>
        ///     Whether or not the device is connected and communication is healthy
        /// </summary>
        protected bool IsAttached
        {
            get => _isAttached;
            set
            {
                if (_isAttached == value)
                {
                    return;
                }

                _isAttached = value;
                if (_isAttached)
                {
                    OnDeviceAttached();
                }
                else
                {
                    OnDeviceDetached();
                }
            }
        }

        /// <summary>
        ///     Ask device for status and process response
        /// </summary>
        /// <returns>True if a valid response was processed</returns>
        protected abstract bool RequestStatus();

        /// <summary>
        ///     Default poll timer handler
        /// </summary>
        /// <param name="obj">N/A</param>
        protected virtual void PollTimerCallback(object obj)
        {
            lock (_lock)
            {
                if (_physicalLayer == null)
                {
                    return;
                }
            }

            if (RequestStatus())
            {
                _failedPollCount = 0;
                IsAttached = true;
            }
            else if (IsAttached && ++_failedPollCount >= MaxFailedPollCount)
            {
                IsAttached = false;
                Logger.Error($"Poll failed {_failedPollCount} times, device detached");                
            }
        }

        protected void AdjustReceiveTimeout(int timeoutMs)
        {
            lock (_lock)
            {
                if (_physicalLayer != null)
                {
                    _physicalLayer.ReadTimeout = timeoutMs;
                }
            }
        }

        protected void AdjustWriteTimeout(int timeoutMs)
        {
            lock (_lock)
            {
                if (_physicalLayer != null)
                {
                    _physicalLayer.WriteTimeout = timeoutMs;
                }
            }
        }

        protected byte[] ConvertIntToEndianBytes(int val, int size, bool bigEndian)
        {
            var buffer = new byte[size];
            WriteIntToEndianBytes(val, size, bigEndian, buffer);
            return buffer;
        }

        protected void WriteIntToEndianBytes(int val, int size, bool bigEndian, byte[] buffer, int bufferOffset = 0)
        {
            if (bufferOffset + size > buffer.Length)
                throw new ArgumentOutOfRangeException($"{nameof(size)} or {nameof(bufferOffset)}");

            var start = bigEndian ? size - 1 : 0;
            var dir = bigEndian ? -1 : 1;
            for (int index = start; bigEndian ? index >= 0 : index < size; index += dir)
            {
                buffer[bufferOffset + index] = (byte)val;
                val >>= 8;
            }
        }

        protected int ConvertEndianBytesToInt(byte[] buffer, int size, bool bigEndian, int bufferOffset = 0)
        {
            if (bufferOffset + size > buffer.Length)
                throw new ArgumentOutOfRangeException($"{nameof(size)} or {nameof(bufferOffset)}");

            var start = bigEndian ? 0 : size - 1;
            var dir = bigEndian ? 1 : -1;
            int result = 0;
            for (int index = start; bigEndian ? index < size : index >= 0; index += dir)
            {
                result <<= 8;
                result += buffer[bufferOffset + index];
            }
            return result;
        }

        protected int CompareByteArrays(byte[] b1, byte[] b2, int count)
        {
            if (b1 == null || b1.Length < count)
                return -1;
            if (b2 == null || b2.Length < count)
                return 1;
            for (var index = 0; index < count; index++)
            {
                if (b1[index] < b2[index])
                    return -1;
                if (b1[index] > b2[index])
                    return 1;
            }
            return 0;
        }

        protected void ResetMessageIn(bool flushBuffer = true)
        {
            lock (_lock)
            {
                _bytesInSoFar = 0;
                _elementsSoFar = 0;
                _crcIn = 0;
                if (!UseSyncMode)
                {
                    _messageUnderConstruction = new byte[1];
                    ReceiveMessageTemplate.CrcEngineIn.Initialize(ReceiveMessageTemplate.CrcSeed);
                }
                else
                {
                    _messageUnderConstruction = null;
                }

                if (flushBuffer)
                {
                    _physicalLayer?.FlushInputAndOutput();
                }
            }
        }

        private void SendCommandInternal(byte[] command, IMessageTemplate tempSendTemplate)
        {
            if (!IsDeviceEnabled)
            {
                return;
            }

            var messageTemplate = tempSendTemplate ?? _defaultMessageTemplate;

            // Build up the message per the template
            var buf = new byte[command.Length + messageTemplate.NonDataLength];
            messageTemplate.CrcEngineOut.Initialize(messageTemplate.CrcSeed);
            var bufIndex = 0;
            var cmdIndex = 0;
            foreach (var element in messageTemplate.Elements)
            {
                switch (element.ElementType)
                {
                    case MessageTemplateElementType.FixedData:
                        Buffer.BlockCopy(command, 0, buf, bufIndex, element.Length);
                        if (element.IncludedInCrc)
                        {
                            messageTemplate.CrcEngineOut.Hash(command, 0, (uint)element.Length);
                        }

                        cmdIndex = element.Length;
                        bufIndex += element.Length;
                        break;
                    case MessageTemplateElementType.VariableData:
                        Buffer.BlockCopy(command, cmdIndex, buf, bufIndex, command.Length - cmdIndex);
                        if (element.IncludedInCrc)
                        {
                            messageTemplate.CrcEngineOut.Hash(
                                command,
                                (uint)cmdIndex,
                                (uint)(command.Length - cmdIndex));
                        }

                        bufIndex += command.Length - cmdIndex;
                        cmdIndex = command.Length;
                        break;
                    case MessageTemplateElementType.FullLength:
                        var fullLenBuf = ConvertIntToEndianBytes(buf.Length, element.Length, element.BigEndian);
                        Buffer.BlockCopy(fullLenBuf, 0, buf, bufIndex, element.Length);
                        if (element.IncludedInCrc)
                        {
                            messageTemplate.CrcEngineOut.Hash(fullLenBuf, 0, (uint)element.Length);
                        }

                        bufIndex += element.Length;
                        break;
                    case MessageTemplateElementType.ConstantDataLengthMask:
                        var calculatedLength = buf.Length - messageTemplate.NonDataLength;
                        var mask = ConvertEndianBytesToInt(command, element.Length, element.BigEndian) & ConvertEndianBytesToInt(element.Value, element.Length, element.BigEndian);
                        var dataMaskedBuf = ConvertIntToEndianBytes(
                            calculatedLength | mask,
                            element.Length,
                            element.BigEndian);
                        Buffer.BlockCopy(dataMaskedBuf, 0, buf, bufIndex, element.Length);
                        if (element.IncludedInCrc)
                        {
                            messageTemplate.CrcEngineOut.Hash(dataMaskedBuf, 0, (uint)element.Length);
                        }

                        cmdIndex = element.Length;
                        bufIndex += element.Length;
                        break;
                    case MessageTemplateElementType.ConstantMask:
                        var constantMask = ConvertEndianBytesToInt(command, element.Length, element.BigEndian);
                        var dataMaskedBuf2 = ConvertIntToEndianBytes(
                            constantMask,
                            element.Length,
                            element.BigEndian);
                        Buffer.BlockCopy(command, 0, buf, bufIndex, element.Length);
                        if (element.IncludedInCrc)
                        {
                            messageTemplate.CrcEngineOut.Hash(dataMaskedBuf2, 0, (uint)element.Length);
                        }

                        cmdIndex = element.Length;
                        bufIndex += element.Length;
                        break;
                    case MessageTemplateElementType.DataLength:
                        var dataLenBuf = ConvertIntToEndianBytes(
                            buf.Length - messageTemplate.NonDataLength,
                            element.Length,
                            element.BigEndian);
                        Buffer.BlockCopy(dataLenBuf, 0, buf, bufIndex, element.Length);
                        if (element.IncludedInCrc)
                        {
                            messageTemplate.CrcEngineOut.Hash(dataLenBuf, 0, (uint)element.Length);
                        }

                        bufIndex += element.Length;
                        break;
                    case MessageTemplateElementType.LengthPlusDataLength:
                        var ldLenBuf = ConvertIntToEndianBytes(
                            buf.Length - messageTemplate.NonDataLength - element.Length,
                            element.Length,
                            element.BigEndian);
                        Buffer.BlockCopy(ldLenBuf, 0, buf, bufIndex, element.Length);
                        if (element.IncludedInCrc)
                        {
                            messageTemplate.CrcEngineOut.Hash(ldLenBuf, 0, (uint)element.Length);
                        }

                        bufIndex += element.Length;
                        break;
                    case MessageTemplateElementType.Crc:
                        var crcBuf = ConvertIntToEndianBytes(
                            messageTemplate.CrcEngineOut.Crc,
                            element.Length,
                            element.BigEndian);
                        Buffer.BlockCopy(crcBuf, 0, buf, bufIndex, element.Length);
                        bufIndex += element.Length;
                        break;
                    case MessageTemplateElementType.Constant:
                        Buffer.BlockCopy(element.Value, 0, buf, bufIndex, element.Length);
                        if (element.IncludedInCrc)
                        {
                            messageTemplate.CrcEngineOut.Hash(element.Value, 0, (uint)element.Length);
                        }

                        bufIndex += element.Length;
                        break;
                }
            }

            for (var index = 0; index < buf.Length; index += MaxBufferLength)
            {
                var toBeSent = Math.Min(MaxBufferLength, buf.Length - index);
                var bufToSend = new byte[toBeSent];
                Buffer.BlockCopy(buf, index, bufToSend, 0, toBeSent);
                _physicalLayer.WriteBuffer(bufToSend);
            }
        }

        private byte[] GetResponseInternal(int count, IMessageTemplate tempGetTemplate = null)
        {
            if (!IsDeviceEnabled || !UseSyncMode)
            {
                return null;
            }

            _temporaryReceiveMessageTemplate = tempGetTemplate;
            var messageTemplate = ReceiveMessageTemplate;

            _messageUnderConstruction = new byte[count > 0 ? count : MaxBufferLength];
            messageTemplate.CrcEngineIn.Initialize(messageTemplate.CrcSeed);

            ConstructMessage();

            _temporaryReceiveMessageTemplate = null;
            var response = _messageUnderConstruction;

            ResetMessageIn();
            return response;
        }

        private void ResetPolling() => _pollTimer?.Change(
            _allowPolling ? PollIntervalMs : Timeout.Infinite,
            _allowPolling ? PollIntervalMs : Timeout.Infinite);

        private void ConstructMessage()
        {
            if (_messageUnderConstruction == null)
            {
                return;
            }

            try
            {
                var messageTemplate = ReceiveMessageTemplate;

                // Assemble incoming message
                while (_elementsSoFar < messageTemplate.Elements.Count && _messageUnderConstruction != null)
                {
                    var element = messageTemplate.Elements[_elementsSoFar];
                    var elementLength = (_elementsSoFar == messageTemplate.GeneralDataIndex ? _messageUnderConstruction.Length - _bytesInSoFar : element.Length);
                    if (elementLength > 0)
                    {
                        if (!UseSyncMode && _physicalLayer.BytesToRead < elementLength ||
                            UseSyncMode && !WaitForCompleteResponse(elementLength))
                        {
                            return;
                        }

                        var temp = new byte[elementLength];
                        var cnt = _physicalLayer.TryReadBuffer(ref temp, 0, elementLength);
                        if (cnt > 0 && cnt == elementLength)
                        {
                            if (element.ElementType != MessageTemplateElementType.Crc && element.IncludedInCrc)
                            {
                                messageTemplate.CrcEngineIn.Hash(temp, 0, (uint)temp.Length);
                            }

                            switch (element.ElementType)
                            {
                                case MessageTemplateElementType.DataLength:
                                    var dataLen = ConvertEndianBytesToInt(temp, element.Length, element.BigEndian);
                                    var tempBuf1 = new byte[_bytesInSoFar];
                                    _messageUnderConstruction = new byte[dataLen];
                                    Buffer.BlockCopy(tempBuf1, 0, _messageUnderConstruction, 0, _bytesInSoFar);
                                    break;
                                case MessageTemplateElementType.ConstantDataLengthMask:
                                    var dataWithMask = ConvertEndianBytesToInt(temp, element.Length, element.BigEndian);
                                    var mask = ConvertEndianBytesToInt(element.Value, element.Length, element.BigEndian);
                                    _messageUnderConstruction = new byte[dataWithMask & ~mask];
                                    Buffer.BlockCopy(temp, 0, _messageUnderConstruction, 0, element.Length);
                                    _bytesInSoFar += element.Length;
                                    break;
                                case MessageTemplateElementType.FullLength:
                                    var fullLen = ConvertEndianBytesToInt(temp, element.Length, element.BigEndian);
                                    var tempBuf2 = new byte[_bytesInSoFar];
                                    _messageUnderConstruction = new byte[fullLen - messageTemplate.NonDataLength];
                                    Buffer.BlockCopy(tempBuf2, 0, _messageUnderConstruction, 0, _bytesInSoFar);
                                    break;
                                case MessageTemplateElementType.LengthPlusDataLength:
                                    var ldLen = ConvertEndianBytesToInt(temp, element.Length, element.BigEndian);
                                    var tempBuf3 = new byte[_bytesInSoFar];

                                    // if expected data length is defined in the message template, use that length
                                    var expectedGeneralDataLength = messageTemplate.Elements[messageTemplate.GeneralDataIndex].Length;
                                    if (expectedGeneralDataLength > 0 && expectedGeneralDataLength <= ldLen)
                                    {
                                        _messageUnderConstruction = new byte[expectedGeneralDataLength];
                                    }
                                    else
                                    {
                                        _messageUnderConstruction = new byte[ldLen - element.Length];
                                    }
                                    Logger.Debug($"ConstructMessage / expected {expectedGeneralDataLength} while length segment returned {ldLen}");
                                    Buffer.BlockCopy(tempBuf3, 0, _messageUnderConstruction, 0, _bytesInSoFar);
                                    break;
                                case MessageTemplateElementType.Crc:
                                    _crcIn = ConvertEndianBytesToInt(temp, element.Length, element.BigEndian);
                                    break;
                                case MessageTemplateElementType.FixedData:
                                case MessageTemplateElementType.VariableData:
                                    Buffer.BlockCopy(temp, 0, _messageUnderConstruction, _bytesInSoFar, cnt);
                                    _bytesInSoFar += cnt;
                                    break;
                                case MessageTemplateElementType.Constant:
                                    // toss it
                                    break;
                            }
                        }
                        else
                            break;
                    }

                    _elementsSoFar++;
                }

                if (_elementsSoFar == messageTemplate.Elements.Count && _messageUnderConstruction?.Length == _bytesInSoFar)
                {
                    if (messageTemplate.CrcEngineIn.Crc != (ushort)_crcIn)
                    {
                        ResetMessageIn(false);
                        Logger.Error($"Invalid CRC (them {_crcIn:X}, us {messageTemplate.CrcEngineIn.Crc:X}");
                        return;
                    }

                    // Message is complete.
                    MessageBuilt?.Invoke(this, new MessagedBuiltEventArgs(_messageUnderConstruction));

                    if (!UseSyncMode)
                    {
                        ResetMessageIn(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"receive error {ex}");
                ResetMessageIn();
            }
        }

        private bool WaitForCompleteResponse(int responseLength)
        {
            var waitMs = MinimumResponseTime + InterBytesDelay * responseLength;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            while (_physicalLayer.BytesToRead < responseLength && stopwatch.ElapsedMilliseconds <= waitMs)
            {
                Thread.Sleep(1);
            }

            stopwatch.Stop();
            if (_physicalLayer.BytesToRead >= responseLength || stopwatch.ElapsedMilliseconds <= waitMs)
            {
                return true;
            }

            Logger.Debug($"WaitForCompleteResponse / read timeout on {_physicalLayer.PortName} expected bytes={responseLength} actual={_physicalLayer.BytesToRead}");
            return false;
        }

        private void PhysicalLayerDataReceived(object sender, EventArgs e)
        {
            lock (_lock)
            {
                if (!IsDeviceEnabled || _physicalLayer.BytesToRead <= 0)
                {
                    return;
                }

                ConstructMessage();
            }
        }
    }
}
