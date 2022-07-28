namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The Class which contains the DateTime at which the counters were captured and their values
    ///     This class will be written to the log files, and read from when asked.
    /// </summary>
    [Serializable]
    public class PerformanceCounters
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public PerformanceCounters(int version = 1)
        {
            Version = version;
            Values = new double[0];
        }

        /// <summary>
        ///     Version for the counters
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        ///     The time at which the counters were captured
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        ///     contains the values of different metrics
        /// </summary>
        public double[] Values { get; set; }

        /// <summary>
        ///     Dictionary that contains the counters to capture
        /// </summary>
        public IReadOnlyDictionary<MetricType, double> CounterDictionary => Values
            ?.Select((v, i) => new { i, v }).ToDictionary(k => (MetricType)k.i, v => v.v);
    }
}