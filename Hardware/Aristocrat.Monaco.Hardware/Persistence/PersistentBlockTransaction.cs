namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System;
    using Contracts.Persistence;

    /// <summary> A persistent block transaction. </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IPersistentTransaction"/>
    public class PersistentBlockTransaction : IPersistentTransaction
    {
        private readonly string _blockName;
        private readonly IPersistentTransaction _transaction;

        /// <inheritdoc/>
        public event EventHandler<TransactionEventArgs> Completed;

        /// <summary> Constructor. </summary>
        /// <param name="blockName">   Name of the block. </param>
        /// <param name="transaction"> The transaction. </param>
        public PersistentBlockTransaction(string blockName, IPersistentTransaction transaction)
        {
            _blockName = blockName;
            _transaction = transaction;
            _transaction.Completed += OnCompleted;
        }

        /// <inheritdoc/>
        public bool GetValue<T>(out T value)
        {
            return _transaction.GetValue(_blockName, out value);
        }

        /// <inheritdoc/>
        public bool GetValue<T>(string key, out T value)
        {
            return _transaction.GetValue(KeyCreator.BlockFieldKey(_blockName, key), out value);
        }

        /// <inheritdoc/>
        public bool GetValue<T>(int index, out T value)
        {
            return _transaction.GetValue(_blockName, index, out value);
        }

        /// <inheritdoc/>
        public bool GetValue<T>(string key, int index, out T value)
        {
            return _transaction.GetValue(KeyCreator.BlockFieldKey(_blockName, key), index, out value);
        }

        /// <inheritdoc/>
        public T GetOrCreateValue<T>() where T : new()
        {
            return _transaction.GetOrCreateValue<T>(_blockName);
        }

        /// <inheritdoc/>
        public T GetOrCreateValue<T>(string key) where T : new()
        {
            return _transaction.GetOrCreateValue<T>(KeyCreator.BlockFieldKey(_blockName, key));
        }

        /// <inheritdoc/>
        public bool SetValue<T>(T value)
        {
            return _transaction.SetValue(_blockName, value);
        }

        /// <inheritdoc/>
        public bool SetValue<T>(string key, T value)
        {
            return _transaction.SetValue(KeyCreator.BlockFieldKey(_blockName, key), value);
        }

        /// <inheritdoc/>
        public bool SetValue<T>(int index, T value)
        {
            return _transaction.SetValue(_blockName, index, value);
        }

        /// <inheritdoc/>
        public bool SetValue<T>(string key, int index, T value)
        {
            return _transaction.SetValue(KeyCreator.BlockFieldKey(_blockName, key), index, value);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public bool Commit()
        {
            return _transaction.Commit();
        }

        protected virtual void OnCompleted(object sender, TransactionEventArgs e)
        {
            Completed?.Invoke(sender, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _transaction.Completed -= OnCompleted;
            }
        }
    }
}