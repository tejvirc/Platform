namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;

    /// <summary>
    ///     An attribute for the MetricType enum that specifies the maximum counter range, for display
    /// </summary>
    public class MaxRangeAttribute : Attribute
    {
        /// <summary>
        ///     The maximum range for the counter
        /// </summary>
        public int MaxRange { get; }

        /// <summary>
        ///     Constructor for the MaxRangeAttribute
        /// </summary>
        /// <param name="maxRange">The maximum range for the counter</param>
        public MaxRangeAttribute(int maxRange)
        {
            MaxRange = maxRange;
        }
    }

    /// <summary>
    ///     An attribute for the MetricType enum that specifies the performance counter type,
    ///     which specifies the general counter type, such as CPU usage or free memory
    /// </summary>
    public class CounterTypeAttribute : Attribute
    {
        /// <summary>
        ///     The type for the counter
        /// </summary>
        public string CounterType { get; }

        /// <summary>
        ///     Constructor for the CounterTypeAttribute
        /// </summary>
        /// <param name="type">The type for the counter</param>
        public CounterTypeAttribute(string type)
        {
            CounterType = type;
        }
    }

    /// <summary>
    ///     An attribute for the MetricType enum that specifies the performance counter instance,
    ///     which usually indicates the process name being measured by the counter
    /// </summary>
    public class InstanceAttribute : Attribute
    {
        /// <summary>
        ///     The instance (process name) for the counter
        /// </summary>
        public string Instance { get; }

        /// <summary>
        ///     Constructor for the InstanceAttribute
        /// </summary>
        /// <param name="instance">The instance (process name) for the counter</param>
        public InstanceAttribute(string instance)
        {
            Instance = instance;
        }
    }

    /// <summary>
    ///     An attribute for the MetricType enum that specifies the performance counter, which
    ///     distinguishes specific counters under a general type and instance name
    /// </summary>
    public class CounterAttribute : Attribute
    {
        /// <summary>
        ///     The specific counter to read for the performance counter
        /// </summary>
        public string Counter { get; }

        /// <summary>
        ///     Constructor for the CounterAttribute
        /// </summary>
        /// <param name="counter">The specific counter to read for the performance counter</param>
        public CounterAttribute(string counter)
        {
            Counter = counter;
        }
    }

    /// <summary>
    ///     An attribute for the MetricType enum that specifies the counter label, for display
    /// </summary>
    public class LabelAttribute : Attribute
    {
        /// <summary>
        ///     The label for the counter
        /// </summary>
        public string Label { get; }

        /// <summary>
        ///     Constructor for the LabelAttribute
        /// </summary>
        /// <param name="label">The label for the counter</param>
        public LabelAttribute(string label)
        {
            Label = label;
        }
    }

    /// <summary>
    ///     An attribute for the MetricType enum that specifies the counter units, for display
    /// </summary>
    public class UnitAttribute : Attribute
    {
        /// <summary>
        ///     The unit of measurement for the counter
        /// </summary>
        public string Unit { get; }

        /// <summary>
        ///     Constructor for the UnitAttribute
        /// </summary>
        /// <param name="unit">The unit of measurement for the counter</param>
        public UnitAttribute(string unit)
        {
            Unit = unit;
        }
    }

    /// <summary>
    ///     An attribute for the MetricType enum that specifies the key to lookup in resources, for multi-language
    /// </summary>
    public class LabelResourceKeyAttribute : Attribute
    {
        /// <summary>
        ///     The key to lookup in resources
        /// </summary>
        public string LabelResourceKey { get; }

        /// <summary>
        ///     Constructor for the LabelResourceKeyAttribute
        /// </summary>
        /// <param name="labelResourceKey">The key for the label</param>
        public LabelResourceKeyAttribute(string labelResourceKey)
        {
            LabelResourceKey = labelResourceKey;
        }
    }
}
