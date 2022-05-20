namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A rule that blocks clearing of persisted data unless the logic door is open.
    /// </summary>
    public abstract class BasePersistenceClearRule : IPersistenceClearRule, IDisposable
    {
        private bool _disposed;

        /// <summary>Disposes the object</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public event EventHandler RuleChangedEvent;

        /// <inheritdoc />
        public bool PartialClearAllowed { get; private set; }

        /// <inheritdoc />
        public bool FullClearAllowed { get; private set; }

        /// <inheritdoc />
        public string ClearDeniedReason { get; private set; }

        /// <summary>
        ///     Sets whether or not partial and full clear are allowed, and raises the required
        ///     RuleChangedEvent if the new value of either differs from the current value.
        /// </summary>
        /// <remarks>
        ///     The denyReason could change while partial or full clear is disallowed.  We want to raise
        ///     a RuleChangedEvent in this scenario.
        /// </remarks>
        /// <param name="partialClearAllowed">Indicates whether or not partial clear is allowed</param>
        /// <param name="fullClearAllowed">Indicates whether or not full clear is allowed</param>
        /// <param name="denyReason">The human-readable reason that clear would be denied.</param>
        protected void SetAllowed(bool partialClearAllowed, bool fullClearAllowed, string denyReason)
        {
            var changed = PartialClearAllowed != partialClearAllowed;
            changed |= FullClearAllowed != fullClearAllowed;

            if (!partialClearAllowed || !fullClearAllowed)
            {
                changed |= ClearDeniedReason != denyReason;
            }

            PartialClearAllowed = partialClearAllowed;
            FullClearAllowed = fullClearAllowed;
            ClearDeniedReason = denyReason;

            if (changed)
            {
                RuleChangedEvent?.Invoke(this, null);
            }
        }

        /// <summary>
        ///     Disposes the object
        /// </summary>
        /// <param name="disposing">Indicates whether or not to clean up managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (disposing)
            {
                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
            }
        }
    }
}