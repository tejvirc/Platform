namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  N/A
    ///     Struct:   N/A
    ///     Request:  MessageConnect
    /// </summary>
    public class EmptyResponse : Response
    {
        // Used when the request does not expect any response.

        /// <summary>
        /// </summary>
        public EmptyResponse()
            : base(Command.CmdInvalidCommand)
        {
        }
    }
}