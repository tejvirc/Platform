namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class MaxRangeAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public int MaxRange { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxRange"></param>
        public MaxRangeAttribute(int maxRange)
        {
            MaxRange = maxRange;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CounterTypeAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string CounterType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public CounterTypeAttribute(string type)
        {
            CounterType = type;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class InstanceAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Instance { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public InstanceAttribute(string instance)
        {
            Instance = instance;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UnitAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        public UnitAttribute(string unit)
        {
            Unit = unit;
        }
    }
}
