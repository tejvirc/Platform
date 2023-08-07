namespace Aristocrat.Monaco.G2S.Handlers.IdReader
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Session;
    using Hardware.Contracts.IdReader;
    using log4net;

    /// <summary>
    ///     If an ID is present, the ID MUST be cleared from the device and event
    ///     G2S_IDE103 ID Cleared From Reader MUST be generated; otherwise unchanged.
    /// </summary>
    [ProhibitWhenDisabled]
    public class SetIdValidation : ICommandHandler<idReader, setIdValidation>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IIdReaderProvider _idReaderProvider;
        private readonly IPlayerService _playerService;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        
        public SetIdValidation(
            IG2SEgm egm,
            IEventLift eventLift,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> commandBuilder,
            IPlayerService playerService,
            IIdReaderProvider idReaderProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
            _idReaderProvider = idReaderProvider ?? throw new ArgumentNullException(nameof(idReaderProvider));
        }

        public async Task<Error> Verify(ClassCommand<idReader, setIdValidation> command)
        {
            if (!command.Command.IsValid())
            {
                // Note : Likely to never encounter this with the front-end XML schema checking we do.
                // We can also set the invalid data error in the 'Handle' command below and include it with the error text.
                return new Error(ErrorCode.G2S_APX009, "");
            }

            return await Sanction.OnlyOwner<IIdReaderDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<idReader, setIdValidation> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<IIdReaderDevice>(command.IClass.deviceId);
            
            var readerAdapter = _idReaderProvider.Adapters.Single(id => id.IdReaderId == command.IClass.deviceId);
            
            var idReader = _idReaderProvider[device.Id];

            var newIdDetails = command.Command;

            if (idReader.IsEgmControlled && newIdDetails.idNumber != readerAdapter.Identity?.Number)
            {
                // Per G2S 3.0 spec, only update status on an EGM controlled device if the IDs match.
                // I.E. Do not set, update, or clear an ID if the numbers don't match.
                // Still send a status-without-update regardless.
                var unUpdatedResponse = command.GenerateResponse<idReaderStatus>();
                await _commandBuilder.Build(device, unUpdatedResponse.Command);
                return;
            }

            if (newIdDetails.idType == Constants.None)
            {
                await ClearReaderAndIdentity(command, device);
            }
            else if (readerAdapter?.Identity == null)
            {
                await HandleWhenNoIdIsCurrentlySet(command, device, newIdDetails, readerAdapter);
            }
            else
            {
                await HandleWhenIdIsCurrentlySet(command, readerAdapter, newIdDetails, device);
            }
        }

        private async Task HandleWhenIdIsCurrentlySet(
            ClassCommand<idReader, setIdValidation> command,
            IIdReader readerAdapter,
            setIdValidation newIdDetails,
            IIdReaderDevice device)
        {
            var originalIdNumber = readerAdapter.Identity.Number;
            var originalIdType = readerAdapter.Identity.Type;
            var originalIdState = readerAdapter.Identity.State;
            var originalValidationExpired = readerAdapter.Identity.ValidationExpired;
            var originalPlayerId = readerAdapter.Identity.PlayerId;
            var idNeededClearing = false;

            // Check for Number / Type / State Change - clear ID if changed
            if (originalIdNumber != newIdDetails.idNumber ||
                readerAdapter.Identity.Type != newIdDetails.idType.ToIdType() ||
                readerAdapter.Identity.State != (IdStates)newIdDetails.idState)
            {
                _idReaderProvider.SetIdValidation(device.Id, null);
                idNeededClearing = true;
            }

            // Lock-up an expired ID - if Expired is ever set to True,
            // it will remain true until the ID is cleared.
            if (!idNeededClearing)
            {
                if (originalValidationExpired)
                {
                    // Never allow the value to be reset to false per G2S 3.0 spec.
                    newIdDetails.idValidExpired = true;
                }
                else if (newIdDetails.idValidExpired)
                {
                    Logger.Info($"idValidExpired is true, posting {EventCode.G2S_IDE107} event");
                    PostEventReport(device, EventCode.G2S_IDE107);
                }

                // Always send the 102 Invalid event.
                // G2S 3.0 spec does not care if the status gets updated or not.
                if (newIdDetails.idType == "G2S_invalid")
                {
                    PostEventReport(device, EventCode.G2S_IDE102);
                }
            }

            var identityFromHost = ProcessValidationDifferences(newIdDetails, readerAdapter.Identity);

            if (identityFromHost != null)
            {
                // No Change - merely report unchanged status 
                await ReportUnchangedStatus(
                    device,
                    identityFromHost,
                    originalIdNumber,
                    originalPlayerId,
                    originalIdType,
                    originalIdState);
            }

            await _commandBuilder.Build(device, command.GenerateResponse<idReaderStatus>().Command);
        }

        private async Task ReportUnchangedStatus(
            IIdReaderDevice device,
            Identity newIdentity,
            string originalIdNumber,
            string originalPlayerId,
            IdTypes originalIdType,
            IdStates originalIdState)
        {
            _idReaderProvider.SetIdValidation(device.Id, newIdentity);
            var updateIdStatus = new idReaderStatus();
            await _commandBuilder.Build(device, updateIdStatus);

            if (originalIdNumber == newIdentity.Number)
            {
                PostEventReport(device, EventCode.G2S_IDE106, updateIdStatus);

                if (originalPlayerId != newIdentity.PlayerId && originalIdType == newIdentity.Type &&
                    originalIdState == newIdentity.State)
                {
                    _playerService.SetSessionParameters(
                        newIdentity); // Update the active player session with the new player identity
                }
            }
        }

        private async Task HandleWhenNoIdIsCurrentlySet(
            ClassCommand<idReader, setIdValidation> command,
            IIdReaderDevice device,
            setIdValidation newIdDetails,
            IIdReader readerAdapter)
        {
            var identity = ProcessValidationDifferences(newIdDetails, readerAdapter.Identity);

            _idReaderProvider.SetIdValidation(device.Id, identity);

            var newIdStatus = command.GenerateResponse<idReaderStatus>();

            await _commandBuilder.Build(device, newIdStatus.Command);

            if (newIdDetails.idValidExpired)
            {
                Logger.Info($"HandleWhenNoIdIsCurrentlySet: Command was unsuccessful, posting {EventCode.G2S_IDE107} event");
                PostEventReport(device, EventCode.G2S_IDE107);
            }
        }

        private async Task ClearReaderAndIdentity(
            ClassCommand<idReader, setIdValidation> command,
            IIdReaderDevice device)
        {
            _idReaderProvider.SetIdValidation(device.Id, null);
            var clearIdStatus = command.GenerateResponse<idReaderStatus>();
            await _commandBuilder.Build(device, clearIdStatus.Command);
        }

        private void PostEventReport(IIdReaderDevice device, string eventCode, idReaderStatus status = null)
        {
            if (status == null)
            {
                status = new idReaderStatus();
                _commandBuilder.Build(device, status);
            }

            _eventLift.Report(device, eventCode, device.DeviceList(status));
        }

        private Identity ProcessValidationDifferences(setIdValidation newIdDetails, Identity currentIdentity)
        {
            currentIdentity = currentIdentity ?? new Identity();

            var identityWasChanged = false;

            if (currentIdentity.Age != newIdDetails.idAge)
            {
                currentIdentity.Age = newIdDetails.idAge;
                identityWasChanged = true;
            }
            if (currentIdentity.Classification != newIdDetails.idClass)
            {
                currentIdentity.Classification = newIdDetails.idClass;
                identityWasChanged = true;
            }
            if (currentIdentity.DisplayMessages != newIdDetails.idDisplayMessages)
            {
                currentIdentity.DisplayMessages = newIdDetails.idDisplayMessages;
                identityWasChanged = true;
            }
            if (currentIdentity.FullName != newIdDetails.idFullName)
            {
                currentIdentity.FullName = newIdDetails.idFullName;
                identityWasChanged = true;
            }
            var newIdGender = (IdGenders)newIdDetails.idGender;
            if (currentIdentity.Gender != newIdGender)
            {
                currentIdentity.Gender = newIdGender;
                identityWasChanged = true;
            }
            if (currentIdentity.IsAnniversary != newIdDetails.idAnniversary)
            {
                currentIdentity.IsAnniversary = newIdDetails.idAnniversary;
                identityWasChanged = true;
            }
            if (currentIdentity.IsBanned != newIdDetails.idBanned)
            {
                currentIdentity.IsBanned = newIdDetails.idBanned;
                identityWasChanged = true;
            }
            if (currentIdentity.IsBirthday != newIdDetails.idBirthday)
            {
                currentIdentity.IsBirthday = newIdDetails.idBirthday;
                identityWasChanged = true;
            }
            if (currentIdentity.IsVip != newIdDetails.idVIP)
            {
                currentIdentity.IsVip = newIdDetails.idVIP;
                identityWasChanged = true;
            }
            if (currentIdentity.LocaleId != newIdDetails.localeId)
            {
                currentIdentity.LocaleId = newIdDetails.localeId;
                identityWasChanged = true;
            }
            if (currentIdentity.Number != newIdDetails.idNumber)
            {
                currentIdentity.Number = newIdDetails.idNumber;
                identityWasChanged = true;
            }
            if (currentIdentity.PlayerId != newIdDetails.playerId)
            {
                currentIdentity.PlayerId = newIdDetails.playerId;
                identityWasChanged = true;
            }
            if (currentIdentity.PlayerRank != newIdDetails.idRank)
            {
                currentIdentity.PlayerRank = newIdDetails.idRank;
                identityWasChanged = true;
            }
            if (currentIdentity.PreferredName != newIdDetails.idPreferName)
            {
                currentIdentity.PreferredName = newIdDetails.idPreferName;
                identityWasChanged = true;
            }

            if (currentIdentity.PrivacyRequested != newIdDetails.idPrivacy)
            {
                currentIdentity.PrivacyRequested = newIdDetails.idPrivacy;
                identityWasChanged = true;
            }

            var newIdState = (IdStates)newIdDetails.idState;
            if (currentIdentity.State != newIdState)
            {
                currentIdentity.State = newIdState;
                identityWasChanged = true;
            }

            if (currentIdentity.Type != newIdDetails.idType.ToIdType())
            {
                currentIdentity.Type = newIdDetails.idType.ToIdType();
                identityWasChanged = true;
            }

            if (currentIdentity.ValidationExpired != newIdDetails.idValidExpired)
            {
                currentIdentity.ValidationExpired = newIdDetails.idValidExpired;
                identityWasChanged = true;
            }

            var newIdValidationSource = (IdValidationSources)newIdDetails.idValidSource;
            if (currentIdentity.ValidationSource != newIdValidationSource)
            {
                currentIdentity.ValidationSource = newIdValidationSource;
                identityWasChanged = true;
            }

            if (newIdDetails.idValidDateTimeSpecified)
            {
                if (currentIdentity.ValidationTime != newIdDetails.idValidDateTime)
                {
                    currentIdentity.ValidationTime = newIdDetails.idValidDateTime;
                    identityWasChanged = true;
                }
            }
            else
            {
                if (currentIdentity.ValidationTime != null)
                {
                    currentIdentity.ValidationTime = null;
                    identityWasChanged = true;
                }
            }

            return identityWasChanged ? currentIdentity : null;
        }
    }
}
