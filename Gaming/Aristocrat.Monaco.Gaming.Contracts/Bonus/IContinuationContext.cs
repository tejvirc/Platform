namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     Defines a contract for paying linked bonuses
    /// </summary>
    public interface IContinuationContext
    {
        /// <summary>
        ///     Determines whether or not we should continue to use this instance after a single commit
        /// </summary>
        bool ShouldPersistPostCommit { get; }
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