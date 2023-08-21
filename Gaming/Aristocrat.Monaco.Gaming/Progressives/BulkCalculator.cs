namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using Application.Contracts;
    using Contracts.Progressives;

    public class BulkCalculator : ICalculatorStrategy
    {
        private const long Divisor = 100000000;
        
        public void ApplyContribution(ProgressiveLevel level, ProgressiveLevelUpdate levelUpdate, IMeter hiddenTotalMeter)
        {
            level.Residual += levelUpdate.Fraction;

            var overflow = level.Residual / Divisor;
            level.Residual %= Divisor;
            level.CurrentValue += levelUpdate.Amount + overflow;
            level.HiddenValue += level.CurrentValue * level.HiddenIncrementRate;
            hiddenTotalMeter?.Increment(level.CurrentValue * level.HiddenIncrementRate);

            if (level.CurrentValue > level.MaximumValue)
            {
                level.Overflow += level.CurrentValue - level.MaximumValue;
                level.OverflowTotal += level.CurrentValue - level.MaximumValue;
                level.CurrentValue = level.MaximumValue;
            }
        }

        public void Increment(ProgressiveLevel level, long wager, long ante, IMeter hiddenTotalMeter)
        {
            throw new NotSupportedException();
        }

        public void Reset(ProgressiveLevel level)
        {
            Reset(level, level.ResetValue);
        }

        public void Reset(ProgressiveLevel level, long resetValue)
        {
            level.CurrentValue = resetValue + level.Overflow + level.HiddenValue;

            var residual = level.Residual;
            level.Residual = 0;
            level.HiddenValue = 0;
            level.Overflow = 0;
            // OverflowTotal will not reset to zero

            ApplyContribution(level, new ProgressiveLevelUpdate(level.LevelId, 0, residual, false), null);
        }

        public long Claim(ProgressiveLevel level)
        {
            return Claim(level, level.ResetValue);
        }

        public long Claim(ProgressiveLevel level, long resetValue)
        {
            var current = level.CurrentValue;

            Reset(level, resetValue);

            return current;
        }
    }
}