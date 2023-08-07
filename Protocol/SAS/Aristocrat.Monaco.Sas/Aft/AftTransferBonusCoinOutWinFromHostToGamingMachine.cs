namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Handles transferring Bonus Coin Out Wins from the Host to gaming machine
    /// </summary>
    public class AftTransferBonusCoinOutWinFromHostToGamingMachine : IAftRequestProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAftTransferProvider _aftProvider;
        private readonly ISasBonusCallback _bonus;
        private readonly Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> _errorConditions;

        /// <summary>
        ///     Instantiates a new instance of the AftTransferBonusCoinOutWinFromHostToGamingMachine class
        /// </summary>
        /// <param name="aftProvider">reference to the AftTransferProvider class</param>
        /// <param name="bonus">reference to the SasBonusCallback class</param>
        /// <param name="propertiesManager">a reference to properties manager</param>
        public AftTransferBonusCoinOutWinFromHostToGamingMachine(
            IAftTransferProvider aftProvider,
            ISasBonusCallback bonus,
            IPropertiesManager propertiesManager)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _errorConditions = InitializeErrorConditions();
        }

        /// <inheritdoc />
        public AftResponseData Process(AftResponseData data)
        {
            // mark request as pending. The following code will change it if there are any errors
            _aftProvider.CurrentTransfer.TransferStatus = AftTransferStatusCode.TransferPending;
            _aftProvider.CheckForErrorConditions(_errorConditions);

            if (_aftProvider.TransferFailure)
            {
                _aftProvider.AftTransferFailed();
            }
            else
            {
                Logger.Debug("Starting Aft bonus coin win to game machine as a task");
                _aftProvider.DoBonus().FireAndForget();
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
                {
                    // check if we're allowed to do bonus transfers
                    () => !_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).AftBonusAllowed,
                    (AftTransferStatusCode.NotAValidTransferFunction, "configuration doesn't allow bonus transfers")
                },
                {
                    // don't allow partial bonus transfers
                    () => _aftProvider.PartialTransfersAllowed,
                    (AftTransferStatusCode.NotAValidTransferFunction, "partial bonus transfers not allowed")
                },
                {
                    // don't allow restricted bonus transfers
                    () => _aftProvider.FullTransferRequested &&
                          _aftProvider.CurrentTransfer.RestrictedAmount > 0,
                    (AftTransferStatusCode.NotAValidTransferFunction, "restricted bonuses not allowed")
                },
                {
                    // don't print a receipt for bonus transfers
                    () => _aftProvider.IsTransferFlagSet(AftTransferFlags.TransactionReceiptRequested),
                    (AftTransferStatusCode.TransactionReceiptNotAllowedForTransferType, "bonus transfers don't support printing a receipt")
                },
                {
                    () => !_bonus.IsAftBonusAllowed(null),
                    (AftTransferStatusCode.GamingMachineUnableToPerformTransfer, "transfers not allowed while disabled")
                }
            };
        }
    }
}