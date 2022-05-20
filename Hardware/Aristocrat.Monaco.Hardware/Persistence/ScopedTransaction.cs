namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System;
    using Contracts.Persistence;

    /// <summary> A scoped transaction. </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IScopedTransaction"/>
    public class ScopedTransaction : PersistentTransaction, IScopedTransaction
    {
        private bool _completed;

        public new EventHandler<EventArgs> Completed;

        public ScopedTransaction(IKeyAccessor accessor)
            : base(accessor)
        {
            _completed = false;
        }

        /// <inheritdoc/>
        public void Complete()
        {
            base.Commit();
            _completed = true;
            OnCompleted();
        }

        /// <inheritdoc/>
        public override bool Commit()
        {
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_completed)
                    return;

                Updates.Clear();
                IndexedUpdates.Clear();
            }
        }

        protected virtual void OnCompleted()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }
}