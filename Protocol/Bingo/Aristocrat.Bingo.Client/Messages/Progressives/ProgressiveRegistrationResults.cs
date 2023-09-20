namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     Class for returning the results of a progressive registration call to the progressive host
    /// </summary>
    public class ProgressiveRegistrationResults : IResponse
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveRegistrationResults" /> class.
        /// </summary>
        /// <param name="code">The response code from the call to the server</param>
        public ProgressiveRegistrationResults(ResponseCode code)
            : this(code, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveRegistrationResults" /> class.
        /// </summary>
        /// <param name="code">The response code from the call to the server</param>
        /// <param name="configurationFailed">A value indicating if the registration failed due to mismatched configuration between game and progressive host</param>
        public ProgressiveRegistrationResults(ResponseCode code, bool configurationFailed)
        {
            ResponseCode = code;
            ConfigurationFailed = configurationFailed;
        }

        /// <summary>
        ///     Gets the response code.
        /// </summary>
        public ResponseCode ResponseCode { get; }

        /// <summary>
        ///     Gets a value indicating that the client registration request did not match the progressive host configuration.
        /// </summary>
        public bool ConfigurationFailed { get; }
    }
}