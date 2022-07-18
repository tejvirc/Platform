namespace Aristocrat.Monaco.Sas.Base
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Contracts.Client;
    using Contracts.Eft;

    /// <summary>
    ///     Provides an Aft/Eft off transfer provider that doesn't do anything for use
    ///     by dependency injection frameworks when Aft/Eft Off is not configured.
    /// </summary>
    public class NullOffTransferProvider : IAftOffTransferProvider, IWatTransferOffProvider, ITransactionRequestor, IEftOffTransferProvider
    {
        /// <inheritdoc />
        public bool EftOffRequest(string requestID, AccountType[] accountTypes, ulong amount) => false;

        /// <inheritdoc />
        public bool CanTransfer => false;

        /// <inheritdoc />
        public (ulong Amount, bool LimitExceeded) GetAcceptedTransferOutAmount(AccountType[] accountTypes) => (0, false);

        /// <inheritdoc />
        public void RestartCashoutTimer() { }

        /// <inheritdoc />
        public bool IsAftPending => false;

        /// <inheritdoc />
        public bool WaitingForKeyOff => false;

        /// <inheritdoc />
        public Guid RequestorGuid => Guid.Empty;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IAftOffTransferProvider), typeof(IWatTransferOffProvider), typeof(IEftOffTransferProvider), };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public bool IsAftOffAvailable => false;

        /// <inheritdoc />
        public bool InitiateAftOff() => false;

        /// <inheritdoc />
        public void CancelAftOff()
        {
        }

        /// <inheritdoc />
        public bool AftOffRequest(AftData data, bool partialAllowed) => false;

        /// <inheritdoc />
        public void AftOffRejected()
        {
        }

        /// <inheritdoc />
        public bool Recover(string transactionId) => false;

        /// <inheritdoc />
        public void OnKeyedOff()
        {
        }

        /// <inheritdoc />
        public void AcknowledgeTransfer(string transactionId)
        {
        }

        /// <inheritdoc />
        public Task<bool> InitiateTransfer(WatTransaction transaction) => Task.FromResult(false);

        /// <inheritdoc />
        public Task CommitTransfer(WatTransaction transaction) => Task.CompletedTask;

        /// <inheritdoc />
        public void NotifyTransactionReady(Guid requestId)
        {
        }
    }
}