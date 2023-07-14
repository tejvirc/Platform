namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Common;
    using log4net;

    /// <summary>
    ///     Represents a performance counter value that is being monitored. This is a wrapper for
    ///     a performance counter that adds the ability to check if a counter is currently valid
    ///     and to manipulate the returned value based on the counter type, to standardize the
    ///     range or units of the counter.
    /// </summary>
    public class PlatformMetric
    {
        private const double MebiBytes = 1024 * 1024;
        private const double MemoryCap = 4096;
        private const double NoValue = 0.0;
        private const int RoundingDecimals = 2;

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly object _lock = new();

        private IPerformanceCounterWrapper _counter;

        /// <summary>
        ///     Constructs a PlatformMetric that will monitor a given system performance counter
        /// </summary>
        /// <param name="metric">A MetricType value that specifies the performance counter</param>
        public PlatformMetric(MetricType metric)
        {
            MetricName = metric;
            Instance = metric.GetAttribute<InstanceAttribute>().Instance;
            Counter = metric.GetAttribute<CounterAttribute>().Counter;
            Category = metric.GetAttribute<CategoryAttribute>().Category;
            CounterType = metric.GetAttribute<CounterTypeAttribute>().CounterType;
        }

        /// <summary>
        ///     Checks if the counter is currently valid and if so returns the value the system
        ///     is currently showing for the counter.
        /// </summary>
        public double Value
        {
            get
            {
                lock (_lock)
                {
                    return GetMetric();
                }
            }
        }

        public void SetMetric(uint value)
        {
            lock (_lock)
            {
                if (!ValidateCounter())
                {
                    return;
                }
                
                _counter.SetRawValue(value);
            }
        }

        public MetricType MetricName { get; }

        private string CounterType { get; }

        private string Category { get; }

        private string Instance { get; }

        private string Counter { get; }

        private double GetMetric()
        {
            if (!ValidateCounter())
            {
                return NoValue;
            }

            try
            {
                switch (CounterType)
                {
                    case "CPU":
                        // If system has multiple cores, that should be taken into account
                        return Math.Round(_counter.NextValue() / Environment.ProcessorCount,
                            RoundingDecimals);

                    case "TotalCPU":
                        // However for total CPU the system gives an already scaled value
                        return Math.Round(_counter.NextValue(), RoundingDecimals);

                    case "Memory":
                        // The return value is in MB
                        return Math.Round(_counter.NextValue() / MebiBytes,
                            RoundingDecimals);

                    case "TotalMemory":
                        // The return value is in MB
                        var mem = Math.Round(_counter.NextValue() / MebiBytes,
                            RoundingDecimals);
                        // Cap it at 4GB, we only really care if it gets lower
                        return Math.Min(mem, MemoryCap);

                    case "GPU":
                        // This is a percentage after we add up all the values per process/type
                        var gpu = Math.Round(_counter.NextValue(), RoundingDecimals);
                        // Sometimes the system gives a wild reading on the first sample, so cap it
                        return Math.Min(gpu, 101.0);

                    default:
                        // "General" which usually means a counter, but we'll keep some decimals
                        return Math.Round(_counter.NextValue(),
                            RoundingDecimals);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return NoValue;
        }

        private bool ValidateCounter()
        {
            if (_counter != null)
            {
                return InstanceCurrentlyValid();
            }

            try
            {
                if (string.IsNullOrEmpty(Instance))
                {
                    var perfMon = new SinglePerformanceCounterWrapper(Category, Counter);

                    Logger.Debug(
                        $"Created a performance counter with category = {Category}, " +
                        $"processName = [SYSTEM], countType = {CounterType}, metricType = {MetricName}");

                    _counter = perfMon;
                }
                else if (Instance == "*")
                {
                    // This is for the GPU where we can't read the total performance from a single counter.
                    var perfMon = new AggregatePerformanceCounterWrapper(Category, Counter);

                    Logger.Debug(
                        $"Created an aggregated performance counter with category = {Category}, " +
                        $"processName = [AGGREGATE], countType = {CounterType}, metricType = {MetricName}");

                    _counter = perfMon;
                }
                else if (MetricName == MetricType.CpuTemperature)
                {
                    var perfMon = new SingleCustomPerformanceCounterWrapper(Category, Counter, "A custom category for Monaco", "CPU Temperature Counter");

                    Logger.Debug(
                        $"Created a performance counter with category = {Category}, " +
                        $"counterType = {CounterType}, metricType = {MetricName}");

                    _counter = perfMon;
                }
                else if (MetricName == MetricType.CpuTemperature)
                {
                    var perfMon = new SingleCustomPerformanceCounterWrapper(Category, Counter, "A custom category for Monaco", "CPU Temperature Counter");

                    Logger.Debug(
                        $"Created a performance counter with category = {Category}, " +
                        $"counterType = {CounterType}, metricType = {MetricName}");

                    _counter = perfMon;
                }
                else
                {
                    var perfMon = new SinglePerformanceCounterWrapper(Category, Counter, Instance);

                    Logger.Debug(
                        $"Created a performance counter with category = {Category}, " +
                        $"processName = {Instance}, countType = {CounterType}, metricType = {MetricName}");

                    _counter = perfMon;
                }
            }
            catch (Exception e) when (e is Win32Exception ||
                                      e is UnauthorizedAccessException ||
                                      e is InvalidOperationException)
            {
                Logger.Debug(
                    $"Failed to create a performance counter with category = {Category}, " +
                    $"processName = {Instance}, countType = {CounterType}, metricType = {MetricName}",
                    e);
            }

            return _counter != null && InstanceCurrentlyValid();
        }

        private bool InstanceCurrentlyValid()
        {
            return string.IsNullOrEmpty(Instance) ||
                   (Instance == "0" && PerformanceCounterCategory.InstanceExists(Instance, Category)) ||    // check if GDK Runtime counter instances are available to avoid unneeded failures when calling NextValue
                   Instance == "*" ||
                   Instance == "_Total" ||
                   Process.GetProcessesByName(Instance).Any();
        }
    }
}