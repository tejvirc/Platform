namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Reflection;
    using Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Contracts.Progressives;
    using log4net;

    /// <summary>
    ///     Progressive Calculator Base class for adding contributions to progressive levels.
    /// </summary>
    public class CalculatorBase : ICalculatorStrategy
    {
        private const long Divisor = 100000000;
        private IMysteryProgressiveProvider _mysteryProgressiveProvider;
        private readonly ILog _logger;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="mysteryProgressiveProvider">An isntance of IMysteryProgressiveProvider.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CalculatorBase(IMysteryProgressiveProvider mysteryProgressiveProvider, ILog logger)
        {
            _mysteryProgressiveProvider = mysteryProgressiveProvider ?? throw new ArgumentNullException(nameof(mysteryProgressiveProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public virtual void Reset(ProgressiveLevel level, long resetValue)
        {
            _logger.Info($"Reset not supported for {level.FundingType}");
        }

        /// <inheritdoc />
        public long Claim(ProgressiveLevel level)
        {
            return Claim(level, level.ResetValue);
        }

        /// <inheritdoc />
        public virtual long Claim(ProgressiveLevel level, long resetValue)
        {
            _logger.Info($"Claim not supported for {level.FundingType}");
            return 0;
        }

        /// <inheritdoc />
        public void ApplyContribution(ProgressiveLevel level, ProgressiveLevelUpdate levelUpdate, IMeter hiddenTotalMeter, IMeter bulkTotalMeter)
        {
            level.Residual += levelUpdate.Fraction;

            var overflow = level.Residual / Divisor;
            level.Residual %= Divisor;
            level.CurrentValue += levelUpdate.Amount + overflow;
            level.HiddenValue += level.CurrentValue * level.HiddenIncrementRate;
            hiddenTotalMeter?.Increment(level.CurrentValue * level.HiddenIncrementRate);
            bulkTotalMeter?.Increment(levelUpdate.Amount);

            CheckOverflow(level);
        }

        /// <inheritdoc />
        public virtual void Increment(ProgressiveLevel level, long wager, long ante, IMeter hiddenTotalMeter)
        {
            _logger.Info($"Increment not supported for {level.FundingType}");
        }

        /// <inheritdoc />
        public void Reset(ProgressiveLevel level)
        {
            Reset(level, level.ResetValue);
        }

        /// <summary>
        ///     Checks for Progressive Level Overflows.
        /// </summary>
        /// <param name="level">ProgressiveLevel to check for overlow.</param>
        protected static void CheckOverflow(ProgressiveLevel level)
        {
            if (level.MaximumValue > 0 && level.CurrentValue > level.MaximumValue)
            {
                level.Overflow += level.CurrentValue - level.MaximumValue;
                level.OverflowTotal += level.CurrentValue - level.MaximumValue;
                level.CurrentValue = level.MaximumValue;
            }
        }

        /// <summary>
        ///     Mystery Progressives require payout via the Trigger Amount (MagicNumber)
        ///     Same calculations of VLT-15592 are done as well as adding the difference of (current - trigger) to the reset amount
        /// </summary>
        /// <param name="level">Progressive Level.</param>
        /// <param name="resetValue">Level reset value.</param>
        /// <returns></returns>
        protected long MysteryClaim(ProgressiveLevel level, long resetValue)
        {
            var current = level.CurrentValue;

            var success = _mysteryProgressiveProvider.TryGetMagicNumber(level, out var triggerAmountInMillicents);
            if (!success)
            {
                var errorMessage = $"Cannot Get MagicNumber from ProgressiveLevel {level}";
                _logger.Error(errorMessage);
                throw new MissingMysteryTriggerException(errorMessage);
            }

            _logger.Debug($"MagicNumber - {triggerAmountInMillicents}");
            _logger.Debug($"CurrentAmount = {current}");

            var differenceTriggerAndCurrent = current - triggerAmountInMillicents;

            var triggerWinAmountNoFraction = triggerAmountInMillicents.MillicentsToDollarsNoFraction();
            var triggerWinMillicentsNoFraction = triggerWinAmountNoFraction.DollarsToMillicents();
            var triggerAmountInDollars = triggerAmountInMillicents.MillicentsToDollars();
            var triggerAmountFractional = (triggerAmountInDollars - triggerWinAmountNoFraction).DollarsToMillicents();

            _logger.Debug($"MillicentsNoFractional - {triggerWinMillicentsNoFraction}");
            _logger.Debug($"CarryOnValue = {differenceTriggerAndCurrent} , Fractional = {triggerAmountFractional}");
            Reset(level, resetValue + triggerAmountFractional + differenceTriggerAndCurrent);
            _logger.Debug($"resetValue = {resetValue + triggerAmountFractional + differenceTriggerAndCurrent}");

            return triggerWinMillicentsNoFraction;
        }
    }
}
