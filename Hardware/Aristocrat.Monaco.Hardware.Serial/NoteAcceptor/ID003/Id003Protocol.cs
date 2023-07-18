namespace Aristocrat.Monaco.Hardware.Serial.NoteAcceptor.ID003
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Contracts;
    using Contracts.Gds.NoteAcceptor;
    using Contracts.NoteAcceptor;
    using Contracts.SharedDevice;
    using log4net;
    using Protocols;

    /// <summary>
    ///     Manage outgoing messages per the ID003 protocol.  Poll for status when no other
    ///     commands are pending.  Process responses.
    /// </summary>
    [SearchableSerialProtocol(DeviceType.NoteAcceptor)]
    [HardwareDevice("ID003", DeviceType.NoteAcceptor)]
    public class Id003Protocol : SerialNoteAcceptor
    {
        private const byte Sync = 0xFC;
        private const byte PolledCommMode = 0;
        private const byte Inhibited = 1;
        private const byte Uninhibited = 0;
        private const byte Interleaved2Of5Code = 1;
        private const byte EnableNotesAndBarcodes = 0xFC;
        private const byte TicketEscrowCode = 0x6F;
        private const int BarcodeLengthMinimum = 6;
        private const int BarcodeLengthMaximum = 18;
        private const int MaxRetrieveInfoAttempts = 3;
        private const int PollingFrequency = 150;
        private const int ExpectedResponseTime = 50;
        private const int ResponseTimeInInitialization = 500;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Format for all ID-003 messages:
        ///     [SYNC][Length][Command/Status]{[Data0][Data1]...}[CrcLo][CrcHi]
        ///     Where:
        ///     SYNC is 0xFC
        ///     Length is the total message length
        ///     Command/Status is defined in <see cref="Id003Command" /> and <see cref="Id003Status" /> enums
        ///     Data0, Data1, ... are additional data bytes as required (or not) by the command/status
        ///     CRC is two bytes, lowest first, using algorithm in <see cref="Crc16Ccitt" />, of all preceding bytes
        /// </summary>
        private static readonly IMessageTemplate Id003DefaultTemplate = new MessageTemplate<Crc16Ccitt>(
            new List<MessageTemplateElement>
            {
                new()
                {
                    ElementType = MessageTemplateElementType.Constant,
                    Length = 1,
                    IncludedInCrc = true,
                    Value = new[] { Sync }
                },
                new() { ElementType = MessageTemplateElementType.FullLength, IncludedInCrc = true, Length = 1 },
                new() { ElementType = MessageTemplateElementType.FixedData, IncludedInCrc = true, Length = 1 },
                new() { ElementType = MessageTemplateElementType.VariableData, IncludedInCrc = true },
                new() { ElementType = MessageTemplateElementType.Crc, Length = 2 }
            },
            0);

        private static readonly Dictionary<int, ISOCurrencyCode> CountryToIsoCurrencyCodeTable = new()
        {
            // The numbers are the "country number" from spec Appendix E
            { 14, ISOCurrencyCode.ARS }, // Argentina
            { 2, ISOCurrencyCode.AUD }, // Australia
            { 15, ISOCurrencyCode.EUR }, // Austria
            { 55, ISOCurrencyCode.EUR }, // Austria
            { 16, ISOCurrencyCode.BBD }, // Barbados
            { 35, ISOCurrencyCode.EUR }, // Belgium
            //{ ? ,? }, // Botswana
            { 12, ISOCurrencyCode.BRL }, // Brazil
            { 8, ISOCurrencyCode.CAD }, // Canada
            { 72, ISOCurrencyCode.CAD }, // Canada
            { 78, ISOCurrencyCode.CLP }, // Chile
            { 45, ISOCurrencyCode.CNY }, // China
            { 25, ISOCurrencyCode.COP }, // Colombia
            { 77, ISOCurrencyCode.CRC }, // Costa Rica
            { 44, ISOCurrencyCode.CZK }, // Czech Republic
            { 58, ISOCurrencyCode.DKK }, // Denmark
            { 23, ISOCurrencyCode.GBP }, // England
            { 62, ISOCurrencyCode.GBP }, // England
            { 38, ISOCurrencyCode.GBP }, // England - Gibraltar
            { 68, ISOCurrencyCode.GBP }, // England - Isle of Man
            { 36, ISOCurrencyCode.EUR }, // Estonia
            { 37, ISOCurrencyCode.EUR }, // Estonia
            { 32, ISOCurrencyCode.EUR }, // Finland
            { 24, ISOCurrencyCode.EUR }, // France
            { 4, ISOCurrencyCode.EUR }, // Germany
            { 52, ISOCurrencyCode.EUR }, // Germany
            { 53, ISOCurrencyCode.EUR }, // Germany
            { 84, ISOCurrencyCode.EUR }, // Germany - Sweden
            { 30, ISOCurrencyCode.EUR }, // Greece
            { 61, ISOCurrencyCode.EUR }, // Greece
            { 48, ISOCurrencyCode.HUF }, // Hungary
            { 73, ISOCurrencyCode.ISK }, // Iceland
            { 42, ISOCurrencyCode.EUR }, // Ireland
            { 86, ISOCurrencyCode.ILS }, // Israel
            { 11, ISOCurrencyCode.EUR }, // Italy
            { 40, ISOCurrencyCode.EUR }, // Italy
            { 10, ISOCurrencyCode.JPY }, // Japan
            { 81, ISOCurrencyCode.KZT }, // Kazakhstan
            { 21, ISOCurrencyCode.KRW }, // Korea
            { 51, ISOCurrencyCode.KRW }, // Korea
            { 70, ISOCurrencyCode.EUR }, // Latvia
            { 79, ISOCurrencyCode.EUR }, // Lithuania
            { 89, ISOCurrencyCode.HKD }, // Macau
            { 33, ISOCurrencyCode.MYR }, // Malaysia
            { 69, ISOCurrencyCode.MYR }, // Malaysia
            { 71, ISOCurrencyCode.MUR }, // Mauritius
            { 9, ISOCurrencyCode.MXN }, // Mexico
            { 59, ISOCurrencyCode.ZAR }, // Nambia
            { 19, ISOCurrencyCode.EUR }, // Netherlands
            { 60, ISOCurrencyCode.EUR }, // Netherlands
            { 13, ISOCurrencyCode.NZD }, // New Zealand
            { 75, ISOCurrencyCode.NZD }, // New Zealand
            { 63, ISOCurrencyCode.NZD }, // New Zealand
            { 7, ISOCurrencyCode.NOK }, // Norway
            { 80, ISOCurrencyCode.NOK }, // Norway
            { 47, ISOCurrencyCode.PEN }, // Peru Soles
            { 74, ISOCurrencyCode.PHP }, // Philippines
            { 26, ISOCurrencyCode.PLN }, // Poland
            { 41, ISOCurrencyCode.PLN }, // Poland
            { 85, ISOCurrencyCode.PLN }, // Poland
            { 20, ISOCurrencyCode.EUR }, // Portugal
            { 66, ISOCurrencyCode.QAR }, // Qatar
            { 76, ISOCurrencyCode.RON }, // Romania
            { 39, ISOCurrencyCode.RUB }, // Russian Federation
            { 34, ISOCurrencyCode.SGD }, // Singapore
            { 64, ISOCurrencyCode.SGD }, // Singapore
            { 65, ISOCurrencyCode.EUR }, // Slovak Republic
            { 6, ISOCurrencyCode.ZAR }, // South Africa
            { 3, ISOCurrencyCode.EUR }, // Spain
            { 5, ISOCurrencyCode.SEK }, // Sweden
            { 22, ISOCurrencyCode.CHF }, // Switzerland
            { 54, ISOCurrencyCode.CHF }, // Switzerland
            { 57, ISOCurrencyCode.CHF }, // Switzerland
            { 29, ISOCurrencyCode.TWD }, // Taiwan
            { 82, ISOCurrencyCode.TZS }, // Tanzania
            { 18, ISOCurrencyCode.THB }, // Thailand
            { 17, ISOCurrencyCode.TTD }, // Trinidad & Tobago
            { 28, ISOCurrencyCode.AED }, // United Arab Emirates
            { 1, ISOCurrencyCode.USD }, // United States of America
            { 27, ISOCurrencyCode.USD }, // United States of America
            { 46, ISOCurrencyCode.USD }, // United States of America
            { 49, ISOCurrencyCode.UYU }, // Uruguay
            { 50, ISOCurrencyCode.UYU }, // Uruguay
            { 31, ISOCurrencyCode.VEF }, // Venezuela
            { 67, ISOCurrencyCode.VEF }, // Venezuela
            { 56, ISOCurrencyCode.VEF }, // Venezuela
            { 224, ISOCurrencyCode.EUR } // European Union

            // TODO: as more currencies are required, add to this table.
        };

        // Map the device's "escrow codes" to Note ID values
        private readonly Dictionary<byte, byte> _escrowNoteMap = new();
        private readonly Stack<Id003StatusError> _currentFaults = new();

        private readonly object _lock = new();

        private Id003StatusState _lastState;
        private bool _allowInsertions;
        private bool _handlingPowerUp;
        private bool _powerUpStacking;
        private bool _powerUpReturning;
        private bool _initialized;
        private bool _initialInspectionDone;
        private bool _deviceConfigured;
        private bool _disconnected;
        private bool? _pendingInhibitRequest;

        /// <summary>
        ///     Construct the <see cref="Id003Protocol" />.
        /// </summary>
        public Id003Protocol()
            : base(Id003DefaultTemplate)
        {
            PollIntervalMs = PollingFrequency;
            UseSyncMode = true;
        }

        /// <inheritdoc />
        public override bool Open()
        {
            EnablePolling(false);
            if (!base.Open())
            {
                return false;
            }

            CommunicationTimeoutMs = ResponseTimeInInitialization;
            MinimumResponseTime = ResponseTimeInInitialization;

            _initialized = false;
            _deviceConfigured = false;
            _initialized = AssureInitialized();
            Logger.Debug($"Initialized? {_initialized}");
            CommunicationTimeoutMs = ExpectedResponseTime;
            MinimumResponseTime = ExpectedResponseTime;
            if (_initialized)
            {
                if (_pendingInhibitRequest != null)
                {
                    AllowInsertion(_pendingInhibitRequest.Value);
                }
            }
            else
            {
                // There's no rediscovery so the polling must be enabled; the connection 
                // can be recovered by doing this.
                EnablePolling(true);
            }

            _initialInspectionDone = true;
            return _initialized;
        }

        /// <inheritdoc />
        protected override void SelfTest()
        {
            ClearFailures();
        }

        /// <inheritdoc />
        protected override bool GetDeviceInformation()
        {
            return GetVersionInfo() && GetBootInfo();
        }

        /// <inheritdoc />
        protected override void CalculateCrc()
        {
            // Done elsewhere.
        }

        /// <inheritdoc />
        protected override void Enable(bool enable)
        {
            lock (_lock)
            {
                AllowInsertion(enable);
            }
        }

        /// <inheritdoc />
        protected override void Accept()
        {
            lock (_lock)
            {
                if (_lastState == Id003StatusState.Escrow || _lastState == Id003StatusState.Holding)
                {
                    SendOperationCommand(Id003OperationCommand.Stack2);
                }
            }
        }

        /// <inheritdoc />
        protected override void Return()
        {
            lock (_lock)
            {
                if (_lastState == Id003StatusState.Rejecting)
                {
                    // It doesn't seem possible to get here after a Holding state is added.
                    // The test is added to make the code complete. 
                    return;
                }

                if (_lastState == Id003StatusState.Escrow || _lastState == Id003StatusState.Holding)
                {
                    SendOperationCommand(Id003OperationCommand.Return);
                }
            }
        }

        /// <inheritdoc />
        protected override bool RequestStatus()
        {
            lock (_lock)
            {
                if ((_lastState == Id003StatusState.Escrow || _lastState == Id003StatusState.Holding) &&
                    EscrowWatch.ElapsedMilliseconds >= DefaultMsInEscrow)
                {
                    // didn't hear from host in time, so return.
                    Logger.Debug("Sending a RETURN command due to escrow timeout.");
                    SendOperationCommand(Id003OperationCommand.Return);
                }

                // Expected response: some Id003StatusState or Id003StatusError enumeration, and in certain cases some supporting data
                var (status, data) = SendCommand((Id003Command)Id003GeneralCommand.StatusRequest);
                var result = true;

                // Some responses are states...
                if (Enum.IsDefined(typeof(Id003StatusState), (byte)status))
                {
                    var state = (Id003StatusState)status;
                    if (state != Id003StatusState.Initializing && !_deviceConfigured)
                    {
                        ConfigureDevice();
                    }

                    if (state != _lastState)
                    {
                        switch (state)
                        {
                            case Id003StatusState.PowerUpBillInAcceptor:
                                _powerUpReturning = true;
                                HandlePowerUp();
                                break;
                            case Id003StatusState.PowerUpBillInStacker:
                                _powerUpStacking = true;
                                HandlePowerUp();
                                break;
                            case Id003StatusState.Initializing:
                                // When entering the Initializing state, it's time to send our configuration commands
                                Logger.Debug("Status is Initializing, so configure device");
                                if (!_handlingPowerUp)
                                {
                                    ConfigureDevice();
                                }

                                break;
                            case Id003StatusState.EnabledIdle:
                            case Id003StatusState.DisabledInhibited:
                                ClearAllFaultsAndWarnings();
                                StackerStatus = new StackerStatus();
                                CheckPendingDocument();
                                break;
                            case Id003StatusState.Escrow:
                                HandleEscrowStatus(data);
                                break;
                            case Id003StatusState.Rejecting:
                                HandleRejectingStatus(data);
                                break;
                            case Id003StatusState.Returning:
                                HandleReturningStatus();
                                break;
                            case Id003StatusState.VendValid:
                                // a.k.a. "stacked"
                                HandleVendValidStatus(true);
                                break;
                            case Id003StatusState.PowerUp:
                                HandlePowerUp();
                                break;
                        }

                        Logger.Debug($"change state from {_lastState} to {state}");
                        _lastState = state;
                    }
                }
                // Other responses are errors...
                else if (Enum.IsDefined(typeof(Id003StatusError), (byte)status))
                {
                    var error = (Id003StatusError)status;
                    Logger.Debug($"received fault {error}");

                    // Set or clear stacker errors and note/ticket errors
                    if (error == Id003StatusError.StackerFull ||
                        error == Id003StatusError.StackerOpen ||
                        error == Id003StatusError.JamInStacker ||
                        error == Id003StatusError.JamInAcceptor ||
                        error == Id003StatusError.Cheated)
                    {
                        SetFault(error);
                    }

                    // Set or clear failures
                    if (error == Id003StatusError.CommunicationError ||
                        error == Id003StatusError.Pause ||
                        error == Id003StatusError.Failure)
                    {
                        OnMessageReceived(
                            new FailureStatus
                            {
                                DiagnosticCode =
                                    error == Id003StatusError.Failure || error == Id003StatusError.Pause,
                                ComponentError = error == Id003StatusError.CommunicationError
                            });
                    }
                    else
                    {
                        ClearFailures();
                    }
                }
                else
                {
                    Logger.Error($"Unknown response {status}");
                    result = false;
                }

                _disconnected = !result;
                return result;
            }
        }

        /// <inheritdoc />
        protected override void HoldInEscrow()
        {
            // determine if a HOLD command must be sent to keep the note/ticket in escrow.
            if (_lastState == Id003StatusState.Escrow || _lastState == Id003StatusState.Holding)
            {
                Logger.Debug("Sending a HOLD command to extend the escrow period.");
                EscrowWatch.Restart();
                SendOperationCommand(Id003OperationCommand.Hold);

                RequestStatus();
            }
        }

        /// <inheritdoc />
        protected override void GetCurrencyAssignment()
        {
            const int escrowBlockSize = 4;
            const int escrowOffset = 0;
            const int countryCodeOffset = 1;
            const int denomBaseOffset = 2;
            const int denomPowerOffset = 3;

            (var status, byte[] data) = (Id003Status.Unknown, null);
            var attempts = 0;
            try
            {
                for (; attempts < MaxRetrieveInfoAttempts; ++attempts)
                {
                    Logger.Debug($"Sending CurrencyAssignRequest in attempt {attempts}");
                    (status, data) = SendCommand(Id003Command.CurrencyAssignRequest);
                    if (data == null)
                    {
                        Logger.Error("The communication channel may not be ready for the currency assignment");
                        return;
                    }

                    if (status == Id003Status.CurrencyAssignRequest && data.Length % escrowBlockSize == 0)
                    {
                        break;
                    }
                }

                if (attempts > MaxRetrieveInfoAttempts)
                {
                    Logger.Fatal("Erroneous currency assignment response");
                    return;
                }

                // Expected response contains a list of escrow blocks
                _escrowNoteMap.Clear();
                NoteTable.Clear();

                var noteId = 1;
                // Each escrow sequence:
                // [escrow code][country type][denomBase][denomPower]
                // country type is defined in protocol spec
                // denom value = denomBase * 10^denomPower
                //
                // If escrow code isn't assigned, then country type and value will be 0
                for (var index = 0; index < data.Length; index += escrowBlockSize)
                {
                    var escrowCode = data[index + escrowOffset];
                    var countryCode = data[index + countryCodeOffset];
                    if (CountryToIsoCurrencyCodeTable.ContainsKey(countryCode) && data[index + denomBaseOffset] > 0)
                    {
                        var code = CountryToIsoCurrencyCodeTable[countryCode];

                        var note = new ReadNoteTable
                        {
                            NoteId = (byte)noteId++,
                            Value = data[index + denomBaseOffset],
                            Sign = true,
                            Scalar = data[index + denomPowerOffset],
                            Currency = Enum.GetName(typeof(ISOCurrencyCode), code),
                            Version = 1
                        };

                        NoteTable[note.NoteId] = note;

                        _escrowNoteMap.Add(escrowCode, (byte)note.NoteId);
                    }
                }
            }
            finally
            {
                EnablePolling(true);
                OnMessageReceived(new NumberOfNoteDataEntries { Number = (byte)NoteTable.Count });
            }
        }

        /// <summary>
        ///     Allow document insertion or not.
        /// </summary>
        /// <param name="allow">True to allow it.</param>
        protected void AllowInsertion(bool allow)
        {
            if (_handlingPowerUp || !_initialized)
            {
                // Cache the requests first. 
                _pendingInhibitRequest = allow;
            }

            _allowInsertions = allow;
            SendSetCommand(Id003SetCommand.InhibitAcceptor, new[] { allow ? Uninhibited : Inhibited });
        }

        private void HandleEscrowStatus(byte[] data)
        {
            // if we got disabled just as the device was inserting something (edge case), return it
            if (!_allowInsertions)
            {
                SendOperationCommand(Id003OperationCommand.Return);
                Logger.Debug("Return note/ticket while disabled");
                return;
            }

            // restart the stopwatch so that we know when to timeout the escrow.
            EscrowWatch.Restart();

            // The device is holding a note/ticket in escrow, so get the details
            // For note: 1 byte with escrow code
            const int escrowByteOffset = 0;
            if (data.Length == 1)
            {
                var escrowCode = data[escrowByteOffset];
                if (_escrowNoteMap.ContainsKey(escrowCode))
                {
                    OnMessageReceived(new NoteValidated { NoteId = _escrowNoteMap[escrowCode] });
                    return;
                }
            }
            // For ticket: 1 byte with special escrow code, followed by 6-18 ASCII bytes of barcode
            else if (data.Length >= BarcodeLengthMinimum + 1 && data.Length <= BarcodeLengthMaximum + 1)
            {
                var escrowCode = data[0];
                if (escrowCode == TicketEscrowCode)
                {
                    var barcode = Encoding.ASCII.GetString(data, 1, data.Length - 1);
                    OnMessageReceived(new TicketValidated { Code = barcode, Length = (byte)barcode.Length });
                    return;
                }
            }

            Logger.Debug("Missing or invalid escrow code");
        }

        private void HandleRejectingStatus(byte[] data)
        {
            if (data.Length == 1 && Enum.IsDefined(typeof(Id003RejectCode), data[0]))
            {
                var rejectCode = (Id003RejectCode)data[0];
                Logger.Warn($"Document rejected: {rejectCode}");
                switch (rejectCode)
                {
                    // Not sure which of the reject codes should cause which events!
                    case Id003RejectCode.InsertionError:
                        NoteOrTicketStatus = new NoteOrTicketStatus { Removed = true };
                        break;
                    default:
                        NoteOrTicketStatus = new NoteOrTicketStatus { Rejected = true };
                        break;
                }

                return;
            }

            Logger.Debug("Missing or invalid reject code");
        }

        private void HandleReturningStatus()
        {
            NoteOrTicketStatus = new NoteOrTicketStatus { Returned = true };

            Logger.Debug("HandleReturningStatus");
        }

        private void HandleVendValidStatus(bool needAck)
        {
            NoteOrTicketStatus = new NoteOrTicketStatus { Accepted = true };

            // VendValid is the only response that requires its own response from us
            if (needAck)
            {
                SendCommand(new[] { (byte)Id003Command.Ack });
                WaitSendComplete();
            }

            Logger.Debug("HandleVendValidStatus");
        }

        private void HandlePowerUp()
        {
            // don't handle it before the initialization or the device is disconnected.
            // in the case of disconnection, the heartbeat handler will initialize the
            // device again.
            if (_handlingPowerUp || !_initialized || _disconnected)
            {
                Logger.Debug("Can't handle power up right now.");
                return;
            }

            EnablePolling(false);
            Logger.Debug("Handling power up...");
            _handlingPowerUp = true;

            // according to the spec's flow chart, the version should only be
            // requested in a normal power up,
            if (!_powerUpStacking && !_powerUpReturning)
            {
                GetVersionInfo();
            }

            if (!(SendOperationCommand(Id003OperationCommand.Reset) &&
                  RequestStatus() && ConfigureDevice()))
            {
                Logger.Fatal("Failed to handle power up.");
            }

            _handlingPowerUp = false;
            if (_pendingInhibitRequest != null)
            {
                AllowInsertion(_pendingInhibitRequest.Value);
            }

            EnablePolling(true);
        }

        /// <summary>
        ///     Wrapper function to talk to the "data layer"
        /// </summary>
        /// <param name="command">One byte command, per protocol spec</param>
        /// <param name="data">Optional additional data for the command, per spec</param>
        /// <returns>Return a tuple of one-byte status and its optional extra data</returns>
        private (Id003Status status, byte[] data) SendCommand(Id003Command command, byte[] data = null)
        {
            // Make a buffer big enough for the command byte, followed by the optional data.
            var buffer = new byte[(data?.Length ?? 0) + 1];
            buffer[0] = (byte)command;
            if (data != null)
            {
                Buffer.BlockCopy(data, 0, buffer, 1, data.Length);
            }

            var response = SendCommandAndGetResponse(buffer, -1);

            // If things didn't work, return an empty response
            if (response == null)
            {
                return (Id003Status.Unknown, null);
            }

            // Buffer to hold the response byte, followed by the optional extra response data
            buffer = new byte[response.Length - 1];
            Buffer.BlockCopy(response, 1, buffer, 0, buffer.Length);
            return ((Id003Status)response[0], buffer);
        }

        private bool GetVersionInfo()
        {
            const int responseWordCount = 4;
            const int modelWordOffset = 0;
            const int nameVersionWordOffset = 1;
            const int crcVersionOffset = 3;
            const int versionPartCount = 2;
            const int versionNameWordOffset = 0;
            const int versionNumberWordOffset = 1;
            const int versionNumberPartCount = 2;
            const int versionVersionWordOffset = 0;
            const int versionRevisionWordOffset = 1;

            // Simple command
            var (status, data) = TryToGetInfo(Id003Command.VersionRequest, Id003Status.VersionRequest);

            if (status == Id003Status.VersionRequest)
            {
                // Expected format is formatted ASCII: "m(mmm)-mm-mm ID003-ppVvvv-rr ddmmyy cccc"
                // (however, the first hyphen (after the right parenthesis) is optional)
                //  m(mmm)[-]mm-mm  = Model
                //  ID003-pp        = Protocol name
                //  Vvvv            = Version
                //  rr              = Revision
                //  ddmmyy          = Date (unused)
                //  cccc            = CRC
                //
                // e.g. "i(USA)100-SS ID003-05V280-42 01AUG17 964D"
                //
                // Parse it into the fields we want
                var versionStr = Encoding.Default.GetString(data).Replace(")-", ")");
                var parts = versionStr.Split(' ');
                if (parts.Length == responseWordCount)
                {
                    var model = parts[modelWordOffset];
                    var crc = int.Parse(parts[crcVersionOffset], NumberStyles.AllowHexSpecifier);

                    parts = parts[nameVersionWordOffset].Split('V');
                    if (parts.Length == versionPartCount)
                    {
                        var protocol = parts[versionVersionWordOffset];
                        var name = parts[versionNameWordOffset];

                        parts = parts[versionNumberWordOffset].Split('-');
                        if (parts.Length == versionNumberPartCount)
                        {
                            //TODO: differentiate manufacturer by model
                            Manufacturer = "JCM";

                            Model = model;
                            Protocol = protocol;
                            Firmware = name;
                            FirmwareVersion = "V" + parts[versionVersionWordOffset];
                            FirmwareRevision = parts[versionRevisionWordOffset];
                            FirmwareCrc = crc;
                            return true;
                        }
                    }
                }
            }

            Logger.Error("Erroneous version response");
            return false;
        }

        private bool GetBootInfo()
        {
            const int bootVersionResponseLength = 4;

            var (status, data) = TryToGetInfo(Id003Command.BootVersionRequest, Id003Status.BootVersionRequest);
            if (status == Id003Status.BootVersionRequest)
            {
                // Expected format is 4 bytes of ASCII: "B05 "
                // Parse it into the fields we want
                var bootStr = Encoding.Default.GetString(data);
                if (bootStr.Length == bootVersionResponseLength)
                {
                    BootVersion = bootStr;
                    return true;
                }
            }

            Logger.Error("Erroneous boot response");
            return false;
        }

        private (Id003Status, byte[]) TryToGetInfo(Id003Command command, Id003Status expectedStatus)
        {
            (var status, byte[] data) = (Id003Status.Unknown, null);
            var attempts = 0;
            do
            {
                Logger.Info($"Trying to get info for {command} in {attempts + 1}");
                (status, data) = SendCommand(command);
            } while (status != expectedStatus && attempts++ <= MaxRetrieveInfoAttempts);

            return (status, data);
        }

        private bool SendOperationCommand(Id003OperationCommand command)
        {
            Logger.Debug($"send operational command {command}");
            var (status, _) = SendCommand((Id003Command)command);

            // Device should respond with Ack
            return status == Id003Status.Ack;
        }

        private bool SendSetCommand(Id003SetCommand command, byte[] data)
        {
            Logger.Debug($"send set command {command}");
            var (status, respData) = SendCommand((Id003Command)command, data);

            // Device should echo back the whole command/data.
            return (byte)status == (byte)command && CompareByteArrays(data, respData, data.Length) == 0;
        }

        private bool ConfigureDevice()
        {
            const byte allBits = 0xff;
            const byte noBits = 0;

            _deviceConfigured =
                SendSetCommand(
                    Id003SetCommand.EnableDisableDenomination,
                    new[] { noBits, noBits }) // Enable all denoms (0 bits == enable denom)
                && SendSetCommand(
                    Id003SetCommand.SecurityDenomination,
                    new[] { allBits, allBits }) // High security, all denoms (1 bits == enable high security)
                && SendSetCommand(
                    Id003SetCommand.CommunicationMode,
                    new[] { PolledCommMode }) // Communication mode = polled
                && SendSetCommand(
                    Id003SetCommand.BarcodeFunction,
                    new[] { Interleaved2Of5Code, (byte)BarcodeLengthMaximum }) // Accept barcode type and length
                && SendSetCommand(
                    Id003SetCommand.BarInhibit,
                    new[] { EnableNotesAndBarcodes }) // Enable bar-codes along with notes
                ;

            return _deviceConfigured;
        }

        private bool AssureInitialized()
        {
            // the first command to send is always Status in initialization.
            var result = false;
            if (RequestStatus())
            {
                result = SendOperationCommand(Id003OperationCommand.Reset) &&
                         RequestStatus() &&
                         (_deviceConfigured || ConfigureDevice() || !_deviceConfigured && !_initialInspectionDone &&
                             _currentFaults.Count > 0);
            }

            return result;
        }

        private void SetFault(Id003StatusError fault)
        {
            //When we are getting StackerFull/JamInStacker we can safely
            //assume that StackerOpen error would have been resolved
            lock (_lock)

            {
                if ((Id003StatusError.StackerFull == fault || Id003StatusError.JamInStacker == fault) &&
                    _currentFaults.Contains(Id003StatusError.StackerOpen))
                {
                    var tempStack = _currentFaults.ToArray().Where(x => !x.Equals(Id003StatusError.StackerOpen))
                        .Reverse();
                    _currentFaults.Clear();
                    foreach (var elem in tempStack)
                    {
                        _currentFaults.Push(elem);
                    }
                }
            }

            if (!_currentFaults.Contains(fault))
            {
                _currentFaults.Push(fault);
                ReportFaults();
            }
            else
            {
                while (_currentFaults.Count > 0 && _currentFaults.Peek() != fault)
                {
                    _currentFaults.Pop();
                    ReportFaults();
                }
            }
        }

        private void ReportFaults()
        {
            StackerStatus = new StackerStatus
            {
                Full = _currentFaults.Contains(Id003StatusError.StackerFull),
                Disconnect = _currentFaults.Contains(Id003StatusError.StackerOpen),
                Jam = _currentFaults.Contains(Id003StatusError.JamInStacker)
            };

            NoteOrTicketStatus = new NoteOrTicketStatus
            {
                Jam = _currentFaults.Contains(Id003StatusError.JamInAcceptor),
                Cheat = _currentFaults.Contains(Id003StatusError.Cheated)
            };
        }

        private void ClearAllFaultsAndWarnings()
        {
            _currentFaults.Clear();
            ClearFailures();
            StackerStatus = new StackerStatus();
            NoteOrTicketStatus = new NoteOrTicketStatus();
        }

        private void ClearFailures()
        {
            OnMessageReceived(new FailureStatus());
        }

        private void CheckPendingDocument()
        {
            if (_powerUpReturning)
            {
                Logger.Debug("Returning a note or ticket while powering up ");
                NoteOrTicketStatus = new NoteOrTicketStatus { Returned = true };
            }
            else if (_powerUpStacking)
            {
                Logger.Debug("Accepting a note or ticket while powering up ");
                NoteOrTicketStatus = new NoteOrTicketStatus { Accepted = true };
            }
            else
            {
                NoteOrTicketStatus = new NoteOrTicketStatus();
            }

            _powerUpReturning = false;
            _powerUpStacking = false;
        }
    }
}