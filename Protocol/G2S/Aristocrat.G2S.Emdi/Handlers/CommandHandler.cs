namespace Aristocrat.G2S.Emdi.Handlers
{
    using System.Threading.Tasks;
    using Emdi.Host;
    using Protocol.v21ext1b1;

    /// <summary>
    /// EMDI message handler class
    /// </summary>
    public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : c_baseCommand
    {
        /// <inheritdoc />
        public RequestContext Context { get; set; }

        /// <inheritdoc />
        public abstract Task<CommandResult> ExecuteAsync(TCommand command);

        /// <summary>
        /// Creates a <see cref="CommandResult"/> instance that wraps the response to be sent to media display content and marks it with success error code.
        /// </summary>
        /// <param name="command">The command response to send to media display content</param>
        /// <returns>Response to send to media display content</returns>
        protected CommandResult Success(c_baseCommand command)
        {
            var result = new CommandResult(EmdiErrorCode.NoError);
            result.SetCommand(command);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="CommandResult"/> instance that wraps the response to be sent to media display content and marks it with the provided error code.
        /// </summary>
        /// <returns>Response to send to media display content</returns>
        protected CommandResult Error(EmdiErrorCode code)
        {
            return new CommandResult(code);
        }

        /// <summary>
        /// Creates an empty <see cref="CommandResult"/> instance indicating that no response is to be sent to media display content
        /// </summary>
        /// <returns>An empty command result</returns>
        protected CommandResult NoResponse()
        {
            return new CommandResult(EmdiErrorCode.NoError);
        }

        /// <summary>
        /// Creates a <see cref="CommandResult"/> instance that wraps the response to be sent to media display content and marks it with <see cref="EmdiErrorCode.ContentToContentNotPermitted"/> error code.
        /// </summary>
        /// <returns>Response to send to media display content</returns>
        protected CommandResult ContentToContentNotPermitted()
        {
            return new CommandResult(EmdiErrorCode.ContentToContentNotPermitted);
        }

        /// <summary>
        /// Creates a <see cref="CommandResult"/> instance that wraps the response to be sent to media display content and marks it with <see cref="EmdiErrorCode.InvalidXml"/> error code.
        /// </summary>
        /// <returns>Response to send to media display content</returns>
        protected CommandResult InvalidXml()
        {
            return new CommandResult(EmdiErrorCode.InvalidXml);
        }

        /// <summary>
        /// Creates a <see cref="CommandResult"/> instance that wraps the response to be sent to media display content and marks it with <see cref="EmdiErrorCode.InterfaceNotOpen"/> error code.
        /// </summary>
        /// <returns>Response to send to media display content</returns>
        protected CommandResult InterfaceNotOpen()
        {
            return new CommandResult(EmdiErrorCode.InterfaceNotOpen);
        }

        /// <summary>
        /// Creates a <see cref="CommandResult"/> instance that wraps the response to be sent to media display content and marks it with <see cref="EmdiErrorCode.InvalidEventCode"/> error code.
        /// </summary>
        /// <returns>Response to send to media display content</returns>
        protected CommandResult InvalidEventCode()
        {
            return new CommandResult(EmdiErrorCode.InvalidEventCode);
        }

        /// <summary>
        /// Creates a <see cref="CommandResult"/> instance that wraps the response to be sent to media display content and marks it with <see cref="EmdiErrorCode.InvalidMeterName"/> error code.
        /// </summary>
        /// <returns>Response to send to media display content</returns>
        protected CommandResult InvalidMeterName()
        {
            return new CommandResult(EmdiErrorCode.InvalidMeterName);
        }

        /// <summary>
        /// Creates a <see cref="CommandResult"/> instance that wraps the response to be sent to media display content and marks it with <see cref="EmdiErrorCode.NotAllowed"/> error code.
        /// </summary>
        /// <returns>Response to send to media display content</returns>
        protected CommandResult NotAllowed()
        {
            return new CommandResult(EmdiErrorCode.NotAllowed);
        }
    }
}
