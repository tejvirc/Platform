namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_PARAMETER_REQUEST
    ///     Struct:   MessageParameterRequest
    ///     Response: SMessageGTParameter
    /// </summary>
    public class ParameterRequest : Request
    {
        /// <summary>
        /// </summary>
        public ParameterRequest()
            : base(Command.CmdParameterRequest)
        {
        }

        /// <summary>
        /// </summary>
        public string SerialNumber;
    }
}