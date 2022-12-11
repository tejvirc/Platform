namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    ///     The class which contains the DateTime at which the counters were captured and their values
    ///     This class will be written to the log files, and read from when asked.
    /// </summary>
    [ProtoContract]
    public class PerformanceCounters
    {
        /// <summary>
        /// Protobuf serialization/deserialization requires an empty constructor.
        /// </summary>
        public PerformanceCounters()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public PerformanceCounters(int version = 1)
        {
            Version = version;
            Values = Array.Empty<double>();
        }

        /// <summary>
        ///     Version for the counters
        /// </summary>
        [ProtoMember(1)]
        public int Version { get; set; }

        /// <summary>
        ///     The time at which the counters were captured
        /// </summary>
        [ProtoMember(2)]
        public DateTime DateTime { get; set; }

        /// <summary>
        ///     Contains the values of different metrics
        /// </summary>
        [ProtoMember(3)]
        public double[] Values { get; set; }

        /// <summary>
        ///     Dictionary that contains the counters to capture
        /// </summary>
        public IReadOnlyDictionary<MetricType, double> CounterDictionary => Values
            ?.Select((v, i) => new { i, v }).ToDictionary(k => (MetricType)k.i, v => v.v);
    }
}