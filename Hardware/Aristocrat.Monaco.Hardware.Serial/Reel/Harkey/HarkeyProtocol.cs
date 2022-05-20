namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using Contracts.Gds;
    using Contracts.Gds.Reel;
    using Contracts.Reel;
    using log4net;
    using Messages;
    using Protocols;
    using HomeReel = Messages.HomeReel;
    using InvokerQueue =
        System.Collections.Concurrent.ConcurrentQueue<System.Action<Messages.HarkeySerializableMessage>>;
    using Nudge = Messages.Nudge;

    public class HarkeyProtocol : SerialReelController
    {
        private const int PollingIntervalMs = 1000;
        private const int ExpectedResponseTime = 50;
        private const int LightsPerReel = 3;
        private const int MaxLightId = HarkeyConstants.MaxReelId * LightsPerReel;
        private const int NumberOfMotorSteps = 200;
        private const int NumberOfStops = 22;
        private const int AllowableStatusResponseTime = PollingIntervalMs + ExpectedResponseTime * 2;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<Type, InvokerQueue> _router =
            new ConcurrentDictionary<Type, InvokerQueue>();

        private readonly object _sequenceLock = new object();
        private readonly object _countLock = new object();
        private readonly object _reelStepsLock = new object();
        private readonly object _homeStepsLock = new object();
        private readonly object _initializeLock = new object();

        private readonly ConcurrentDictionary<int, (byte sequenceId, bool responseReceived)> _homeReelMessageDictionary =
            new ConcurrentDictionary<int, (byte, bool)>();

        private (byte sequenceId, byte commandedReelBits) _lastSpinCommandReels;

        private readonly Stopwatch _statusResponse = new Stopwatch();
        private readonly ReelFaults _faults = ReelFaults.None;
        private readonly List<ReelLampData> _currentLightState = new List<ReelLampData>();
        private readonly List<bool> _reelsConnected = new List<bool>();
        private readonly byte[] _homeSteps = new byte[HarkeyConstants.MaxReelId];
        private readonly ReelColors[] _lastReelColors = new ReelColors[HarkeyConstants.MaxReelId];
        private byte _sequenceId;
        private bool _disposed;
        private byte[] _reelSteps = new byte[HarkeyConstants.MaxReelId];
        private int[] _offsets = Array.Empty<int>();
        private bool _setOffsetsPending;
        private bool _isInitialized;

        public HarkeyProtocol()
            : base(1, HarkeyConstants.MaxReelId, HarkeyConstants.MessageTemplate)
        {
            UseSyncMode = false;
            PollIntervalMs = PollingIntervalMs;
            CommunicationTimeoutMs = ExpectedResponseTime;
            MinimumResponseTime = ExpectedResponseTime;
            RegisterCallback<ReelStatusResponse>(HandleStatus);
            RegisterCallback<HomeReelResponse>(HandleHomeReelResponse);
            RegisterCallback<Rm6VersionResponse>(HandleVersionResponse);
            RegisterCallback<SpinReelsToGoalResponse>(HandleSpinResponse);
            RegisterCallback<NudgeResponse>(HandleNudgeResponse);
            RegisterCallback<AbortAndSlowSpinResponse>(HandleAbortAndSlowSpinResponse);
            RegisterCallback<SetFaultsResponse>(HandleSetFaultsResponse);
            RegisterCallback<ProtocolErrorResponse>(HandleProtocolErrorResponse);
            RegisterCallback<HardwareErrorResponse>(HandleHardwareErrorResponse);
            RegisterCallback<SetReelLightBrightnessResponse>(HandleSimpleAckResponse);
            RegisterCallback<ChangeSpeedResponse>(HandleSimpleAckResponse);
            RegisterCallback<SetReelLightColorResponse>(HandleSimpleAckResponse);
            RegisterCallback<UnsolicitedErrorTamperResponse>(HandleUnsolicitedErrorTamperResponse);
            RegisterCallback<UnsolicitedErrorStallResponse>(HandleUnsolicitedErrorStallResponse);

            for (var lightId = 0; lightId < MaxLightId; ++lightId)
            {
                _currentLightState.Add(new ReelLampData(Color.Black, false, lightId + 1));
            }

            lock (_countLock)
            {
                for (var reelId = 0; reelId < HarkeyConstants.MaxReelId; ++reelId)
                {
                    _reelsConnected.Add(false);
                    _lastReelColors[reelId] = new ReelColors();
                }
            }

            MessageBuilt += OnMessageBuilt;
        }

        private int ReelConnectedCount
        {
            get
            {
                lock (_countLock)
                {
                    return _reelsConnected.Count(connected => connected);
                }
            }
        }

        protected override bool HasDisablingFault => _faults != ReelFaults.None;

        public override bool Open()
        {
            EnablePolling(false);

            try
            {
                HarkeySerializableMessage.Initialize();

                base.Open();
                ResetLights();

                return RequestStatus();
            }
            finally
            {
                lock (_sequenceLock)
                {
                    _statusResponse.Restart();
                }

                EnablePolling(true);
            }
        }

        protected override void SelfTest()
        {
            _homeReelMessageDictionary.Clear();

            // Clear any hardware faults that are active
            OnMessageReceived(new FailureStatusClear { HardwareError = true });
            OnMessageReceived(new FailureStatusClear { CommunicationError = true });

            lock (_countLock)
            {
                for (var reelIndex = 1; reelIndex <= HarkeyConstants.MaxReelId; ++reelIndex)
                {
                    if (!_reelsConnected[reelIndex - 1])
                    {
                        continue;
                    }

                    // Clear unsolicited tamper detected fault
                    OnMessageReceived(new FailureStatusClear { ReelId = reelIndex, TamperDetected = true });

                    // Clear unsolicited stall detected fault
                    OnMessageReceived(new FailureStatusClear { ReelId = reelIndex, StallDetected = true });
                }
            }

            // It doesn't support this
            OnMessageReceived(new FailureStatus());
        }

        protected override bool GetDeviceInformation()
        {
            foreach (var reelId in Enumerable.Range(1, HarkeyConstants.MaxReelId))
            {
                // Determine what reels are connected in the background homing reels can take up to 10 seconds
                HomeReel(reelId);
            }

            HandleModel();
            SendCommand(new GetRm6Version());
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _router.Clear();
                MessageBuilt -= OnMessageBuilt;
            }

            _disposed = true;
            base.Dispose(disposing);
        }

        protected override void CalculateCrc()
        {
            // This device does not support CRC
            FirmwareCrc = UnknownCrc;
        }

        protected override void Enable(bool enable)
        {
        }

        protected override bool RequestStatus()
        {
            SendCommand(new GetReelStatus());

            lock (_sequenceLock)
            {
                return _statusResponse.ElapsedMilliseconds < AllowableStatusResponseTime;
            }
        }

        protected override void HomeReel(int reelId, int position)
        {
            if (reelId <= 0 || reelId > HarkeyConstants.MaxReelId ||
                position < -1 || position > NumberOfMotorSteps || !IsAttached)
            {
                return;
            }

            if (position != -1)
            {
                // Set the fault position for the reel
                lock (_homeStepsLock)
                {
                    _homeSteps[reelId - 1] = (byte)position;
                    SetFaults(_homeSteps);
                }
            }

            HomeReel(reelId);
        }

        protected override void SetLights(params ReelLampData[] lampData)
        {
            var colorChanged = false;
            var stateChanged = false;
            foreach (var data in lampData)
            {
                if (data.Id < 1 || data.Id > MaxLightId)
                {
                    continue;
                }

                var newColor = _currentLightState[data.Id - 1].Color;
                var newState = _currentLightState[data.Id - 1].IsLampOn;
                if (data.IsLampOn != _currentLightState[data.Id - 1].IsLampOn)
                {
                    stateChanged = true;
                    newState = data.IsLampOn;
                }

                // Only set the color if the lamp is being turned on, otherwise just ignore the color
                if (data.IsLampOn && data.Color != _currentLightState[data.Id - 1].Color)
                {
                    colorChanged = true;
                    newColor = data.Color;
                }

                if (stateChanged || colorChanged)
                {
                    _currentLightState[data.Id - 1] = new ReelLampData(newColor, newState, data.Id);
                }
            }

            if (colorChanged)
            {
                SetLightsColors(_currentLightState);
            }

            if (stateChanged)
            {
                SetLightsStates(_currentLightState);
            }
        }

        protected override void SetReelLightBrightness(int reelId, int brightness)
        {
            var reelConnectedCount = ReelConnectedCount;
            if (reelId < 0 || reelId > reelConnectedCount || brightness < 1 || brightness > 100)
            {
                return;
            }

            // ReelId of 0 indicates that all reels should be set
            if (reelId == 0)
            {
                for (var reel = 1; reel <= reelConnectedCount; ++reel)
                {
                    SendCommand(new SetReelLightBrightness { ReelId = reel, Brightness = brightness });
                }
            }
            else
            {
                SendCommand(new SetReelLightBrightness { ReelId = reelId, Brightness = brightness });
            }
        }

        protected override void SetReelSpeed(params ReelSpeedData[] speedData)
        {
            // The Harkey protocol does not set individual reel speeds so using the speed in the first parameter array item

            if (speedData[0].Speed < 0 || speedData[0].Speed > 200)
            {
                return;
            }

            SendCommand(new ChangeSpeed { Speed = speedData[0].Speed });
        }

        protected override void SetReelOffsets(params int[] offsets)
        {
            _setOffsetsPending = true;
            _offsets = offsets;
        }

        protected override void SpinReel(params ReelSpinData[] spinData)
        {
            if (!IsAttached)
            {
                return;
            }

            var directions = GetReelDirections(spinData);
            byte[] reelSteps;
            lock (_reelStepsLock)
            {
                _reelSteps = reelSteps = GetReelSteps(spinData, true);
            }

            var reels = GetSelectedReels(spinData);

            byte rampTable = HarkeyConstants.DefaultRampTable;
            if (spinData[0].Rpm >= byte.MinValue && spinData[0].Rpm <= byte.MaxValue)
            {
                rampTable = (byte)spinData[0].Rpm;
            }

            // No spin freely implemented.  The current Harkey firmware does not support it correctly.
            // Once the reels are spinning freely, there is no way to stop them.
            SpinReelsToGoal(directions, reelSteps, reels, rampTable);
        }

        protected override void NudgeReel(params NudgeReelData[] nudgeData)
        {
            if (!IsAttached)
            {
                return;
            }

            var directions = GetReelDirections(nudgeData);

            byte[] reelSteps;
            lock (_reelStepsLock)
            {
                _reelSteps = reelSteps = GetReelSteps(nudgeData, false);
            }

            var reels = GetSelectedReels(nudgeData);

            byte rampTable = HarkeyConstants.DefaultNudgeDelay;
            if (nudgeData[0].Delay >= byte.MinValue && nudgeData[0].Delay <= byte.MaxValue)
            {
                rampTable = (byte)nudgeData[0].Delay;
            }

            Nudge(directions, reelSteps, reels, rampTable);
        }

        protected override void TiltReels()
        {
            SendCommand(new AbortAndSlowSpin { SelectedReels = GetAllReelsSelectedBits() });
        }

        protected override void OnDeviceDetached()
        {
            OnMessageReceived(new FailureStatusClear { HardwareError = true });
            OnMessageReceived(new FailureStatusClear { CommunicationError = true });
            lock (_initializeLock)
            {
                _homeReelMessageDictionary.Clear();
                for (var i = 0; i < HarkeyConstants.MaxReelId; ++i)
                {
                    _reelsConnected[i] = false;
                }

                _isInitialized = false;
            }

            var status = new ReelStatusResponse
            {
                Reel1Status = ReelStatus.None,
                Reel2Status = ReelStatus.None,
                Reel3Status = ReelStatus.None,
                Reel4Status = ReelStatus.None,
                Reel5Status = ReelStatus.None,
                Reel6Status = ReelStatus.None
            };

            HandleStatus(status);
            base.OnDeviceDetached();
        }

        /// <inheritdoc />
        protected override void GetReelLightIdentifiers()
        {
            int reelConnectedCount;
            lock (_countLock)
            {
                reelConnectedCount = _reelsConnected.Count(connected => connected);
            }

            var response = new ReelLightIdentifiersResponse { StartId = 1, EndId = reelConnectedCount * LightsPerReel };
            SetReelLightsIdentifiers(response);
        }

        private static string GetVersionString(IVersionProvider version)
        {
            return $"{version.MajorVersion}.{version.MinorVersion}.{version.MiniVersion}";
        }

        private static T BuildMessage<T>(byte[] message) where T : HarkeySerializableMessage
        {
            try
            {
                var serializableMessage = HarkeySerializableMessage.Deserialize(message) as T;
                return serializableMessage;
            }
            catch (Exception exception)
            {

                Logger.Error("Failed to parse message", exception);
            }

            return null;
        }

        private static byte GetReelDirections(IEnumerable<ISpinData> spinData)
        {
            return spinData.Where(data => data.ReelId >= 1 && data.ReelId <= HarkeyConstants.MaxReelId && data.Direction != SpinDirection.Forward)
                .Aggregate<ISpinData, byte>(0, (current, data) => (byte)(current | (byte)(1 << data.ReelId - 1)));
        }

        private static byte GetSelectedReels(IEnumerable<ISpinData> spinData)
        {
            return spinData.Where(data => data.ReelId >= 1 && data.ReelId <= HarkeyConstants.MaxReelId)
                .Aggregate<ISpinData, byte>(0, (current, data) => (byte)(current | (byte)(1 << data.ReelId - 1)));
        }

        private static bool IsRequestError(int responseCode)
        {
            return (responseCode & (int)HarkeyRequestErrorCodes.ErrorMask) == (int)HarkeyRequestErrorCodes.ErrorMask;
        }

        private static bool IsResponseError(int responseCode)
        {
            return responseCode == (int)HarkeyResponseErrorCodes.NotAvailable ||
                   responseCode == (int)HarkeyResponseErrorCodes.NoSync ||
                   responseCode == (int)HarkeyResponseErrorCodes.Skew ||
                   responseCode == (int)HarkeyResponseErrorCodes.Stall;
        }

        private static void HandleSimpleAckResponse(ISimpleAckResponse response)
        {
            // There is no need to check the ACK/NACK/Error codes for this command. Handling it to remove any logging warnings.
            Logger.Debug($"{response.CommandId} ACKed");
        }

        private static IMessageTemplate GetNoProtocolMessageTemplate(int messageLength)
        {
            const int commandLength = 1;
            return new MessageTemplate<XorChecksumEngine>
            (
                new List<MessageTemplateElement>
                {
                    new MessageTemplateElement
                    {
                        ElementType = MessageTemplateElementType.ConstantMask,
                        BigEndian = false,
                        Length = commandLength,
                        IncludedInCrc = true
                    },
                    new MessageTemplateElement
                    {
                        ElementType = MessageTemplateElementType.VariableData,
                        BigEndian = false,
                        Length = messageLength - commandLength,
                        IncludedInCrc = true
                    },
                    new MessageTemplateElement
                    {
                        ElementType = MessageTemplateElementType.Crc,
                        BigEndian = false,
                        Length = 1,
                        IncludedInCrc = false
                    }
                },
                0
            );
        }

        private byte[] GetReelSteps(IEnumerable<ISpinData> spinData, bool applyOffsets)
        {
            var steps = new byte[HarkeyConstants.MaxReelId];

            foreach (var data in spinData)
            {
                if (data.ReelId < 1 || data.ReelId > HarkeyConstants.MaxReelId ||
                    data.Step < byte.MinValue || data.Step > byte.MaxValue)
                {
                    continue;
                }

                steps[data.ReelId - 1] = (byte)data.Step;
                steps[data.ReelId - 1] = (byte)(applyOffsets ? GetOffsetPosition(data.ReelId, data.Step) : data.Step);
            }

            return steps;
        }

        private int GetOffsetPosition(int reelId, int position)
        {
            var offset =  _offsets.Length >= reelId ? _offsets[reelId - 1] : 0;
            var offsetPosition = position + offset;

            if (offsetPosition >= NumberOfMotorSteps)
            {
                return offsetPosition - NumberOfMotorSteps;
            }

            if (offsetPosition < 0)
            {
                return offsetPosition + NumberOfMotorSteps;
            }

            return offsetPosition;
        }

        private int GetAllReelsSelectedBits()
        {
            var bits = 0;
            for (var i = 0; i < ReelConnectedCount; i++)
            {
                bits |= 1 << i;
            }

            return bits;
        }

        private void HandleAbortAndSlowSpinResponse(AbortAndSlowSpinResponse response)
        {
            var commandedReels = new BitArray(new[] { GetAllReelsSelectedBits() });
            for (var i = 0; i < commandedReels.Length; i++)
            {
                if (commandedReels[i])
                {
                    OnMessageReceived(new ReelSpinningStatus { ReelId = i + 1, SlowSpinning = true });
                }
            }
        }

        private void HandleProtocolErrorResponse(ProtocolErrorResponse response)
        {
            OnMessageReceived(new FailureStatus { CommunicationError = true, ErrorCode = (byte)response.ResponseCode });
        }

        private void HandleHardwareErrorResponse(HardwareErrorResponse response)
        {
            switch ((HardwareErrorCodes)response.ResponseCode)
            {
                case HardwareErrorCodes.PowerUpReset:
                    IsAttached = false; // Reset the COMs if we initialized in these states
                    return;
                case HardwareErrorCodes.ResetInstructionFromFirmware:
                case HardwareErrorCodes.MotorVoltageLow:
                case HardwareErrorCodes.LampVoltageLow:
                    // Response ACK to the Soft Reset and power up commands should be ignored.
                    // Voltage low responses can be ignored as they will be reported for reel or lamp commands.
                    return;
                default:
                    OnMessageReceived(new FailureStatus { HardwareError = true, ErrorCode = (byte)response.ResponseCode });
                    break;
            }
        }

        private void HandleUnsolicitedErrorTamperResponse(UnsolicitedErrorTamperResponse response)
        {
            var responseCode = (byte)response.ResponseCode;
            var reelId = responseCode & 0x0F;
            OnMessageReceived(new FailureStatus { ReelId = reelId, TamperDetected = true, ErrorCode = responseCode });
        }

        private void HandleUnsolicitedErrorStallResponse(UnsolicitedErrorStallResponse response)
        {
            var responseCode = (byte)response.ResponseCode;
            var reelId = responseCode & 0x0F;
            OnMessageReceived(new FailureStatus { ReelId = reelId, StallDetected = true, ErrorCode = responseCode });
        }

        private void HandleHomeReelResponse(HomeReelResponse response)
        {
            var reel = _homeReelMessageDictionary.FirstOrDefault(x => x.Value.sequenceId == response.SequenceId).Key;
            if (reel <= 0)
            {
                return;
            }

            if (response.ResponseCode == (int)HomeReelResponseCodes.Homed)
            {
                int homeStep;

                lock (_homeStepsLock)
                {
                    homeStep = _homeSteps[reel - 1];
                }

                var step = GetOffsetPosition(reel, homeStep);
                var stop = ReelExtensions.GetStopFromStep(step, NumberOfMotorSteps, NumberOfStops);

                UpdateReelStatus(
                    reel,
                    x =>
                    {
                        x.Connected = true;
                        x.ReelStall = false;
                        x.ReelTampered = false;
                        x.LowVoltage = false;
                        x.RequestError = false;
                        x.FailedHome = false;
                        return x;
                    });
                OnMessageReceived(new ReelSpinningStatus { ReelId = reel, IdleAtStop = true, Step = step, Stop = stop });
            }
            else if (IsRequestError(response.ResponseCode))
            {
                UpdateReelStatus(
                    reel,
                    x =>
                    {
                        x.RequestError = true;
                        return x;
                    });
                OnMessageReceived(new ReelSpinningStatus { ReelId = reel });
            }
            else if (response.ResponseCode == (int)HomeReelResponseCodes.FailedHome)
            {
                UpdateReelStatus(
                    reel,
                    x =>
                    {
                        x.FailedHome = true;
                        return x;
                    });
                OnMessageReceived(new ReelSpinningStatus { ReelId = reel, FailedHome = true });
            }
            else
            {
                return;
            }

            _homeReelMessageDictionary.TryUpdate(reel, (response.SequenceId, true), (response.SequenceId, false));

            if (_homeReelMessageDictionary.All(x => x.Value.responseReceived))
            {
                _homeReelMessageDictionary.Clear();
            }

            lock (_initializeLock)
            {
                if (_isInitialized || _homeReelMessageDictionary.IsEmpty)
                {
                    return;
                }

                OnMessageReceived(new GatData { Data = GatData });
                OnMessageReceived(new ControllerInitializedStatus());
                _isInitialized = true;
            }
        }

        private void HandleNudgeResponse(NudgeResponse response)
        {
            (byte sequenceId, byte commandedReelBits) lastSpinCommandReels;
            lock (_sequenceLock)
            {
                lastSpinCommandReels = _lastSpinCommandReels;
            }

            if (lastSpinCommandReels == default || lastSpinCommandReels.sequenceId != response.SequenceId)
            {
                return;
            }

            if (response.ResponseCode1 == (int)SpinResponseCodes.Accepted)
            {
                HandleAck80Accepted(lastSpinCommandReels.commandedReelBits);
            }
            else if (IsRequestError(response.ResponseCode1))
            {
                HandleRequestError(response.ResponseCode1, lastSpinCommandReels.commandedReelBits);
            }
            else if ((response.ResponseCode1 & (int)SpinResponseCodes.StoppingReelMask) ==
                     (int)SpinResponseCodes.StoppingReelMask)
            {
                if (response.ResponseCode1 == (int)SpinResponseCodes.AllReelsStopped)
                {
                    return;
                }

                var reelId = response.ResponseCode1 ^ (int)SpinResponseCodes.StoppingReelMask;
                HandleResponseReelStopped(reelId);
            }
            else if (IsResponseError(response.ResponseCode2))
            {
                 HandleResponseError(response.ResponseCode2, lastSpinCommandReels.commandedReelBits, response.ResponseCode1);
            }
        }

        private void HandleSpinResponse(SpinReelsToGoalResponse response)
        {
            (byte sequenceId, byte commandedReelBits) lastSpinCommandReels;
            lock (_sequenceLock)
            {
                lastSpinCommandReels = _lastSpinCommandReels;
            }

            if (lastSpinCommandReels == default || lastSpinCommandReels.sequenceId != response.SequenceId)
            {
                return;
            }

            if (response.ResponseCode1 == (int)SpinResponseCodes.Accepted)
            {
                HandleAck80Accepted(lastSpinCommandReels.commandedReelBits);
            }
            else if (IsRequestError(response.ResponseCode1))
            {
                HandleRequestError(response.ResponseCode1, lastSpinCommandReels.commandedReelBits);
            }
            else if ((response.ResponseCode1 & (int)SpinResponseCodes.StoppingReelMask) ==
                     (int)SpinResponseCodes.StoppingReelMask)
            {
                if (response.ResponseCode1 == (int)SpinResponseCodes.AllReelsStopped)
                {
                    return;
                }

                var reelId = response.ResponseCode1 ^ (int)SpinResponseCodes.StoppingReelMask;
                HandleResponseReelStopped(reelId);
            }
            else
            if (response.ResponseCode1 == (int)HarkeyResponseErrorCodes.Skew)
            {
                // According to spec, skew indicates that the reel was in a tamper state while spinning to goal and it was successfully re-synced
                var direction = response.ResponseCode3 == 0 ? "forward" : "backward";
                Logger.Error($"HandleSpinResponse Reel {response.ResponseCode2} skewed {direction} {response.ResponseCode4} steps");
            }
            else if (IsResponseError(response.ResponseCode2))
            {
                HandleResponseError(response.ResponseCode2, lastSpinCommandReels.commandedReelBits, response.ResponseCode1);
            }
        }

        private void HandleVersionResponse(Rm6VersionResponse version)
        {
            FirmwareVersion = GetVersionString(version);
        }

        private void HandleStatus(ReelStatusResponse status)
        {
            Status = status.GlobalStatus.ToFailureStatus();

            Contracts.Gds.Reel.ReelStatus reel1Status;
            Contracts.Gds.Reel.ReelStatus reel2Status;
            Contracts.Gds.Reel.ReelStatus reel3Status;
            Contracts.Gds.Reel.ReelStatus reel4Status;
            Contracts.Gds.Reel.ReelStatus reel5Status;
            Contracts.Gds.Reel.ReelStatus reel6Status;

            lock (_countLock)
            {
                // We need to ignore the connected status when homing after a tilt because the controller reports no status.
                reel1Status = status.Reel1Status.ToReelStatus(1, _homeReelMessageDictionary.ContainsKey(1) && _reelsConnected[0]);
                reel2Status = status.Reel2Status.ToReelStatus(2, _homeReelMessageDictionary.ContainsKey(2) && _reelsConnected[1]);
                reel3Status = status.Reel3Status.ToReelStatus(3, _homeReelMessageDictionary.ContainsKey(3) && _reelsConnected[2]);
                reel4Status = status.Reel4Status.ToReelStatus(4, _homeReelMessageDictionary.ContainsKey(4) && _reelsConnected[3]);
                reel5Status = status.Reel5Status.ToReelStatus(5, _homeReelMessageDictionary.ContainsKey(5) && _reelsConnected[4]);
                reel6Status = status.Reel6Status.ToReelStatus(6, _homeReelMessageDictionary.ContainsKey(6) && _reelsConnected[5]);

                _reelsConnected[0] = reel1Status.Connected;
                _reelsConnected[1] = reel2Status.Connected;
                _reelsConnected[2] = reel3Status.Connected;
                _reelsConnected[3] = reel4Status.Connected;
                _reelsConnected[4] = reel5Status.Connected;
                _reelsConnected[5] = reel6Status.Connected;
            }

            SetReelStatus(1, reel1Status);
            SetReelStatus(2, reel2Status);
            SetReelStatus(3, reel3Status);
            SetReelStatus(4, reel4Status);
            SetReelStatus(5, reel5Status);
            SetReelStatus(6, reel6Status);
        }

        private void HandleSetFaultsResponse(SetFaultsResponse response)
        {
            if (response.ResponseCode != HarkeyConstants.CommandAcknowledged20)
            {
                return;
            }

            _setOffsetsPending = false;
        }

        private void HandleModel()
        {
            Model = "RM6";
            Manufacturer = "Aristocrat";
            Protocol = "Harkey";
        }

        private void HomeReel(int reelId)
        {
            if (_homeReelMessageDictionary.ContainsKey(reelId))
            {
                return;
            }

            if (_setOffsetsPending)
            {
                lock (_homeStepsLock)
                {
                    SetFaults(_homeSteps);
                }
            }

            byte sequenceId;
            lock (_sequenceLock)
            {
                sequenceId = NextSequenceId();
                _homeReelMessageDictionary.TryAdd(reelId, (sequenceId, false));
            }

            SendCommand(new HomeReel { ReelId = reelId, SequenceId = sequenceId }, false);
        }

        private void SetFaults(params byte[] positions)
        {
            if (positions.Length != HarkeyConstants.MaxReelId || !IsAttached)
            {
                return;
            }

            SendCommand(
                new SetFaults
                {
                    Fault1 = (byte)GetOffsetPosition(1, positions[0]),
                    Fault2 = (byte)GetOffsetPosition(2, positions[1]),
                    Fault3 = (byte)GetOffsetPosition(3, positions[2]),
                    Fault4 = (byte)GetOffsetPosition(4, positions[3]),
                    Fault5 = (byte)GetOffsetPosition(5, positions[4]),
                    Fault6 = (byte)GetOffsetPosition(6, positions[5]),
                });
        }

        private void SpinReelsToGoal(byte directions, byte[] stops, byte selectedReels, byte rampTable)
        {
            byte sequenceId;
            lock (_sequenceLock)
            {
                sequenceId = NextSequenceId();
                _lastSpinCommandReels = (sequenceId, selectedReels);
            }

            SendCommand(new SpinReelsToGoal
            {
                Direction = directions,
                Goal1 = stops[0],
                Goal2 = stops[1],
                Goal3 = stops[2],
                Goal4 = stops[3],
                Goal5 = stops[4],
                Goal6 = stops[5],
                SelectedReels = selectedReels,
                RampTable = rampTable,
                SequenceId = sequenceId
            }, false);
        }

        private void Nudge(byte directions, IReadOnlyList<byte> steps, byte selectedReels, byte delay)
        {
            byte sequenceId;
            lock (_sequenceLock)
            {
                sequenceId = NextSequenceId();
                _lastSpinCommandReels = (sequenceId, selectedReels);
            }

            SendCommand(new Nudge
            {
                Direction = directions,
                Nudge1 = steps[0],
                Nudge2 = steps[1],
                Nudge3 = steps[2],
                Nudge4 = steps[3],
                Nudge5 = steps[4],
                Nudge6 = steps[5],
                Delay = delay,
                SequenceId = sequenceId
            }, false);
        }

        private static int CreateLightMask(ReelLampData topLight, ReelLampData middleLight, ReelLampData bottomLight)
        {
            var lightMask = 0x00;

            if (topLight.IsLampOn)
            {
                lightMask |= 0x04;
            }

            if (middleLight.IsLampOn)
            {
                lightMask |= 0x02;
            }

            if (bottomLight.IsLampOn)
            {
                lightMask |= 0x01;
            }

            return lightMask;
        }

        private void SetLightsStates(IList<ReelLampData> lampData)
        {
            SendCommand(new SetReelLights
            {
                Reel1Lights = CreateLightMask(lampData[0], lampData[1], lampData[2]),
                Reel2Lights = CreateLightMask(lampData[3], lampData[4], lampData[5]),
                Reel3Lights = CreateLightMask(lampData[6], lampData[7], lampData[8]),
                Reel4Lights = CreateLightMask(lampData[9], lampData[10], lampData[11]),
                Reel5Lights = CreateLightMask(lampData[12], lampData[13], lampData[14]),
                Reel6Lights = CreateLightMask(lampData[15], lampData[16], lampData[17])
            });
        }

        private void SetLightsColors(IList<ReelLampData> lampData)
        {
            if (lampData.Count != MaxLightId)
            {
                Logger.Warn("Unable to set reel light colors with insufficient lamp data");
                return;
            }

            var reelConnected = new List<bool>();
            lock (_countLock)
            {
                reelConnected.AddRange(_reelsConnected);
            }

            for (var i = 0; i < HarkeyConstants.MaxReelId; i++)
            {
                if (!reelConnected[i])
                {
                    continue;
                }

                var top = ColorToWord(lampData[i * LightsPerReel].Color);
                var middle = ColorToWord(lampData[i * LightsPerReel + 1].Color);
                var bottom = ColorToWord(lampData[i * LightsPerReel + 2].Color);

                if (top == _lastReelColors[i].TopColor && middle == _lastReelColors[i].MiddleColor && bottom == _lastReelColors[i].BottomColor)
                {
                    continue;
                }

                _lastReelColors[i] = new ReelColors(top, middle, bottom);

                SendCommand(
                    new SetReelLightColor
                    {
                        ReelId = i + 1,
                        TopColor = top,
                        MiddleColor = middle,
                        BottomColor = bottom
                    });
            }
        }

        private void OnMessageBuilt(object sender, MessagedBuiltEventArgs e)
        {
            var message = BuildMessage<HarkeySerializableMessage>(e.Message);

            if (message is null)
            {
                var byteString = string.Join("-", e.Message);
                Logger.Warn($"Unable to parse Harkey message: {byteString}");
                return;
            }

            Logger.Debug($"Got Message: {message}");
            foreach (var invoker in ReportInvokers(message.GetType()))
            {
                invoker(message);
            }
        }

        private InvokerQueue ReportInvokers(Type reportType)
        {
            return _router.GetOrAdd(reportType, _ => new InvokerQueue());
        }

        private void RegisterCallback<T>(Action<T> callback)
            where T : HarkeySerializableMessage
        {
            var invokers = ReportInvokers(typeof(T));
            invokers.Enqueue(
                buffer =>
                {
                    try
                    {
                        lock (_sequenceLock)
                        {
                            _statusResponse.Restart();
                        }

                        callback(buffer as T);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"ReportReceived: exception processing object {e}");
                    }
                });
        }

        private byte NextSequenceId()
        {
            lock (_sequenceLock)
            {
                _sequenceId = (byte)((_sequenceId + 1) % HarkeyConstants.MaxSequenceId);
                return _sequenceId;
            }
        }

        private void SendCommand(HarkeySerializableMessage message, bool updateSequenceId = true)
        {
            if (updateSequenceId && message is ISequencedCommand command)
            {
                command.SequenceId = NextSequenceId();
            }

            var messageBytes = message.Serialize();

            if (message.UseCommandInsteadOfProtocol)
            {
                SendCommand(messageBytes, GetNoProtocolMessageTemplate(messageBytes.Length));
            }
            else
            {
                SendCommand(messageBytes);
            }
        }

        private void HandleAck80Accepted(byte commandedReelBits)
        {
            var commandedReels = new BitArray(new[] { commandedReelBits });
            for (var i = 0; i < commandedReels.Length; i++)
            {
                if (commandedReels[i])
                {
                    OnMessageReceived(new ReelSpinningStatus { ReelId = i + 1, Spinning = true });
                }
            }
        }

        private void HandleResponseReelStopped(int reelId)
        {
            lock (_reelStepsLock)
            {
                var stop = ReelExtensions.GetStopFromStep(_reelSteps[reelId - 1], NumberOfMotorSteps, NumberOfStops);
                OnMessageReceived(new ReelSpinningStatus { ReelId = reelId, Stop = stop, Step = _reelSteps[reelId - 1], IdleAtStop = true });
            }
        }

        private void HandleRequestError(int errorCode, byte commandedReelBits)
        {
            switch ((HarkeyRequestErrorCodes)errorCode)
            {
                case HarkeyRequestErrorCodes.InvalidValue:
                case HarkeyRequestErrorCodes.ReelInError:
                case HarkeyRequestErrorCodes.ReelAlreadySpinning:
                case HarkeyRequestErrorCodes.BadState:
                case HarkeyRequestErrorCodes.OutOfSync:
                case HarkeyRequestErrorCodes.ReelNotAvailable:
                case HarkeyRequestErrorCodes.LowVoltage:
                case HarkeyRequestErrorCodes.GameChecksumError:
                case HarkeyRequestErrorCodes.FaultChecksumError:
                    HandleSlowSpinError(errorCode, commandedReelBits);
                    break;
                default:
                    Logger.Error($"Request error {errorCode} received - UNHANDLED.");
                    break;
            }
        }

        private void HandleResponseError(int errorCode, byte commandedReelBits, int reelId)
        {
            switch ((HarkeyResponseErrorCodes)errorCode)
            {
                case HarkeyResponseErrorCodes.NoSync:
                case HarkeyResponseErrorCodes.Stall:
                    HandleSlowSpinError(errorCode, commandedReelBits, reelId);
                    break;
                default:
                    Logger.Error($"Response error {errorCode} received - UNHANDLED.");
                    break;
            }
        }

        private void HandleSlowSpinError(int errorCode, byte commandedReelBits, int reelId = 0)
        {
            var commandedReels = new BitArray(new[] { commandedReelBits });
            switch ((HarkeyResponseErrorCodes)errorCode)
            {
                case HarkeyResponseErrorCodes.Stall:
                    OnMessageReceived(new FailureStatus { ReelId = reelId, MechanicalError = true, ErrorCode = (byte)errorCode });
                    break;
                case HarkeyResponseErrorCodes.NoSync:
                    OnMessageReceived(new FailureStatus { ReelId = reelId, TamperDetected = true, ErrorCode = (byte)errorCode });
                    break;
            }

            for (var i = 0; i < commandedReels.Length; i++)
            {
                if (!commandedReels[i])
                {
                    continue;
                }

                var reelSpinningStatus =  new ReelSpinningStatus { ReelId = i + 1, SlowSpinning = true };
                switch ((HarkeyRequestErrorCodes)errorCode)
                {
                    case HarkeyRequestErrorCodes.InvalidValue:
                    case HarkeyRequestErrorCodes.ReelInError:
                    case HarkeyRequestErrorCodes.ReelAlreadySpinning:
                    case HarkeyRequestErrorCodes.BadState:
                    case HarkeyRequestErrorCodes.ReelNotAvailable:
                        OnMessageReceived(new FailureStatus { ReelId = i + 1, ComponentError = true, ErrorCode = (byte)errorCode});
                        break;
                    case HarkeyRequestErrorCodes.OutOfSync:
                        OnMessageReceived(new FailureStatus { ReelId = i + 1, TamperDetected = true, ErrorCode = (byte)errorCode});
                        break;
                    case HarkeyRequestErrorCodes.LowVoltage:
                        OnMessageReceived(new FailureStatus { ReelId = i + 1, LowVoltageDetected = true, ErrorCode = (byte)errorCode});
                        break;
                    case HarkeyRequestErrorCodes.GameChecksumError:
                    case HarkeyRequestErrorCodes.FaultChecksumError:
                        OnMessageReceived(new FailureStatus { ReelId = i + 1, FirmwareError = true, ErrorCode = (byte)errorCode});
                        break;
                    default:
                        Logger.Error($"Request error {errorCode} received - UNHANDLED.");
                        break;
                }

                OnMessageReceived(reelSpinningStatus);
            }
        }

        private static ushort ColorToWord(Color color)
        {
            const int redShift = 11; // red is top 5 bits of the word
            const int greenShift = 5; // green is middle 6 bits of word

            const int maxRed = 31; // Red is 5 bits
            const int maxGreen = 31; // Green is 6 bits
            const int maxBlue = 31; // Blue is 5 bits
            const int maxByte = 255;

            var red = (double)color.R / maxByte * maxRed;
            var green = (double)color.G / maxByte * maxGreen;
            var blue = (double)color.B / maxByte * maxBlue;

            return (ushort)(((int)red << redShift) | ((int)green << greenShift) | (int)blue);
        }

        private void ResetLights()
        {
            // The reel controller is supposed to reset the lights on the soft reset command but that doesn't happen.
            // It appears the reel controller's status bits get reset but the light controller does not get updated.
            // The reel controller will not update the lights unless there is a change in the status.
            // After a reset the controller thinks the lights are off but they could be on.
            // First we need to tell the reel controller to turn all lights on, then turn them all off.
            SetLightsStates(
                Enumerable.Range(0, MaxLightId).Select(i => new ReelLampData(Color.White, true, i + 1)).ToList());
            SetLightsStates(
                Enumerable.Range(0, MaxLightId).Select(i => new ReelLampData(Color.White, false, i + 1)).ToList());
        }
    }
}