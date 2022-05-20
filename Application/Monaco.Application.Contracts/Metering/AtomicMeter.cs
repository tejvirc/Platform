namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Threading;
    using Hardware.Contracts.Persistence;
    using Kernel.LockManagement;
    using Metering;

    /// <summary>
    ///     This delegate type defines the signature of the method IMeterProvider requires
    ///     from meter implementations to be able to clear the period values
    /// </summary>
    public delegate void ClearPeriodMeter();

    /// <summary>
    ///     A meter implementation for meters without any compound formulas.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A meter instantiated with this class does not depend on any other meters to get its
    ///         value. The <c>Lifetime</c> and <c>Period</c> values are persisted.
    ///     </para>
    /// </remarks>
    public class AtomicMeter : IMeter, IDisposable
    {
        private const int TotalPersistedDataSize = 2 * sizeof(long);
        private const string LifetimeMeterSuffix = "Lifetime";
        private const string PeriodMeterSuffix = "Period";

        private readonly IPersistentStorageAccessor _block;
        private readonly int _blockIndex;
        private readonly bool _useGenericName;

        /// <summary>
        ///     To synchronize all read and write access to this meter
        /// </summary>
        private readonly ReaderWriterLockSlim _meterAccess;

        private long? _lifetime;
        private long? _period;
        private long _session;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AtomicMeter" /> class.
        /// </summary>
        /// <param name="name">The name of the meter</param>
        /// <param name="persistenceBlock">The block of persistent storage to be used</param>
        /// <param name="classification">The classification of this meter</param>
        /// <param name="provider">The meter provider for this meter</param>
        public AtomicMeter(
            string name,
            IPersistentStorageAccessor persistenceBlock,
            MeterClassification classification,
            IMeterProvider provider)
        {
            Name = name;
            _block = persistenceBlock;
            Classification = classification;

            _meterAccess = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            provider.RegisterMeterClearDelegate(ClearPeriod);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AtomicMeter" /> class.
        /// </summary>
        /// <param name="name">The name of the meter</param>
        /// <param name="persistenceBlock">The block of persistent storage to be used</param>
        /// <param name="persistenceBlockIndexer">The array index into the block</param>
        /// <param name="classification">The classification of this meter</param>
        /// <param name="provider">The meter provider for this meter</param>
        public AtomicMeter(
            string name,
            IPersistentStorageAccessor persistenceBlock,
            int persistenceBlockIndexer,
            string classification,
            IMeterProvider provider)
            : this(
                name,
                persistenceBlock,
                persistenceBlockIndexer,
                MeterUtilities.ParseClassification(classification),
                provider)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AtomicMeter" /> class.
        /// </summary>
        /// <param name="name">The name of the meter</param>
        /// <param name="persistenceBlock">The block of persistent storage to be used</param>
        /// <param name="persistenceBlockIndexer">The array index into the block</param>
        /// <param name="classification">The classification of this meter</param>
        /// <param name="provider">The meter provider for this meter</param>
        /// <param name="lifetime">The current lifetime meter value</param>
        /// <param name="period">The current period meter value</param>
        public AtomicMeter(
            string name,
            IPersistentStorageAccessor persistenceBlock,
            int persistenceBlockIndexer,
            string classification,
            IMeterProvider provider,
            long lifetime,
            long period)
            : this(
                name,
                persistenceBlock,
                persistenceBlockIndexer,
                MeterUtilities.ParseClassification(classification),
                provider)
        {
            _lifetime = lifetime;
            _period = period;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AtomicMeter" /> class.
        /// </summary>
        /// <param name="name">The name of the meter</param>
        /// <param name="persistenceBlock">The block of persistent storage to be used</param>
        /// <param name="persistenceBlockIndexer">The array index into the block</param>
        /// <param name="classification">The classification of this meter</param>
        /// <param name="provider">The meter provider for this meter</param>
        public AtomicMeter(
            string name,
            IPersistentStorageAccessor persistenceBlock,
            int persistenceBlockIndexer,
            MeterClassification classification,
            IMeterProvider provider)
        {
            Name = name;
            _block = persistenceBlock;
            _blockIndex = persistenceBlockIndexer;
            _useGenericName = true;
            Classification = classification;

            _meterAccess = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            provider.RegisterMeterClearDelegate(ClearPeriod);
        }

        /// <summary>
        ///     Gets the size of the data AtomicMeter needs to persist.
        /// </summary>
        public static int PersistedDataSize => TotalPersistedDataSize;

        /// <inheritdoc />
        public event EventHandler<MeterChangedEventArgs> MeterChangedEvent;

        /// <inheritdoc />
        public string UniqueLockableName => Name;

        /// <inheritdoc />
        public IDisposable AcquireReadOnlyLock()
        {
            return _meterAccess.GetReadLock();
        }

        /// <inheritdoc />
        public bool TryAcquireReadOnlyLock(int timeout, out IDisposable disposableToken)
        {
            return _meterAccess.TryGetReadLock(timeout, out disposableToken);
        }

        /// <inheritdoc />
        public IDisposable AcquireExclusiveLock()
        {
            return _meterAccess.GetWriteLock();
        }

        /// <inheritdoc />
        public bool TryAcquireExclusiveLock(int timeout, out IDisposable token)
        {
            return _meterAccess.TryGetWriteLock(timeout, out token);
        }

        /// <inheritdoc />
        public void ReleaseLock()
        {
            _meterAccess.ReleaseLock();
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public MeterClassification Classification { get; }

        /// <inheritdoc />
        public long Lifetime
        {
            get
            {
                using (AcquireReadOnlyLock())
                {
                    return (_lifetime ??= (long)_block[_blockIndex,
                        _useGenericName ? LifetimeMeterSuffix : Name + LifetimeMeterSuffix]);
                }
            }
            private set
            {
                var name = _useGenericName ? LifetimeMeterSuffix : Name + LifetimeMeterSuffix;
                _block[_blockIndex, name] = _lifetime = value;
            }
        }

        /// <inheritdoc />
        public long Period
        {
            get
            {
                using (AcquireReadOnlyLock())
                {
                    return (_period ??= (long)_block[_blockIndex,
                        _useGenericName ? PeriodMeterSuffix : Name + PeriodMeterSuffix]);
                }
            }
            private set
            {
                var name = _useGenericName ? PeriodMeterSuffix : Name + PeriodMeterSuffix;
                _block[_blockIndex, name] = _period = value;
            }
        }

        /// <inheritdoc />
        public long Session
        {
            get
            {
                using (AcquireReadOnlyLock())
                {
                    return _session;
                }
            }
        }

        /// <inheritdoc />
        public void Increment(long amount)
        {
            using (AcquireExclusiveLock())
            {
                if (amount == 0)
                {
                    return;
                }

                _block.StartUpdate(true);

                if (Lifetime + amount <= 0)
                {
                    Lifetime = 0;
                }
                else if (Classification.UpperBounds > Lifetime + amount)
                {
                    Lifetime += amount;
                }
                else
                {
                    Lifetime = (Lifetime + amount) % Classification.UpperBounds;
                }

                if (Period + amount <= 0)
                {
                    Period = 0;
                }
                else if (Classification.UpperBounds > Period + amount)
                {
                    Period += amount;
                }
                else
                {
                    Period = (Period + amount) % Classification.UpperBounds;
                }

                _block.Commit();

                if (_session + amount <= 0)
                {
                    _session = 0;
                }
                else if (Classification.UpperBounds > Session + amount)
                {
                    _session += amount;
                }
                else
                {
                    _session = (_session + amount) % Classification.UpperBounds;
                }

                MeterChangedEvent?.Invoke(this, new MeterChangedEventArgs(amount));
            }
        }

        /// <summary>
        ///     Resets the meter to the resetValue value.
        /// </summary>
        /// <param name="resetValue">The value to reset meter values.</param>
        /// <remarks>
        ///     <para>
        ///         In this method, all meter values in three time frames will be reset to the specified
        ///         value. The meter value's wrap-around in terms of the classification.
        ///     </para>
        ///     <para>
        ///         The change is notified if the <c>MeterChangedEvent</c> is hooked up.
        ///     </para>
        /// </remarks>
        public void ResetMeter(long resetValue)
        {
            using (AcquireExclusiveLock())
            {
                if (Classification.UpperBounds > resetValue)
                {
                    _block.StartUpdate(true);
                    Lifetime = resetValue;
                    Period = resetValue;
                    _session = resetValue;
                    _block.Commit();
                }
                else
                {
                    _block.StartUpdate(true);
                    Lifetime = resetValue % Classification.UpperBounds;
                    Period = resetValue % Classification.UpperBounds;
                    _session = resetValue % Classification.UpperBounds;
                    _block.Commit();
                }
            }

            MeterChangedEvent?.Invoke(this, new MeterChangedEventArgs(resetValue));
        }

        /// <summary>
        ///     Resets the meter to 0 for its Period value.
        ///     </remarks>
        public void ClearPeriod()
        {
            using (AcquireExclusiveLock())
            {
                _block.StartUpdate(true);

                Period = 0;

                _block.Commit();
            }
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
                _meterAccess.Dispose();
            }

            _disposed = true;
        }
    }
}