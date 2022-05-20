namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using AftTransferProvider;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using IPrinter = Hardware.Contracts.Printer.IPrinter;

    /// <summary>
    ///     AFT Transfer associations
    /// </summary>
    public class AftTransferAssociations : IAftTransferAssociations
    {
        private readonly IAftTransferProvider _aftProvider;
        private readonly IAftOffTransferProvider _aftOff;
        private readonly IAftOnTransferProvider _aftOn;
        private readonly IHostCashOutProvider _hostCashOutProvider;
        private readonly IFundsTransferDisable _fundsTransferDisable;
        private readonly IAftRegistrationProvider _aftRegistrationProvider;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Instantiate an instance of the AftTransferAssociations class.
        /// </summary>
        /// <param name="aftProvider">reference to the AFT transfer provider class</param>
        /// <param name="aftOff">reference to the AFT Off transfer provider class</param>
        /// <param name="aftOn">reference to the AFT On transfer provider class</param>
        /// <param name="hostCashOutProvider">reference to the host cashout provider</param>
        /// <param name="fundsTransferDisable">reference to the funds transfer disable class</param>
        /// <param name="aftRegistrationProvider">reference to the AFT registration provider class</param>
        /// <param name="propertiesManager">reference to the properties manager class</param>
        public AftTransferAssociations(
            IAftTransferProvider aftProvider,
            IAftOffTransferProvider aftOff,
            IAftOnTransferProvider aftOn,
            IHostCashOutProvider hostCashOutProvider,
            IFundsTransferDisable fundsTransferDisable,
            IAftRegistrationProvider aftRegistrationProvider,
            IPropertiesManager propertiesManager)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            _aftOff = aftOff ?? throw new ArgumentNullException(nameof(aftOff));
            _aftOn = aftOn ?? throw new ArgumentNullException(nameof(aftOn));
            _hostCashOutProvider = hostCashOutProvider ?? throw new ArgumentNullException(nameof(hostCashOutProvider));
            _fundsTransferDisable = fundsTransferDisable ?? throw new ArgumentNullException(nameof(fundsTransferDisable));
            _aftRegistrationProvider = aftRegistrationProvider ?? throw new ArgumentNullException(nameof(aftRegistrationProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public AftAvailableTransfers GetAvailableTransfers()
        {
            var settings = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            if (AftDisabled(settings))
            {
                return AftAvailableTransfers.None;
            }

            var availableTransfers = AftAvailableTransfers.None;
            if (settings.TransferInAllowed && _aftOn.IsAftOnAvailable)
            {
                if (!_fundsTransferDisable.TransferOnDisabledInGame &&
                     !_fundsTransferDisable.TransferOnDisabledTilt &&
                     !_fundsTransferDisable.TransferOnDisabledOverlay)
                {
                    availableTransfers |= AftAvailableTransfers.TransferToGamingMachineOk;
                }
            }

            if (settings.TransferOutAllowed && _aftOff.IsAftOffAvailable)
            {
                if (!_hostCashOutProvider.CashOutWinPending)
                {
                    availableTransfers |= AftAvailableTransfers.TransferFromGamingMachineOk;
                }
            }

            if (settings.TransferToTicketAllowed && (ServiceManager.GetInstance()?.TryGetService<IPrinter>()?.CanPrint ?? false))
            {
                if (!_fundsTransferDisable.TransferOnDisabledInGame &&
                     !_fundsTransferDisable.TransferOnDisabledTilt &&
                     !_fundsTransferDisable.TransferOnDisabledOverlay)
                {
                    availableTransfers |= AftAvailableTransfers.TransferToPrinterOk;
                }
            }


            if (settings.TransferOutAllowed && _hostCashOutProvider.CanCashOut && _hostCashOutProvider.CashOutWinPending)
            {
                availableTransfers |= AftAvailableTransfers.WinAmountPendingCashoutToHost;
            }

            if (settings.AftBonusAllowed)
            {
                if (!_fundsTransferDisable.TransferOnDisabledTilt && !_fundsTransferDisable.TransferOnDisabledOverlay)
                {
                    availableTransfers |= AftAvailableTransfers.BonusAwardToGamingMachineOk;
                }
            }

            return availableTransfers;
        }

        /// <inheritdoc />
        public bool TransferConditionsMet(AftGameLockAndStatusData data)
        {
            var availableTransfers = GetAvailableTransfers();
            if (availableTransfers == AftAvailableTransfers.None)
            {
                return false;
            }

            if ((AftTransferConditions.TransferFromGamingMachineOk & data.TransferConditions) != 0 &&
                ((AftAvailableTransfers.TransferFromGamingMachineOk | AftAvailableTransfers.WinAmountPendingCashoutToHost) & availableTransfers) == 0)
            {
                return false;
            }

            if ((AftTransferConditions.TransferToGamingMachineOk & data.TransferConditions) != 0 &&
                (AftAvailableTransfers.TransferToGamingMachineOk & availableTransfers) == 0)
            {
                return false;
            }

            if ((AftTransferConditions.TransferToPrinterOk & data.TransferConditions) != 0 &&
                (AftAvailableTransfers.TransferToPrinterOk & availableTransfers) == 0)
            {
                return false;
            }

            if ((AftTransferConditions.BonusAwardToGamingMachineOk & data.TransferConditions) != 0 &&
                (AftAvailableTransfers.BonusAwardToGamingMachineOk & availableTransfers) == 0)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public AftStatus GetAftStatus()
        {
            AftStatus returnValue = 0;

            var settings = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            if (AftDisabled(settings))
            {
                return returnValue;
            }

            if (_aftRegistrationProvider.IsAftRegistered)
            {
                returnValue |= AftStatus.AftRegistered;
            }

            if (_propertiesManager.GetValue(SasProperties.PrinterAsHandPayDeviceSupportedKey, false))
            {
                returnValue |= AftStatus.PrinterAvailableForTransactionReceipts;
            }

            if (settings.PartialTransferAllowed)
            {
                returnValue |= AftStatus.TransferToHostOfLessThanFullAvailableAmountAllowed;
            }

            if (_propertiesManager.GetValue(SasProperties.AftCustomTicketsSupportedKey, false))
            {
                returnValue |= AftStatus.CustomTicketDataSupported;
            }

            if (settings.TransferInAllowed || settings.TransferOutAllowed)
            {
                returnValue |= AftStatus.InHouseTransfersEnabled;
            }

            if (settings.AftBonusAllowed)
            {
                returnValue |= AftStatus.BonusTransfersEnabled;
            }

            if (settings.DebitTransfersAllowed)
            {
                returnValue |= AftStatus.DebitTransfersEnabled;
            }

            if (settings.TransferInAllowed || settings.TransferOutAllowed)
            {
                returnValue |= AftStatus.AnyAftEnabled;
            }

            return returnValue;
        }

        private bool AftDisabled(SasFeatures features)
        {
            return !features.AftAllowed ||
                   !(_aftOff.IsAftOffAvailable || _aftOn.IsAftOnAvailable) || // Can we do a transfer?
                   _aftProvider.IsTransferInProgress; // Is a transfer in progress?
        }

    }
}