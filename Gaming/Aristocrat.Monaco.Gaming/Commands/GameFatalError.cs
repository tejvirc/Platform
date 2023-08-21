namespace Aristocrat.Monaco.Gaming.Commands
{
    using Contracts;

    /// <summary>
    ///     Game fatal error Command
    /// </summary>
    public class GameFatalError
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameFatalError" /> class.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorMessage">Error message</param>
        public GameFatalError(GameErrorCode errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        ///     Gets or sets the error code.
        /// </summary>
        public GameErrorCode ErrorCode { get; }

        /// <summary>
        ///     Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
