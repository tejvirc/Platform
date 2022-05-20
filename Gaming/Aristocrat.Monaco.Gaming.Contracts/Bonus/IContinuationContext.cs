namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     Defines a contract for paying linked bonuses
    /// </summary>
    public interface IContinuationContext
    {
    }

    /// <summary>
    ///     Defines a contract for paying linked bonuses
    /// </summary>
    public interface IContinuationContext<out T> : IContinuationContext
    {
        /// <summary>
        ///     Gets the context associated with this continuation
        /// </summary>
        T Context { get; }
    }
}