namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Reflection;
    using Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Contracts.Progressives;
    using log4net;

    public class StandardCalculator : ICalculatorStrategy
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IMysteryProgressiveProvider _mysteryProgressiveProvider;

        public StandardCalculator(IMysteryProgressiveProvider mysteryProgressiveProvider)
        {
            _mysteryProgressiveProvider = mysteryProgressiveProvider ?? throw new ArgumentNullException(nameof(mysteryProgressiveProvider));
        }

        public void ApplyContribution(ProgressiveLevel level, ProgressiveLevelUpdate levelUpdate, IMeter hiddenTotalMeter)
        {
            throw new NotSupportedException();
        }

        public void Increment(ProgressiveLevel level, long wager, long ante, IMeter hiddenTotalMeter)
        {
            const decimal percentToFraction = 0.01m;

            var incrValue = level.IncrementRate.ToPercentage() * percentToFraction * wager;
            var hiddenIncrementValue =
                (long)decimal.Round(level.HiddenIncrementRate.ToPercentage() * percentToFraction * wager);

            // Use banker's rounding (midpoint to even) to prevent millicent truncation on conversion to long
            level.CurrentValue += (long)decimal.Round(incrValue);
            level.HiddenValue += hiddenIncrementValue;
            hiddenTotalMeter?.Increment(hiddenIncrementValue);

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
            level.CurrentValue = resetValue + level.Overflow + level.HiddenValue;

            level.Overflow = 0;
            level.HiddenValue = 0;
            // OverflowTotal will not reset to zero

            CheckOverflow(level);
        }

        public long Claim(ProgressiveLevel level)
        {
            return Claim(level, level.ResetValue);
        }

        /// <summary>
        /// Mystery Progressives require payout via the Trigger Amount (MagicNumber)
        /// Same calculations of VLT-15592 are done as well as adding the difference of (current - trigger) to the reset amount
        /// </summary>
        /// <param name="level"></param>
        /// <param name="resetValue"></param>
        /// <returns></returns>
        public long MysteryClaim(ProgressiveLevel level, long resetValue)
        {
            var current = level.CurrentValue;

            var success = _mysteryProgressiveProvider.TryGetMagicNumber(level, out var triggerAmountInMillicents);
            Logger.Debug($"MagicNumber - {triggerAmountInMillicents}");
            Logger.Debug($"CurrentAmount = {current}");

            var differenceTriggerAndCurrent = current - triggerAmountInMillicents;

            var triggerWinAmountNoFraction = triggerAmountInMillicents.MillicentsToDollarsNoFraction();
            var triggerWinMillicentsNoFraction = triggerWinAmountNoFraction.DollarsToMillicents();
            var triggerAmountInDollars = triggerAmountInMillicents.MillicentsToDollars();
            var triggerAmountFractional = (triggerAmountInDollars - triggerWinAmountNoFraction).DollarsToMillicents();

            Logger.Debug($"MillicentsNoFractional - {triggerWinMillicentsNoFraction}");
            Logger.Debug($"CarryOnValue = {differenceTriggerAndCurrent} , Fractional = {triggerAmountFractional}");
            Reset(level, resetValue + triggerAmountFractional + differenceTriggerAndCurrent);
            Logger.Debug($"resetValue = {resetValue + triggerAmountFractional + differenceTriggerAndCurrent}");

            return triggerWinMillicentsNoFraction;
        }

        public long Claim(ProgressiveLevel level, long resetValue)
        {

            if (level.TriggerControl == TriggerType.Mystery)
            {
                return MysteryClaim(level, resetValue);
            }

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