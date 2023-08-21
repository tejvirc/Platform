namespace Aristocrat.Monaco.Application.Contracts
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     A snapshot of values for a given meter
    /// </summary>
    public struct MeterSnapshot
    {
        /// <summary>
        ///     Gets or sets the name of the meter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the values of the meter for each value type (lifetime, period, session)
        /// </summary>
        public Dictionary<MeterValueType, long> Values { get; set; }

        /// <summary>
        ///     Compares two MeterSnapshot objects
        /// </summary>
        /// <param name="snap1">The first MeterSnapshot to compare</param>
        /// <param name="snap2">The second MeterSnapshot to compare</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public static bool operator ==(MeterSnapshot snap1, MeterSnapshot snap2)
        {
            return snap1.Equals(snap2);
        }

        /// <summary>
        ///     Compares two MeterSnapshot objects
        /// </summary>
        /// <param name="snap1">The first MeterSnapshot to compare</param>
        /// <param name="snap2">The second MeterSnapshot to compare</param>
        /// <returns>True if they are not equal, false otherwise</returns>
        public static bool operator !=(MeterSnapshot snap1, MeterSnapshot snap2)
        {
            return !snap1.Equals(snap2);
        }

        /// <summary>
        ///     Compares the passed in object to the current instance
        /// </summary>
        /// <param name="obj">The object to compare to this instance</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is MeterSnapshot))
            {
                return false;
            }

            var snapshot = (MeterSnapshot)obj;

            if (Name != snapshot.Name)
            {
                return false;
            }

            var comparisonValues = snapshot.Values;
            foreach (var pair in Values)
            {
                if (comparisonValues.ContainsKey(pair.Key)
                    && comparisonValues[pair.Key] != Values[pair.Key])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Gets a unique hash value for this object
        /// </summary>
        /// <returns>A unique hash value for this object</returns>
        public override int GetHashCode()
        {
            var baseHash = base.GetHashCode();

            var totalMeterValue = 0;
            if (Values != null)
            {
                totalMeterValue += Values.Sum(pair => (int)pair.Value);
            }

            return baseHash + totalMeterValue * 17;
        }
    }
}
