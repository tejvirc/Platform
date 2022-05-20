namespace Aristocrat.Monaco.G2S.Handlers.IdReader
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts.IdReader;

    public class IdReaderProfileCommandBuilder : ICommandBuilder<IIdReaderDevice, idReaderProfile>
    {
        private readonly IIdReaderProvider _provider;

        public IdReaderProfileCommandBuilder(IIdReaderProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <inheritdoc />
        public async Task Build(IIdReaderDevice device, idReaderProfile command)
        {
            var idReader = _provider[device.Id];

            command.configurationId = device.ConfigurationId;
            command.restartStatus = device.RestartStatus;
            command.useDefaultConfig = device.UseDefaultConfig;
            command.requiredForPlay = device.RequiredForPlay;

            command.egmPhysicallyControls = idReader.IsEgmControlled;
            command.idReaderType = idReader.IdReaderType.ToReaderType();
            command.idReaderTrack = idReader.IdReaderTrack.ToTrackId();
            command.idValidMethod = idReader.ValidationMethod.ToValidationMethod();
            command.timeToLive = device.TimeToLive;
            command.waitTimeOut = idReader.WaitTimeout;
            command.offLineValid = idReader.SupportsOfflineValidation;
            command.validTimeOut = idReader.ValidationTimeout;
            command.removalDelay = device.RemovalDelay;
            command.attractMsg = device.AttractMsg;
            command.waitMsg = device.WaitMsg;
            command.validMsg = device.ValidMsg;
            command.invalidMsg = device.InvalidMsg;
            command.lostMsg = device.LostMsg;
            command.offLineMsg = device.OffLineMsg;
            command.abandonMsg = device.AbandonMsg;
            command.msgDuration = device.MsgDuration;
            command.idEncoding = "G2S_unknown";
            command.noPlayerMessages = device.NoPlayerMessages;
            command.multiLingualSupport = false;
            command.idTypeProfile = idReader.Patterns
                .Select(p => new idTypeProfile { idType = p.IdType, offLinePattern = p.OfflinePattern }).ToArray();

            command.configComplete = device.ConfigComplete;
            command.configDateTime = device.ConfigDateTime;

            await Task.CompletedTask;
        }
    }
}
