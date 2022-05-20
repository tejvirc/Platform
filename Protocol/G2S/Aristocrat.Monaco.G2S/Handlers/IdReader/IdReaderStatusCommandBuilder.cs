namespace Aristocrat.Monaco.G2S.Handlers.IdReader
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts.IdReader;

    public class IdReaderStatusCommandBuilder : ICommandBuilder<IIdReaderDevice, idReaderStatus>
    {
        private const t_idSources DefaultIdValidSource = t_idSources.G2S_none;
        private const t_idStates DefaultIdState = t_idStates.G2S_inactive;
        private const string DefaultIdPreferName = "";
        private const string DefaultIdFullName = "";
        private const string DefaultIdClass = "";
        private const string DefaultPlayerId = "";
        private const bool DefaultIdValidExpired = true;
        private const bool DefaultIdVip = false;
        private const bool DefaultIdBirthday = false;
        private const bool DefaultIdAnniversary = false;
        private const bool DefaultIdBanned = false;
        private const bool DefaultIdPrivacy = false;
        private const t_idGenders DefaultIdGender = t_idGenders.G2S_Unknown;
        private const int DefaultIdRank = 0;
        private const int DefaultIdAge = 0;
        private const bool DefaultIdDisplayMessages = true;
        private readonly IIdReaderProvider _idReaderProvider;
        private readonly ILocalization _localeProvider;

        public IdReaderStatusCommandBuilder(ILocalization localeProvider, IIdReaderProvider idReaderProvider)
        {
            _localeProvider = localeProvider ?? throw new ArgumentNullException(nameof(localeProvider));
            _idReaderProvider = idReaderProvider ?? throw new ArgumentNullException(nameof(idReaderProvider));
        }

        /// <inheritdoc />
        public Task Build(IIdReaderDevice device, idReaderStatus command)
        {
            var idReader = _idReaderProvider?[device.Id];

            if (idReader == null)
            {
                return Task.CompletedTask;
            }

            command.configurationId = device.ConfigurationId;
            // VLT-6094: report device enabled if connected
            command.egmEnabled = idReader.IsImplementationEnabled;
            command.hostEnabled = device.HostEnabled;

            // disconnected to illegalActivity not supported
            command.disconnected = !idReader.Connected;
            command.firmwareFault = false;
            command.mechanicalFault = false;
            command.mechanicalFault = false;
            command.opticalFault = false;
            command.componentFault = false;
            command.nvMemoryFault = false;
            command.illegalActivity = false;

            if (idReader.Identity != null)
            {
                command.idNumber = idReader.Identity.Number;
                command.idType = idReader.Identity.Type.ToIdType();
                command.idValidDateTime = idReader.Identity.ValidationTime?.ToUniversalTime() ??
                                          DateTime.MinValue.ToUniversalTime();
                command.idValidDateTimeSpecified = idReader.Identity.ValidationTime != null;
                command.idValidSource = (t_idSources)idReader.Identity.ValidationSource;
                command.idState = (t_idStates)idReader.Identity.State;
                command.idPreferName = idReader.Identity.PreferredName;
                command.idFullName = idReader.Identity.FullName;
                command.idClass = idReader.Identity.Classification;
                command.localeId = idReader.Identity.LocaleId;
                command.playerId = idReader.Identity.PlayerId;
                command.idValidExpired = idReader.Identity.ValidationExpired;
                command.idVIP = idReader.Identity.IsVip;
                command.idBirthday = idReader.Identity.IsBirthday;
                command.idAnniversary = idReader.Identity.IsAnniversary;
                command.idBanned = idReader.Identity.IsBanned;
                command.idPrivacy = idReader.Identity.PrivacyRequested;
                command.idGender = (t_idGenders)idReader.Identity.Gender;
                command.idRank = idReader.Identity.PlayerRank;
                command.idAge = idReader.Identity.Age;
                command.idDisplayMessages = idReader.Identity.DisplayMessages;
            }
            else
            {
                command.idValidDateTime = idReader.TimeOfLastIdentityHandled.ToUniversalTime();
                command.idValidDateTimeSpecified = true;
                command.idValidSource = DefaultIdValidSource;
                command.idState = DefaultIdState;
                command.idPreferName = DefaultIdPreferName;
                command.idFullName = DefaultIdFullName;
                command.idClass = DefaultIdClass;
                command.localeId = device.LocaleId(_localeProvider?.CurrentCulture);
                command.playerId = DefaultPlayerId;
                command.idValidExpired = DefaultIdValidExpired;
                command.idVIP = DefaultIdVip;
                command.idBirthday = DefaultIdBirthday;
                command.idAnniversary = DefaultIdAnniversary;
                command.idBanned = DefaultIdBanned;
                command.idPrivacy = DefaultIdPrivacy;
                command.idGender = DefaultIdGender;
                command.idRank = DefaultIdRank;
                command.idAge = DefaultIdAge;
                command.idDisplayMessages = DefaultIdDisplayMessages;
            }

            command.configComplete = device.ConfigComplete;
            command.configDateTime = device.ConfigDateTime;

            return Task.CompletedTask;
        }
    }
}
