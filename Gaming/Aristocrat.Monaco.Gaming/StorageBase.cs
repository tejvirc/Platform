namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using Aristocrat.Monaco.Gaming.Commands;
    using System.Security.Cryptography.X509Certificates;
    using Hardware.Contracts.Persistence;
    using System.Windows.Documents;

    public abstract class StorageBase
    {
        private const string StorageSuffix = "@Game:";
        private const string GameNameSuffix = "@Name:";
        private const string BetLevelNameSuffix = "@BetLevel:";
        private const string ExtraKeySuffix = "@Key:";

        private const string Data = "Data";

        private readonly PersistenceLevel _level;

        private readonly IPersistentStorageManager _persistentStorage;
        private readonly object _sync = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameStorageManager" /> class.
        /// </summary>
        /// <param name="persistentStorage">An <see cref="IPersistentStorageManager" /> instance</param>
        /// <param name="level"></param>
        protected StorageBase(IPersistentStorageManager persistentStorage, PersistenceLevel level)
        {
            _level = level;
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
        }

        public T GetValue<T>(string name)
        {
            var block = GetBlock(name);

            return (T)(GetValue(block) ?? default(T));
        }

        public T GetValue<T>(string name, string keyName)
        {
            var block = GetBlock(name, keyName);

            return (T)(GetValue(block) ?? default(T));
        }

        public T GetValue<T>(int gameId, long betAmount, string name)
        {
            var block = GetBlock(gameId, betAmount, name);

            return (T)(GetValue(block) ?? default(T));
        }

        public T GetValue<T>(int gameId, long betAmount, string name, string keyName)
        {
            var block = GetBlock(gameId, betAmount, name, keyName);

            return (T)(GetValue(block) ?? default(T));
        }

        public IEnumerable<T> GetValues<T>(int gameId, string name)
        {
            var block = GetBlock(gameId, name);

            var list = GetValue(block) as IEnumerable;

            return list?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        public IEnumerable<T> GetValues<T>(int gameId, long betAmount, string name)
        {
            var block = GetBlock(gameId, betAmount, name);

            var list = GetValue(block) as IEnumerable;

            return list?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        public IEnumerable<T> GetValues<T>(int gameId, long betAmount, string name, string keyName)
        {
            var block = GetBlock(gameId, betAmount, name, keyName);

            var list = GetValue(block) as IEnumerable;

            return list?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        public bool TryGetValues<T>(int gameId, string name, out IEnumerable<T> values)
        {
            if (!Exists(gameId, name))
            {
                values = Enumerable.Empty<T>();

                return false;
            }

            values = GetValues<T>(gameId, name);

            return true;
        }

        public bool TryGetValues<T>(int gameId, long betAmount, string name, out IEnumerable<T> values)
        {
            if (!Exists(gameId, betAmount, name))
            {
                values = Enumerable.Empty<T>();

                return false;
            }

            values = GetValues<T>(gameId, betAmount, name);

            return true;
        }

        public bool TryGetValues<T>(int gameId, long betAmount, string name, string keyName, out IEnumerable<T> values)
        {
            if (!Exists(gameId, betAmount, name, keyName))
            {
                values = Enumerable.Empty<T>();

                return false;
            }

            values = GetValues<T>(gameId, betAmount, name, keyName);

            return true;
        }

        public void SetValue<T>(string name, T value)
        {
            var block = GetBlock(name);

            SetValue(block, value);
        }

        public void SetValue<T>(string storageName, string keyName, T value)
        {
            var block = GetBlock(storageName, keyName);

            SetValue(block, value);
        }

        public void SetValue<T>(int gameId, string name, T value)
        {
            var block = GetBlock(gameId, name);

            SetValue(block, value);
        }

        public void SetValue<T>(int gameId, long betAmount, string name, T value)
        {
            var block = GetBlock(gameId, betAmount, name);

            SetValue(block, value);
        }

        public void SetValue<T>(int gameId, long betAmount, string name, string keyName, T value)
        {
            var block = GetBlock(gameId, betAmount, name, keyName);

            SetValue(block, value);
        }

        public void ClearAllValuesWithKeyName(int gameId, long betAmount, string storageName)
        {
            var name = $"{StorageSuffix}{GetStorageName(gameId, betAmount, storageName)}{ExtraKeySuffix}";
            var blocks = _persistentStorage.GetBlocksStartWith(name);

            foreach (IPersistentStorageAccessor block in blocks)
            {
                SetValue(block, "");
            }
        }

        public void ClearAllValuesWithKeyName(string storageName)
        {
            var name = $"{StorageSuffix}{storageName}{ExtraKeySuffix}";
            var blocks = _persistentStorage.GetBlocksStartWith(name);

            foreach (IPersistentStorageAccessor block in blocks)
            {
                SetValue(block, "");
            }
        }

        public Dictionary<string, string> GetKeyNameAndValues(string storageName)
        {
            var name = $"{StorageSuffix}{storageName}{ExtraKeySuffix}";
            var blocks = _persistentStorage.GetBlocksStartWith(name);

            Dictionary<string, string>  values = blocks.ToDictionary(block => block.Name.Replace(name, ""),
                block  => (string)(GetValue(block) ?? default(string)));

            return values;
        }

        public Dictionary<string,string> GetKeyNameAndValues(int gameId, long betAmount, string storageName)
        {
            var name = $"{StorageSuffix}{GetStorageName(gameId, betAmount, storageName)}{ExtraKeySuffix}";
            var blocks = _persistentStorage.GetBlocksStartWith(name);

            Dictionary<string, string> values = blocks.ToDictionary(block => block.Name.Replace(name,""),
                block => (string)(GetValue(block) ?? default(string)));

            return values;
        }



        internal IPersistentStorageAccessor GetBlock(int gameId, string storageName)
        {
            var blockName = GetStorageName(gameId, storageName);

            return GetBlock(blockName);
        }

        internal IPersistentStorageAccessor GetBlock(int gameId, long betAmount, string storageName)
        {
            var blockName = GetStorageName(gameId, betAmount, storageName);

            return GetBlock(blockName);
        }

        internal IPersistentStorageAccessor GetBlock(int gameId, long betAmount, string storageName, string keyName)
        {
            var blockName = GetStorageName(gameId, betAmount, storageName, keyName);

            return GetBlock(blockName);
        }

        internal IPersistentStorageAccessor GetBlock(string storageName, string keyName)
        {
            var blockName = GetStorageName(storageName, keyName);

            return GetBlock(blockName);
        }

        internal IPersistentStorageAccessor GetBlock(string blockName)
        {
            IPersistentStorageAccessor block;

            var name = $"{StorageSuffix}{blockName}";

            if (Exists(name))
            {
                block = _persistentStorage.GetBlock(name);
            }
            else
            {
                var blockFormat = new BlockFormat();

                var field = new FieldDescription(FieldType.Byte, 2, Data);
                blockFormat.AddFieldDescription(field);

                block = _persistentStorage.CreateDynamicBlock(_level, name, 1, blockFormat);
            }

            return block;
        }

        internal bool Exists(int gameId, string storageName)
        {
            return Exists($"{StorageSuffix}{GetStorageName(gameId, storageName)}");
        }

        internal bool Exists(int gameId, long betAmount, string storageName)
        {
            return Exists($"{StorageSuffix}{GetStorageName(gameId, betAmount, storageName)}");
        }

        internal bool Exists(int gameId, long betAmount, string storageName, string keyName)
        {
            return Exists($"{StorageSuffix}{GetStorageName(gameId, betAmount, storageName, keyName)}");
        }

        internal bool Exists(string storageName, string keyName)
        {
            return Exists($"{StorageSuffix}{GetStorageName(storageName, keyName)}");
        }

        internal bool Exists(string blockName)
        {
            return _persistentStorage.BlockExists(blockName);
        }

        internal object GetValue(IPersistentStorageAccessor block)
        {
            lock (_sync)
            {
                var data = (byte[])block[Data];
                if (data.Length <= 2)
                {
                    return null;
                }

                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();

                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;

                    return formatter.Deserialize(stream);
                }
            }
        }

        internal void SetValue<T>(IPersistentStorageAccessor block, T value)
        {
            lock (_sync)
            {
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();

                    formatter.Serialize(stream, value);

                    using (var transaction = block.StartTransaction())
                    {
                        transaction[Data] = stream.ToArray();

                        transaction.Commit();
                    }
                }
            }
        }

        private static string GetStorageName(string storageName, string keyName)
        {
            return $"{storageName}{ExtraKeySuffix}{keyName}";
        }

        private static string GetStorageName(int gameId, string storageName)
        {
            return $"{storageName}{GameNameSuffix}{gameId}";
        }

        private static string GetStorageName(int gameId, long betAmount, string storageName)
        {
            return $"{GetStorageName(gameId, storageName)}{storageName}{BetLevelNameSuffix}{betAmount}";
        }


        private static string GetStorageName(int gameId, long betAmount, string storageName, string keyName)
        {
            return $"{GetStorageName(gameId, betAmount, storageName)}{ExtraKeySuffix}{keyName}";
        }


    }
}
