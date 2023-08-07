namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AftTransferProvider;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Gaming.Contracts;
    using log4net;
    using SimpleInjector;

    /// <summary>
    ///     Handles full and partial transfers
    /// </summary>
    public class AftTransferFullPartial : IAftRequestProcessorTransferCode
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Dictionary<AftTransferType, Func<AftResponseData, AftResponseData>> _handlers;
        private readonly Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> _errorConditions;
        private readonly IAftRequestProcessor _aftTransferBonusCoinOutWinFromHostToGamingMachine;
        private readonly IAftRequestProcessor _aftTransferBonusJackpotWinFromHostToGamingMachine;
        private readonly IAftRequestProcessor _aftTransferDebitFromHostToGamingMachine;
        private readonly IAftRequestProcessor _aftTransferDebitFromHostToTicket;
        private readonly IAftRequestProcessor _aftTransferInHouseFromGameMachineToHost;
        private readonly IAftRequestProcessor _aftTransferInHouseFromHostToGameMachine;
        private readonly IAftRequestProcessor _aftTransferInHouseFromHostToTicket;
        private readonly IAftRequestProcessor _aftTransferWinAmountFromGameMachineToHost;
        private readonly IAftTransferProvider _aftProvider;
        private readonly IHostCashOutProvider _hostCashOutProvider;
        private readonly IFundsTransferDisable _fundsTransferDisable;
        private readonly IAutoPlayStatusProvider _autoPlayStatusProvider;
        private readonly IAftRegistrationProvider _registrationProvider;
        private bool _autoPlayActive;

        /// <summary>
        ///     Instantiate an instance of the AftTransferFullPartial class.
        /// </summary>
        /// <param name="aftProvider">reference to the AftTransferProvider class</param>
        /// <param name="hostCashOutProvider">reference to the host cashout provider</param>
        /// <param name="fundsTransferDisable">reference to the FundsTransferDisable class</param>
        /// <param name="autoPlayStatusProvider">reference to the AutoPlayStatusProvider class</param>
        /// <param name="registrationProvider">An instance of <see cref="IAftRegistrationProvider"/></param>
        /// <param name="aftRequestProcessors">reference to the list of request processors</param>
        public AftTransferFullPartial(
            IAftTransferProvider aftProvider,
            IHostCashOutProvider hostCashOutProvider,
            IFundsTransferDisable fundsTransferDisable,
            IAutoPlayStatusProvider autoPlayStatusProvider,
            IAftRegistrationProvider registrationProvider,
            IEnumerable<IAftRequestProcessor> aftRequestProcessors)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            _hostCashOutProvider = hostCashOutProvider ?? throw new ArgumentNullException(nameof(hostCashOutProvider));
            _fundsTransferDisable = fundsTransferDisable ?? throw new ArgumentNullException(nameof(fundsTransferDisable));
            _autoPlayStatusProvider = autoPlayStatusProvider ?? throw new ArgumentNullException(nameof(autoPlayStatusProvider));
            _registrationProvider = registrationProvider ?? throw new ArgumentNullException(nameof(registrationProvider));

            var requestProcessors = aftRequestProcessors.ToList();
            _aftTransferBonusCoinOutWinFromHostToGamingMachine = GetAftRequestProcessor(requestProcessors, typeof(AftTransferBonusCoinOutWinFromHostToGamingMachine));
            _aftTransferBonusJackpotWinFromHostToGamingMachine = GetAftRequestProcessor(requestProcessors, typeof(AftTransferBonusJackpotWinFromHostToGamingMachine));
            _aftTransferDebitFromHostToGamingMachine = GetAftRequestProcessor(requestProcessors, typeof(AftTransferDebitFromHostToGamingMachine));
            _aftTransferDebitFromHostToTicket = GetAftRequestProcessor(requestProcessors, typeof(AftTransferDebitFromHostToTicket));
            _aftTransferInHouseFromGameMachineToHost = GetAftRequestProcessor(requestProcessors, typeof(AftTransferInHouseFromGameMachineToHost));
            _aftTransferInHouseFromHostToGameMachine = GetAftRequestProcessor(requestProcessors, typeof(AftTransferInHouseFromHostToGameMachine));
            _aftTransferInHouseFromHostToTicket = GetAftRequestProcessor(requestProcessors, typeof(AftTransferInHouseFromHostToTicket));
            _aftTransferWinAmountFromGameMachineToHost = GetAftRequestProcessor(requestProcessors, typeof(AftTransferWinAmountFromGameMachineToHost));

            _errorConditions = InitializeErrorConditions();
            _handlers = InitializeHandlers();
        }

        /// <inheritdoc />
        public AftResponseData Process(AftResponseData data)
        {
            Logger.Debug("Aft Transfer Full and Partial");

            // check if a transfer is already in progress
            if (_aftProvider.IsTransferInProgress)
            {
                Logger.Debug("A transfer is already in progress");
                return _aftProvider.CurrentTransfer;
            }

            _aftProvider.CurrentTransfer = data;
            Logger.Debug($"Asset number is {_aftProvider.CurrentTransfer.AssetNumber:X8}");

            if (!_handlers.TryGetValue(data.TransferType, out var handler))
            {
                _aftProvider.CurrentTransfer.TransferStatus = AftTransferStatusCode.UnsupportedTransferCode;
                _aftProvider.AftTransferFailed();
                return data;
            }

            _aftProvider.CheckTransactionId(_aftProvider.CurrentTransfer.TransactionId);
            _aftProvider.TransferAmount = _aftProvider.CurrentTransfer.CashableAmount + _aftProvider.CurrentTransfer.RestrictedAmount +
                                          _aftProvider.CurrentTransfer.NonRestrictedAmount;

            // auto play should end if we try to do an AFT transfer
            _autoPlayActive = _autoPlayStatusProvider.EndAutoPlayIfActive();
            _aftProvider.CheckForErrorConditions(_errorConditions);

            Logger.Debug($"TransferFailure is {_aftProvider.TransferFailure}");
            if (_aftProvider.TransferFailure)
            {
                _aftProvider.AftTransferFailed();
                _autoPlayStatusProvider.UnpausePlayerAutoPlay();
                return _aftProvider.CurrentTransfer;
            }

            _aftProvider.IsTransferAcknowledgedByHost = false;
            return handler(_aftProvider.CurrentTransfer);
        }

        /// <summary>
        ///     create a list of error conditions to check.
        /// </summary>
        private Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> InitializeErrorConditions()
        {
            return new Dictionary<Func<bool>, (AftTransferStatusCode code, string message)>
            {
                {   // bad transaction id
                    () => !_aftProvider.TransactionIdUnique,
                    (AftTransferStatusCode.TransactionIdNotUnique, "transaction id not unique")
                },
                {   // wrong transaction index
                    () => _aftProvider.CurrentTransfer.TransactionIndex != 0,
                    (AftTransferStatusCode.NotAValidTransferFunction, "wrong transaction index")
                },
                {   // not a valid asset number
                    () => _aftProvider.CurrentTransfer.AssetNumber == 0 ||
                          _aftProvider.CurrentTransfer.AssetNumber != _aftProvider.AssetNumber,
                    (AftTransferStatusCode.AssetNumberZeroOrDoesNotMatch, "not a valid asset number")
                },
                {   // not locked check
                    () => _aftProvider.IsTransferFlagSet(AftTransferFlags.AcceptTransferOnlyIfLocked) && !_aftProvider.IsLocked,
                    (AftTransferStatusCode.GamingMachineNotLocked, "aft not locked")
                },
                {   // invalid transaction id
                    () => !_aftProvider.TransactionIdValid,
                    (AftTransferStatusCode.TransactionIdNotValid, "invalid transaction id")
                },
                {   // if auto play is active fail the transfer request
                    () => _autoPlayActive,
                    (AftTransferStatusCode.GamingMachineUnableToPerformTransfer, "can't transfer while auto play is active")
                },
                {   // can't print receipt if printer disabled
                    () => _aftProvider.IsTransferFlagSet(AftTransferFlags.TransactionReceiptRequested) &&
                          !_aftProvider.IsPrinterAvailable,
                    (AftTransferStatusCode.UnableToPrintTransactionReceipt, "receipt requested and printer not available")
                },
                {   // trying to do partial transfer but configuration doesn't allow it
                    () => !_aftProvider.FullTransferRequested && !_aftProvider.PartialTransfersAllowed,
                    (AftTransferStatusCode.GamingMachineUnableToPerformPartial, "configuration doesn't allow partial transfers")
                },
                {   // receipt requested but required fields are missing
                    () => _aftProvider.MissingRequiredReceiptFields,
                    (AftTransferStatusCode.InsufficientDataToPrintTransactionReceipt, "missing Patron Account number or Debit account number")
                },
                {
                    // if we have a key from the host we must be registered
                    () => !_aftProvider.IsRegistrationKeyAllZeros &&
                          !_registrationProvider.IsAftRegistered,
                    (AftTransferStatusCode.GamingMachineNotRegistered, "gaming machine is not registered and a non zero registration key was used")
                },
                {
                    // if we have a key from the host it must match
                    () => !_aftProvider.IsRegistrationKeyAllZeros &&
                          _registrationProvider.IsAftRegistered &&
                          !_registrationProvider.RegistrationKeyMatches(_aftProvider.RegistrationKey),
                    (AftTransferStatusCode.RegistrationKeyDoesNotMatch, "registration key is non zero and does not match")
                },
                {   // can't transfer on when in host cashout pending state
                    () => _hostCashOutProvider.HostCashOutPending && !_aftProvider.TransferOff,
                    (AftTransferStatusCode.GamingMachineUnableToPerformTransfer, "only aft off transfers can be accepted during a host cashout scenario")
                },
                {   // disabled when trying to transfer on due to tilt
                    () => !_aftProvider.TransferOff && _fundsTransferDisable.TransferOnDisabledTilt,
                    (AftTransferStatusCode.GamingMachineUnableToPerformTransfer, "can't transfer on while tilts are present")
                },
                {   // disabled when trying to transfer on due to overlay
                    () => !_aftProvider.TransferOff && _fundsTransferDisable.TransferOnDisabledOverlay,
                    (AftTransferStatusCode.GamingMachineUnableToPerformTransfer, "can't transfer on while Game disabled with overlay")
                },
                {   // disabled when trying to transfer on due to active game and not a bonus transfer
                    () => !_aftProvider.TransferOff && _fundsTransferDisable.TransferOnDisabledInGame &&
                          !(_aftProvider.CurrentTransfer.TransferType == AftTransferType.HostToGameBonusCoinOut ||
                            _aftProvider.CurrentTransfer.TransferType == AftTransferType.HostToGameBonusJackpot),
                    (AftTransferStatusCode.GamingMachineUnableToPerformTransfer, "can't transfer on while in a game")
                },
                {   // disabled when trying to transfer off due to tilts
                    () => _aftProvider.TransferOff && _fundsTransferDisable.TransferOffDisabled && !_hostCashOutProvider.CanCashOut,
                    (AftTransferStatusCode.GamingMachineUnableToPerformTransfer, "can't transfer off while tilts present or in a game")
                },
            };
        }

        private Dictionary<AftTransferType, Func<AftResponseData, AftResponseData>> InitializeHandlers()
        {
            return new Dictionary<AftTransferType, Func<AftResponseData, AftResponseData>>
            {
                {
                    AftTransferType.HostToGameBonusCoinOut,
                    data => _aftTransferBonusCoinOutWinFromHostToGamingMachine.Process(data)
                },
                {
                    AftTransferType.HostToGameBonusJackpot,
                    data => _aftTransferBonusJackpotWinFromHostToGamingMachine.Process(data)
                },
                { AftTransferType.HostToGameDebit, data => _aftTransferDebitFromHostToGamingMachine.Process(data) },
                { AftTransferType.HostToGameDebitTicket, data => _aftTransferDebitFromHostToTicket.Process(data) },
                { AftTransferType.GameToHostInHouse, data => _aftTransferInHouseFromGameMachineToHost.Process(data) },
                { AftTransferType.HostToGameInHouse, data => _aftTransferInHouseFromHostToGameMachine.Process(data) },
                { AftTransferType.HostToGameInHouseTicket, data => _aftTransferInHouseFromHostToTicket.Process(data) },
                { AftTransferType.GameToHostInHouseWin, data => _aftTransferWinAmountFromGameMachineToHost.Process(data) },
            };
        }

        /// <summary>
        ///     Gets a specific type that implements IAftRequestProcessor from the list of
        ///     objects that the dependency injection framework gives us.
        /// </summary>
        /// <remarks>
        /// Since there are multiple classes that implement IAftRequestProcessor the dependency injection
        /// framework gives us an IEnumerable list of the classes. We have to go thru the list and find an
        /// entry for a concrete type in order to call the correct processor. We can't inject the list
        /// into this class since the processors depend on this class and it would create circular dependencies.
        /// </remarks>
        /// <param name="requestProcessors">The list of types from the DI framework</param>
        /// <param name="type">The specific type that implements the interface</param>
        /// <returns>The requested type</returns>
        /// <exception cref="ArgumentException">Thrown if the requested type is not in the list</exception>
        private IAftRequestProcessor GetAftRequestProcessor(List<IAftRequestProcessor> requestProcessors, Type type)
        {
            return requestProcessors.SingleOrDefault(type.IsInstanceOfType) ??
                   throw new ArgumentException($"{type.ToFriendlyName()} not in aftRequestProcessors");
        }
    }
}