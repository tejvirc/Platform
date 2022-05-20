namespace Aristocrat.Bingo.Client.Messages.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using ServerApiGateway;
    using IMessage = Google.Protobuf.IMessage;

    /// <summary>
    ///     A process for command request from the server
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>
        ///     Process the command from the server
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>The task that returns the response message to use</returns>
        public Task<IMessage> ProcessCommand(Command command, CancellationToken token);
    }
}