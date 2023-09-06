namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    public class ProgressiveRegistrationResults : IResponse
    {
        public ProgressiveRegistrationResults(ResponseCode code)
            : this(code, false)
        {
        }

        public ProgressiveRegistrationResults(ResponseCode code, bool configurationFailed)
        {
            ResponseCode = code;
            ConfigurationFailed = configurationFailed;
        }

        public ResponseCode ResponseCode { get; }

        /// <summary>
        ///     Gets a value indicating that the client registration request did not match the progressive host configuration.
        /// </summary>
        public bool ConfigurationFailed { get; }
    }
}