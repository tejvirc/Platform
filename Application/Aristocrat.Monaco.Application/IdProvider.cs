namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     An implementation of <see cref="IdProvider" />
    /// </summary>
    public class IdProvider : IIdProvider, IService
    {
        private const PersistenceLevel BlockLevel = PersistenceLevel.Critical;

        private const string TransactionId = @"TransactionId";
        private const string LogSequenceNumber = @"LogSequenceNumber";

        private readonly IPersistentStorageManager _storage;
        private readonly Dictionary<Type, long> _currentSequence = new Dictionary<Type, long>();
        private readonly Dictionary<Type, IPersistentStorageAccessor> _logAccessors =
            new Dictionary<Type, IPersistentStorageAccessor>();

        private readonly object _lock = new object();

        private long? _currentTransactionId;
        private IPersistentStorageAccessor _idAccessor;

        public IdProvider()
            : this (ServiceManager.GetInstance().GetService<IPersistentStorageManager>())
        {
        }

        public IdProvider(IPersistentStorageManager storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <inheritdoc />
        public long CurrentTransactionId
        {
            get
            {
                lock (_lock)
                {
                    return _currentTransactionId ?? 0;
                }
            }
        }

        /// <inheritdoc />
        public long GetNextTransactionId()
        {
            lock (_lock)
            {
                using (var transaction = _idAccessor.StartTransaction())
                {
                    var transactionId =
                        (long)(_currentTransactionId ?? (_currentTransactionId = (long)_idAccessor[TransactionId]));

                    transaction[TransactionId] = _currentTransactionId = ++transactionId;

                    transaction.Commit();

                    return transactionId;
                }
            }
        }

        /// <inheritdoc />
        public long GetCurrentLogSequence<T>()
            where T : class
        {
            return GetCurrentLogSequence(typeof(T));
        }

        /// <inheritdoc />
        public long GetCurrentLogSequence(Type type)
        {
            return GetCurrentId(type);
        }

        /// <inheritdoc />
        public long GetNextLogSequence<T>()
            where T : class
        {
            return GetNextLogSequence(typeof(T));
        }

        /// <inheritdoc />
        public long GetNextLogSequence(Type type)
        {
            return GetNextId(type);
        }

        /// <inheritdoc />
        public long GetNextLogSequence<T>(long maxValue)
            where T : class
        {
            return GetNextId(typeof(T), maxValue);
        }

        /// <inheritdoc />
        public int GetNextDeviceId<T>() where T : class
        {
            return GetNextDeviceId(typeof(T));
        }

        /// <inheritdoc />
        public int GetNextDeviceId(Type type)
        {
            return (int)GetNextId(type);
        }

        /// <inheritdoc />
        public int GetCurrentDeviceId<T>() where T : class
        {
            return GetCurrentDeviceId(typeof(T));
        }

        /// <inheritdoc />
        public int GetCurrentDeviceId(Type type)
        {
            return (int)GetCurrentId(type);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IIdProvider) };

        /// <inheritdoc />
        public void Initialize()
        {
            _idAccessor = _storage.GetAccessor(BlockLevel, GetType().FullName);
        }

        private static string GetBlockName(Type type)
        {
            return $"Aristocrat.Monaco.Application.IdProvider.{type.Name}";
        }

        private IPersistentStorageAccessor GetBlock(Type type)
        {
            if (!_logAccessors.TryGetValue(type, out var accessor))
            {
                var blockName = GetBlockName(type);

                if (_storage.BlockExists(blockName))
                {
                    accessor = _storage.GetBlock(blockName);
                }
                else
                {
                    var blockFormat = new BlockFormat();

                    var field = new FieldDescription(FieldType.Int64, 0, LogSequenceNumber);
                    blockFormat.AddFieldDescription(field);

                    accessor = _storage.CreateDynamicBlock(BlockLevel, blockName, 1, blockFormat);
                }

                _logAccessors[type] = accessor;
            }

            return accessor;
        }
        
        private long GetCurrentId(Type type)
        {
            lock (_lock)
            {
                if (!_currentSequence.TryGetValue(type, out var value))
                {
                    value = (long)GetBlock(type)[LogSequenceNumber];
                }

                return value;
            }
        }

        private long GetNextId(Type type, long maxValue = long.MaxValue)
        {
            lock (_lock)
            {
                var accessor = GetBlock(type);

                using (var transaction = accessor.StartTransaction())
                {
                    if (!_currentSequence.TryGetValue(type, out var sequence))
                    {
                        sequence = (long)accessor[LogSequenceNumber];
                    }

                    ++sequence;

                    if (sequence + 1 >= maxValue)
                    {
                        sequence = 0;
                    }

                    transaction[LogSequenceNumber] = _currentSequence[type] = sequence;
                    
                    transaction.Commit();

                    return sequence;
                }
            }
        }
    }
}