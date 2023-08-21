namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Kernel;

    /// <summary>
    ///     Handles registering the gaming machine
    /// </summary>
    public class LP73AftRegisterGamingMachineHandler : ISasLongPollHandler<AftRegisterGamingMachineResponseData, AftRegisterGamingMachineData>
    {
        private readonly IAftRegistrationProvider _aftRegistrationProvider;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Creates a new instance of the LP73AftRegisterGamingMachineHandler
        /// </summary>
        /// <param name="aftRegistrationProvider">a reference to the aftRegistrationProvider</param>
        /// <param name="propertiesManager">a reference to the properties manager</param>
        public LP73AftRegisterGamingMachineHandler(
            IAftRegistrationProvider aftRegistrationProvider,
            IPropertiesManager propertiesManager)
        {
            _aftRegistrationProvider = aftRegistrationProvider ?? throw new ArgumentNullException(nameof(aftRegistrationProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.AftRegisterGamingMachine };

        /// <inheritdoc/>
        public AftRegisterGamingMachineResponseData Handle(AftRegisterGamingMachineData data)
        {
            DoRegisterAction(data);
            return GenerateResponse();
        }

        /// <summary>
        ///     Deal with registration commands
        /// </summary>
        /// <param name="data"></param>
        private void DoRegisterAction(AftRegisterGamingMachineData data)
        {
            _aftRegistrationProvider.ProcessAftRegistration(data.RegistrationCode, data.AssetNumber, data.RegistrationKey, data.PosId);
        }

        /// <summary>
        ///     Generate the response to this long poll
        /// </summary>
        /// <returns>AftRegisterGamingMachineResponseData</returns>
        private AftRegisterGamingMachineResponseData GenerateResponse()
        {
            var response = new AftRegisterGamingMachineResponseData
            {
                RegistrationStatus = _aftRegistrationProvider.AftRegistrationStatus,
                AssetNumber = _propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0),
                RegistrationKey = _aftRegistrationProvider.AftRegistrationKey,
                PosId = _aftRegistrationProvider.PosId
            };

            // Do not leave this in the pending state, clear it by requesting acknowledgement again
            if (response.RegistrationStatus == AftRegistrationStatus.RegistrationPending)
            {
                _aftRegistrationProvider.ProcessAftRegistration(AftRegistrationCode.RequestOperatorAcknowledgement, response.AssetNumber, response.RegistrationKey, response.PosId);
            }

            return response;
        }
    }
}