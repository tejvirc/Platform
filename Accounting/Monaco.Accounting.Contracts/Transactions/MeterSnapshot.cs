namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System.Globalization;
    using Application.Contracts;

    /// <summary>
    ///     Definition of the MeterSnapshot structure, which holds values of a given meter obtained at a particular point in
    ///     time.
    /// </summary>
    /// <remarks>
    ///     Properties of this structure are set by the constructor and are immutable.
    /// </remarks>
    public struct MeterSnapshot
    {
        private readonly long _lifetime;
        private readonly long _period;
        private readonly long _session;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterSnapshot" /> struct.
        /// </summary>
        /// <param name="meter">the meter to copy</param>
        public MeterSnapshot(IMeter meter)
        {
            Name = meter.Name;
            _lifetime = meter.Lifetime;
            _period = meter.Period;
            _session = meter.Session;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterSnapshot" /> struct.
        /// </summary>
        /// <param name="meterSnapshot">The snapshot of the meter data</param>
        public MeterSnapshot(Application.Contracts.MeterSnapshot meterSnapshot)
        {
            Name = meterSnapshot.Name;
            meterSnapshot.Values.TryGetValue(MeterValueType.Lifetime, out _lifetime);
            meterSnapshot.Values.TryGetValue(MeterValueType.Period, out _period);
            meterSnapshot.Values.TryGetValue(MeterValueType.Session, out _session);
        }

        /// <summary>
        ///     Gets the name of the meter
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the Lifetime meter value
        /// </summary>
        public long Lifetime => _lifetime;

        /// <summary>
        ///     Gets the Period meter value
        /// </summary>
        public long Period => _period;

        /// <summary>
        ///     Gets the Session meter value
        /// </summary>
        public long Session => _session;

        /// <summary>
        ///     Returns the result of comparison of two objects
        /// </summary>
        /// <param name="meterX">the first object</param>
        /// <param name="meterY">the second object</param>
        /// <returns>true of the objects are equal</returns>
        public static bool operator ==(
            MeterSnapshot meterX,
            MeterSnapshot meterY)
        {
            return meterX.Name == meterY.Name
                   && meterX.Lifetime == meterY.Lifetime
                   && meterX.Period == meterY.Period
                   && meterX.Session == meterY.Session;
        }

        /// <summary>
        ///     Returns the result of comparison of two objects
        /// </summary>
        /// <param name="meterX">the first object</param>
        /// <param name="meterY">the second object</param>
        /// <returns>true of the objects are not equal</returns>
        public static bool operator !=(
            MeterSnapshot meterX,
            MeterSnapshot meterY)
        {
            return !(meterX == meterY);
        }

        /// <summary>
        ///     Returns the result of comparison with another object
        /// </summary>
        /// <param name="obj">object to compare with</param>
        /// <returns>true if the objects are equal</returns>
        public override bool Equals(object obj)
        {
            return obj is MeterSnapshot snapshot && this == snapshot;
        }

        /// <summary>
        ///     Returns the hash code
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        ///     Creates a string representation of the meter snapshot
        /// </summary>
        /// <returns>The string representation of the meter snapshot</returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [name={1},lifetime={2},period={3},session={4}]",
                GetType(),
                Name,
                _lifetime,
                _period,
                _session);
        }
    }
}