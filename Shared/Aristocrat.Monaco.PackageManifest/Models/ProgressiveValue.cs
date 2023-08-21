namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System;

    /// <summary>
    ///     Defines a Progressive Value
    /// </summary>
    [Serializable]
    public class ProgressiveValue
    {
        /// <summary>
        ///     Constructor of ProgressiveValue
        /// </summary>
        public ProgressiveValue(string value)
        {
            IsCredit = value.EndsWith("cr");

            if (IsCredit)
            {
                value = value.Substring(0, value.Length - 2);
            }

            Value = long.Parse(value); // throw exception if cannot be parsed
        }

        /// <summary>
        ///     
        /// </summary>
        public long Value { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public bool IsCredit { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public long ToCurrency(long denom) => IsCredit ? denom * Value : Value;
    }
}
