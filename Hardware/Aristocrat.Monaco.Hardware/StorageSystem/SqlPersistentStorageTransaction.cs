namespace Aristocrat.Monaco.Hardware.StorageSystem
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.SQLite;
    using System.Globalization;
    using System.Reflection;
    using Common.Storage;
    using Contracts.Persistence;
    using log4net;
    using StorageAdapters;

    /// <summary>
    ///     Definition of the SqlPersistentStorageTransaction class
    /// </summary>
    [DbConfigurationType(typeof(SQLiteConfiguration))]
    public class SqlPersistentStorageTransaction : IPersistentStorageTransaction
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<SqlPersistentStorageAccessor> _blocks = new List<SqlPersistentStorageAccessor>();

        private readonly IPersistenceSqlConnectionProvider _connectionProvider;

        // Dictionary<Name of Block, Dictionary<Name of Field, Value of Field>>
        private readonly Dictionary<string, Dictionary<string, object>> _fields =
            new Dictionary<string, Dictionary<string, object>>();

        // Dictionary<Name of Block, Dictionary<Tuple<Index of Field, Name of Field>, Value of Field at Index>>
        private readonly Dictionary<string, Dictionary<Tuple<int, string>, object>> _indexedFields =
            new Dictionary<string, Dictionary<Tuple<int, string>, object>>();

        private int _currentIndex;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlPersistentStorageTransaction" /> class.
        /// </summary>
        /// <param name="connectionProvider">Provides a connection to the database.</param>
        public SqlPersistentStorageTransaction(IPersistenceSqlConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlPersistentStorageTransaction" /> class.
        /// </summary>
        /// <param name="block">Accessor to use for transaction.</param>
        /// <param name="connectionProvider">Provides a connection to the database.</param>
        public SqlPersistentStorageTransaction(IPersistentStorageAccessor block, IPersistenceSqlConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
            AddBlock(block); // This is the first and default block
        }

        /// <inheritdoc />
        public event EventHandler<TransactionEventArgs> OnCompleted;

        /// <inheritdoc />
        public object this[string blockFieldName]
        {
            get => this[_blocks[_currentIndex].Name, blockFieldName];

            set => this[_blocks[_currentIndex].Name, blockFieldName] = value;
        }

        /// <inheritdoc />
        public object this[int arrayIndex, string blockFieldName]
        {
            get => this[_blocks[_currentIndex].Name, arrayIndex, blockFieldName];

            set => this[_blocks[_currentIndex].Name, arrayIndex, blockFieldName] = value;
        }

        /// <inheritdoc />
        public object this[string blockName, string blockFieldName]
        {
            get
            {
                if (_fields.ContainsKey(blockName) && _fields[blockName].ContainsKey(blockFieldName))
                {
                    return _fields[blockName][blockFieldName];
                }

                var block = _blocks.Find(b => b.Name == blockName);
                return block[blockFieldName];
            }

            set
            {
                if (!_fields.ContainsKey(blockName))
                {
                    _fields[blockName] = new Dictionary<string, object>();
                }

                _fields[blockName][blockFieldName] = value;
            }
        }

        /// <inheritdoc />
        public object this[string blockName, int arrayIndex, string blockFieldName]
        {
            get
            {
                var key = new Tuple<int, string>(arrayIndex, blockFieldName);
                if (_indexedFields.ContainsKey(blockName) && _indexedFields[blockName].ContainsKey(key))
                {
                    return _indexedFields[blockName][key];
                }

                var block = _blocks.Find(b => b.Name == blockName);
                return block[arrayIndex, blockFieldName];
            }

            set
            {
                if (!_indexedFields.ContainsKey(blockName))
                {
                    _indexedFields[blockName] = new Dictionary<Tuple<int, string>, object>();
                }

                _indexedFields[blockName][new Tuple<int, string>(arrayIndex, blockFieldName)] = value;
            }
        }

        /// <inheritdoc />
        public void AddBlock(IPersistentStorageAccessor block)
        {
            var accessor = (SqlPersistentStorageAccessor)block;

            _currentIndex = _blocks.IndexOf(accessor);
            if (_currentIndex == -1)
            {
                _currentIndex = _blocks.Count;
                _blocks.Add((SqlPersistentStorageAccessor)block);
            }
        }

        /// <inheritdoc />
        public void Commit()
        {
            if (PersistenceTransaction.Current != null && !PersistenceTransaction.Ready)
            {
                return;
            }

            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    connection.Open();

                    using (var update = connection.CreateCommand())
                    using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        update.Transaction = transaction;

                        foreach (var block in _blocks)
                        {
                            update.CommandText =
                                "UPDATE StorageBlockField SET Data = @Data WHERE BlockName = @BlockName AND FieldName = @FieldName";
                            update.Parameters.Add(new SQLiteParameter("@BlockName", block.Name));
                            update.Parameters.Add(new SQLiteParameter("@FieldName", string.Empty));
                            update.Parameters.Add(new SQLiteParameter("@Data", DbType.Binary));

                            if (_fields.ContainsKey(block.Name))
                            {
                                foreach (var field in _fields[block.Name])
                                {
                                    UpdateField(update, block, field.Key, field.Value);
                                }
                            }

                            if (_indexedFields.ContainsKey(block.Name))
                            {
                                foreach (var field in _indexedFields[block.Name])
                                {
                                    UpdateField(update, block, field.Key.Item2, field.Value, field.Key.Item1);
                                }
                            }
                        }

                        transaction.Commit();
                        NotifyComplete(true);
                    }
                }
            }
            catch (Exception e)
            {
                SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
            }
        }

        /// <inheritdoc />
        public void Rollback()
        {
            if (PersistenceTransaction.Current != null && !PersistenceTransaction.Ready)
            {
                return;
            }

            foreach (var block in _blocks)
            {
                if (_fields.ContainsKey(block.Name))
                {
                    _fields[block.Name].Clear();
                }

                if (_indexedFields.ContainsKey(block.Name))
                {
                    _indexedFields[block.Name].Clear();
                }
            }

            _fields.Clear();
            _indexedFields.Clear();

            NotifyComplete(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Rollback();
            }

            _disposed = true;
        }

        private static void UpdateField(
            SQLiteCommand update,
            SqlPersistentStorageAccessor block,
            string blockFieldName,
            object data,
            int arrayIndex = -1)
        {
            var fieldName = arrayIndex >= 1 ? blockFieldName + "@" + arrayIndex : blockFieldName;

            update.Parameters["@FieldName"].Value = fieldName;
            update.Parameters["@Data"].Value = block.Format.ConvertTo(blockFieldName, data);

            if (update.ExecuteNonQuery() == 0)
            {
                // This shouldn't happen
                Logger.ErrorFormat(CultureInfo.InvariantCulture, $"{block.Name}: Failed to update {fieldName} - Zero rows affected");
                throw new BlockFieldNotFoundException(
                    $"StorageBlockField not found in SQLite repository: {block.Name}.{fieldName}");
            }
        }

        private void NotifyComplete(bool committed)
        {
            OnCompleted?.Invoke(this, new TransactionEventArgs(committed));
        }
    }
}