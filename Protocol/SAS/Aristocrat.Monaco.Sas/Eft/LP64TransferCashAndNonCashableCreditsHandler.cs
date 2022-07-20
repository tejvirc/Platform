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
    using Localization.Properties;

    /// <summary>
    ///     Handler for LP64-Transfer cashable/non-cashable credits to the host
    /// </summary>
    public class LP64TransferCashAndNonCashableCreditsHandler : ISasLongPollHandler<EftTransactionResponse, EftTransferData>, IEftTransferHandler
    {
        private readonly IEftStateController _stateController;
        private readonly IEftTransferProvider _provider;
        private readonly IEventBus _eventBus;

        /// <inheritdoc cref="ISasLongPollHandler" />
        public List<LongPoll> Commands { get; } = new() { LongPoll.EftTransferCashAndNonCashableCreditsToHost };

        /// <inheritdoc />
        public (ulong Amount, bool TransferAmountExceeded) CheckTransferAmount(ulong _) => _provider.GetAcceptedTransferOutAmount(new[] { AccountType.Cashable, AccountType.Promo });

        /// <inheritdoc />
        public bool ProcessTransfer(ulong amountToBeTransferred, int transactionNumber) => _provider.DoEftOff(transactionNumber.ToString("X2"), new[] { AccountType.Cashable, AccountType.Promo }, amountToBeTransferred);

        /// <summary>
        ///     Creates and returns a new instance of LP64 handler.
        /// </summary>
        public LP64TransferCashAndNonCashableCreditsHandler(
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
            //Check if last transaction of type LP64 needs recovery.
            _stateController.RecoverIfRequired(this);
        }

        /// <inheritdoc />
        public EftTransactionResponse Handle(EftTransferData data) => _stateController.Handle(data, this);

        /// <inheritdoc />
        public bool StopTransferIfDisabledByHost() => false;

        /// <inheritdoc />
        public string GetDisableString() => Localizer.For(CultureFor.Player).GetString(ResourceKeys.EftTransferOutInProgress);
    }
}