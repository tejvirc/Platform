namespace Aristocrat.Monaco.Hardware.StorageAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SQLite;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Contracts;
    using Contracts.Persistence;
    using Kernel;
    using log4net;
    using StorageSystem;

    /// <summary>
    ///     Implementation of <c>IPersistentStorageManager</c> that uses Block repository to store and manage Blocks.
    /// </summary>
    public class SqlPersistentStorageManager : IService, IPersistentStorageManager, IPersistenceSqlConnectionProvider, IDisposable
    {
        private const string StorageBlockTableCreate =
            "CREATE TABLE StorageBlock (Name TEXT PRIMARY KEY NOT NULL, Version INTEGER, Level TEXT, Count INTEGER) WITHOUT ROWID";

        private const string StorageBlockFieldTableCreate =
            "CREATE TABLE StorageBlockField (BlockName TEXT NOT NULL, FieldName TEXT NOT NULL, DataType TEXT, Data NONE, Count INTEGER, PRIMARY KEY(BlockName, FieldName)) WITHOUT ROWID";

        private const string StorageBlockNamesSelect = "SELECT Name FROM StorageBlock";

        private const string StorageBlockExistsFormat =
            "SELECT EXISTS (SELECT Name FROM StorageBlock WHERE Name = '{0}')";

        private const string StorageBlockStartWithFormat =
            "SELECT Name FROM StorageBlock WHERE Name LIKE '{0}%'";

        private const string StorageBlockLevelClearFormat =
            "DELETE FROM StorageBlockField WHERE StorageBlockField.BlockName IN (SELECT StorageBlock.Name FROM StorageBlock WHERE StorageBlock.Level IN ({0})); DELETE FROM StorageBlock WHERE Level IN ({0})";

        private const string UpdatePersistenceLevelStatement =
            "UPDATE StorageBlock SET Level = '{1}' WHERE Name = '{0}'";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool _initializedSqlMirror;

        private readonly Dictionary<string, SqlPersistentStorageAccessor> _accessors =
            new Dictionary<string, SqlPersistentStorageAccessor>();

        private readonly IPathMapper _pathMapper;
        private readonly IEventBus _bus;
        private readonly string _databaseFilename;
        private readonly string _databasePassword;

        private readonly object _sync = new object();

        private string _dataRoot;

        private bool _disposed;
        private string _mirrorRoot;
        private SQLiteConnection _connection;
        private readonly bool _keepConnectionOpen;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlPersistentStorageManager" /> class.
        /// </summary>
        public SqlPersistentStorageManager()
            : this(
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                StorageConstants.DatabaseFileName,
                StorageConstants.DatabasePassword,
                true)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlPersistentStorageManager" /> class.
        /// </summary>
        public SqlPersistentStorageManager(
            IPathMapper pathMapper,
            IEventBus bus,
            string name = StorageConstants.DatabaseFileName,
            string password = StorageConstants.DatabasePassword,
            bool keepConnectionOpen = false)
        {
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            _databaseFilename = name;
            _databasePassword = password;
            _keepConnectionOpen = keepConnectionOpen;

            InternalInitialize();
        }

        /// <summary>
        ///     Gets a value indicating whether persistent storage clear has started.
        /// </summary>
        public bool ClearStarted { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public event EventHandler<StorageEventArgs> StorageClearingEventHandler;

        /// <inheritdoc />
        public event EventHandler<StorageEventArgs> StorageClearedEventHandler;

        /// <inheritdoc />
        public void Clear(PersistenceLevel level)
        {
            ClearStarted = true;

            _bus.Publish(new PersistentStorageClearStartedEvent(level));
        }

        /// <inheritdoc />
        public bool VerifyIntegrity(bool full)
        {
            bool verified;

            try
            {
                verified = SqlExecuteScalar(full ? "PRAGMA integrity_check(1)" : "PRAGMA quick_check(1)").ToString() == "ok";
            }
            catch (Exception)
            {
                verified = false;
            }

            if (!verified)
            {
                Logger.Error($"Persistent Storage failure: {MethodBase.GetCurrentMethod()?.Name}");

                _bus.Publish(new PersistentStorageIntegrityCheckFailedEvent());
            }

            return verified;
        }

        /// <inheritdoc />
        public void Defragment()
        {
            SqlExecuteScalar(@"VACUUM");
        }

        /// <inheritdoc />
        public IPersistentStorageAccessor CreateBlock(PersistenceLevel level, string name, int arraySize)
        {
            return CreateDynamicBlock(level, name, arraySize, null);
        }

        /// <inheritdoc />
        public IPersistentStorageAccessor CreateDynamicBlock(
            PersistenceLevel level,
            string name,
            int arraySize,
            BlockFormat format)
        {
            lock (_sync)
            {
                if (BlockExists(name))
                {
                    var errorMessage = $"Block '{name}' already exists.";
                    Logger.Error(errorMessage);
                    throw new DuplicateBlockException(errorMessage);
                }

                try
                {
                    using (var connection = CreateConnection())
                    {
                        connection.Open();

                        using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                        {
                            CreateStorageBlockEntities(transaction, level, name, 0, arraySize);

                            var accessor = _accessors[name] =
                                new SqlPersistentStorageAccessor(this, transaction, name, arraySize, format);

                            transaction.Commit();

                            return accessor;
                        }
                    }
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);

                    // This should really return null, but the callers of this are not expecting a null value
                    return _accessors[name] = SqlPersistentStorageAccessor.InvalidAccessor(name, arraySize, format);
                }
            }
        }

        /// <inheritdoc />
        public bool BlockExists(string name)
        {
            lock (_sync)
            {
                long exists = 0;

                if (_accessors.ContainsKey(name))
                {
                    return true;
                }

                try
                {
                    exists = (long)SqlExecuteScalar(string.Format(StorageBlockExistsFormat, name));
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ReadFailure);
                }

                return exists == 1;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IPersistentStorageAccessor> GetBlocksStartWith(string name)
        {
            var blocks = new List<IPersistentStorageAccessor>();
            lock (_sync)
            {
                try
                {
                    using var connection = CreateConnection();
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = string.Format(StorageBlockStartWithFormat, name);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var key = reader.GetString(0);
                                blocks.Add(GetPersistentStorageAccessorByName(key));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ReadFailure);
                }
            }
            return blocks;
        }


        /// <inheritdoc />
        public IPersistentStorageAccessor GetBlock(string name)
        {
            IPersistentStorageAccessor accessor = GetPersistentStorageAccessorByName(name);

            if (accessor == null)
            {
                SqlPersistentStorageExceptionHandler.Handle(
                    new KeyNotFoundException(nameof(accessor)),
                    StorageError.InvalidHandle);
            }

            return accessor;
        }

        /// <inheritdoc />
        public void ResizeBlock(string name, int size)
        {
            var accessor = GetPersistentStorageAccessorByName(name);

            accessor.Resize(size);
        }

        /// <inheritdoc />
        public void UpdatePersistenceLevel(string name, PersistenceLevel level)
        {
            try
            {
                SqlExecuteNonQuery(string.Format(UpdatePersistenceLevelStatement, name, level));
            }
            catch (Exception e)
            {
                SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
            }
        }

        /// <inheritdoc />
        public IScopedTransaction ScopedTransaction()
        {
            return new ScopedTransaction(this);
        }

        /// <inheritdoc />
        public string Name => typeof(SqlPersistentStorageManager).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPersistentStorageManager) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <summary>
        ///     Opens a connection to the database. To benefit from the .NET connection pool  sharing, we need
        ///     to have at least one connection opened. 
        /// </summary>
        public SQLiteConnection CreateConnection()
        {
            SQLiteConnection connection;
            if (_keepConnectionOpen)
            {
                if (_connection == null)
                {
                    lock (_sync)
                    {
                        // Use double check locking pattern to avoid unnecessary locks
                        if (_connection == null)
                        {
                            _connection = new SQLiteConnection(ConnectionString());
                            _connection.Open();
                        }
                    }
                }
                connection = new SQLiteConnection(_connection.ConnectionString);
            }
            else
            {
                if (_connection != null)
                {
                    ClosePersistentConnection();
                }

                connection = new SQLiteConnection(ConnectionString());
            }

            return connection;
        }

        /// <summary>
        ///     Handles cleaning up the object instance.
        /// </summary>
        /// <param name="disposing">Indicates whether or not the class is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
                //Copy ClosePersistentConnection() here to satisfy SonarQube
                if (_connection != null)
                {
                    lock (_sync)
                    {
                        // Use double check locking pattern to avoid unnecessary locks
                        if (_connection != null)
                        {
                            _connection.Close();
                            _connection.Dispose();
                            _connection = null;
                        }
                    }
                }
            }

            _disposed = true;
        }

        private string ConnectionString()
        {
            const int retries = 10;
            const int timeout = 15000;
            var fileName = GetFileName();

            var sqlBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = fileName,
                Pooling = true,
                PrepareRetries = retries,
                FailIfMissing = true,
                JournalMode = SQLiteJournalModeEnum.Wal,
                SyncMode = SynchronizationModes.Full,
                DefaultIsolationLevel = IsolationLevel.Serializable,
                BusyTimeout = timeout,
                ["Max Pool Size"] = int.MaxValue,
                Password = _databasePassword
            };

            return $"{sqlBuilder.ConnectionString};";
        }


        /// <summary>
        ///     Closes the persistent connection if any.
        /// </summary>
        private void ClosePersistentConnection()
        {
            if (_connection != null)
            {
                lock (_sync)
                {
                    // Use double check locking pattern to avoid unnecessary locks
                    if (_connection != null)
                    {
                        _connection.Close();
                        _connection.Dispose();
                        _connection = null;
                    }
                }
            }

        }

        private string GetFileName()
        {
            return GetFileName(_dataRoot);
        }

        private string GetFileName(string dataRoot)
        {
            return Path.Combine(dataRoot, _databaseFilename);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Safe to suppress. No user input.")]
        private void SqlExecuteNonQuery(string commandText)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand(commandText, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Safe to suppress. No user input")]
        private object SqlExecuteScalar(string commandText)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand(commandText, connection))
                {
                    return command.ExecuteScalar();
                }
            }
        }

        private SqlPersistentStorageAccessor GetPersistentStorageAccessorByName(string name)
        {
            return !_accessors.TryGetValue(name, out var accessor) ? null : accessor;
        }

        private void CreateStorageBlockEntities(
            SQLiteTransaction transaction,
            PersistenceLevel level,
            string name,
            int version,
            int arraySize)
        {
            const string sql =
                @"insert into StorageBlock (Name, Level, Version, Count) values (@StorageBlockName, @StorageBlockLevel, @StorageBlockVersion, @StorageBlockCount)";
            using (var command = new SQLiteCommand(sql, transaction.Connection, transaction))
            {
                command.Parameters.Add("@StorageBlockName", DbType.String).Value = name;
                command.Parameters.Add("@StorageBlockLevel", DbType.String).Value = level;
                command.Parameters.Add("@StorageBlockVersion", DbType.Int16).Value = version;
                command.Parameters.Add("@StorageBlockCount", DbType.Int16).Value = arraySize;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    Logger.Error(
                        $"Persistent Storage failure: {MethodBase.GetCurrentMethod()?.Name} {e} {e.InnerException} {e.StackTrace}");

                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
                }
            }
        }

        private void InternalInitialize()
        {
            _bus.Subscribe<PersistentStorageClearReadyEvent>(this, HandleEvent);

            _dataRoot = _pathMapper.GetDirectory(HardwareConstants.DataPath).FullName;

            ClearStarted = false;

            InitializeDatabaseFile();

            UpdateRepository();

            var now = DateTime.UtcNow;

            if (!BlockExists(StorageConstants.PersistenceStaticCleared))
            {
                var staticLevel = CreateBlock(PersistenceLevel.Static, StorageConstants.PersistenceStaticCleared, 1);
                staticLevel["LastClearTime"] = now;
            }

            if (!BlockExists(StorageConstants.PersistenceCriticalCleared))
            {
                var staticLevel = CreateBlock(
                    PersistenceLevel.Critical,
                    StorageConstants.PersistenceCriticalCleared,
                    1);
                staticLevel["LastClearTime"] = now;
                staticLevel["JustExecuted"] = true;
            }

            if (!BlockExists(StorageConstants.PersistenceTransientCleared))
            {
                var staticLevel = CreateBlock(
                    PersistenceLevel.Transient,
                    StorageConstants.PersistenceTransientCleared,
                    1);
                staticLevel["LastClearTime"] = now;
            }

            Defragment();
        }

        private void ValidateAndCheckMirror(string dataRoot)
        {
            if (_initializedSqlMirror)
            {
                return;
            }

            _initializedSqlMirror = true;

            _mirrorRoot = SqlSecondaryStorageManager.GetMirrorPath(dataRoot);

            if (_mirrorRoot == null)
            {
                Logger.Debug("Data mirror is disabled.");
                return;
            }

            // In case a persistent connection is open, close it.
            // Note: The wal and shm files which might still be present in the primary space (e.g: crash ) but they will be removed during the integrity
            // check of the database by the check itself.
            ClosePersistentConnection();

            var secondaryStorageManager = ServiceManager.GetInstance().GetService<ISecondaryStorageManager>();

            secondaryStorageManager.SetPaths(dataRoot, _mirrorRoot);

            if (!secondaryStorageManager.Verify())
            {
                Logger.Debug("Secondary media initialization failed.");

                return;
            }

            Logger.Debug($"Data mirror is enabled on {_mirrorRoot}.");

            NativeMethods.set_mirror_dir(_mirrorRoot);

            var mirrorFileName = GetFileName(_mirrorRoot);

            if (File.Exists(mirrorFileName))
            {
                return;
            }

            SQLiteConnection.CreateFile(mirrorFileName);
        }

        private void InitializeDatabaseFile()
        {
            var dataRoot = _dataRoot;
            lock (_sync)
            {
                ValidateAndCheckMirror(dataRoot);

                if (File.Exists(GetFileName()))
                {
                    return;
                }

                SQLiteConnection.CreateFile(GetFileName());
                using (var connection = CreateConnection())
                {
                    connection.Open();

                    using (var command = new SQLiteCommand(connection))
                    {
                        try
                        {
                            command.CommandText = StorageBlockTableCreate;
                            command.ExecuteNonQuery();

                            command.CommandText = StorageBlockFieldTableCreate;
                            command.ExecuteNonQuery();
                        }
                        catch (SQLiteException e)
                        {
                            Logger.Error(
                                $"Persistent Storage failure: {MethodBase.GetCurrentMethod().Name} {e} {e.InnerException} {e.StackTrace}");

                            SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
                        }
                        catch (Exception e)
                        {
                            SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
                        }
                    }
                }
            }
        }

        private void UpdateRepository()
        {
            lock (_sync)
            {
                if (!File.Exists(GetFileName()))
                {
                    return;
                }

                using (var connection = CreateConnection())
                {
                    connection.Open();

                    using (var command = new SQLiteCommand(StorageBlockNamesSelect, connection))
                    {
                        try
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var name = reader.GetString(0);
                                    _accessors[name] = new SqlPersistentStorageAccessor(this, name);
                                }
                            }
                        }
                        catch (SQLiteException e)
                        {
                            Logger.Error(
                                $"Persistent Storage failure: {MethodBase.GetCurrentMethod().Name} {e} {e.InnerException} {e.StackTrace}");

                            SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ReadFailure);
                        }
                        catch (Exception e)
                        {
                            SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ReadFailure);
                        }
                    }
                }
            }
        }

        private void OnClearingStorage(PersistenceLevel level)
        {
            StorageClearingEventHandler?.Invoke(this, new StorageEventArgs(level));
        }

        private void OnClearedStorage(PersistenceLevel level)
        {
            StorageClearedEventHandler?.Invoke(this, new StorageEventArgs(level));
        }

        private void HandleEvent(PersistentStorageClearReadyEvent clearReadyEvent)
        {
            var level = clearReadyEvent.Level;
            var sb = new StringBuilder();
            foreach (PersistenceLevel l in Enum.GetValues(typeof(PersistenceLevel)))
            {
                if (l >= level)
                {
                    sb.AppendFormat($"'{l}',");
                }
            }

            var levels = sb.ToString().TrimEnd(',');

            Logger.Debug($"Clear Ready:  Clearing Persistence Levels: {levels}");

            lock (_sync)
            {
                OnClearingStorage(level);

                Logger.Debug($"Cleared subscribers: {levels}");

                try
                {
                    // Clear all existing fields
                    SqlExecuteNonQuery(string.Format(StorageBlockLevelClearFormat, levels));

                    _accessors.Clear();

                    UpdateRepository();

                    OnClearedStorage(level);

                    Logger.Debug($"Cleared: {levels}");

                    Defragment();

                    VerifyIntegrity(true);

                    _bus.Publish(new PersistentStorageClearedEvent(level));

                    SqlPersistentStorageExceptionHandler.ClearFaultedState();
                }
                catch (SQLiteException e)
                {
                    Logger.Error(
                        $"Persistent Storage failure: {MethodBase.GetCurrentMethod().Name} {e} {e.InnerException} {e.StackTrace}");

                    // Remove all handles on the file
                    new SQLiteConnection(ConnectionString()).Close();
                    GC.Collect();

                    // Attempt to delete the file
                    var worked = false;
                    var tries = 1;
                    while (tries < 4 && !worked)
                    {
                        try
                        {
                            Thread.Sleep(tries * 100);
                            File.Delete(GetFileName());

                            if (_mirrorRoot != null)
                            {
                                File.Delete(GetFileName(_mirrorRoot));
                            }

                            worked = true;
                        }
                        catch (IOException)
                        {
                            tries++;
                        }
                    }

                    if (!worked)
                    {
                        Logger.Error(
                            $"Persistent Storage failure: {MethodBase.GetCurrentMethod().Name} Unable to clear nor delete NVRam database.");

                        SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ClearFailure);
                    }
                    else
                    {
                        OnClearedStorage(level);
                    }
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ClearFailure);
                }
            }
        }
    }
}