namespace Aristocrat.Monaco.Gaming
{
    using ProtoBuf;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Hardware.Contracts.Persistence;

    public abstract class StorageBase
    {
        private const string StorageSuffix = "Game";
        private const string GameNameSuffix = "Name";
        private const string BetLevelNameSuffix = "AtBetLevel";

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

            return GetValue<T>(block);
        }

        public T GetValue<T>(int gameId, long betAmount, string name)
        {
            var block = GetBlock(gameId, betAmount, name);

            return GetValue<T>(block);
        }

        public IEnumerable<T> GetValues<T>(int gameId, string name)
        {
            var block = GetBlock(gameId, name);
            var list = GetValue<T>(block) as IEnumerable;
            return list?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        public IEnumerable<T> GetValues<T>(int gameId, long betAmount, string name)
        {
            var block = GetBlock(gameId, betAmount, name);
            var list = GetValues<T>(block) as IEnumerable;
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

        public void SetValue<T>(string name, T value)
        {
            var block = GetBlock(name);

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

        internal bool Exists(string blockName)
        {
            return _persistentStorage.BlockExists(blockName);
        }

        internal T GetValue<T>(IPersistentStorageAccessor block)
        {
            lock (_sync)
            {
                var data = (byte[])block[Data];
                if (data.Length <= 2)
                {
                    return default(T);
                }

                using (var stream = new MemoryStream())
                {
                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;
                    return Serializer.Deserialize<T>(stream);
                }
            }
        }

        internal List<T> GetValues<T>(IPersistentStorageAccessor block)
        {
            lock (_sync)
            {
                var data = (byte[])block[Data];
                if (data.Length <= 2)
                {
                    return default(List<T>);
                }

                using (var stream = new MemoryStream())
                {
                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;
                    return Serializer.Deserialize<List<T>>(stream);
                }
            }
        }

        internal void SetValue<T>(IPersistentStorageAccessor block, T value)
        {
            lock (_sync)
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, value);
                    using (var transaction = block.StartTransaction())
                    {
                        transaction[Data] = stream.ToArray();

                        transaction.Commit();
                    }
                }
            }
        }

        private static string GetStorageName(int gameId, string storageName)
        {
            return $"{storageName}{GameNameSuffix}{gameId}";
        }

        private static string GetStorageName(int gameId, long betAmount, string storageName)
        {
            return $"{GetStorageName(gameId, storageName)}{storageName}{BetLevelNameSuffix}{betAmount}";
        }
    }
}
