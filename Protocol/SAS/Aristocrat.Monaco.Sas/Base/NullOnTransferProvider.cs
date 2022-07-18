namespace Aristocrat.Monaco.Sas.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Contracts.Client;
    using Contracts.Eft;

    /// <summary>
    ///     Provides an Aft/Eft On transfer provider that doesn't do anything for use
    ///     by dependency injection frameworks when Aft/Eft On is not configured.
    /// </summary>
    public class NullOnTransferProvider : IAftOnTransferProvider, IWatTransferOnProvider, ITransactionRequestor, IEftOnTransferProvider
    {
        /// <inheritdoc />
        public bool IsAftOnAvailable => false;

        /// <inheritdoc />
        public bool IsAftPending => false;

        /// <inheritdoc />
        public Guid RequestorGuid => Guid.Empty;

        /// <inheritdoc />
        public bool EftOnRequest(string requestID, AccountType accountType, ulong amount) => false;

        /// <inheritdoc cref="IEftOnTransferProvider" />
        public bool CanTransfer => false;

        /// <inheritdoc />
        public (ulong Amount, bool LimitExceeded) GetAcceptedTransferInAmount(ulong amount) => (0, true);

        /// <inheritdoc />
        public string Name => ServiceTypes?.ElementAt(0).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IAftOnTransferProvider), typeof(IWatTransferOnProvider), typeof(IEftOffTransferProvider) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public Task<bool> InitiateTransfer(WatOnTransaction transaction) => Task.FromResult(false);

        /// <inheritdoc />
        public Task CommitTransfer(WatOnTransaction transaction) => Task.CompletedTask;

        /// <inheritdoc />
        public bool InitiateAftOn() => false;

        /// <inheritdoc />
        public void CancelAftOn()
        {
        }

        /// <inheritdoc />
        public bool AftOnRequest(AftData data, bool partialAllowed) => false;

        /// <inheritdoc />
        public void AftOnRejected()
        {
        }

        /// <inheritdoc />
        public bool Recover(string transactionId) => false;

        /// <inheritdoc />
        public void AcknowledgeTransfer(string transactionId)
        {
        }

        /// <inheritdoc />
        public void NotifyTransactionReady(Guid requestId)
        {
        }
    }
}