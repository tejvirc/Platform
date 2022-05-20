namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_PARAMETER_GT
    ///     Struct:   SMessageGtParameter
    ///     Request:  MessageParameterRequest
    /// </summary>
    public class ParameterResponse : Response
    {
        /// <summary>
        /// </summary>
        public ParameterResponse()
            : base(Command.CmdParameterGt)
        {
        }

        /// <summary>
        ///     Parameter device id.
        /// </summary>
        public uint ParameterDeviceId;

        /// <summary>
        ///     Number of GameIds received from Server.
        /// </summary>
        public int GameIdCount;

        /// <summary>
        ///     Array of GameIds sent by Server against which we need to send GameOpen commands
        /// </summary>
        public int[] GameIds;

        /// <summary>
        /// </summary>
        public bool EzBetFlag;

        /// <summary>
        ///     Handicap Stats Timer
        /// </summary>
        public int HandicapStatTimer;

        /// <summary>
        ///     Handicap Pick Timer
        /// </summary>
        public int HandicapPickTimer;

        /// <summary>
        ///     UDP Ip
        /// </summary>
        public string UdpIp;

        /// <summary>
        ///     Last transaction Id
        /// </summary>
        public uint LastTransactionId;

        /// <summary>
        ///     Manual handicap mode - quick or auto
        /// </summary>
        public uint ManualHandicapMode;
    }
}