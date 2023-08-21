namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Diagnostics;
    using Protocol.v21;

    /// <summary>
    ///     The download class is used to control software download, upload, installation, and un-installation related to an
    ///     EGM.The class includes commands for managing the software content of an EGM, which includes reading
    ///     the current software content on an EGM, assigning new software content to the EGM, removing unwanted
    ///     content from an EGM, and specifying when the new content is to be installed or uninstalled.The download
    ///     class provides a standardized command set that fulfills the following software download activities:
    ///     • The G2S download protocol will provide:
    ///     • A standardized protocol to manage the software content on all G2S-compliant EGMs from
    ///     all G2S-compliant host systems.This includes both download and upload transfer types.
    ///     • A means for installation of software.
    ///     • A means for removal of software (uninstall).
    ///     • A means for creating software packages on the EGM for upload purposes.
    ///     • A means to schedule the installation and/or un-installation of software, providing various
    ///     scheduling options that relate to a specific time, EGM state, or interaction with the host or
    ///     technician.
    ///     • The G2S message protocol will:
    ///     • Support reading an inventory of software packages and installed modules. This provides the
    ///     capability to effectively manage the content on the EGM.
    ///     • Provide a means to record transaction logs for packages and scripts.This provides an audit
    ///     capability to determine how content came to be on an EGM.
    ///     This strategy provides a central point of control, where a consistent and common interface is used to affect
    ///     software download and upload, with a goal of inter-operation among all G2S hosts and all EGM equipment.
    ///     Additional features provide sequencing of scheduled activities with one or more hosts, providing each host the
    ///     opportunity to read the EGM’s status, logs, and meters.
    ///     The download class is a single-device class. The EGM MUST expose only one active download device.
    /// </summary>
    public class DownloadDevice : ClientDeviceBase<download>, IDownloadDevice
    {
        /// <summary>
        ///     The no response timer default value
        /// </summary>
        public const int DefaultNoResponseTimer = 0;

        /// <summary>
        ///     The minimum package log entries default value
        /// </summary>
        public const int DefaultMinPackageLogEntries = 35;

        /// <summary>
        ///     The minimum script log entries default value
        /// </summary>
        public const int DefaultMinScriptLogEntries = 35;

        /// <summary>
        ///     The minimum package list entries default value
        /// </summary>
        public const int DefaultMinPackageListEntries = 10;

        /// <summary>
        ///     The minimum script list entries default value
        /// </summary>
        public const int DefaultMinScriptListEntries = 10;

        /// <summary>
        ///     The minimum authorization wait timeout default value
        /// </summary>
        public const int DefaultAuthorizationWaitTimeOut = 60000;

        /// <summary>
        ///     The minimum authorization wait retries default value
        /// </summary>
        public const int DefaultAuthorizationWaitRetries = 2;

        /// <summary>
        ///     The upload enabled default value
        /// </summary>
        public const bool DefaultUploadEnabled = true;

        /// <summary>
        ///     The scripting enabled default value
        /// </summary>
        public const bool DefaultScriptingEnabled = true;

        /// <summary>
        ///     The transport protocol list support default value
        /// </summary>
        public const bool DefaultProtocolListSupport = false;

        /// <summary>
        ///     The restart status default value
        /// </summary>
        public const bool DefaultRestartStatus = true;

        /// <summary>
        ///     The no message timer default value
        /// </summary>
        public const int DefaultNoMessageTimer = 0;

        /// <summary>
        ///     The transfer progress frequency default value
        /// </summary>
        public const int DefaultTransferProgressFrequency = 0;

        /// <summary>
        ///     The pause supported default value
        /// </summary>
        public const bool DefaultPauseSupported = false;

        /// <summary>
        ///     The abort transfer default value
        /// </summary>
        public const bool DefaultAbortTransferSupported = false;

        private CancellationTokenSource _sendStatusCancellationToken;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DownloadDevice" /> class.
        /// </summary>
        /// <param name="deviceStateObserver">An IDeviceStateObserver instance.</param>
        /// <param name="downloadEnabled">Indicates whether downloads are enabled or not</param>
        public DownloadDevice(IDeviceObserver deviceStateObserver, bool downloadEnabled)
            : base(1, deviceStateObserver)
        {
            RestartStatus = true;
            DownloadEnabled = downloadEnabled;

            SetDefaults();
        }

        /// <inheritdoc />
        public bool AbortTransferSupported { get; protected set; }

        /// <inheritdoc />
        public int AuthenticationWaitRetries { get; protected set; }

        /// <inheritdoc />
        public int AuthenticationWaitTimeOut { get; protected set; }

        /// <inheritdoc />
        public bool DownloadEnabled { get; set; }

        /// <inheritdoc />
        public bool UploadEnabled { get; protected set; }

        /// <inheritdoc />
        public int MinPackageListEntries { get; protected set; }

        /// <inheritdoc />
        public int MinPackageLogEntries { get; protected set; }

        /// <inheritdoc />
        public int MinScriptListEntries { get; protected set; }

        /// <inheritdoc />
        public int MinScriptLogEntries { get; protected set; }

        /// <inheritdoc />
        public int NoMessageTimer { get; protected set; }

        /// <inheritdoc />
        public bool PauseSupported { get; protected set; }

        /// <inheritdoc />
        public bool ProtocolListSupport { get; protected set; }

        /// <inheritdoc />
        public bool RestartStatus { get; protected set; }

        /// <inheritdoc />
        public bool ScriptingEnabled { get; protected set; }

        /// <inheritdoc />
        public int TransferProgressFrequency { get; protected set; }

        /// <inheritdoc />
        public TimeSpan NoResponseTimer { get; protected set; }

        /// <inheritdoc />
        public int TimeToLive { get; protected set; }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
        {
            _sendStatusCancellationToken?.Cancel(false);
            _sendStatusCancellationToken?.Dispose();
            _sendStatusCancellationToken = null;
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
                G2SParametersNames.NoResponseTimerParameterName,
                optionConfigValues,
                parameterId => { NoResponseTimer = optionConfigValues.TimeSpanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.NoMessageTimerParameterName,
                optionConfigValues,
                parameterId => { NoMessageTimer = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.MinPackageLogEntriesParameterName,
                optionConfigValues,
                parameterId => { MinPackageLogEntries = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.MinScriptLogEntriesParameterName,
                optionConfigValues,
                parameterId => { MinScriptLogEntries = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.MinPackageListEntriesParameterName,
                optionConfigValues,
                parameterId => { MinPackageListEntries = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.MinScriptListEntriesParameterName,
                optionConfigValues,
                parameterId => { MinScriptListEntries = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.AuthorizationWaitTimeOutParameterName,
                optionConfigValues,
                parameterId => { AuthenticationWaitTimeOut = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.AuthorizationWaitRetryParameterName,
                optionConfigValues,
                parameterId => { AuthenticationWaitRetries = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.DownloadEnabledParameterName,
                optionConfigValues,
                parameterId => { DownloadEnabled = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.UploadEnabledParameterName,
                optionConfigValues,
                parameterId => { UploadEnabled = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.ScriptingEnabledParameterName,
                optionConfigValues,
                parameterId => { ScriptingEnabled = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.ProtocolListSupportParameterName,
                optionConfigValues,
                parameterId => { ProtocolListSupport = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.TransferProgressFrequencyParameterName,
                optionConfigValues,
                parameterId => { TransferProgressFrequency = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.PauseSupportedParameterName,
                optionConfigValues,
                parameterId => { PauseSupported = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.DownloadDevice.AbortTransferSupportedParameterName,
                optionConfigValues,
                parameterId => { AbortTransferSupported = optionConfigValues.BooleanValue(parameterId); });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            //// TODO: trigger G2S_DLE001 and G2S_DLE002
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE004);
            //// TODO: G2S_DLE005 pending optionChangeStatus command impl
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE005);
            //// TODO: G2S_DLE006 pending operator config impl
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE006);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE007);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE008);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE108);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE109);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE110);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE120);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE121);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE122);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE123);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE124);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE125);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE140);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE201);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE202);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE203);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE204);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE205);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE206);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE207);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE208);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE209);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE210);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE211);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE212);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE213);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE301);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE302);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE303);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE304);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_DLE305);

            base.RegisterEvents();
        }

        /// <inheritdoc />
        public async Task SendStatus(c_baseCommand command, Action hostAcknowledgedCallBack = null)
        {
            await SendStatus(command, default(DateTime), hostAcknowledgedCallBack);
        }

        /// <inheritdoc />
        public async Task SendStatus(c_baseCommand command, DateTime endTime, Action hostAcknowledgedCallBack = null)
        {
            if (endTime != default(DateTime) && endTime <= DateTime.UtcNow)
            {
                return;
            }

            if (endTime == default(DateTime) && NoResponseTimer > TimeSpan.Zero)
            {
                endTime = DateTime.UtcNow.Add(NoResponseTimer);
            }

            var request = InternalCreateClass();
            request.Item = command;

            var session = SendRequest(request);
            session.WaitForCompletion();
            if (session.SessionState != SessionStatus.Success)
            {
                if (_sendStatusCancellationToken == null)
                {
                    _sendStatusCancellationToken = new CancellationTokenSource();
                }

                if (session.SessionState == SessionStatus.TimedOut)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"DownloadDevice.SendStatus : c_baseCommand Timed Out.  Will try again
    Device Id : {0}",
                        Id);

                    await Task.Run(
                        async () => await SendStatus(command, endTime, hostAcknowledgedCallBack),
                        _sendStatusCancellationToken.Token);
                }
                else
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"DownloadDevice.SendStatus : c_baseCommand Failed.  Will try again
    Device Id : {0}",
                        Id);

                    await Task.Delay(TimeToLive, _sendStatusCancellationToken.Token)
                        .ContinueWith(
                            async task =>
                            {
                                if (!task.IsCanceled)
                                {
                                    await SendStatus(command, endTime, hostAcknowledgedCallBack);
                                }
                            });
                }
            }
            else
            {
                hostAcknowledgedCallBack?.Invoke();
            }
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
            NoResponseTimer = TimeSpan.FromMilliseconds(DefaultNoResponseTimer);

            AbortTransferSupported = DefaultAbortTransferSupported;
            AuthenticationWaitRetries = DefaultAuthorizationWaitRetries;
            AuthenticationWaitTimeOut = DefaultAuthorizationWaitTimeOut;
            MinPackageListEntries = DefaultMinPackageListEntries;
            MinPackageLogEntries = DefaultMinPackageLogEntries;
            MinScriptListEntries = DefaultMinScriptListEntries;
            MinScriptLogEntries = DefaultMinScriptLogEntries;
            NoMessageTimer = DefaultNoMessageTimer;
            PauseSupported = DefaultPauseSupported;
            ProtocolListSupport = DefaultProtocolListSupport;
            RestartStatus = DefaultRestartStatus;
            ScriptingEnabled = DefaultScriptingEnabled;
            TransferProgressFrequency = DefaultTransferProgressFrequency;
            UploadEnabled = DefaultUploadEnabled;
        }
    }
}