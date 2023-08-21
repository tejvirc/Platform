namespace Aristocrat.Monaco.Bingo.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     The command handling factory for all the platform generated requests
    /// </summary>
    public interface ICommandHandlerFactory
    {
        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <typeparam name="TCommand">The command type to execute</typeparam>
        /// <param name="command">The command to execute</param>
        /// <param name="token">The cancellation token for the task</param>
        /// <returns>The task for handling the command</returns>
        Task Execute<TCommand>(TCommand command, CancellationToken token = default)
            where TCommand : class;
    }
}