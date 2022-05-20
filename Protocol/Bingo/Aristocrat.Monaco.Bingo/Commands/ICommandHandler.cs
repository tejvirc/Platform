namespace Aristocrat.Monaco.Bingo.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     The command handler for the <see cref="TCommand"/> command
    /// </summary>
    /// <typeparam name="TCommand">The command type to be handled</typeparam>
    public interface ICommandHandler<in TCommand>
    {
        /// <summary>
        ///     Handles the requested command
        /// </summary>
        /// <param name="command">The command details to handle</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The task for handling the command</returns>
        Task Handle(TCommand command, CancellationToken token = default);
    }
}