namespace Aristocrat.Monaco.Common.CommandHandlers
{
    /// <summary>
    ///     Parameters action handler interface
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter.</typeparam>
    public interface IParametersActionHandler<in TParameter>
        where TParameter : class
    {
        /// <summary>
        ///     Executes the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        void Execute(TParameter parameter);
    }
}