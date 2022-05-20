namespace Aristocrat.G2S.Emdi.Host
{
    using Protocol.v21ext1b1;

    /// <summary>
    /// The response to be sent to the media display content
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandResult"/> class.
        /// </summary>
        public CommandResult(EmdiErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets the command
        /// </summary>
        public c_baseCommand Command { get; private set; }

        /// <summary>
        /// Gets the status code
        /// </summary>
        public EmdiErrorCode ErrorCode { get; }

        /// <summary>
        /// Set the command to be sent to the media display content
        /// </summary>
        /// <param name="response"></param>
        /// <typeparam name="TResponse"></typeparam>
        public void SetCommand<TResponse>(TResponse response)
            where TResponse : c_baseCommand
        {
            Command = response;
        }
    }
}