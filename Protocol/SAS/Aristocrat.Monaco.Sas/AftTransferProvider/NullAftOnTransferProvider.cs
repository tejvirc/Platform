namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Contracts.Client;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Provides an Aft On transfer provider that doesn't do anything for use
    ///     by dependency injection frameworks when Aft On is not configured.
    /// </summary>
    public class NullAftOnTransferProvider : AftTransferProviderBase, IAftOnTransferProvider, IWatTransferOnProvider, ITransactionRequestor
    {
        /// <summary>Initializes a new instance of the NullAftOffTransferProvider class.</summary>
        /// <param name="sasHost">The sas host</param>
        /// <param name="aftLock">The aft lock handler</param>
        /// <param name="propertiesManager">the properties manager</param>
        /// <param name="transactionCoordinator">The transaction coordinator</param>
        /// <param name="timeService">the time service</param>
        /// <param name="autoPlayStatusProvider">the auto play provider service</param>
        public NullAftOnTransferProvider(
            ISasHost sasHost,
            IAftLockHandler aftLock,
            ITime timeService,
            IPropertiesManager propertiesManager,
            ITransactionCoordinator transactionCoordinator,
            IAutoPlayStatusProvider autoPlayStatusProvider)
            : base(
                aftLock,
                sasHost,
                timeService,
                propertiesManager,
                transactionCoordinator,
                autoPlayStatusProvider)
        {
        }

        /// <inheritdoc />
        public bool IsAftOnAvailable => false;

        /// <inheritdoc />
        public bool IsAftPending => false;

        /// <inheritdoc />
        public Guid RequestorGuid => Guid.Empty;

        /// <inheritdoc />
        public bool CanTransfer => false;

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IAftOnTransferProvider), typeof(IWatTransferOnProvider) };

        /// <inheritdoc />
        public Task<bool> InitiateTransfer(WatOnTransaction transaction)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task CommitTransfer(WatOnTransaction transaction)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool InitiateAftOn()
        {
            return false;
        }

        /// <inheritdoc />
        public void CancelAftOn()
        {
        }

        /// <inheritdoc />
        public bool AftOnRequest(AftData data, bool partialAllowed)
        {
            return false;
        }

        /// <inheritdoc />
        public void AftOnRejected()
        {
        }

        /// <inheritdoc />
        public bool Recover(string transactionId)
        {
            return false;
        }

        /// <inheritdoc />
        public void AcknowledgeTransfer(string transactionId)
        {
        }

        /// <inheritdoc />
        public void NotifyTransactionReady(Guid requestId)
        {
        }

        /// <inheritdoc />
        protected override void HandleLockAcquired()
        {
        }

        /// <inheritdoc />
        protected override void AftDisabledByEvent(bool isEnabled)
        {
        }
    }
}