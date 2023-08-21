namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getDownloadProfile G2S message
    /// </summary>
    public class GetDownloadProfile : ICommandHandler<download, getDownloadProfile>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetDownloadProfile" /> class.
        ///     Creates a new instance of the GetDownloadProfile handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public GetDownloadProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getDownloadProfile> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getDownloadProfile> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var downloadDevice = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (downloadDevice == null)
                {
                    return;
                }

                var response = command.GenerateResponse<downloadProfile>();
                var profile = response.Command;

                profile.useDefaultConfig = downloadDevice.UseDefaultConfig;
                profile.abortTransferSupported = downloadDevice.AbortTransferSupported;
                if (downloadDevice.AuthenticationWaitRetries > 0)
                {
                    profile.authWaitRetries = downloadDevice.AuthenticationWaitRetries;
                }

                profile.authWaitTimeOut = downloadDevice.AuthenticationWaitTimeOut;
                profile.configComplete = downloadDevice.ConfigComplete;

                if (downloadDevice.ConfigDateTime != default(DateTime))
                {
                    profile.configDateTimeSpecified = true;
                    profile.configDateTime = downloadDevice.ConfigDateTime;
                }

                profile.configurationId = downloadDevice.ConfigurationId;
                profile.downloadEnabled = downloadDevice.DownloadEnabled;

                if (downloadDevice.MinPackageListEntries > 0)
                {
                    profile.minPackageListEntries = downloadDevice.MinPackageListEntries;
                }

                if (downloadDevice.MinPackageLogEntries > 0)
                {
                    profile.minPackageLogEntries = downloadDevice.MinPackageLogEntries;
                }

                if (downloadDevice.MinScriptListEntries > 0)
                {
                    profile.minScriptListEntries = downloadDevice.MinScriptListEntries;
                }

                if (downloadDevice.MinScriptLogEntries > 0)
                {
                    profile.minScriptLogEntries = downloadDevice.MinScriptLogEntries;
                }

                profile.noMessageTimer = downloadDevice.NoMessageTimer;
                profile.pauseSupported = downloadDevice.PauseSupported;
                profile.protocolListSupport = downloadDevice.ProtocolListSupport;
                profile.requiredForPlay = downloadDevice.RequiredForPlay;
                profile.restartStatus = downloadDevice.RestartStatus;
                profile.scriptingEnabled = downloadDevice.ScriptingEnabled;
                profile.transferProgressFreq = downloadDevice.TransferProgressFrequency;
                profile.noResponseTimer = (int)downloadDevice.NoResponseTimer.TotalMilliseconds;
                profile.timeToLive = downloadDevice.TimeToLive;
            }

            await Task.CompletedTask;
        }
    }
}