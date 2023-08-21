namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Kernel;

    /// <summary>
    ///     Provides an Aft lock handler that doesn't do anything
    ///     for use with dependency injection when Aft is not configured.
    /// </summary>
    public class NullAftLockHandler : IAftLockHandler, IService, ITransactionRequestor
    {
        /// <inheritdoc />
        public event EventHandler<EventArgs> OnLocked;

        /// <inheritdoc />
        public AftGameLockStatus LockStatus => AftGameLockStatus.GameNotLocked;

        /// <inheritdoc />
        public string Name => typeof(NullAftLockHandler).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAftLockHandler) };

        /// <inheritdoc />
        public Guid RequestorGuid => Guid.Empty;

        /// <inheritdoc />
        public void AftLock(bool requestLock, uint timeout)
        {
            OnLocked?.Invoke(this, null);
        }

        /// <inheritdoc />
        public Guid RetrieveTransactionId()
        {
            return Guid.Empty;
        }

        /// <inheritdoc />
        public AftTransferConditions AftLockTransferConditions { get; set; }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public void NotifyTransactionReady(Guid requestId)
        {
        }
    }
}