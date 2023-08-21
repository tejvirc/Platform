namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Contracts.Client;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Provides an Aft off transfer provider that doesn't do anything for use
    ///     by dependency injection frameworks when Aft Off is not configured.
    /// </summary>
    public class NullAftOffTransferProvider : AftTransferProviderBase, IAftOffTransferProvider, IWatTransferOffProvider, ITransactionRequestor
    {
        /// <summary>Initializes a new instance of the NullAftOffTransferProvider class.</summary>
        /// <param name="sasHost">The sas host</param>
        /// <param name="aftLock">The aft lock handler</param>
        /// <param name="propertiesManager">the properties manager</param>
        /// <param name="transactionCoordinator">The transaction coordinator</param>
        /// <param name="timeService">the timer service</param>
        /// <param name="autoPlayStatusProvider">the auto play provider service</param>
        public NullAftOffTransferProvider(
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
        public bool CanTransfer { get; } = false;

        /// <inheritdoc />
        public bool IsAftPending => false;

        /// <inheritdoc />
        public Guid RequestorGuid => Guid.Empty;

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IAftOffTransferProvider), typeof(IWatTransferOffProvider) };

        /// <inheritdoc />
        public bool IsAftOffAvailable => false;

        /// <inheritdoc />
        public bool InitiateAftOff()
        {
            return false;
        }

        /// <inheritdoc />
        public void CancelAftOff()
        {
        }

        /// <inheritdoc />
        public bool AftOffRequest(AftData data, bool partialAllowed)
        {
            return false;
        }

        /// <inheritdoc />
        public void AftOffRejected()
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
        public Task<bool> InitiateTransfer(WatTransaction transaction)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task CommitTransfer(WatTransaction transaction)
        {
            return null;
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