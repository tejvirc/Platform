namespace Aristocrat.Monaco.Hardware.StorageSystem
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;
    using Contracts.Persistence;
    using log4net;
    using Microsoft.Data.Sqlite;
    using Aristocrat.Monaco.Common;

    /// <summary>
    ///     Sql persistent storage accessor
    /// </summary>
    public class SqlPersistentStorageAccessor : IPersistentStorageAccessor
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString;

        private readonly object _transactionLock = new object();

        private IPersistentStorageTransaction _scopedTransaction;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlPersistentStorageAccessor" /> class.
        /// </summary>
        /// <param name="connectionString">Connection string to SQLite db</param>
        /// <param name="name">Name of the block</param>
        public SqlPersistentStorageAccessor(string connectionString, string name)
        {
            _connectionString = connectionString;

            Initialize(name);

            var blockExists = BlockExists(name);

            Format = LoadBlockFormat(Name, Version, blockExists);

            if (!blockExists)
            {
                AddFields(Format, Format.FieldDescriptions);
            }
        }

        public SqlPersistentStorageAccessor(SqliteTransaction transaction, string name, int count, BlockFormat format)
        {
            _connectionString = transaction?.Connection.ConnectionString;

            Name = name;
            Version = format?.Version ?? 0;
            Count = count;

            var blockExists = BlockExists(name);

            if (format == null)
            {
                Format = LoadBlockFormat(Name, Version, blockExists);
            }
            else
            {
                Format = format;
                Format.FinalizeLayout();
            }

            if (!blockExists && transaction != null)
            {
                AddFields(transaction, Format, Format.FieldDescriptions);
            }
        }

        /// <summary>
        ///     Gets the Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Gets the Format.
        /// </summary>
        public BlockFormat Format { get; }

        /// <inheritdoc />
        public int Count { get; private set; }

        /// <inheritdoc />
        public PersistenceLevel Level { get; private set; }

        /// <inheritdoc />
        public int Version { get; private set; }

        /// <inheritdoc />
        public object this[string blockFieldName]
        {
            get => GetField(blockFieldName);

            set
            {
                lock (_transactionLock)
                {
                    if (_scopedTransaction != null)
                    {
                        _scopedTransaction[blockFieldName] = value;
                    }
                    else
                    {
                        using (var transaction = StartTransaction())
                        {
                            transaction[Name, blockFieldName] = value;

                            try
                            {
                                transaction.Commit();
                            }
                            catch (Exception e)
                            {
                                SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public object this[int arrayIndex, string blockFieldName]
        {
            get => GetField(blockFieldName, arrayIndex);

            set
            {
                lock (_transactionLock)
                {
                    if (_scopedTransaction != null)
                    {
                        _scopedTransaction[arrayIndex, blockFieldName] = value;
                    }
                    else
                    {
                        using (var transaction = new SqlPersistentStorageTransaction(this, _connectionString))
                        {
                            transaction[arrayIndex, blockFieldName] = value;
                            transaction.Commit();
                        }
                    }
                }
            }
        }

        public IDictionary<int, Dictionary<string, object>> GetAll()
        {
            var results = new Dictionary<int, Dictionary<string, object>>();

            using (var connection = CreateConnection())
            {
                using var benchmarck = new Benchmark(nameof(GetAll));

                try
                {
                    connection.Open();

                    using (var command = new SqliteCommand("SELECT FieldName, Data FROM StorageBlockField WHERE BlockName = @BlockName", connection))
                    {
                        command.Parameters.Add(new SqliteParameter("@BlockName", Name));

                        using (var result = command.ExecuteReader())
                        {
                            while (result.Read())
                            {
                                var internalName = result["FieldName"].ToString().Split('@');

                                var fieldName = internalName[0];

                                var index = 0;
                                if (internalName.Length > 1)
                                {
                                    index = Convert.ToInt32(internalName[1]);
                                }

                                if (!results.TryGetValue(index, out var values))
                                {
                                    values = new Dictionary<string, object>();

                                    results.Add(index, values);
                                }

                                var fieldData = (byte[])result["Data"];

                                var fd = Format.GetFieldDescription(fieldName);

                                if (fd != null && fd.DataType == FieldType.Byte && fd.Count == 2 && fd.Size == 1 && fieldData?.Length > fd.Count)
                                {
                                    values.Add(fieldName, fieldData);
                                }
                                else
                                {
                                    try
                                    {
                                        values.Add(fieldName, Format.Convert(fieldName, fieldData));
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Debug("Failed to convert data", ex);
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ReadFailure);
                }
            }

            return results;
        }

        /// <inheritdoc />
        public bool StartUpdate(bool waitForLock)
        {
            Monitor.Enter(_transactionLock);

            if (_scopedTransaction != null)
            {
                if (waitForLock)
                {
                    while (_scopedTransaction != null)
                    {
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    Monitor.Exit(_transactionLock);
                    return false;
                }
            }

            _scopedTransaction = StartTransaction();

            return true;
        }

        /// <inheritdoc />
        public void Commit()
        {
            try
            {
                _scopedTransaction?.Commit();
            }
            catch (Exception e)
            {
                SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
            }

            _scopedTransaction?.Dispose();
            _scopedTransaction = null;

            Monitor.Exit(_transactionLock);
        }

        /// <inheritdoc />
        public void Rollback()
        {
            _scopedTransaction?.Rollback();
            _scopedTransaction?.Dispose();
            _scopedTransaction = null;

            Monitor.Exit(_transactionLock);
        }

        /// <inheritdoc />
        public IPersistentStorageTransaction StartTransaction()
        {
            if (PersistenceTransaction.Current != null)
            {
                PersistenceTransaction.Current.AddBlock(this);

                return PersistenceTransaction.Current;
            }

            return new SqlPersistentStorageTransaction(this, _connectionString);
        }

        public static SqlPersistentStorageAccessor InvalidAccessor(string name, int count, BlockFormat format)
        {
            return new SqlPersistentStorageAccessor(null, name, count, format);
        }

        /// <summary>
        ///     Resizes the block
        /// </summary>
        /// <param name="size">The new size</param>
        public void Resize(int size)
        {
            if (size == Count)
            {
                return;
            }

            using (var connection = CreateConnection())
            {
                using var benchmarck = new Benchmark(nameof(Resize));

                try
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        if (size < Count)
                        {
                            foreach (var description in Format.FieldDescriptions)
                            {
                                for (var i = size; i < Count; i++)
                                {
                                    var fieldName = description.FieldName + "@" + i;

                                    using (var command = new SqliteCommand(
                                        "DELETE FROM StorageBlockField WHERE BlockName = @BlockName AND FieldName = @FieldName",
                                        connection,
                                        transaction))
                                    {
                                        command.Parameters.Add(new SqliteParameter("@BlockName", Name));
                                        command.Parameters.Add(new SqliteParameter("@FieldName", fieldName));

                                        command.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var description in Format.FieldDescriptions)
                            {
                                var defaultValue = Format.ConvertTo(
                                    description.FieldName,
                                    DefaultValueFromFieldDescription(description));

                                for (var i = Count; i < size; i++)
                                {
                                    using (var command = new SqliteCommand(
                                        "INSERT INTO StorageBlockField (BlockName, FieldName, DataType, Data, Count) VALUES (@BlockName, @FieldName, @DataType, @Data, @Count)",
                                        connection,
                                        transaction))
                                    {
                                        command.Parameters.Add(new SqliteParameter("@BlockName", Name));
                                        command.Parameters.Add(
                                            new SqliteParameter("@FieldName", description.FieldName + "@" + i));
                                        command.Parameters.Add(
                                            new SqliteParameter(
                                                "@DataType",
                                                Format.GetFieldType(description.FieldName).ToString()));
                                        command.Parameters.Add(new SqliteParameter("@Data", defaultValue));
                                        command.Parameters.Add(new SqliteParameter("@Count", description.Count));

                                        command.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        using (var command = new SqliteCommand(
                            "UPDATE StorageBlock SET Count = @Count WHERE Name = @Name",
                            connection,
                            transaction))
                        {
                            command.Parameters.Add(new SqliteParameter("@Name", Name));
                            command.Parameters.Add(new SqliteParameter("@Count", size));

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        Count = size;
                    }
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
                }
            }
        }

        private static BlockFormat[] DeserializeBlockFormats(string pathName)
        {
            BlockFormat[] blockFormats = { };

            var serializerArray = new XmlSerializer(typeof(BlockFormat[]), new XmlRootAttribute("ArrayOfBlockFormat"));

            var singleBlockFormatXmlRootAttribute = Attribute.GetCustomAttributes(typeof(BlockFormat))
                .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
            var serializerSingle = new XmlSerializer(typeof(BlockFormat), singleBlockFormatXmlRootAttribute ?? new XmlRootAttribute(nameof(BlockFormat)));

            var content = File.ReadAllText(pathName);
            using (var reader = new StringReader(content))
            using (var xmlReader = XmlReader.Create(reader))
            {
                if (serializerArray.CanDeserialize(xmlReader))
                {
                    Logger.Debug("Can deserialize multiple block formats...");
                    var blockFormatArray = (BlockFormat[])serializerArray.Deserialize(xmlReader);
                    Array.ForEach(blockFormatArray, x => x.UpdateDictionary());
                    blockFormats = blockFormatArray;
                }
                else if (serializerSingle.CanDeserialize(xmlReader))
                {
                    var blockFormatSingle = (BlockFormat)serializerSingle.Deserialize(xmlReader);
                    blockFormatSingle.UpdateDictionary();
                    blockFormats = new[] { blockFormatSingle };
                }
            }

            return blockFormats;
        }

        private static object DefaultValueFromFieldDescription(FieldDescription fieldDescription)
        {
            if (fieldDescription == null)
            {
                return null;
            }

            var type = Type.GetType(
                $"System.{(fieldDescription.DataType == FieldType.Bool ? "Boolean" : fieldDescription.DataType.ToString())}{(fieldDescription.Count > 1 ? "[]" : string.Empty)}");
            if (type == null)
            {
                return null;
            }

            if (!type.IsArray)
            {
                return type == typeof(string) ? string.Empty : Activator.CreateInstance(type);
            }

            if (type == typeof(string[]))
            {
                var strings = new string[fieldDescription.Count];
                for (var i = 0; i < fieldDescription.Count; i++)
                {
                    strings[i] = string.Empty;
                }

                return strings;
            }

            var elementType = type.GetElementType();

            return elementType == null ? null : Array.CreateInstance(elementType, fieldDescription.Count);
        }

        private void Initialize(string name)
        {
            using (var connection = CreateConnection())
            {
                using var benchmarck = new Benchmark(nameof(Initialize));

                try
                {
                    connection.Open();

                    using (var command = new SqliteCommand("SELECT * FROM StorageBlock WHERE Name = @Name", connection))
                    {
                        command.Parameters.Add(new SqliteParameter("@Name", name));

                        using var benchmarck1 = new Benchmark(nameof(Initialize) + "_1");

                        using (var result = command.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            while (result.Read())
                            {
                                Name = result["Name"].ToString();
                                Version = Convert.ToInt32(result["Version"]);
                                if (Enum.TryParse(result["Level"].ToString(), out PersistenceLevel level))
                                {
                                    Level = level;
                                }

                                Count = Convert.ToInt32(result["Count"]);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ReadFailure);
                }
            }
        }

        private BlockFormat LoadBlockFormat(string blockName, int formatVersion, bool blockExists)
        {
            BlockFormat[] blockFormats;

            Logger.DebugFormat(CultureInfo.InvariantCulture, $"LoadBlockFormat {blockName} {formatVersion}");

            var pathName = Path.Combine(".", blockName + ".xml");

            if (File.Exists(pathName))
            {
                // Get block format from XML
                blockFormats = DeserializeBlockFormats(pathName);
            }
            else
            {
                // Populate block format from database
                var newFormat = new BlockFormat { Name = blockName, Version = formatVersion, ElementSize = 0 };

                using (var connection = CreateConnection())
                {
                    using var benchmarck = new Benchmark(nameof(LoadBlockFormat));

                    try
                    {
                        connection.Open();

                        using (var command = new SqliteCommand("SELECT * FROM StorageBlockField WHERE BlockName = @BlockName", connection))
                        {
                            command.Parameters.Add(new SqliteParameter("@BlockName", blockName));

                            using (var result = command.ExecuteReader())
                            {
                                while (result.Read())
                                {
                                    var fieldName = Count > 1 ? result["FieldName"].ToString().Split('@')[0] : result["FieldName"].ToString();
                                    if (newFormat.FieldDescriptions.Any(d => d.FieldName == fieldName))
                                    {
                                        continue;
                                    }

                                    var description = new FieldDescription
                                    {
                                        FieldName = fieldName,
                                        Count = Convert.ToInt32(result["Count"]),
                                        DataType = Enum.TryParse(result["DataType"].ToString(), out FieldType type) ? type : FieldType.Unused
                                    };

                                    newFormat.AddFieldDescription(description);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ReadFailure);
                    }
                }

                newFormat.FinalizeLayout();

                blockFormats = new[] { newFormat };
            }

            return blockFormats.Length == 1 ? blockFormats[0] : UpgradeToVersion(blockFormats, blockExists);
        }

        private BlockFormat UpgradeToVersion(BlockFormat[] blockFormats, bool blockExists)
        {
            var block = Array.FindLast(blockFormats, x => x.Version == blockFormats.Max(y => y.Version));

            if (!blockExists || Version == block.Version)
            {
                return block;
            }

            var existing = new List<FieldDescription>();

            using (var connection = CreateConnection())
            {
                using var benchmarck = new Benchmark(nameof(UpgradeToVersion));

                try
                {
                    connection.Open();

                    using (var command = new SqliteCommand("SELECT * FROM StorageBlockField WHERE BlockName = @BlockName", connection))
                    {
                        command.Parameters.Add(new SqliteParameter("@BlockName", Name));

                        using (var result = command.ExecuteReader())
                        {
                            while (result.Read())
                            {
                                var description = new FieldDescription { FieldName = result["FieldName"].ToString() };
                                if (Enum.TryParse(result["DataType"].ToString(), out FieldType type))
                                {
                                    description.DataType = type;
                                }

                                description.Count = Convert.ToInt32(result["Count"]);

                                existing.Add(description);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ReadFailure);
                }
            }

            var added = block.FieldDescriptions.Where(f => existing.All(e => e.FieldName != f.FieldName));

            AddFields(block, added, block.Version);

            return block;
        }

        private void AddFields(BlockFormat format, IEnumerable<FieldDescription> fields, int version = -1)
        {
            using (var connection = CreateConnection())
            {
                using var benchmarck = new Benchmark(nameof(AddFields));

                try
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        AddFields(transaction, format, fields, version);

                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.WriteFailure);
                }
            }
        }

        private void AddFields(SqliteTransaction transaction, BlockFormat format, IEnumerable<FieldDescription> fields, int version = -1)
        {
            using var benchmarck = new Benchmark(nameof(AddFields) + "_1");

            using (var command = new SqliteCommand(
                "INSERT INTO StorageBlockField (BlockName, FieldName, DataType, Data, Count) VALUES (@BlockName, @FieldName, @DataType, @Data, @Count)",
                transaction.Connection,
                transaction))
            {
                foreach (var description in fields)
                {
                    var defaultValue = format.ConvertTo(
                        description.FieldName,
                        DefaultValueFromFieldDescription(description));

                    command.Parameters.Add(new SqliteParameter("@BlockName", Name));
                    command.Parameters.Add(new SqliteParameter("@FieldName", description.FieldName));
                    command.Parameters.Add(
                        new SqliteParameter("@DataType", format.GetFieldType(description.FieldName).ToString()));
                    command.Parameters.Add(new SqliteParameter("@Data", defaultValue));
                    command.Parameters.Add(new SqliteParameter("@Count", description.Count));

                    command.ExecuteNonQuery();
                    command.Parameters.Clear();

                    for (var i = 1; i < Count; i++)
                    {
                        command.Parameters.Add(new SqliteParameter("@BlockName", Name));
                        command.Parameters.Add(
                            new SqliteParameter("@FieldName", description.FieldName + "@" + i));
                        command.Parameters.Add(
                            new SqliteParameter(
                                "@DataType",
                                format.GetFieldType(description.FieldName).ToString()));
                        command.Parameters.Add(new SqliteParameter("@Data", defaultValue));
                        command.Parameters.Add(new SqliteParameter("@Count", description.Count));

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                }
            }

            if (version > 0)
            {
                using (var command = new SqliteCommand(
                    "UPDATE StorageBlock SET Version = @Version WHERE Name = @Name",
                    transaction.Connection,
                    transaction))
                {
                    using var benchmarck2 = new Benchmark(nameof(AddFields) + "_2");

                    command.Parameters.Add(new SqliteParameter("@Name", Name));
                    command.Parameters.Add(new SqliteParameter("@Version", version));

                    command.ExecuteNonQuery();
                }
            }
        }

        private object GetField(string blockFieldName, int arrayIndex = -1)
        {
            var fieldName = arrayIndex >= 1 ? blockFieldName + "@" + arrayIndex : blockFieldName;

            using (var connection = CreateConnection())
            {
                try
                {
                    connection.Open();

                    using (var command = new SqliteCommand("SELECT Data FROM StorageBlockField WHERE BlockName = @BlockName AND FieldName = @FieldName", connection))
                    {
                        command.Parameters.Add(new SqliteParameter("@BlockName", Name));
                        command.Parameters.Add(new SqliteParameter("@FieldName", fieldName));

                        object res;
                        byte[] fieldData;
                        using (var benchmarck = new Benchmark(nameof(GetField)))
                        {
                            //var fieldData = (byte[])command.ExecuteScalar(CommandBehavior.SingleRow);
                            res = command.ExecuteScalar();
                            fieldData = (byte[])res;
                            benchmarck[$"{Name}:{fieldName}"] = fieldData;
                        }

                        var fd = Format.GetFieldDescription(blockFieldName);

                        // Get the raw bytes from the database.  I don't like this "special case" and this
                        // logic is not intuitive.  Can we do something more explicit like a RawByteArray type
                        // for storing byte blobs in the database?
                        if (fd != null && fd.DataType == FieldType.Byte && fd.Count == 2 && fd.Size == 1 &&
                            fieldData?.Length > fd.Count)
                        {
                            return fieldData;
                        }

                        return Format.Convert(blockFieldName, fieldData);
                    }
                }
                catch (Exception e)
                {
                    SqlPersistentStorageExceptionHandler.Handle(e, StorageError.ReadFailure);

                    return DefaultValueFromFieldDescription(
                        Format.FieldDescriptions.FirstOrDefault(format => format.FieldName == blockFieldName));
                }
            }
        }

        private bool BlockExists(string name)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    using var benchmarck = new Benchmark(nameof(BlockExists));

                    connection.Open();

                    using (var command = new SqliteCommand(
                        "SELECT COUNT(FieldName) FROM StorageBlockField WHERE BlockName = @BlockName",
                        connection))
                    {
                        command.Parameters.Add(new SqliteParameter("@BlockName", name));

                        return Convert.ToInt32(command.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("BlockExists check failed", ex);
                return false;
            }
        }

        private SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);

            // connection.SetPassword(StorageConstants.DatabasePassword);

            return connection;
        }
    }
}