namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_PROGRESSIVE_PRIZE
    ///     Struct:   SMessageProgressivePrize
    ///     Request:  None
    /// </summary>
    /// 
    public class GameProgressiveUpdate : Response
    {
        /// <summary>
        /// </summary>
        public GameProgressiveUpdate()
            : base(Command.CmdProgressivePrize)
        {
        }

        /// <summary>
        /// </summary>
        public uint Id;

        /// <summary>
        /// This amount is in cents
        /// </summary>
        public uint Amount;

        /// <summary>
        /// </summary>
        public uint Status;

        /// <summary>
        /// </summary>
        public uint CreditsBet;
    }
}
