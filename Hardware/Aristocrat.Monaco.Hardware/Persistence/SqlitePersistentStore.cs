namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Contracts.Persistence;
    using Kernel;
    using log4net;
    using Microsoft.Data.Sqlite;
    using Newtonsoft.Json;

    /// <summary> A sqlite persistent store. </summary>
    /// <seealso cref="IPersistentStore" />
    /// <seealso cref="IDisposable" />
    public class SqlitePersistentStore : IPersistentStore, IDisposable
    {
        private const string CreateTablesCommand = @"CREATE TABLE KeyLevelTable (Key TEXT PRIMARY KEY NOT NULL, Level TEXT NOT NULL); CREATE TABLE KeyValueTable (Key TEXT PRIMARY KEY NOT NULL, Value BLOB)";
        private const string IfKeyLevelExistsCommand = @"SELECT EXISTS (SELECT Key FROM KeyLevelTable WHERE Key = @Key)";
        private const string AddOrUpdateKeyLevelCommand = @"INSERT OR REPLACE INTO KeyLevelTable (Key, Level) VALUES (@Key, @Level)";
        private const string GetKeyLevelCommand = @"SELECT Level FROM KeyLevelTable WHERE Key = @Key";
        private const string GetAllKeyLevelsCommand = @"SELECT * FROM KeyLevelTable";
        private const string AddOrUpdateKeyValueCommand = @"INSERT OR REPLACE INTO KeyValueTable (Key, Value) VALUES (@Key, @Value)";

        private const string GetKeyValueCommand = @"SELECT Value FROM KeyValueTable WHERE Key = @Key";
        private const string GetAllValueKeysCommand = @"SELECT Key FROM KeyValueTable";
        private const string DeleteKeyValueCommand = @"DELETE FROM KeyValueTable WHERE Key = @Key";

        private const string FullIntegrityCheckCommand = @"PRAGMA integrity_check(1)";
        private const string QuickIntegrityCheckCommand = @"PRAGMA quick_check(1)";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connection;
        private readonly string _path;

        public SqlitePersistentStore(string directory, string name)
        {
            var databaseDirectoryName = directory ?? throw new ArgumentNullException(nameof(directory));
            var databaseFileName = name ?? throw new ArgumentNullException(nameof(name));

            var databaseDirectoryPath = ServiceManager.GetInstance().GetService<IPathMapper>()
                .GetDirectory(databaseDirectoryName);

            if (!databaseDirectoryPath.Exists)
            {
                throw new DirectoryNotFoundException("SqlitePersistentStore creation failed.");
            }

            _path = Path.Combine(databaseDirectoryPath.FullName, databaseFileName);
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = _path,
                Pooling = true,
                Password = SqliteStoreConstants.DatabasePassword,
                DefaultTimeout = 15
            };

            _connection = builder.ConnectionString + ";FailIfMissing=True;Journal Mode=WAL;Synchronous=FULL;";

            if (File.Exists(_path))
            {
                return;
            }

            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var command = new SqliteCommand(CreateTablesCommand, connection);
                command.ExecuteNonQuery();
            }
            catch (SqliteException exception)
            {
                Logger.Error($"{SqliteStoreConstants.DatabaseName} creation failed: {exception.Message}");
            }
        }

        public SqlitePersistentStore()
            : this(SqliteStoreConstants.DataPath, SqliteStoreConstants.DatabaseName)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool AddOrUpdateLevel(string key, PersistenceLevel level)
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = AddOrUpdateKeyLevelCommand;
                    command.Parameters.Add(new SqliteParameter("@Key", key));
                    command.Parameters.Add(new SqliteParameter("@Level", level));

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (InvalidOperationException)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (SqliteException exception)
            {
                Logger.Error($"Error in updating level {level} of key {key} in {SqliteStoreConstants.DatabaseName}: {exception.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public bool TryGetLevel(string key, out PersistenceLevel level)
        {
            if (!KeyLevelExists(key))
            {
                level = default(PersistenceLevel);
                return false;
            }

            level = GetLevel(key);
            return true;
        }

        /// <inheritdoc />
        public bool TryRemoveLevel(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryRemoveLevel(IEnumerable<string> keys)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, PersistenceLevel>> LevelData()
        {
            var keyLevels = new Dictionary<string, PersistenceLevel>();
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    using var command = connection.CreateCommand();
                    command.CommandText = GetAllKeyLevelsCommand;
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var key = reader.GetString(0);
                        var level = (PersistenceLevel)Enum.Parse(typeof(PersistenceLevel), reader.GetString(1));
                        keyLevels.Add(key, level);
                    }
                }

                return keyLevels.AsEnumerable();
            }
            catch (SqliteException)
            {
                Logger.Error($"Error in getting key persistence levels from {SqliteStoreConstants.DatabaseName}");
                return null;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> LevelKeys()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool AddOrUpdateValue<T>(string key, T value)
        {
            var serialized = JsonConvert.SerializeObject(value, Formatting.None);
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = AddOrUpdateKeyValueCommand;
                    command.Parameters.Add(new SqliteParameter("@Key", key));
                    command.Parameters.Add(new SqliteParameter("@Value", serialized));

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (InvalidOperationException)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (SqliteException exception)
            {
                Logger.Error($"Error in adding/updating key {key} in {SqliteStoreConstants.DatabaseName}: {exception.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public bool AddOrUpdateValue(IEnumerable<(string key, object value)> values)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = AddOrUpdateKeyValueCommand;
                            command.Parameters.Add(new SqliteParameter("@Key", DbType.String));
                            command.Parameters.Add(new SqliteParameter("@Value", DbType.Binary));
                            foreach (var (key, value) in values)
                            {
                                var serialized = JsonConvert.SerializeObject(value, Formatting.None);
                                command.Parameters["@Key"].Value = key;
                                command.Parameters["@Value"].Value = Encoding.ASCII.GetBytes(serialized); // TODO: Is this correct encoding ?
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (InvalidOperationException)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (SqliteException exception)
            {
                Logger.Error($"Error in adding/updating keys in {SqliteStoreConstants.DatabaseName}: {exception.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public bool TryGetValue<T>(string key, out T value)
        {
            return InternalTryGetValue(key, out value);
        }

        /// <inheritdoc />
        public bool TryRemoveValue(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryRemoveValue(IEnumerable<string> keys)
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = DeleteKeyValueCommand;
                    command.Parameters.Add(new SqliteParameter("@Key", DbType.String));
                    foreach (var key in keys)
                    {
                        command.Parameters["@Key"].Value = key;
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (InvalidOperationException)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (SqliteException exception)
            {
                Logger.Error($"Error in deleting keys from KeyValue table in {SqliteStoreConstants.DatabaseName}: {exception.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, object>> ValueData()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<string> ValueKeys()
        {
            var keys = new List<string>();
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = GetAllValueKeysCommand;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                keys.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                return keys;
            }
            catch (SqliteException)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public bool Verify(bool full)
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = full ? FullIntegrityCheckCommand : QuickIntegrityCheckCommand;
                var result = command.ExecuteScalar();
                return result.ToString().Equals("ok");
            }
            catch (SqliteException exception)
            {
                Logger.Error($"{SqliteStoreConstants.DatabaseName} verification failed: {exception.Message}");
                return false;
            }
        }

        public void DeleteStore()
        {
            var connection = CreateConnection();
            connection.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(_path);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        private SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connection);
            return connection;
        }

        private bool InternalTryGetValue<T>(string key, out T value)
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = GetKeyValueCommand;
                command.Parameters.Add(new SqliteParameter("@Key", key));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var result = reader.GetString(0);

                    value = JsonConvert.DeserializeObject<T>(result);

                    return true;
                }
            }
            catch (SqliteException exception)
            {
                Logger.Error($"Error in reading key value for key {key} in {SqliteStoreConstants.DatabaseName}: {exception.Message}");
            }

            value = default;

            return false;
        }

        private PersistenceLevel GetLevel(string key)
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = GetKeyLevelCommand;
                command.Parameters.Add(new SqliteParameter("@Key", key));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var level = reader.GetString(0);
                    return JsonConvert.DeserializeObject<PersistenceLevel>(level);
                }
            }
            catch (SqliteException exception)
            {
                Logger.Error($"Error in reading key level for key {key} in {SqliteStoreConstants.DatabaseName}: {exception.Message}");
                return default;
            }

            return default;
        }

        private bool KeyLevelExists(string key)
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = IfKeyLevelExistsCommand;
                command.Parameters.Add(new SqliteParameter("@Key", key));
                var result = command.ExecuteScalar() ?? 0;
                return (long)result > 0;
            }
            catch (SqliteException exception)
            {
                Logger.Error($"Error in finding key level for {key} in {SqliteStoreConstants.DatabaseName}: {exception.Message}");
                return false;
            }
        }
    }
}