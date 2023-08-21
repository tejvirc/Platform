namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Diagnostics;
    using Protocol.v21;

    /// <summary>
    ///     The <i>idReader</i> class is used to manage player and employee ID
    ///     reader (identification reader) devices installed in an EGM.
    ///     Such devices include, but are not limited to, magnetic card readers,
    ///     proximity card readers, fingerprint scanners, retina scanners, smart
    ///     card readers, direct-entry keypads, and Hollerith cards.
    /// </summary>
    public class IdReaderDevice : ClientDeviceBase<idReader>, IIdReaderDevice
    {
        /// <summary>
        ///     Default Removal Delay Parameter Name
        /// </summary>
        public const int DefaultRemovalDelayParameterName = 5000;

        /// <summary>
        ///     Default Msg Duration
        /// </summary>
        public const int DefaultMsgDuration = 15000;

        /// <summary>
        ///     Default Attract Msg
        /// </summary>
        public const string DefaultAttractMsg = @"Insert Card";

        /// <summary>
        ///     Default Wait Msg
        /// </summary>
        public const string DefaultWaitMsg = @"Validating";

        /// <summary>
        ///     Default Valid Msg
        /// </summary>
        public const string DefaultValidMsg = @"Card Validated";

        /// <summary>
        ///     Default Invalid Msg
        /// </summary>
        public const string DefaultInvalidMsg = @"Invalid Card";

        /// <summary>
        ///     Default Lost Msg
        /// </summary>
        public const string DefaultLostMsg = @"Lost Card";

        /// <summary>
        ///     Default OffLine Msg
        /// </summary>
        public const string DefaultOffLineMsg = @"Unable To Validate";

        /// <summary>
        ///     Default Abandon Msg
        /// </summary>
        public const string DefaultAbandonMsg = @"Remove Card";

        /// <summary>
        ///     Default No Player Messages
        /// </summary>
        public const bool DefaultNoPlayerMessages = true;

        private bool _deviceCommunicationClosed;
        private bool _disposed;
        private CancellationTokenSource _getIdValidationCancellationToken;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdReaderDevice" /> class.
        /// </summary>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="deviceId">The device identifier.</param>
        public IdReaderDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver, false)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public bool RestartStatus { get; private set; }

        /// <inheritdoc />
        public int TimeToLive { get; protected set; }

        /// <inheritdoc />
        public int RemovalDelay { get; protected set; }

        /// <inheritdoc />
        public int MsgDuration { get; protected set; }

        /// <inheritdoc />
        public string AttractMsg { get; protected set; }

        /// <inheritdoc />
        public string WaitMsg { get; protected set; }

        /// <inheritdoc />
        public string ValidMsg { get; protected set; }

        /// <inheritdoc />
        public string InvalidMsg { get; protected set; }

        /// <inheritdoc />
        public string LostMsg { get; protected set; }

        /// <inheritdoc />
        public string OffLineMsg { get; protected set; }

        /// <inheritdoc />
        public string AbandonMsg { get; protected set; }

        /// <inheritdoc />
        public bool NoPlayerMessages { get; protected set; }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
            _deviceCommunicationClosed = false;
        }

        /// <inheritdoc />
        public override void Close()
        {
            _deviceCommunicationClosed = true;
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.RestartStatusParameterName,
                optionConfigValues,
                parameterId => { RestartStatus = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.UseDefaultConfigParameterName,
                optionConfigValues,
                parameterId => { UseDefaultConfig = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.RequiredForPlayParameterName,
                optionConfigValues,
                parameterId => { RequiredForPlay = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.TimeToLiveParameterName,
                optionConfigValues,
                parameterId => { TimeToLive = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.RemovalDelayParameterName,
                optionConfigValues,
                parameterId => { RemovalDelay = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.MsgDurationParameterName,
                optionConfigValues,
                parameterId => { MsgDuration = optionConfigValues.Int32Value(parameterId); });

            // ID Reader Messages Sub-Parameter Definitions
            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.AttractMsgParameterName,
                optionConfigValues,
                parameterId => { AttractMsg = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.WaitMsgParameterName,
                optionConfigValues,
                parameterId => { WaitMsg = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.ValidMsgParameterName,
                optionConfigValues,
                parameterId => { ValidMsg = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.InvalidMsgParameterName,
                optionConfigValues,
                parameterId => { InvalidMsg = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.LostMsgParameterName,
                optionConfigValues,
                parameterId => { LostMsg = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.OffLineMsgParameterName,
                optionConfigValues,
                parameterId => { OffLineMsg = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.AbandonMsgParameterName,
                optionConfigValues,
                parameterId => { AbandonMsg = optionConfigValues.StringValue(parameterId); });

            // Multi-Lingual Option Sub-Parameter Definitions
            //SetDeviceValue(
            //    G2SParametersNames.IdReaderDevice.,
            //    optionConfigValues,
            //    parameterId => { // = optionConfigValues.BooleanValue(parameterId); });

            // TODO : Support these Multi-Lingual options:
            //public const string LocaleIdParameterName = "G2S_localeId";
            //public const string LocaleNameParameterName = "G2S_localeName";

            // G2S No Player Messages Sub-Parameter Definitions
            SetDeviceValue(
                G2SParametersNames.IdReaderDevice.NoPlayerMessagesParameterName,
                optionConfigValues,
                parameterId => { NoPlayerMessages = optionConfigValues.BooleanValue(parameterId); });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE006);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE099);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE108);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE901);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE902);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE903);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE904);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE905);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE906);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE907);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IDE908);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE200);
        }

        /// <inheritdoc />
        public async Task<setIdValidation> GetIdValidation(
            getIdValidation idValidationCommand,
            TimeSpan timeout,
            bool allowOffline,
            IReadOnlyCollection<(string type, string pattern)> offlinePatterns)
        {
            setIdValidation validId = null;
            var offlineValidationAttempted = false;

            if (!HostEnabled || !Queue.CanSend && _deviceCommunicationClosed ||
                _getIdValidationCancellationToken != null && _getIdValidationCancellationToken.IsCancellationRequested)
            {
                _getIdValidationCancellationToken?.Cancel(false);
                _getIdValidationCancellationToken?.Dispose();
                _getIdValidationCancellationToken = null;
                // Why? Because at that level we also need to check ID type!
                // Swipe cards don't generate GetIdValidation; neither does an expiring timer or ID card out!
                return null;
            }

            // Check to see if this request to G2S Host has expired.
            if (timeout <= TimeSpan.Zero)
            {
                // TODO : Do not attempt to validate offline if the ID Reader is a swipe device!
                // TODO : Check that there are more than 0 idTypeProfiles!
                // Validate offline if we're able to.
                if (allowOffline)
                {
                    validId = ValidateIdOffline(idValidationCommand.idNumber, offlinePatterns);
                    offlineValidationAttempted = true;
                }
                else
                {
                    return null;
                }
            }

            // Update the expiry time to complete.
            if (timeout != TimeSpan.MaxValue)
            {
                timeout = timeout.Subtract(TimeSpan.FromMilliseconds(TimeToLive));
            }

            var request = InternalCreateClass();
            request.Item = idValidationCommand;

            var session = SendRequest(request);
            session.WaitForCompletion();

            var response = session.Responses.Count > 0 ? session.Responses[0] : null;

            // Process a successful and adequate response.
            if (session.SessionState == SessionStatus.Success &&
                response?.IClass.Item is setIdValidation responseId)
            {
                return responseId;
            }

            if (offlineValidationAttempted)
            {
                return validId;
            }

            if (_getIdValidationCancellationToken == null)
            {
                _getIdValidationCancellationToken = new CancellationTokenSource();
            }

            // TODO : Do not attempt to validate again if the ID Reader is a swipe device!
            if (session.SessionState == SessionStatus.TimedOut)
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"IdReaderDevice.GetIdValidation : getIdValidation timed out. Trying again
                    Device Id : {0}",
                    Id);

                return await Task.Run(
                    async () => await GetIdValidation(idValidationCommand, timeout, allowOffline, offlinePatterns),
                    _getIdValidationCancellationToken.Token);
            }

            SourceTrace.TraceInformation(
                G2STrace.Source,
                @"IdReaderDevice.GetIdValidation : getIdValidation failed. Trying again
                Device Id : {0}",
                Id);

            // TODO : Do not attempt to validate again if the ID Reader is a swipe device!
            await Task.Delay(TimeToLive, _getIdValidationCancellationToken.Token)
                .ContinueWith(
                    async task =>
                    {
                        if (!task.IsCanceled)
                        {
                            validId = await GetIdValidation(
                                idValidationCommand,
                                timeout,
                                allowOffline,
                                offlinePatterns);
                        }
                    }
                );

            return validId;
        }

        /// <inheritdoc />
        public void CancelGetIdValidation()
        {
            _getIdValidationCancellationToken?.Cancel(false);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources;
        ///     false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_getIdValidationCancellationToken != null)
                {
                    _getIdValidationCancellationToken.Cancel(false);
                    _getIdValidationCancellationToken.Dispose();
                    _getIdValidationCancellationToken = null;
                }

                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"IdReaderDevice.Dispose : Disposed of ID Reader Device
                    Host Id : {0}",
                    Owner);
            }

            _disposed = true;
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        private static setIdValidation ValidateIdOffline(
            string idNumber,
            IEnumerable<(string type, string pattern)> offlinePatterns)
        {
            return (from pattern in offlinePatterns
                where Regex.IsMatch(idNumber, pattern.pattern)
                select new setIdValidation
                {
                    idAge = 0,
                    idAnniversary = false,
                    idBanned = false,
                    idBirthday = false,
                    idClass = string.Empty,
                    idDisplayMessages = true,
                    idFullName = string.Empty,
                    idGender = t_idGenders.G2S_Unknown,
                    idNumber = idNumber,
                    idPreferName = string.Empty,
                    idPrivacy = false,
                    idRank = 0,
                    idState = t_idStates.G2S_active,
                    idType = pattern.type,
                    idValidDateTime = DateTime.UtcNow,
                    idValidDateTimeSpecified = true,
                    idValidExpired = false,
                    idValidSource = t_idSources.G2S_offLine,
                    idVIP = false,
                    localeId = "en_US", // cabinetProfile.localeId
                    playerId = string.Empty
                }).FirstOrDefault();
        }

        private void SetDefaults()
        {
            RequiredForPlay = false;
            RestartStatus = true;
            TimeToLive = (int)Constants.DefaultTimeout.TotalMilliseconds;

            RemovalDelay = DefaultRemovalDelayParameterName;
            MsgDuration = DefaultMsgDuration;
            AttractMsg = DefaultAttractMsg;
            WaitMsg = DefaultWaitMsg;
            ValidMsg = DefaultValidMsg;
            InvalidMsg = DefaultInvalidMsg;
            LostMsg = DefaultLostMsg;
            OffLineMsg = DefaultOffLineMsg;
            AbandonMsg = DefaultAbandonMsg;
            NoPlayerMessages = DefaultNoPlayerMessages;
        }
    }
}