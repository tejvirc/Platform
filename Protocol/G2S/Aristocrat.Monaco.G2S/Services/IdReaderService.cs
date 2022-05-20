namespace Aristocrat.Monaco.G2S.Services
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Data.Profile;
    using Handlers;
    using Handlers.IdReader;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using log4net;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    internal class IdReaderService : IDisposable
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IIdReaderProvider _idProvider;
        private readonly IProfileService _profileService;
        private readonly object _sync = new object();
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private (int idReaderId, string idNumber) _lastVerification = (0, null);
        private bool _firstDisableEvent = true;
        private bool _disposed;

        /// <summary>
        ///     Instantiate the logical handler of ID reader data.
        ///     <see cref="IdReaderService" />
        /// </summary>
        public IdReaderService(
            IG2SEgm egm,
            IEventLift eventLift,
            IEventBus eventBus,
            IIdReaderProvider idProvider,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> command,
            IProfileService profileService
        )
        {
            _commandBuilder = command ?? throw new ArgumentNullException(nameof(command));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            
            _eventBus.Subscribe<ValidationRequestedEvent>(this, Handle);
            _eventBus.Subscribe<IdClearedEvent>(this, Handle);
            _eventBus.Subscribe<DisabledEvent>(this, Handle);
            _eventBus.Subscribe<CommunicationsStateChangedEvent>(this, Handle);

            foreach (var reader in _idProvider.Adapters.Where(a => a.IsEgmControlled))
            {
                if (!string.IsNullOrEmpty(reader.CardData))
                {
                    _lastVerification = (reader.IdReaderId, reader.CardData);
                    break;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void Handle(CommunicationsStateChangedEvent theEvent)
        {
            if (_lastVerification.idNumber != null)
            {
                var device = _egm.GetDevice<IIdReaderDevice>(_lastVerification.idReaderId);

                if (theEvent.HostId != device?.Owner)
                {
                    return;
                }

                if (theEvent.Online)
                {
                    Task.Run(() => ValidateId(_lastVerification.idReaderId, _lastVerification.idNumber));
                }
            }
        }

        private void Handle(DisabledEvent theEvent)
        {
            ClearId(theEvent.IdReaderId);

            // This is called here because the ID Reader is initially disabled on startup before being enabled
            // At this point the IdReaderDisconnectedConsumer has been initialized so G2S event will be sent
            if (_firstDisableEvent)
            {
                // If ID Reader is required for play but is disconnected, display the Disconnected error on startup
                var device = new IdReaderDevice(1, null);
                _profileService.Populate(device);
                var reader = _idProvider.Adapters.FirstOrDefault(r => r.IdReaderId == device.Id);

                if (device.RequiredForPlay && reader != null && reader.LogicalState == IdReaderLogicalState.Disconnected)
                {
                    _eventBus.Publish(new DisconnectedEvent(device.Id));
                }

                _firstDisableEvent = false;
            }
        }

        private void Handle(ValidationRequestedEvent theEvent)
        {
            _lastVerification = (theEvent.IdReaderId, theEvent.TrackData?.IdNumber);
            Task.Run(() => ValidateId(theEvent.IdReaderId, theEvent.TrackData?.IdNumber));
        }

        private void Handle(IdClearedEvent theEvent)
        {
            Task.Run(() => ClearId(theEvent.IdReaderId));
        }

        private void ValidateId(int idReaderId, string idNumber)
        {
            lock(_sync)
            {
                var currentIdentity = _idProvider.GetCurrentIdentity();

                if (currentIdentity != null && currentIdentity.Type != IdTypes.None)
                {
                    if (currentIdentity.Number == idNumber)
                    {
                        // ID needs to be set again to return to Validated state
                        // This occurs when a card is inserted within the timeout window of removing the card
                        SetIdValidation(idReaderId, idNumber);
                        return;
                    }

                    ClearId(idReaderId);
                }

                if (currentIdentity?.Number != idNumber)
                {
                    SetIdValidation(idReaderId, idNumber);
                }
            }
        }

        private void SetIdValidation(int idReaderId, string idNumber)
        {
            var setIdValidation = GetIdValidation(idReaderId, idNumber);

            if (setIdValidation == null)
                return;

            _lastVerification = (0, null);
            _idProvider?.SetIdValidation(idReaderId, setIdValidation.ToIdentity());
        }

        private setIdValidation GetIdValidation(int idReaderId, string idNumberAsString)
        {
            var idReader = _idProvider[idReaderId];

            var idDevice = _egm.GetDevice<IIdReaderDevice>(idReaderId);
            var timeout = idReader.WaitTimeout == 0
                ? TimeSpan.MaxValue
                : TimeSpan.FromMilliseconds(idReader.WaitTimeout);
            var now = DateTime.UtcNow;
            
            var status = new idReaderStatus();

            var idCommand = new getIdValidation();

            try
            {
                idCommand.idNumber = idNumberAsString;
            }
            catch(ArgumentOutOfRangeException e)
            {
                Logger.Info($"invalid id, exception: {e.Message} ,posting {EventCode.G2S_IDE102} event");
                _commandBuilder.Build(idDevice, status);
                _eventLift.Report(idDevice, EventCode.G2S_IDE102, idDevice.DeviceList(status));
                return null;
            }

            var idData = idDevice?.GetIdValidation(
                idCommand,
                timeout,
                idReader.SupportsOfflineValidation,
                idReader.Patterns.Select(p => (p.IdType, p.OfflinePattern)).ToList()).Result;

            _commandBuilder.Build(idDevice, status);

            // Command was unsuccessful
            if (idData == null)
            {
                if (DateTime.UtcNow - now > timeout)
                {
                    Logger.Info($"Command was unsuccessful, posting {EventCode.G2S_IDE107} event");
                    _eventLift.Report(idDevice, EventCode.G2S_IDE107, idDevice.DeviceList(status));
                }

                return null;
            }

            // Bad data received
            if (!idData.IsValid())
            {
                _eventLift.Report(idDevice, EventCode.G2S_IDE108, idDevice.DeviceList(status));
                // Required to behave as if the ID was cleared from the device
                _eventLift.Report(idDevice, EventCode.G2S_IDE103, idDevice.DeviceList(status));
                return null;
            }

            // Offline validation failed - a default was returned
            if (idData.idType == Constants.None && idData.idValidSource == t_idSources.G2S_none &&
                idData.idState == t_idStates.G2S_inactive && idData.idValidDateTime == DateTime.MinValue)
            {
                _eventLift.Report(idDevice, EventCode.G2S_IDE105, idDevice.DeviceList(status));
                return null;
            }

            if (idData.idValidSource == t_idSources.G2S_offLine)
            {
                _eventLift.Report(idDevice, EventCode.G2S_IDE104, idDevice.DeviceList(status));
            }

            return idData;
        }

        private void ClearId(int idReaderId)
        {
            lock (_sync)
            {
                var device = _egm.GetDevice<IIdReaderDevice>(idReaderId);
                device?.CancelGetIdValidation();

                if (_idProvider?.GetCurrentIdentity() != null)
                {
                    _idProvider?.SetIdValidation(idReaderId, null);
                }

                _lastVerification = (0, null);
            }
        }
    }
}
