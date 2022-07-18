namespace Aristocrat.Monaco.Sas.Eft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.EFT;
    using Contracts.Eft;
    using Gaming.Contracts;
    using Kernel;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Handler for LP 6A, Available Eft Transfers from EGM
    /// </summary>
    public class LP6ASendAvailableEftTransferHandler : ISasLongPollHandler<AvailableEftTransferResponse, LongPollData>
    {
        private readonly IEftTransferProvider _transferProvider;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IGamePlayState _gamePlayState;
        private readonly IOperatorMenuLauncher _operatorMenu;

        /// <summary>
        ///     Creates a new instance of the LPA0SendEnabledFeaturesHandler class.
        /// </summary>
        public LP6ASendAvailableEftTransferHandler(
            IEftTransferProvider transferProvider,
            ISystemDisableManager systemDisableManager,
            IGamePlayState gamePlayState,
            IOperatorMenuLauncher operatorMenu)
        {
            _transferProvider = transferProvider ?? throw new ArgumentNullException(nameof(transferProvider));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
        }

        /// <inheritdoc cref="ISasLongPollHandler" />
        public List<LongPoll> Commands => new() { LongPoll.EftSendAvailableEftTransfers };

        /// <inheritdoc />
        public AvailableEftTransferResponse Handle(LongPollData data)
        {
            var response = new AvailableEftTransferResponse { TransferAvailability = 0 };

            if (_gamePlayState.CurrentState != PlayState.Idle || _operatorMenu.IsShowing)
            {
                return response;
            }

            var (eftInAllowed, eftOutAllowed) = _transferProvider.GetSupportedTransferTypes();

            if (eftInAllowed)
            {
                if (!_systemDisableManager.IsDisabled)
                {
                    response.TransferAvailability |=
                        AvailableEftTransferResponse.EftTransferAvailability.TransferToGamingMachine;
                }
                else
                {
                    //Check if the EGM is in Only EFT lockup condition
                    if (_systemDisableManager.CurrentDisableKeys?.Except(
                            new List<Guid> { SasConstants.EftTransactionLockUpGuid }).ToList().Count == 0)
                    {
                        response.TransferAvailability |=
                            AvailableEftTransferResponse.EftTransferAvailability.TransferToGamingMachine;
                    }
                }
            }

            if (eftOutAllowed)
            {
                if (!_systemDisableManager.IsDisabled)
                {
                    response.TransferAvailability |=
                        AvailableEftTransferResponse.EftTransferAvailability.TransferFromGamingMachine;
                }
                else
                {
                    var disabledByHost = new List<Guid>
                    {
                        ApplicationConstants.DisabledByHost0Key,
                        ApplicationConstants.DisabledByHost1Key,
                        ApplicationConstants.Host0CommunicationsOfflineDisableKey,
                        ApplicationConstants.Host1CommunicationsOfflineDisableKey,
                        SasConstants.EftTransactionLockUpGuid
                    };
                    if (_systemDisableManager.CurrentDisableKeys?.Except(disabledByHost).ToList().Count == 0)
                    {
                        response.TransferAvailability |=
                            AvailableEftTransferResponse.EftTransferAvailability.TransferFromGamingMachine;
                    }
                }
            }

            return response;
        }
    }
}