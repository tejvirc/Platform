namespace Aristocrat.Monaco.Gaming.Progressives
{
    using Application.Contracts;
    using Contracts.Progressives;
    using log4net;
    using System.Reflection;

    /// <summary>
    ///     Bulk Calculator used for adding contributions to bulk progressive levels.
    /// </summary>
    public class BulkCalculator : CalculatorBase, ICalculatorStrategy
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="mysteryProgressiveProvider">An isntance of IMysteryProgressiveProvider.</param>
        public BulkCalculator(IMysteryProgressiveProvider mysteryProgressiveProvider)
            : base(mysteryProgressiveProvider, LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType))
        {
        }

        /// <inheritdoc />
        public override void Reset(ProgressiveLevel level, long resetValue)
        {
            level.CurrentValue = resetValue + level.Overflow + level.HiddenValue;

            var residual = level.Residual;
            level.Residual = 0;
            level.HiddenValue = 0;
            level.Overflow = 0;
            // OverflowTotal will not reset to zero

            ApplyContribution(level, new ProgressiveLevelUpdate(level.LevelId, 0, residual, false), null, null);
        }

        /// <inheritdoc />
        public override long Claim(ProgressiveLevel level, long resetValue)
        {
            if (level.TriggerControl == TriggerType.Mystery)
            {
                return MysteryClaim(level, resetValue);
            }

            var current = level.CurrentValue;

            Reset(level, resetValue);

            return current;
        }
    }
}