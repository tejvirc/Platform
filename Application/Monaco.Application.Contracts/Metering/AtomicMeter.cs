namespace Aristocrat.Monaco.Application.Contracts
{
    using Hardware.Contracts.Persistence;
    using Metering;
    using System;

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
    public class AtomicMeter : IMeter
    {
        private const int TotalPersistedDataSize = 2 * sizeof(long);
        private const string LifetimeMeterSuffix = "Lifetime";
        private const string PeriodMeterSuffix = "Period";

        private readonly IPersistentStorageAccessor _block;
        private readonly int _blockIndex;
        private readonly bool _useGenericName;

        private long? _lifetime;
        private long? _period;

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

            provider.RegisterMeterClearDelegate(ClearPeriod);
        }

        /// <summary>
        ///     Gets the size of the data AtomicMeter needs to persist.
        /// </summary>
        public static int PersistedDataSize => TotalPersistedDataSize;

        /// <inheritdoc />
        public event EventHandler<MeterChangedEventArgs> MeterChangedEvent;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public MeterClassification Classification { get; }

        /// <inheritdoc />
        public long Lifetime
        {
            get
            {
                return (_lifetime ??= (long)_block[_blockIndex,
                        _useGenericName ? LifetimeMeterSuffix : Name + LifetimeMeterSuffix]);
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
                return (_period ??= (long)_block[_blockIndex,
                        _useGenericName ? PeriodMeterSuffix : Name + PeriodMeterSuffix]);
            }
            private set
            {
                var name = _useGenericName ? PeriodMeterSuffix : Name + PeriodMeterSuffix;
                _block[_blockIndex, name] = _period = value;
            }
        }

        /// <inheritdoc />
        public long Session { get; private set; }

        /// <inheritdoc />
        public void Increment(long amount)
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

            if (Session + amount <= 0)
            {
                Session = 0;
            }
            else if (Classification.UpperBounds > Session + amount)
            {
                Session += amount;
            }
            else
            {
                Session = (Session + amount) % Classification.UpperBounds;
            }

            MeterChangedEvent?.Invoke(this, new MeterChangedEventArgs(amount));
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
            if (Classification.UpperBounds > resetValue)
            {
                _block.StartUpdate(true);
                Lifetime = resetValue;
                Period = resetValue;
                Session = resetValue;
                _block.Commit();
            }
            else
            {
                _block.StartUpdate(true);
                Lifetime = resetValue % Classification.UpperBounds;
                Period = resetValue % Classification.UpperBounds;
                Session = resetValue % Classification.UpperBounds;
                _block.Commit();
            }

            MeterChangedEvent?.Invoke(this, new MeterChangedEventArgs(resetValue));
        }

        /// <summary>
        ///     Resets the meter to 0 for its Period value.
        ///     </remarks>
        public void ClearPeriod()
        {
            _block.StartUpdate(true);

            Period = 0;

            _block.Commit();
        }
    }
}