namespace Aristocrat.Monaco.Common.CommandHandlers
{
    /// <summary>
    ///     Parameters function handler interface
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IParametersFuncHandler<in TParameter, out TResult>
    {
        /// <summary>
        ///     Executes the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The results of execution.</returns>
        TResult Execute(TParameter parameter);
    }
}