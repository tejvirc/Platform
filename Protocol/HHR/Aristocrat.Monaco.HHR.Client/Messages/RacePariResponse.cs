namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using Data;

    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_RACE_PARI
    ///     Struct:   GMessageRacePariResponse
    ///     Request:  GMessageRacePariRequest
    /// </summary>
    public class RacePariResponse : Response
    {
        /// <summary>
        /// </summary>
        public RacePariResponse()
            : base(Command.CmdRacePari)
        {
        }

        /// <summary>
        /// </summary>
        public CTemplatePool[] TemplatePool;
    }
}