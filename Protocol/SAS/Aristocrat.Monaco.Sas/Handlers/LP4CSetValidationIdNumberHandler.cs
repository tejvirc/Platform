namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Storage.Models;
    using Storage.Repository;

    /// <summary>
    ///     Handles setting validation number
    /// </summary>
    public class LP4CSetValidationIdNumberHandler : ISasLongPollHandler<LongPoll4CResponse, LongPoll4CData>
    {
        private const int MachineValidationQueryId = 0;
        private readonly ISasDisableProvider _disableProvider;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IStorageDataProvider<ValidationInformation> _validationProvider;

        /// <summary>
        ///     Create an instance of the LP4CSetValidationIdNumberHandler class
        /// </summary>
        /// <param name="disableProvider">The sas disable provider</param>
        /// <param name="propertiesManager">The property provider</param>
        /// <param name="validationProvider">THe validation data provider</param>
        public LP4CSetValidationIdNumberHandler(
            IPropertiesManager propertiesManager,
            ISasDisableProvider disableProvider,
            IStorageDataProvider<ValidationInformation> validationProvider)
        {
            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _validationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.SetSecureEnhancedValidationId };

        /// <inheritdoc/>
        public LongPoll4CResponse Handle(LongPoll4CData data)
        {
            var machineId = data.MachineValidationId;
            var newSequenceNumber = data.SequenceNumber;
            var persistData = false;

            var validationInformation = _validationProvider.GetData();
            var sasValidationType = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ValidationType;
            // check if secure enhanced validation is turned on
            if (sasValidationType != SasValidationType.SecureEnhanced)
            {
                return new LongPoll4CResponse();
            }

            var id = validationInformation.MachineValidationId;
            var sequenceNumber = validationInformation.SequenceNumber;
            if (machineId != MachineValidationQueryId)
            {
                // check if this machine's validation ID matches the passed in ID
                if (validationInformation.MachineValidationId != machineId)
                {
                    id = machineId;
                    persistData = true;
                }

                // We update if the value set does not match the previously sent value or the machine ID is different
                if (!validationInformation.ValidationConfigured || validationInformation.LastReceivedSequenceNumber != newSequenceNumber || persistData)
                {
                    sequenceNumber = newSequenceNumber;
                    persistData = true;
                }
            }

            if (persistData)
            {
                UpdateValidationInformation(machineId, newSequenceNumber, validationInformation).FireAndForget();
            }

            return new LongPoll4CResponse
            {
                UsingSecureEnhancedValidation = true,
                MachineValidationId = (uint)id,
                SequenceNumber = (uint)(sequenceNumber % (SasConstants.MaxValidationSequenceNumber + 1))
            };
        }

        private async Task UpdateValidationInformation(
            uint machineId,
            uint newSequenceNumber,
            ValidationInformation validationInformation)
        {
            validationInformation.MachineValidationId = machineId;
            validationInformation.SequenceNumber = newSequenceNumber;
            validationInformation.LastReceivedSequenceNumber = newSequenceNumber;
            validationInformation.ValidationConfigured = true;
            await _validationProvider.Save(validationInformation);
            await _disableProvider.Enable(DisableState.ValidationIdNeeded);
        }
    }
}