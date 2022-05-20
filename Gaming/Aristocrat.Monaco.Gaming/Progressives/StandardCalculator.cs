namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Contracts.Progressives;

    public class StandardCalculator : ICalculatorStrategy
    {
        public void ApplyContribution(ProgressiveLevel level, ProgressiveLevelUpdate levelUpdate)
        {
            throw new NotSupportedException();
        }

        public void Increment(ProgressiveLevel level, long wager, long ante)
        {
            const decimal percentToFraction = 0.01m;

            var incrValue = level.IncrementRate.ToPercentage() * percentToFraction * wager;
            var hiddenIncrementValue = level.HiddenIncrementRate.ToPercentage() * percentToFraction * wager;

            // Use banker's rounding (midpoint to even) to prevent millicent truncation on conversion to long
            level.CurrentValue += (long)decimal.Round(incrValue);
            level.HiddenTotal += (long)decimal.Round(hiddenIncrementValue);

            if (level.LevelType == ProgressiveLevelType.Sap)
            {
                level.CanEdit = false;
            }

            CheckOverflow(level);
        }

        public void Reset(ProgressiveLevel level)
        {
            Reset(level, level.ResetValue);
        }

        public void Reset(ProgressiveLevel level, long resetValue)
        {
            level.CurrentValue = resetValue + level.Overflow + level.HiddenTotal;

            level.Overflow = 0;
            level.HiddenTotal = 0;
            // OverflowTotal will not reset to zero

            CheckOverflow(level);
        }

        public long Claim(ProgressiveLevel level)
        {
            return Claim(level, level.ResetValue);
        }

        public long Claim(ProgressiveLevel level, long resetValue)
        {
            var current = level.CurrentValue;

            // VLT-15592 - Take the truncated fractional amount out of the level value before its claimed, 
            // and add to back to the level during the reset. Ex: 4006650 ($40.06650), fractional: ....650            
            var dollarsNoFractional = current.MillicentsToDollarsNoFraction();
            var dollars = current.MillicentsToDollars();
            var fractional = (dollars - dollarsNoFractional).DollarsToMillicents();
            var millicentsNoFractional = dollarsNoFractional.DollarsToMillicents();

            Reset(level, resetValue + fractional);

            return millicentsNoFractional;
        }

        private static void CheckOverflow(ProgressiveLevel level)
        {
            if (level.MaximumValue > 0 && level.CurrentValue > level.MaximumValue)
            {
                level.Overflow += level.CurrentValue - level.MaximumValue;
                level.OverflowTotal += level.CurrentValue - level.MaximumValue;
                level.CurrentValue = level.MaximumValue;
            }
        }
    }
}