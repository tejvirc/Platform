namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    public enum MetricType
    {
        /// <summary>
        /// 
        /// </summary>
        [MaxRange(100)]
        [CounterType("CPU")]
        [Unit("%")]
        [Instance("Bootstrap")]
        [Category("Process")]
        [Description("% Processor Time")]
        MonacoProcessorTime,

        /// <summary>
        /// 
        /// </summary>
        [MaxRange(200)]
        [CounterType("General")]
        [Unit("#")]
        [Instance("Bootstrap")]
        [Category("Process")]
        [Description("Thread Count")]
        MonacoThreadCount,

        /// <summary>
        /// 
        /// </summary>
        [MaxRange(1000)]
        [CounterType("Memory")]
        [Unit("MB")]
        [Instance("Bootstrap")]
        [Category("Process")]
        [Description("Private Bytes")]
        MonacoPrivateBytes,

        /// <summary>
        /// 
        /// </summary>
        [MaxRange(1000)]
        [CounterType("Memory")]
        [Unit("MB")]
        [Instance("Bootstrap")]
        [Category(".Net CLR Memory")]
        [Description("# Bytes in All Heaps")]
        ClrBytes,

        /// <summary>
        /// 
        /// </summary>
        [MaxRange(100)]
        [CounterType("CPU")]
        [Unit("%")]
        [Instance("GDKRuntimeHost")]
        [Category("Process")]
        [Description("% Processor Time")]
        GdkProcessorTime,

        /// <summary>
        /// 
        /// </summary>
        [MaxRange(200)]
        [CounterType("General")]
        [Unit("#")]
        [Instance("GDKRuntimeHost")]
        [Category("Process")]
        [Description("Thread Count")]
        GdkThreadCount,

        /// <summary>
        /// 
        /// </summary>
        [MaxRange(1000)]
        [CounterType("Memory")]
        [Unit("MB")]
        [Instance("GDKRuntimeHost")]
        [Category("Process")]
        [Description("Private Bytes")]
        GdkPrivateBytes
    }
}
