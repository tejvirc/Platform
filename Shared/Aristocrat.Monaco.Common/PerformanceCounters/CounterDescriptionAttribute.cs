namespace Aristocrat.Monaco.Common.PerformanceCounters
{
    using System;
    using System.Diagnostics;

    /// <summary>
    ///     Defines a storage block entity
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class CounterDescriptionAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CounterDescriptionAttribute" /> class.
        /// </summary>
        public CounterDescriptionAttribute(string name, PerformanceCounterType type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CounterDescriptionAttribute" /> class.
        /// </summary>
        public CounterDescriptionAttribute(string category, string name, PerformanceCounterType type)
        {
            Category = category;
            Name = name;
            Type = type;
        }

        /// <summary>
        ///     Gets or sets the performance counter category
        /// </summary>
        public string Category { get; } = @"Monaco Platform";

        /// <summary>
        ///     Gets or sets the performance counter name
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the performance counter type
        /// </summary>
        public PerformanceCounterType Type { get; }
    }
}