namespace Aristocrat.Monaco.Hhr.Commands
{
    using System.Threading.Tasks;

    /// <summary>
    /// </summary>
    public interface ICommandHandlerFactory
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        Task Execute<TCommand>(TCommand command)
            where TCommand : class;
    }
}