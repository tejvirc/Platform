namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    public enum MetricType
    {
        /// <summary>
        ///     Total system CPU usage
        /// </summary>
        [CounterType("TotalCPU")]
        [Instance("_Total")]
        [Category("Processor")]
        [Counter("% Processor Time")]
        [Label("Total CPU")]
        [Unit("%")]
        [MaxRange(100)]
        [LabelResourceKey("MetricTotalCpu")]
        TotalProcessorTime,

        /// <summary>
        ///     Total system free memory
        /// </summary>
        [CounterType("TotalMemory")]
        [Instance("")]
        [Category("Memory")]
        [Counter("Available Bytes")]
        [Label("Free Memory")]
        [Unit("MB")]
        [MaxRange(4096)]
        [LabelResourceKey("MetricFreeMemory")]
        [UnitResourceKey("MBLabel")]
        FreeMemory,

        /// <summary>
        ///     Total system GPU usage
        /// </summary>
        [CounterType("GPU")]
        [Instance("*")]
        [Category("GPU Engine")]
        [Counter("Utilization Percentage")]
        [Label("GPU Usage")]
        [Unit("%")]
        [MaxRange(100)]
        [LabelResourceKey("MetricGpuUsage")]
        TotalGpuTime,

        /// <summary>
        ///     Platform CPU usage
        /// </summary>
        [CounterType("CPU")]
        [Instance("Bootstrap")]
        [Category("Process")]
        [Counter("% Processor Time")]
        [Label("Platform CPU")]
        [Unit("%")]
        [MaxRange(100)]
        [LabelResourceKey("MetricCpuTime")]
        MonacoProcessorTime,

        /// <summary>
        ///     Platform thread count
        /// </summary>
        [CounterType("General")]
        [Instance("Bootstrap")]
        [Category("Process")]
        [Counter("Thread Count")]
        [Label("Platform Threads")]
        [Unit("#")]
        [MaxRange(200)]
        [LabelResourceKey("MetricThreadCount")]
        MonacoThreadCount,

        /// <summary>
        ///     Platform memory usage
        /// </summary>
        [CounterType("Memory")]
        [Instance("Bootstrap")]
        [Category("Process")]
        [Counter("Private Bytes")]
        [Label("Platform Memory")]
        [Unit("MB")]
        [MaxRange(3072)]
        [LabelResourceKey("MetricMonacoPrivateBytes")]
        [UnitResourceKey("MBLabel")]
        MonacoPrivateBytes,

        /// <summary>
        ///     Dotnet memory usage
        /// </summary>
        [CounterType("Memory")]
        [Instance("Bootstrap")]
        [Category(".Net CLR Memory")]
        [Counter("# Bytes in All Heaps")]
        [Label("CLR Memory")]
        [Unit("MB")]
        [MaxRange(1000)]
        [LabelResourceKey("MetricClrBytes")]
        [UnitResourceKey("MBLabel")]
        ClrBytes,

        /// <summary>
        ///     GDK CPU usage
        /// </summary>
        [CounterType("CPU")]
        [Instance("GDKRuntimeHost")]
        [Category("Process")]
        [Counter("% Processor Time")]
        [Label("Game CPU")]
        [Unit("%")]
        [MaxRange(100)]
        [LabelResourceKey("MetricGdkProcessorTime")]
        GdkProcessorTime,

        /// <summary>
        ///     GDK thread count
        /// </summary>
        [CounterType("General")]
        [Instance("GDKRuntimeHost")]
        [Category("Process")]
        [Counter("Thread Count")]
        [Label("Game Threads")]
        [Unit("#")]
        [MaxRange(200)]
        [LabelResourceKey("MetricGdkThreadCount")]
        GdkThreadCount,

        /// <summary>
        ///     GDK memory usage
        /// </summary>
        [CounterType("Memory")]
        [Instance("GDKRuntimeHost")]
        [Category("Process")]
        [Counter("Private Bytes")]
        [Label("Game Memory")]
        [Unit("MB")]
        [MaxRange(1000)]
        [LabelResourceKey("MetricGdkPrivateBytes")]
        [UnitResourceKey("MBLabel")]
        GdkPrivateBytes,
        /// <summary>
        /// Number of frames dropped.
        /// </summary>
        [CounterType("General")]
        [Instance("0")]
        [Category("Aristocrat Runtime Host")]
        [Counter("Frame Drops")]
        [Label("Frame Drops")]
        [Unit("#")]
        [MaxRange(10000)]
        [LabelResourceKey("MetricGdkFrameDrops")]
        FrameDrops,

        /// <summary>
        /// Number of frames per second.
        /// </summary>
        [CounterType("General")]
        [Instance("0")]
        [Category("Aristocrat Runtime Host")]
        [Counter("Framerate")]
        [Label("Framerate")]
        [Unit("#")]
        [MaxRange(100)]
        [LabelResourceKey("MetricGdkFramerate")]
        Framerate,

        /// <summary>
        /// Update Time(ms).
        /// </summary>
        [CounterType("General")]
        [Instance("0")]
        [Category("Aristocrat Runtime Host")]
        [Counter("Game Update Time(ms)")]
        [Label("Update Time")]
        [Unit("ms")]
        [MaxRange(10)]
        [LabelResourceKey("MetricGdkGamesUpdateTimeMs")]
        [UnitResourceKey("MillisecondLabel")]
        GamesUpdateTime,

        /// <summary>
        /// Total Time spent in IPC Communication(ms).
        /// </summary>
        [CounterType("General")]
        [Instance("0")]
        [Category("Aristocrat Runtime Host")]
        [Counter("IPC Blocking Time Acc(ms)")]
        [Label("IPC Blocking Time Acc")]
        [Unit("ms")]
        [MaxRange(100)]
        [LabelResourceKey("MetricGdkIPCBlockingTimeAccMs")]
        [UnitResourceKey("MillisecondLabel")]
        IPCBlockingTimeAcc,

        /// <summary>
        /// Dotnet memory usage
        /// </summary>
        [CounterType("General")]
        [Instance("0")]
        [Category("Aristocrat Runtime Host")]
        [Counter("Render Time(ms)")]
        [Label("Render Time")]
        [Unit("ms")]
        [MaxRange(100)]
        [LabelResourceKey("MetricGdkRenderTimeMs")]
        [UnitResourceKey("MillisecondLabel")]
        RenderTime,

        /// <summary>
        /// Runtime Update Time(ms).
        /// </summary>
        [CounterType("General")]
        [Instance("0")]
        [Category("Aristocrat Runtime Host")]
        [Counter("Runtime Update Time(ms)")]
        [Label("Runtime Update Time")]
        [Unit("ms")]
        [MaxRange(100)]
        [LabelResourceKey("MetricGdkRuntimeUpdateTimeMs")]
        [UnitResourceKey("MillisecondLabel")]
        RuntimeUpdateTime,

        /// <summary>
        /// Swapbuffer Time(ms).
        /// </summary>
        [CounterType("General")]
        [Instance("0")]
        [Category("Aristocrat Runtime Host")]
        [Counter("Swapbuffer Time(ms)")]
        [Label("Swapbuffer Time")]
        [Unit("ms")]
        [MaxRange(100)]
        [LabelResourceKey("MetricGdkSwapbufferTimeMs")]
        [UnitResourceKey("MillisecondLabel")]
        SwapbufferTime,
        /// <summary>
        ///     CPU Temperature
        /// </summary>
        [CounterType("CPUTemp")]
        [Instance("Bootstrap")]
        [Category("CPUTemp")]
        [Counter("CPU Temp")]
        [Label("CPU Temperature")]
        [Unit("*")]
        [MaxRange(100)]
        [LabelResourceKey("MetricCpuTemperature")]
        CpuTemperature
    }
}
