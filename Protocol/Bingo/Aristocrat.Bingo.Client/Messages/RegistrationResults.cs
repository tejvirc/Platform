namespace Aristocrat.Bingo.Client.Messages
{
    using System;

    public class RegistrationResults : IResponse
    {
        private const string UnknownVersion = "Unknown";

        public RegistrationResults(ResponseCode code)
            : this(code, string.Empty)
        {
        }

        public RegistrationResults(ResponseCode code, string serverVersion)
        {
            ResponseCode = code;
            ServerVersion = string.IsNullOrEmpty(serverVersion) ? UnknownVersion : serverVersion;
        }

        public ResponseCode ResponseCode { get; }

        public string ServerVersion { get; }
    }
}