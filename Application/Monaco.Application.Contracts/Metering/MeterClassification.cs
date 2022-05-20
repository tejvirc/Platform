namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    ///     An abstract class used as a base for defining a specific classification
    /// </summary>
    public abstract class MeterClassification : IEquatable<MeterClassification>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterClassification" /> class.
        /// </summary>
        /// <param name="name">The name of the meter classification, e.g, "Currency".</param>
        /// <param name="upperBounds">The upper bounds of the meter classification, e.g, 1000000000000000</param>
        protected MeterClassification(string name, long upperBounds)
        {
            Name = name;
            UpperBounds = upperBounds;
        }

        /// <summary>
        ///     Gets the name of the meter classification.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the upper bounds of this classification
        /// </summary>
        public long UpperBounds { get; }

        /// <summary>
        ///     Returns whether or not the object is equal to the MeterClassification instance
        /// </summary>
        /// <param name="other">The MeterClassification object to test for equality</param>
        /// <returns>
        ///     A value indicating whether or not the object is equal to the MeterClassification instance
        /// </returns>
        public bool Equals(MeterClassification other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return Name == other.Name;
        }

        /// <summary>
        ///     Returns whether or not the two MeterClassification objects are equal
        /// </summary>
        /// <param name="left">The lvalue in the expression</param>
        /// <param name="right">The rvalue in the expression</param>
        /// <returns>
        ///     A value indicating whether or not the two MeterClassification objects are equal
        /// </returns>
        public static bool operator ==(MeterClassification left, MeterClassification right)
        {
            if (ReferenceEquals(null, left))
            {
                return ReferenceEquals(null, right);
            }

            return left.Equals(right);
        }

        /// <summary>
        ///     Returns whether or not the two MeterClassification objects are different
        /// </summary>
        /// <param name="left">The lvalue in the expression</param>
        /// <param name="right">The rvalue in the expression</param>
        /// <returns>
        ///     A value indicating whether or not the two MeterClassification objects are different
        /// </returns>
        public static bool operator !=(MeterClassification left, MeterClassification right)
        {
            return !(left == right);
        }

        /// <summary>
        ///     Creates and returns a string representation of the value
        /// </summary>
        /// <param name="meterValue">The value to convert to a string</param>
        /// <returns>A string representation of the value</returns>
        public abstract string CreateValueString(long meterValue);

        /// <summary>
        ///     Returns the hash value for the classification.
        /// </summary>
        /// <returns>the hash value for the classification</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        ///     Returns whether or not the object is equal to the MeterClassification instance
        /// </summary>
        /// <param name="obj">The object to test for equality</param>
        /// <returns>
        ///     A value indicating whether or not the object is equal to the MeterClassification instance
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MeterClassification);
        }
    }
}
