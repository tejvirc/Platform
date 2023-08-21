namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using log4net;

    /// <summary>
    ///     Handles debit transfers from the host to the gaming machine
    /// </summary>
    public class AftTransferDebitFromHostToGamingMachine : IAftRequestProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAftTransferProvider _aftProvider;
        private readonly IAftRegistrationProvider _aftRegistrationProvider;
        private readonly Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> _errorConditions;

        /// <summary>
        ///     Instantiates a new instance of the AftTransferDebitFromHostToGamingMachine class
        /// </summary>
        /// <param name="aftProvider">reference to the AftTransferProvider class</param>
        /// <param name="aftRegistrationProvider">reference to the AftRegistrationProvider class</param>
        public AftTransferDebitFromHostToGamingMachine(
            IAftTransferProvider aftProvider,
            IAftRegistrationProvider aftRegistrationProvider)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            _aftRegistrationProvider = aftRegistrationProvider ?? throw new ArgumentNullException(nameof(aftRegistrationProvider));
            _errorConditions = InitializeErrorConditions();
        }

        /// <inheritdoc />
        public AftResponseData Process(AftResponseData data)
        {
            Logger.Debug("Aft Debit transfer from host to gaming machine");

            // mark request as pending. The following code will change it if there are any errors
            _aftProvider.CurrentTransfer.TransferStatus = AftTransferStatusCode.TransferPending;

            _aftProvider.CheckForErrorConditions(_errorConditions);
            if (_aftProvider.TransferFailure)
            {
                _aftProvider.AftTransferFailed();
            }
            else
            {
                Logger.Debug("Starting Aft debit to game as a task");
                _aftProvider.DoAftOn().FireAndForget();
            }

            return _aftProvider.CurrentTransfer;
        }

        /// <summary>
        ///     create a list of error conditions to check.
        /// </summary>
        private Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> InitializeErrorConditions()
        {
            return new Dictionary<Func<bool>, (AftTransferStatusCode code, string message)>
            {
                // do we support debit transfers
                { () => !_aftRegistrationProvider.IsAftDebitTransferEnabled, (AftTransferStatusCode.NotAValidTransferFunction, "configuration doesn't allow debit transfers") },

                // are we registered
                { () => !_aftRegistrationProvider.IsAftRegistered, (AftTransferStatusCode.GamingMachineNotRegistered, "not registered for debit transfers") },

                { () => !_aftRegistrationProvider.RegistrationKeyMatches(_aftProvider.CurrentTransfer.RegistrationKey),
                    (AftTransferStatusCode.RegistrationKeyDoesNotMatch, "registration keys don't match") },

                { () => _aftProvider.PosIdZero, (AftTransferStatusCode.NoPosId, "position id is zero") },

                // debit transfers can only be type cashable
                { () => (_aftProvider.CurrentTransfer.RestrictedAmount > 0 || _aftProvider.CurrentTransfer.NonRestrictedAmount > 0),
                    (AftTransferStatusCode.NotAValidTransferFunction, "not a cash transfer") },
            };
        }
    }
}