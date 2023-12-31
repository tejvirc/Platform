﻿namespace Aristocrat.Bingo.Client.Messages.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using ServerApiGateway;
    using IMessage = Google.Protobuf.IMessage;

    /// <summary>
    ///     A factory for getting and processing commands
    /// </summary>
    public interface ICommandProcessorFactory
    {
        /// <summary>
        ///     Process the command from the server
        /// </summary>
        /// <param name="command">The command to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>The task that returns the response message to use or null if the command process is not found</returns>
        public Task<IMessage> ProcessCommand(Command command, CancellationToken token);
    }
}