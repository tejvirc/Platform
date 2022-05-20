namespace Aristocrat.G2S.Client.Devices.v21
{
    using System.Threading;
    using System.Threading.Tasks;
    using Diagnostics;
    using Protocol.v21;

    /// <summary>
    ///     The game authentication terminal (<i>gat</i>) class provides a set of commands specifically tailored to
    ///     authentication required by regulators.The <i>gat</i> class also provides extended or special capability at the
    ///     option of the EGM manufacturer; these are referred to as special functions.
    /// </summary>
    /// <remarks>
    ///     The device identifiers for devices within the <i>gat</i> class MUST be equal to the host identifier of the host
    ///     that owns the device.When a host is registered, the EGM MUST create and remove <i>gat</i> devices, as required.
    ///     When a host is unregistered, the EGM MUST remove any gat devices owned by the host.The EGM MUST NOT own <i>gat</i>
    ///     devices.See Section 8.16 (setCommChange command) for more details.
    ///     <para>
    ///         The <i>gat</i> class is a multi-device class. Each registered host may own a single <i>gat</i> device.
    ///     </para>
    /// </remarks>
    public class GatDevice :
        HostOrientedDevice<gat>,
        IGatDevice
    {
        /// <summary>
        ///     The special functions default value
        /// </summary>
        public const t_g2sBoolean DefaultSpecialFunctions = t_g2sBoolean.G2S_false;

        /// <summary>
        ///     The minimum queued component requests default value
        /// </summary>
        public const int DefaultMinQueuedComps = 1;

        private CancellationTokenSource _sendVerificationCancellationToken;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GatDevice" /> class.
        /// </summary>
        /// <param name="deviceId">device id</param>
        /// <param name="deviceStateObserver">An IDeviceStateObserver instance.</param>
        public GatDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver)
        {
            /* TODO:
             * SpecialFunctions
             * Indicates whether commands related to special
      functions should be processed by the EGM for
      the device; when special functions are not
      supported for the device, the EGM MUST set
      the value to G2S_false and the EGM MUST
      NOT allow the value to be changed; when set to
      G2S_false, special function commands MUST
      NOT be processed for the device. If the value is
      set to G2S_unknown, special functions MAY be
      supported or not supported by the EGM – the
      host should use other mechanisms to determine
      whether special functions are supported. The
      value MUST NOT be set to G2S_unknown if the
      EGM supports this extension. If the value is set
      to G2S_true, special functions MAY be
      supported by the EGM for the device.*/

            SetDefaults();
        }

        /// <inheritdoc />
        public int MinLogEntries { get; protected set; }

        /// <inheritdoc />
        public int TimeToLive { get; protected set; }

        /// <inheritdoc />
        public int IdReaderId { get; protected set; }

        /// <inheritdoc />
        public int MinQueuedComps { get; protected set; }

        /// <inheritdoc />
        public t_g2sBoolean SpecialFunctions { get; protected set; }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
        {
            _sendVerificationCancellationToken?.Cancel(false);
            _sendVerificationCancellationToken?.Dispose();
            _sendVerificationCancellationToken = null;
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.UseDefaultConfigParameterName,
                optionConfigValues,
                parameterId => { UseDefaultConfig = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.TimeToLiveParameterName,
                optionConfigValues,
                parameterId => { TimeToLive = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.GatDevice.IdReaderIdParameterName,
                optionConfigValues,
                parameterId => { IdReaderId = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.GatDevice.MinQueuedCompsParameterName,
                optionConfigValues,
                parameterId => { MinQueuedComps = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.GatDevice.SpecialFunctionsParameterName,
                optionConfigValues,
                parameterId =>
                {
                    var stringValue = optionConfigValues.StringValue(parameterId);
                    SpecialFunctions = t_g2sBoolean.G2S_true.ToString() == stringValue
                        ? t_g2sBoolean.G2S_true
                        : t_g2sBoolean.G2S_false;
                });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GAE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GAE006);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GAE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GAE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GAE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GAE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GAE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GAE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GAE107);
        }

        /// <inheritdoc />
        public verificationResultAck SendVerificationResult(c_baseCommand command)
        {
            var request = InternalCreateClass();
            request.Item = command;
            var session = SendRequest(request);

            session.WaitForCompletion(300000);

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0 &&
                session.Responses[0].IClass.Item is verificationResultAck)
            {
                _sendVerificationCancellationToken?.Cancel(false);
                _sendVerificationCancellationToken?.Dispose();
                _sendVerificationCancellationToken = null;

                return session.Responses[0].IClass.Item as verificationResultAck;
            }

            if (_sendVerificationCancellationToken == null)
            {
                _sendVerificationCancellationToken = new CancellationTokenSource();
            }

            if (session.SessionState == SessionStatus.TimedOut)
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"GatDevice.SendVerificationResult : c_baseCommand Timed Out.  Will try again
    Device Id : {0}",
                    Id);

                return Task.Run(() => SendVerificationResult(command), _sendVerificationCancellationToken.Token)
                    .Result;
            }

            SourceTrace.TraceInformation(
                G2STrace.Source,
                @"GatDevice.SendVerificationResult : c_baseCommand Failed.  Will try again
    Device Id : {0}",
                Id);
            verificationResultAck result = null;
            Task.Delay(TimeToLive, _sendVerificationCancellationToken.Token)
                .ContinueWith(
                    task =>
                    {
                        if (!task.IsCanceled)
                        {
                            result = SendVerificationResult(command);
                        }
                    });

            return result;
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        private void SetDefaults()
        {
            TimeToLive = (int)Constants.DefaultTimeout.TotalMilliseconds;

            MinLogEntries = Constants.DefaultMinLogEntries;
            MinQueuedComps = DefaultMinQueuedComps;
            SpecialFunctions = DefaultSpecialFunctions;
        }
    }
}