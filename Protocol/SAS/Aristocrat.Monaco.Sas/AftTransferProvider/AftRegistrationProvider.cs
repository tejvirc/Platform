namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Stateless;
    using Storage.Models;
    using Storage.Repository;

    /// <summary>Definition of the AftRegisteredProvider class.</summary>
    public sealed class AftRegistrationProvider : IAftRegistrationProvider
    {
        private readonly IStorageDataProvider<AftRegistration> _registrationDataProvider;
        private readonly IPropertiesManager _propertiesManager;

        private readonly StateMachine<AftRegistrationStatus, AftRegistrationCode> _aftRegistrationStatus;

        /// <summary>Constructs the AftRegisteredProvider object</summary>
        public AftRegistrationProvider(
            IStorageDataProvider<AftRegistration> registrationDataProvider,
            ISasExceptionHandler exceptionHandler,
            IPropertiesManager propertiesManager)
        {
            if (exceptionHandler == null)
            {
                throw new ArgumentNullException(nameof(exceptionHandler));
            }

            _registrationDataProvider = registrationDataProvider ?? throw new ArgumentNullException(nameof(registrationDataProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            ZeroRegistrationKey = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            AftRegistrationKey = ZeroRegistrationKey;
            var aftRegistration = _registrationDataProvider.GetData();

            _aftRegistrationStatus =
                new StateMachine<AftRegistrationStatus, AftRegistrationCode>(aftRegistration.RegistrationStatus);
            AftRegistrationKey = aftRegistration.AftRegistrationKey;
            PosId = (uint)aftRegistration.PosId;

            //  Any                 -> NotRegistered
            //  NotRegistered       -> RegistrationReady
            //  RegistrationReady   -> Registered
            //  RegistrationReady   -> RegistrationPending
            //  RegistrationPending -> RegistrationReady
            _aftRegistrationStatus.Configure(AftRegistrationStatus.RegistrationReady)
                .Ignore(AftRegistrationCode.InitializeRegistration)
                .Permit(AftRegistrationCode.RegisterGamingMachine, AftRegistrationStatus.Registered)
                .Permit(AftRegistrationCode.RequestOperatorAcknowledgement, AftRegistrationStatus.RegistrationPending)
                .Permit(AftRegistrationCode.UnregisterGamingMachine, AftRegistrationStatus.NotRegistered)
                .Ignore(AftRegistrationCode.ReadCurrentRegistration);

            _aftRegistrationStatus.Configure(AftRegistrationStatus.Registered)
                .Permit(AftRegistrationCode.InitializeRegistration, AftRegistrationStatus.RegistrationReady)
                .Permit(AftRegistrationCode.RegisterGamingMachine, AftRegistrationStatus.NotRegistered)
                .Permit(AftRegistrationCode.RequestOperatorAcknowledgement, AftRegistrationStatus.NotRegistered)
                .Permit(AftRegistrationCode.UnregisterGamingMachine, AftRegistrationStatus.NotRegistered)
                .Ignore(AftRegistrationCode.ReadCurrentRegistration);

            _aftRegistrationStatus.Configure(AftRegistrationStatus.RegistrationPending)
                .OnEntry(
                    () => exceptionHandler.ReportException(
                        new GenericExceptionBuilder(GeneralExceptionCode.AftRegistrationAcknowledged)))
                .Permit(AftRegistrationCode.InitializeRegistration, AftRegistrationStatus.NotRegistered)
                .Permit(AftRegistrationCode.RegisterGamingMachine, AftRegistrationStatus.NotRegistered)
                .Permit(AftRegistrationCode.RequestOperatorAcknowledgement, AftRegistrationStatus.RegistrationReady)
                .Permit(AftRegistrationCode.UnregisterGamingMachine, AftRegistrationStatus.NotRegistered)
                .Ignore(AftRegistrationCode.ReadCurrentRegistration);

            _aftRegistrationStatus.Configure(AftRegistrationStatus.NotRegistered)
                .OnEntry(
                    () => exceptionHandler.ReportException(
                        new GenericExceptionBuilder(GeneralExceptionCode.AftRegistrationCanceled)))
                .Permit(AftRegistrationCode.InitializeRegistration, AftRegistrationStatus.RegistrationReady)
                .PermitReentry(AftRegistrationCode.RegisterGamingMachine)
                .PermitReentry(AftRegistrationCode.RequestOperatorAcknowledgement)
                .Ignore(AftRegistrationCode.UnregisterGamingMachine)
                .Ignore(AftRegistrationCode.ReadCurrentRegistration);
        }

        /// <inheritdoc />
        public bool IsAftRegistered => AftRegistrationStatus == AftRegistrationStatus.Registered &&
                                       AssetNumber() != 0 &&
                                       _registrationDataProvider.GetData().AftRegistrationKey.Any(x => x != 0);

        /// <inheritdoc />
        public bool IsAftDebitTransferEnabled => IsAftRegistered &&
                                                 _propertiesManager.GetValue(
                                                     SasProperties.SasFeatureSettings,
                                                     new SasFeatures()).DebitTransfersAllowed &&
                                                 _registrationDataProvider.GetData().PosId != 0;

        /// <inheritdoc />
        public byte[] ZeroRegistrationKey { get; }

        /// <inheritdoc />
        public AftRegistrationStatus AftRegistrationStatus => _aftRegistrationStatus.State;

        /// <inheritdoc />
        public byte[] AftRegistrationKey { get; private set; }

        /// <inheritdoc />
        public uint PosId { get; private set; }

        /// <inheritdoc />
        public void ProcessAftRegistration(
            AftRegistrationCode registrationCode,
            uint assetNumber,
            byte[] registrationKey,
            uint posId)
        {
            var fireRegistrationCode = true;
            if (AssetNumberIsValid(assetNumber))
            {
                switch (registrationCode)
                {
                    case AftRegistrationCode.InitializeRegistration:
                        InitializeRegistration(registrationKey, posId);
                        break;
                    case AftRegistrationCode.RegisterGamingMachine:
                        fireRegistrationCode = RegisterGamingMachine(registrationKey, posId);
                        break;
                }
            }
            else
            {
                switch (registrationCode)
                {
                    case AftRegistrationCode.InitializeRegistration:
                    case AftRegistrationCode.RegisterGamingMachine:
                    case AftRegistrationCode.RequestOperatorAcknowledgement:
                        ForceAftNotRegistered();
                        fireRegistrationCode = false;
                        break;
                }
            }

            if (fireRegistrationCode)
            {
                _aftRegistrationStatus.Fire(registrationCode);
            }

            UpdatePersistence(AftRegistrationStatus).FireAndForget();
        }

        /// <inheritdoc />
        public void AftRegistrationCycleInterrupted()
        {
            if (AftRegistrationStatus != AftRegistrationStatus.Registered)
            {
                ForceAftNotRegistered();
            }
        }

        /// <inheritdoc />
        public void ForceAftNotRegistered()
        {
            ProcessAftRegistration(AftRegistrationCode.UnregisterGamingMachine, 0, ZeroRegistrationKey, 0);
        }

        /// <inheritdoc />
        public bool RegistrationKeyMatches(byte[] checkRegistrationKey)
        {
            return checkRegistrationKey.SequenceEqual(_registrationDataProvider.GetData().AftRegistrationKey);
        }

        /// <inheritdoc />
        public bool RegistrationKeyMatches(byte[] registrationKeyLeft, byte[] registrationKeyRight)
        {
            return registrationKeyLeft.SequenceEqual(registrationKeyRight);
        }

        private void InitializeRegistration(byte[] registrationKey, uint posId)
        {
            // Registration key may be deleted by initialize
            if (RegistrationKeyMatches(registrationKey, ZeroRegistrationKey))
            {
                AftRegistrationKey = ZeroRegistrationKey;
            }

            // POS ID may be deleted by initialize
            if (posId == 0)
            {
                PosId = 0;
            }
        }

        private bool RegisterGamingMachine(byte[] registrationKey, uint posId)
        {
            // Registration key must be nonzero.  Zero POS ID is ok.
            if (RegistrationKeyMatches(registrationKey, ZeroRegistrationKey))
            {
                ForceAftNotRegistered();
                return false;
            }

            AftRegistrationKey = registrationKey;

            if (posId != (uint)AftPosIdDefinition.NoChange)
            {
                PosId = posId;
            }

            return true;
        }

        private bool AssetNumberIsValid(uint assetNumber)
        {
            if (assetNumber == 0)
            {
                return false;
            }

            if (assetNumber != AssetNumber())
            {
                return false;
            }

            return true;
        }

        private uint AssetNumber()
        {
            return _propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
        }

        private async Task<AftRegistration> UpdatePersistence(AftRegistrationStatus status)
        {
            var registration = _registrationDataProvider.GetData();
            registration.AftRegistrationKey = AftRegistrationKey;
            registration.PosId = PosId;
            registration.RegistrationStatus = status;
            await _registrationDataProvider.Save(registration);

            return registration;
        }
    }
}
