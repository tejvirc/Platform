namespace Aristocrat.Monaco.Gaming
{
    using System;
    using Contracts;
    using Contracts.Progressives;

    /// <summary>
    ///     Extension methods for ProgressiveTypes
    /// </summary>
    public static class ProgressiveTypeExtensions
    {
        /// <summary>
        ///     Converts the progressive type to a progressive level type
        /// </summary>
        /// <param name="type">The type to convert</param>
        /// <returns>The converted type</returns>
        public static ProgressiveLevelType ToProgressiveLevelType(this ProgressiveTypes type)
        {
            switch (type)
            {
                case ProgressiveTypes.SAP:
                    return ProgressiveLevelType.Sap;
                case ProgressiveTypes.LAP:
                    return ProgressiveLevelType.LP;
                case ProgressiveTypes.Selectable:
                    return ProgressiveLevelType.Selectable;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}