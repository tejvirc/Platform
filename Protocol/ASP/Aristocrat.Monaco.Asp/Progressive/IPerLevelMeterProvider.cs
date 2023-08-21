namespace Aristocrat.Monaco.Asp.Progressive
{
    /// <summary>
    /// Provides access to link progressive "meters" which track per progressive level
    /// </summary>
    public interface IPerLevelMeterProvider
    {
        /// <summary>
        /// Gets per level id meter value
        /// </summary>
        long GetValue(int levelId, string meterName);

        /// <summary>
        /// Sets and persists per level id meter value
        /// </summary>
        void SetValue(int levelId, string meterName, long value);

        /// <summary>
        /// Increments and persists per level id meter value
        /// </summary>
        void IncrementValue(int levelId, string meterName, long incrementBy = 1);
    }
}