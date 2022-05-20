namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     Factory for progressive calculators
    /// </summary>
    public interface IProgressiveCalculatorFactory
    {
        /// <summary>
        ///     Creates the progressive calculator strategy for the specified type
        /// </summary>
        /// <param name="type">The progressive funding type</param>
        /// <returns>The progressive calculator strategy</returns>
        ICalculatorStrategy Create(SapFundingType type);
    }
}
