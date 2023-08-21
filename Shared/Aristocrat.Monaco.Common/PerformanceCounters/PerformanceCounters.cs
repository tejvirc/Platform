namespace Aristocrat.Monaco.Common.PerformanceCounters
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using log4net;

    /// <summary>
    ///     Provides a set utility methods that can be used to work with performance counters
    /// </summary>
    public static class PerformanceCounters
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Registers all discovered performance counters from classes that use <see cref="CounterDescriptionAttribute" />
        /// </summary>
        /// <param name="assembly">The assembly to scan</param>
        public static void RegisterFromAttribute(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var attributes =
                from type in assembly.GetExportedTypes()
                where type.IsClass
                where Attribute.IsDefined(type, typeof(CounterDescriptionAttribute))
                select Attribute.GetCustomAttribute(type, typeof(CounterDescriptionAttribute)) as
                    CounterDescriptionAttribute;

            var counters = attributes as CounterDescriptionAttribute[] ?? attributes.ToArray();
            if (!counters.Any())
            {
                return;
            }

            try
            {
                var categories = counters.Select(c => c.Category).Distinct();
                foreach (var category in categories)
                {
                    if (PerformanceCounterCategory.Exists(category))
                    {
                        PerformanceCounterCategory.Delete(category);
                    }
                }
            }
            catch (Win32Exception e)
            {
                // ReSharper disable once StringLiteralTypo
                Logger.Error("Failed to delete performance counter.  Run lodctr /R from the command line to rebuild the performance counters and correct this error.", e);

                return;
            }

            var currentCategory = string.Empty;
            var data = new CounterCreationDataCollection();

            foreach (var counter in counters.OrderBy(c => c.Category))
            {
                if (!string.Equals(currentCategory, counter.Category))
                {
                    if (!string.IsNullOrEmpty(currentCategory))
                    {
                        PerformanceCounterCategory.Create(
                            currentCategory,
                            "Monaco Platform Counters",
                            PerformanceCounterCategoryType.SingleInstance,
                            data);
                    }

                    currentCategory = counter.Category;
                    data = new CounterCreationDataCollection();
                }

                data.Add(
                    new CounterCreationData(
                        counter.Name,
                        $"Reports the {counter.Type} for {counter.Name}",
                        counter.Type));

                if (counter.Type == PerformanceCounterType.AverageCount64 ||
                    counter.Type == PerformanceCounterType.AverageTimer32)
                {
                    data.Add(
                        new CounterCreationData
                        {
                            CounterType = PerformanceCounterType.AverageBase,
                            CounterHelp = "Base for the average",
                            CounterName = $"{counter.Name}Base"
                        });
                }
            }

            PerformanceCounterCategory.Create(
                currentCategory,
                "Monaco Platform Counters",
                PerformanceCounterCategoryType.SingleInstance,
                data);
        }
    }
}