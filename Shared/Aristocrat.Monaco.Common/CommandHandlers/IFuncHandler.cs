namespace Aristocrat.Monaco.Common.CommandHandlers
{
    /// <summary>
    ///     Function handler interface
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IFuncHandler<out TResult>
    {
        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>The result of execution.</returns>
        TResult Execute();
    }
}