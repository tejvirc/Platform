namespace Aristocrat.Monaco.Asp.Progressive
{
    using System;
    using System.Reflection;
    using Hardware.Contracts.Persistence;
    using log4net;
    using System.Collections.Generic;
    using System.Linq;

    /// <inheritdoc />
    public class PerLevelMeterProvider : IPerLevelMeterProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IPersistentStorageAccessor _persistentStorageAccessor;

        private IEnumerable<string> PerLevelMeterNames { get; } = new List<string>
        {
            ProgressivePerLevelMeters.JackpotHitStatus,
            ProgressivePerLevelMeters.LinkJackpotHitAmountWon,
            ProgressivePerLevelMeters.CurrentJackpotNumber,
            ProgressivePerLevelMeters.JackpotControllerId,
            ProgressivePerLevelMeters.JackpotAmountUpdate,
            ProgressivePerLevelMeters.TotalJackpotAmount,
            ProgressivePerLevelMeters.TotalJackpotHitCount,
            ProgressivePerLevelMeters.JackpotResetCounter,
        };

        public PerLevelMeterProvider(IPersistentStorageManager persistentStorageManager)
        {
            if (persistentStorageManager == null) throw new ArgumentNullException(nameof(persistentStorageManager));

            InitialisePerLevelMeterStorage(persistentStorageManager);
        }

        /// <inheritdoc />
        public long GetValue(int levelId, string meterName)
        {
            try
            {
                var key = GetName(levelId, meterName);

                var meters = _persistentStorageAccessor.GetAll()?.FirstOrDefault();

                var levelMeters = meters?.Value;
                if (levelMeters == null || !levelMeters.ContainsKey(key)) return 0;

                if (!long.TryParse(levelMeters[key].ToString(), out var meterValue)) return 0;

                return meterValue;
            }
            catch (Exception ex)
            {
                Log.Error($"Exception thrown getting value from per level meter ${meterName} for levelId {levelId}", ex);
                return 0;
            }
        }

        /// <inheritdoc />
        public void SetValue(int levelId, string meterName, long value)
        {
            try
            {
                using (var persistentStorageTransaction = _persistentStorageAccessor.StartTransaction())
                {
                    var key = GetName(levelId, meterName);

                    persistentStorageTransaction[key] = value;
                    persistentStorageTransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception thrown setting per level meter value ${meterName} for levelId {levelId}", ex);
            }
        }

        /// <inheritdoc />
        public void IncrementValue(int levelId, string meterName, long incrementBy)
        {
            try
            {
                SetValue(levelId, meterName, GetValue(levelId, meterName) + incrementBy);
            }
            catch (Exception ex)
            {
                Log.Error($"Exception thrown incrementing per level meter ${meterName} for levelId {levelId}", ex);
            }
        }

        private static string GetName(int levelId, string meterName)
        {
            return $"{meterName}_Level_{levelId}";
        }

        private void InitialisePerLevelMeterStorage(IPersistentStorageManager persistentStorageManager)
        {
            var blockName = GetType().ToString();

            var blockExists = persistentStorageManager.BlockExists(blockName);
            if (blockExists)
            {
                _persistentStorageAccessor = persistentStorageManager.GetBlock(blockName);
                return;
            }

            var blockFormat = new BlockFormat();
            PerLevelMeterNames.SelectMany(meterName => Enumerable.Range(0, ProgressiveConstants.LinkProgressiveMaxLevels)
                    .Select(levelId => new FieldDescription(FieldType.Int64, 0, $"{meterName}_Level_{levelId}")))
                .ToList()
                .ForEach(f => blockFormat.AddFieldDescription(f));

            _persistentStorageAccessor = persistentStorageManager.CreateDynamicBlock(PersistenceLevel.Static, blockName, 1, blockFormat);

            using (var persistentStorageTransaction = _persistentStorageAccessor.StartTransaction())
            {
                //Initialise meters to 0 for the max levels supported under ASP for progressive controllers
                PerLevelMeterNames.SelectMany(m => Enumerable.Range(0, ProgressiveConstants.LinkProgressiveMaxLevels).Select(s => GetName(s, m)))
                    .ToList()
                    .ForEach(f => persistentStorageTransaction[f] = (long)0);

                persistentStorageTransaction.Commit();
            }
        }
    }
}
