namespace Aristocrat.Bingo.Client.Messages
{
    public class RegistrationResults : IResponse
    {
        public RegistrationResults(ResponseCode code)
            : this(code, string.Empty)
        {
        }

        public RegistrationResults(ResponseCode code, string serverVersion)
        {
            ResponseCode = code;
            ServerVersion = serverVersion ?? string.Empty;
        }

        public ResponseCode ResponseCode { get; }

        public string ServerVersion { get; }
    }
}