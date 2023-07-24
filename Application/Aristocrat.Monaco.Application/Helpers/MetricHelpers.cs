namespace Aristocrat.Monaco.Application.Helpers
{
    using Common;
    using Contracts.Localization;
    using PerformanceCounter;

    public static class MetricHelpers
    {
        /// <summary>
        /// Helper to localize a MetricType based on the attributes
        /// </summary>
        /// <param name="metricType">The metric to get the label for</param>
        /// <returns>A localized label for the MetricType</returns>
        public static string GetMetricLabel(this MetricType metricType)
        {
            var metricLabel = Localizer.For(CultureFor.Operator).GetString(metricType.GetAttribute<LabelResourceKeyAttribute>().LabelResourceKey);
            var metricUnitResourceKey = metricType.GetAttribute<UnitResourceKeyAttribute>()?.UnitResourceKey;
            var metricUnit = string.IsNullOrWhiteSpace(metricUnitResourceKey)
                ? metricType.GetAttribute<UnitAttribute>().Unit
                : Localizer.For(CultureFor.Operator).GetString(metricUnitResourceKey);
            return metricLabel + " " + metricUnit;
        }
    }
}
