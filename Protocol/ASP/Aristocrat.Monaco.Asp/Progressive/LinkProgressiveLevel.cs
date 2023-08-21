namespace Aristocrat.Monaco.Asp.Progressive
{
    using System;

    /// <inheritdoc cref="ILinkProgressiveLevel"/>
    public class LinkProgressiveLevel : ILinkProgressiveLevel
    {
        private readonly Func<int, string, long> _getPerLevelMeterValueCallback;
        private readonly Func<int, long> _getLevelAmountUpdateCallback;

        public int LevelId { get; }
        public string Name { get; }

        public long ProgressiveJackpotAmountUpdate => _getLevelAmountUpdateCallback(LevelId);

        public long JackpotResetCounter => _getPerLevelMeterValueCallback(LevelId, ProgressivePerLevelMeters.JackpotResetCounter);
        public long TotalAmountWon => _getPerLevelMeterValueCallback(LevelId, ProgressivePerLevelMeters.TotalJackpotAmount);
        public long TotalJackpotHitCount => _getPerLevelMeterValueCallback(LevelId, ProgressivePerLevelMeters.TotalJackpotHitCount);
        public long LinkJackpotHitAmountWon => _getPerLevelMeterValueCallback(LevelId, ProgressivePerLevelMeters.LinkJackpotHitAmountWon);
        public long JackpotHitStatus => _getPerLevelMeterValueCallback(LevelId, ProgressivePerLevelMeters.JackpotHitStatus);
        public long CurrentJackpotNumber => _getPerLevelMeterValueCallback(LevelId, ProgressivePerLevelMeters.CurrentJackpotNumber);
        public int JackpotControllerIdByteOne => GetJackpotControllerId()?.ByteOne ?? 0;
        public int JackpotControllerIdByteTwo => GetJackpotControllerId()?.ByteTwo ?? 0;
        public int JackpotControllerIdByteThree => GetJackpotControllerId()?.ByteThree ?? 0;

        /// <summary>
        /// Constructor for LinkProgressiveLevel
        /// </summary>
        /// <param name="levelId">The progressive level number</param>
        /// <param name="levelName">The name of the level, eg: Major, Minor</param>
        /// <param name="getPerLevelMeterValueCallback">Callback to allow getting value from meters which track per progressive level</param>
        /// <param name="getLevelAmountUpdateCallback">Callback to allow getting updated level details from the game layer</param>
        public LinkProgressiveLevel(int levelId, string levelName, Func<int, string, long> getPerLevelMeterValueCallback, Func<int, long> getLevelAmountUpdateCallback)
        {
            _getPerLevelMeterValueCallback = getPerLevelMeterValueCallback ?? throw new ArgumentNullException(nameof(getPerLevelMeterValueCallback));
            _getLevelAmountUpdateCallback = getLevelAmountUpdateCallback ?? throw new ArgumentNullException(nameof(getLevelAmountUpdateCallback));

            LevelId = levelId;
            Name = levelName;
        }

        private (int ByteOne, int ByteTwo, int ByteThree)? GetJackpotControllerId()
        {
            var currentValue = _getPerLevelMeterValueCallback(LevelId, ProgressivePerLevelMeters.JackpotControllerId);
            if (currentValue == 0) return null;

            var bytes = DecodeJackpotControllerId(currentValue);
            return (bytes[0], bytes[1], bytes[2]);
        }

        private static int[] DecodeJackpotControllerId(long jackpotControllerId)
        {
            return new[]
            {
                (int)jackpotControllerId & 0xff,
                (int)(jackpotControllerId >> 8) & 0xff,
                (int)(jackpotControllerId >> 16) & 0xff,
            };
        }
    }
}