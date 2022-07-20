namespace Aristocrat.Monaco.Sas.Eft
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.Eft;
    using Contracts.Eft;
    using EftTransferProvider;
    using Localization.Properties;

    /// <summary>
    ///     (From section 8.EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     Handles LP 69, cashable credit transfers to the EGM. Most of the workflow logic is handled in
    ///     <see cref="IEftStateController". />
    /// </summary>
    public class LP69TransferCashableCreditHandler : ISasLongPollHandler<EftTransactionResponse, EftTransferData>,
        IEftTransferHandler
    {
        /// <inheritdoc cref="ISasLongPollHandler" />
        public List<LongPoll> Commands { get; } = new() { LongPoll.EftTransferCashableCreditsToMachine };

        private readonly IEftStateController _stateController;
        private readonly IEftTransferProvider _provider;
        private readonly IEventBus _eventBus;

        /// <inheritdoc />
        public (ulong Amount, bool TransferAmountExceeded) CheckTransferAmount(ulong amount)
        {
            return _provider.GetAcceptedTransferInAmount(amount);
        }

        /// <inheritdoc />
        public bool ProcessTransfer(ulong amountToBeTransferred, int transactionNumber)
        {
            return _provider.DoEftOn(
                transactionNumber.ToString(),
                AccountType.Cashable,
                amountToBeTransferred
            );
        }

        /// <summary>
        ///     Creates and returns a new instance of LP69 handler.
        /// </summary>
        public LP69TransferCashableCreditHandler(
            IEftStateController stateController,
            IEftTransferProvider provider,
            IEventBus eventBus)
        {
            _stateController = stateController ?? throw new ArgumentNullException(nameof(stateController));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<ProtocolsInitializedEvent>(this, HandleEvent);
        }

        private void HandleEvent(ProtocolsInitializedEvent obj)
        {
            //Check if last transaction of type LP69 needs recovery.
            _stateController.RecoverIfRequired(this);
        }

        /// <inheritdoc />
        public EftTransactionResponse Handle(EftTransferData data)
        {
            var response = _stateController.Handle(data, this);
            return response;
        }

        /// <inheritdoc />
        public bool StopTransferIfDisabledByHost()
        {
            return true;
        }

        /// <inheritdoc />
        public string GetDisableString()
        {
            return Localizer.For(CultureFor.Player).GetString(ResourceKeys.EftTransferInInProgress);
        }
    }
}