namespace Aristocrat.Monaco.Hardware.Serial.NoteAcceptor.EBDS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Contracts.Gds.NoteAcceptor;
    using Contracts.NoteAcceptor;
    using Contracts.SharedDevice;
    using log4net;
    using Protocols;

    /// <summary>
    ///     Manage outgoing messages per the EBDS protocol.  Poll for status when no other
    ///     commands are pending.  Process responses.
    /// </summary>
    [SearchableSerialProtocol(DeviceType.NoteAcceptor)]
    public class EbdsProtocol : SerialNoteAcceptor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IList<EbdsDeviceState> PendingTransactionStates = new List<EbdsDeviceState>
        {
            EbdsDeviceState.EscrowedState,
            EbdsDeviceState.Stacking,
            EbdsDeviceState.Returning
        };

        private const int PollingIntervalMs = 200;
        private const int ExpectedResponseTime = 50;
        private const int MaxRetryCount = 2;

        private readonly object _lock = new object();

        private bool _selfTestInProgress;
        private bool _openReturnPending;
        private EbdsControl _currentAckStatus = EbdsControl.Nack;
        private EbdsDenomSupport _currentDenomSupport = EbdsDenomSupport.All;
        private OmnibusConfigurations _currentConfigurations = OmnibusConfigurations.Default | OmnibusConfigurations.BarcodeSupport;
        private EbdsDeviceState _lastState;
        private EbdsDeviceStatus? _lastStatus;
        private OmnibusExceptionalStatus _lastExceptionalStatus;

        public EbdsProtocol()
            : base(EbdsProtocolConstants.DefaultMessageTemplate)
        {
            UseSyncMode = true;
            PollIntervalMs = PollingIntervalMs;
            CommunicationTimeoutMs = ExpectedResponseTime;
            MinimumResponseTime = ExpectedResponseTime;
        }

        public override bool Open()
        {
            EnablePolling(false);

            try
            {
                // Clear the self test inprogress when we disconnect
                _selfTestInProgress = false;
                var open = base.Open() && RequestStatus();
                if (open)
                {
                    _openReturnPending = _lastState.HasFlag(EbdsDeviceState.EscrowedState);
                }

                return open;
            }
            finally
            {
                EnablePolling(true);
            }
        }

        protected override void OnDeviceDetached()
        {
            // Clear the self test inprogress when we disconnect
            _selfTestInProgress = false;
            base.OnDeviceDetached();
        }

        protected override void SelfTest()
        {
            lock (_lock)
            {
                if (PendingTransactionStates.Any(x => _lastState.HasFlag(x)))
                {
                    Logger.Debug(
                        "Self test is not supported for some firmware versions while the device is in a pending state.  Just say everything passed.");
                    OnMessageReceived(new FailureStatus());
                    return;
                }

                const int expectedLength = 7;
                const int ignoredBytes = 1;
                var result = TrySendCommand(
                    EbdsControl.AuxiliaryCommand,
                    (byte)SelfTestOperatorType.StartTest,
                    (byte)SelfTestLevel.MmiAndMotorsTest,
                    (byte)AuxiliaryCommands.DiagnosticsSelfTest);

                _selfTestInProgress = result?.Length == expectedLength && result.Skip(ignoredBytes).All(x => x == 0x00);
                if (!_selfTestInProgress && ExtendCommandIgnored(result))
                {
                    Logger.Debug("Self test is not supported for this device.  Just say everything passed");
                    OnMessageReceived(new FailureStatus());
                }
            }
        }

        protected override bool GetDeviceInformation()
        {
            var deviceInfoRead = GetApplicationInformation() &&
                                 GetVariantName() &&
                                 GetVariantVersion() &&
                                 GetModelInformation() &&
                                 GetBootInformation() &&
                                 GetSerialNumber();
            if (_openReturnPending)
            {
                Logger.Debug("Doing a delayed return");
                _openReturnPending = false;
                Return();
            }

            return deviceInfoRead;
        }

        protected override void CalculateCrc()
        {
            var result = TrySendCommand(
                EbdsControl.AuxiliaryCommand,
                0x00,
                0x00,
                (byte)AuxiliaryCommands.QuerySoftwareCrc);
            if (result?.Length != EbdsProtocolConstants.CrcResponseLength)
            {
                return;
            }

            FirmwareCrc = ConvertBcd(EbdsProtocolConstants.CrcDigitStartIndex, EbdsProtocolConstants.CrcDigitCount, result);
        }

        protected override void Enable(bool enable)
        {
            lock (_lock)
            {
                _currentDenomSupport = enable ? EbdsDenomSupport.All : EbdsDenomSupport.None;
                _currentConfigurations = enable
                    ? (OmnibusConfigurations.Default | OmnibusConfigurations.BarcodeSupport)
                    : OmnibusConfigurations.Default;

                if (!enable && _lastState.HasFlag(EbdsDeviceState.EscrowedState) && !_openReturnPending)
                {
                    Logger.Debug(
                        $"Last State found while trying to disable the note acceptor {_lastState} attempting to return the note.");
                    EscrowWatch.Stop();
                    EscrowWatch.Reset();
                    Return();
                }

                SendCommand(
                    EbdsControl.StandardOmnibusCommand,
                    (byte)_currentDenomSupport,
                    (byte)OmnibusOperations.Default,
                    (byte)_currentConfigurations);
            }
        }

        protected override bool RequestStatus()
        {
            lock (_lock)
            {
                CheckEscrowTimeout();

                if (_selfTestInProgress)
                {
                    var result = SendCommand(
                        EbdsControl.AuxiliaryCommand,
                        (byte)SelfTestOperatorType.QueryResults,
                        (byte)SelfTestLevel.MmiAndMotorsTest,
                        (byte)AuxiliaryCommands.DiagnosticsSelfTest);
                    return ParseSelfTestResults(result);
                }
                else
                {
                    var data = new[]
                    {
                        (byte)_currentDenomSupport, (byte)OmnibusOperations.Default, (byte)_currentConfigurations
                    };

                    var result = SendCommand(EbdsControl.StandardOmnibusCommand, data);
                    return ProcessStatusResponse(result);
                }
            }
        }

        protected override void GetCurrencyAssignment()
        {
            EnablePolling(false);

            try
            {
                const int expectedResults = 26;
                const int ignoredBytes = 8;

                byte index = 0;
                NoteTable.Clear();
                var result = TrySendCommand(
                    EbdsControl.ExtendedMessageSet,
                    (byte)ExtendedCommands.NoteSpecificationMessage,
                    (byte)_currentDenomSupport,
                    (byte)OmnibusOperations.Default,
                    (byte)_currentConfigurations,
                    ++index);
                while (result?.Length == expectedResults && result.Skip(ignoredBytes).Any(x => x != 0x00))
                {
                    var noteTable = ParseNoteTable(result, ignoredBytes);
                    if (noteTable != null)
                    {
                        NoteTable[noteTable.NoteId] = noteTable;
                    }

                    result = TrySendCommand(
                        EbdsControl.ExtendedMessageSet,
                        (byte)ExtendedCommands.NoteSpecificationMessage,
                        (byte)_currentDenomSupport,
                        (byte)OmnibusOperations.Default,
                        (byte)_currentConfigurations,
                        ++index);
                }

                OnMessageReceived(new NumberOfNoteDataEntries { Number = (byte)NoteTable.Count });
            }
            finally
            {
                EnablePolling(true);
            }
        }

        protected override void Accept()
        {
            EnablePolling(false);

            try
            {
                var result = TrySendCommand(
                    EbdsControl.StandardOmnibusCommand,
                    (byte)_currentDenomSupport,
                    (byte)(OmnibusOperations.Default | OmnibusOperations.DocumentStackCommand),
                    (byte)_currentConfigurations);
                if (result != null)
                {
                    RequestStatus();
                }
            }
            finally
            {
                EnablePolling(true);
            }
        }

        protected override void Return()
        {
            EnablePolling(false);

            try
            {
                var result = SendCommand(
                    EbdsControl.StandardOmnibusCommand,
                    (byte)_currentDenomSupport,
                    (byte)(OmnibusOperations.Default | OmnibusOperations.DocumentReturnCommand),
                    (byte)_currentConfigurations);
                if (result != null)
                {
                    RequestStatus();
                }
            }
            finally
            {
                EnablePolling(true);
            }
        }

        protected override void HoldInEscrow()
        {
            ConfigureEscrowTimeout(); // Re-send the escrow timeout
        }

        private void CheckEscrowTimeout()
        {
            if (_lastState != EbdsDeviceState.EscrowedState || EscrowWatch.ElapsedMilliseconds < DefaultMsInEscrow)
            {
                return;
            }

            // didn't hear from host in time, so return.
            Logger.Debug("Sending a RETURN command due to escrow timeout.");
            SendCommand(
                EbdsControl.StandardOmnibusCommand,
                (byte)_currentDenomSupport,
                (byte)(OmnibusOperations.Default | OmnibusOperations.DocumentReturnCommand),
                (byte)_currentConfigurations);
        }

        private bool ExtendCommandIgnored(IReadOnlyList<byte> result)
        {
            /*
             * Did we get a CRC message?
             * When the host does not support an extended command it sends the CRC message as its response.
             * It should always match when this happens.
             */
            return result?.Count == EbdsProtocolConstants.CrcResponseLength &&
                   FirmwareCrc == ConvertBcd(EbdsProtocolConstants.CrcDigitStartIndex, EbdsProtocolConstants.CrcDigitCount, result);
        }

        private static int ConvertBcd(int startIndex, int count, IReadOnlyList<byte> data)
        {
            const int mask = 0x0F;
            const int digitShit = 4;

            var result = 0;
            var bitShift = 0;
            for (var i = count + startIndex - 1; i >= startIndex; --i)
            {
                result += (data[i] & mask) << bitShift;
                bitShift += digitShit;
            }

            return result;
        }

        private bool ParseSelfTestResults(IReadOnlyList<byte> response)
        {
            const int expectedLength = 7;
            const int transportMotorIndex = 1;
            const int stackerMotorIndex = 2;
            if (response?.Count != expectedLength)
            {
                return false;
            }

            var transportResult = (SelfTestTransportMotorResults)response[transportMotorIndex];
            var stackerResult = (SelfTestStackerMotorResults)response[stackerMotorIndex];
            _selfTestInProgress = transportResult == SelfTestTransportMotorResults.ResultsNotReady ||
                              stackerResult == SelfTestStackerMotorResults.ResultsNotReady;
            if (!_selfTestInProgress)
            {
                OnMessageReceived(
                    new FailureStatus
                    {
                        MechanicalError = transportResult != SelfTestTransportMotorResults.Pass ||
                                          stackerResult != SelfTestStackerMotorResults.Pass
                    });
            }

            return true;
        }

        private bool ProcessStatusResponse(byte[] result)
        {
            if (result is null || result.Length < EbdsProtocolConstants.StandardOmnibusResponseLength)
            {
                return false;
            }

            var responseControl = (EbdsControl)result[EbdsProtocolConstants.EbdsControlIndex];
            switch (responseControl & EbdsControl.MessageType)
            {
                case EbdsControl.ExtendedMessageSet:
                    return HandleExtendMessageReply(result);
                case EbdsControl.StandardOmnibusReply:
                    return HandleDeviceStatus(result);
                default:
                    return false;
            }
        }

        private void ConfigureEscrowTimeout()
        {
            // determine if a HOLD command must be sent to keep the note/ticket in escrow.
            if (_lastState == EbdsDeviceState.EscrowedState)
            {
                EscrowWatch.Restart();
            }
        }

        private bool HandleExtendMessageReply(byte[] result)
        {
            const int subTypeIndex = 1;
            const int statusHandlingOffset = 1;
            var subType = (ExtendedCommands)result[subTypeIndex];

            switch (subType)
            {
                case ExtendedCommands.BarcodeReply:
                    HandleVoucherInReply(result);
                    break;
                case ExtendedCommands.NoteSpecificationMessage:
                    HandleNoteInReply(result);
                    break;
            }

            return HandleDeviceStatus(result, statusHandlingOffset);
        }

        private void HandleNoteInReply(byte[] result)
        {
            if (PendingTransactionStates.Any(x => _lastState.HasFlag(x)))
            {
                return;
            }

            const int ignoredBytes = 8;
            var noteTable = ParseNoteTable(result, ignoredBytes);

            // The extended command always has the index set to 0 so we have to look up which Note we are after
            var note = NoteTable.Select(x => x.Value).FirstOrDefault(
                x => x.Version == noteTable?.Version && x.Currency == noteTable.Currency &&
                     x.Sign == noteTable.Sign && x.Value == noteTable.Value &&
                     x.Scalar == noteTable.Scalar);
            if (note != null)
            {
                EscrowWatch.Restart();
                OnMessageReceived(new NoteValidated { NoteId = note.NoteId });
            }
            else
            {
                Logger.Warn("Unknown note received.  Returning the note");
                SendCommand(
                    EbdsControl.StandardOmnibusCommand,
                    (byte)_currentDenomSupport,
                    (byte)(OmnibusOperations.Default | OmnibusOperations.DocumentReturnCommand),
                    (byte)_currentConfigurations);
            }
        }

        private void HandleVoucherInReply(IEnumerable<byte> result)
        {
            if (_lastState.HasFlag(EbdsDeviceState.EscrowedState))
            {
                return;
            }

            const int ignoreBytes = 8;
            const byte barCodeEnd = 0x28; // The end of barcode is the first ASCII '('

            EscrowWatch.Restart();
            var barcode = Encoding.ASCII.GetString(result.Skip(ignoreBytes).TakeWhile(x => x != barCodeEnd).ToArray());
            OnMessageReceived(new TicketValidated { Code = barcode, Length = (byte)barcode.Length });
        }

        private bool HandleDeviceStatus(IReadOnlyList<byte> result, int startIndex = 0)
        {
            const int statusIndex = 2;
            const int stateIndex = 1;
            const int exceptionStatusIndex = 3;

            var deviceStatus = (EbdsDeviceStatus)result[statusIndex + startIndex];
            var exceptionStatus = (OmnibusExceptionalStatus)result[exceptionStatusIndex + startIndex];
            if ((deviceStatus & EbdsDeviceStatus.Paused) != (_lastStatus & EbdsDeviceStatus.Paused) ||
                (exceptionStatus & OmnibusExceptionalStatus.Failure) != (_lastExceptionalStatus & OmnibusExceptionalStatus.Failure))
            {
                OnMessageReceived(
                    new FailureStatus
                    {
                        ComponentError = exceptionStatus.HasFlag(OmnibusExceptionalStatus.Failure),
                        DiagnosticCode = deviceStatus.HasFlag(EbdsDeviceStatus.Paused)
                    });
            }

            _lastExceptionalStatus = exceptionStatus;
            UpdateDeviceState((EbdsDeviceState)result[stateIndex + startIndex]);
            UpdateDeviceStatus((EbdsDeviceStatus)result[statusIndex + startIndex]);

            return true;
        }

        private void UpdateDeviceState(EbdsDeviceState deviceState)
        {
            if (deviceState == _lastState)
            {
                return;
            }

            if (deviceState.HasFlag(EbdsDeviceState.StackedEvent))
            {
                OnMessageReceived(new NoteOrTicketStatus { Accepted = true });
            }

            if (deviceState.HasFlag(EbdsDeviceState.ReturnedEvent))
            {
                OnMessageReceived(new NoteOrTicketStatus { Returned = true });
            }

            _lastState = deviceState;
        }

        private void UpdateDeviceStatus(EbdsDeviceStatus deviceStatus)
        {
            if (deviceStatus == _lastStatus)
            {
                return;
            }

            StackerStatus = new StackerStatus
            {
                Full = deviceStatus.HasFlag(EbdsDeviceStatus.StackerFull),
                Disconnect = !deviceStatus.HasFlag(EbdsDeviceStatus.CassetteAttached)
            };

            NoteOrTicketStatus = new NoteOrTicketStatus
            {
                Jam = deviceStatus.HasFlag(EbdsDeviceStatus.Jammed),
                Cheat = deviceStatus.HasFlag(EbdsDeviceStatus.Cheated),
                Rejected = deviceStatus.HasFlag(EbdsDeviceStatus.Rejected)
            };

            _lastStatus = deviceStatus;
        }

        private static ReadNoteTable ParseNoteTable(byte[] noteData, int startIndex)
        {
            const int noteIdIndex = 0;
            const int isoCodeLength = 3;
            const int isoCodeIndex = 1;
            const int baseValueIndex = 4;
            const int baseValueLength = 3;
            const int signIndex = 7;
            const int exponentIndex = 8;
            const int exponentLength = 2;
            const int versionIndex = 14;
            const byte asciiPlus = 0x2B;
            const byte asciiMinus = 0x2D;

            var valid = Enum.TryParse(
                Encoding.ASCII.GetString(noteData, isoCodeIndex + startIndex, isoCodeLength),
                true,
                out ISOCurrencyCode isoCode);
            valid &= ushort.TryParse(
                Encoding.ASCII.GetString(noteData, baseValueIndex + startIndex, baseValueLength),
                out var baseValue);
            var isPositive = noteData[signIndex + startIndex] == asciiPlus;
            valid &= isPositive || noteData[signIndex + startIndex] == asciiMinus;
            valid &= byte.TryParse(
                Encoding.ASCII.GetString(noteData, exponentIndex + startIndex, exponentLength),
                out var exponent);
            if (!valid)
            {
                return null;
            }

            return new ReadNoteTable
            {
                NoteId = noteData[noteIdIndex + startIndex],
                Currency = Enum.GetName(typeof(ISOCurrencyCode), isoCode),
                Value = baseValue,
                Scalar = exponent,
                Sign = isPositive,
                Version = noteData[versionIndex + startIndex]
            };
        }

        private bool GetModelInformation()
        {
            const int minResponseLength = 3;
            const int modelIndex = 1;

            var result = TrySendCommand(
                EbdsControl.AuxiliaryCommand,
                0x00,
                0x00,
                (byte)AuxiliaryCommands.QueryAcceptorType);
            if (!(result?.Length > minResponseLength))
            {
                return false;
            }

            var model = Encoding.ASCII.GetString(result, modelIndex, result.Skip(modelIndex).TakeWhile(x => x != 0).Count());
            Model = model;
            Manufacturer = "MEI";
            Protocol = "EBDS";

            Logger.Debug($"Received model number {model}");
            return true;
        }

        private bool GetVariantName()
        {
            const int minResponseLength = 3;
            const int variantIndex = 1;

            var result = TrySendCommand(
                EbdsControl.AuxiliaryCommand,
                0x00,
                0x00,
                (byte)AuxiliaryCommands.QueryVariantName);
            if (!(result?.Length > minResponseLength))
            {
                return false;
            }

            var variantName = Encoding.ASCII.GetString(result, variantIndex, result.Skip(variantIndex).TakeWhile(x => x != 0).Count());
            VariantName = variantName;

            Logger.Debug($"Received variant name {variantName}");
            return true;
        }

        private bool GetVariantVersion()
        {
            var result = TrySendCommand(
                EbdsControl.AuxiliaryCommand,
                0x00,
                0x00,
                (byte)AuxiliaryCommands.QueryVariantPartNumber);
            var (majorVersion, minorVersion) = ParseVersionRequest(result);
            if (minorVersion == -1 || majorVersion == -1)
            {
                return false;
            }

            VariantVersion = $"V{majorVersion}.{minorVersion:D2}";
            Logger.Debug($"Received the variant version major={majorVersion}, minor={minorVersion}");
            return true;
        }

        private bool GetApplicationInformation()
        {
            var result = TrySendCommand(
                EbdsControl.AuxiliaryCommand,
                0x00,
                0x00,
                (byte)AuxiliaryCommands.QueryApplicationPartNumber);
            var (majorVersion, minorVersion) = ParseVersionRequest(result);
            if (minorVersion == -1 || majorVersion == -1)
            {
                return false;
            }

            FirmwareVersion = $"V{majorVersion}";
            FirmwareRevision = $"{minorVersion}";
            Logger.Debug($"Received the application version major={majorVersion}, minor={minorVersion}");
            return true;
        }

        private static (int majorVersion, int minorVersion) ParseVersionRequest(byte[] versionData)
        {
            const int majorVersionIndex = 7;
            const int majorVersionLength = 1;
            const int minorVersionIndex = 8;
            const int minorVersionLength = 2;
            const int versionResponseLength = 10;

            var majorVersion = 0;
            var minorVersion = 0;
            var validVersion = versionData?.Length == versionResponseLength &&
                               int.TryParse(Encoding.ASCII.GetString(versionData, majorVersionIndex, majorVersionLength), out majorVersion) &&
                               int.TryParse(Encoding.ASCII.GetString(versionData, minorVersionIndex, minorVersionLength), out minorVersion);

            return validVersion ? (majorVersion, minorVersion) : (-1, -1);
        }

        private bool GetBootInformation()
        {
            var result = TrySendCommand(
                EbdsControl.AuxiliaryCommand,
                0x00,
                0x00,
                (byte)AuxiliaryCommands.QueryApplicationPartNumber);
            var (majorVersion, minorVersion) = ParseVersionRequest(result);
            if (minorVersion == -1 || majorVersion == -1)
            {
                return false;
            }

            BootVersion = $"V{majorVersion}.{minorVersion:D2}";
            Logger.Debug($"Received the boot version major={majorVersion}, minor={minorVersion}");
            return true;
        }

        private bool GetSerialNumber()
        {
            const int minResponseLength = 3;
            const int serialNumberIndex = 1;

            var result = TrySendCommand(
                EbdsControl.AuxiliaryCommand,
                0x00,
                0x00,
                (byte)AuxiliaryCommands.QuerySerialNumber);
            if (!(result?.Length > minResponseLength))
            {
                return false;
            }

            var serialNumber = Encoding.ASCII.GetString(result, serialNumberIndex, result.Skip(serialNumberIndex).TakeWhile(x => x != 0).Count());
            SerialNumber = serialNumber;
            Logger.Debug($"Received the serial number {serialNumber}");
            return true;
        }

        private byte[] TrySendCommand(EbdsControl control, params byte[] data)
        {
            lock (_lock)
            {
                var sendCount = 0;
                byte[] response = null;
                while (response is null && sendCount++ < MaxRetryCount)
                {
                    response = SendCommand(control, data);
                }

                return response;
            }
        }

        private byte[] SendCommand(EbdsControl control, params byte[] data)
        {
            lock (_lock)
            {
                var command = new List<byte> { (byte)(control | _currentAckStatus) };

                command.AddRange(data);
                var commandData = command.ToArray();
                var response = SendCommandAndGetResponse(commandData, -1);
                if (!(response?.Length > EbdsProtocolConstants.DefaultMessageTemplate.GeneralDataIndex) ||
                    (response[EbdsProtocolConstants.EbdsControlIndex] & (byte)EbdsControl.Ack) != (byte)_currentAckStatus)
                {
                    Logger.Debug("The command was NACKed or Ignored by the BNA");
                    return null;
                }

                _currentAckStatus = (_currentAckStatus & EbdsControl.Ack) != 0 ? EbdsControl.Nack : EbdsControl.Ack;
                return response;
            }
        }
    }
}
