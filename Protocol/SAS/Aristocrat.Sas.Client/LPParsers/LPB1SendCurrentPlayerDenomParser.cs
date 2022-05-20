namespace Aristocrat.Sas.Client.LPParsers
{
    using LongPollDataClasses;
    using System.Collections.Generic;

    /// <summary>
    ///     This handles the Send Current Player Denomination Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B1        Send Current Player Denomination
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B1        Send Current Player Denomination
    /// Current player denom 1     01-3F       Binary number representing the player denomination currently selected.
    ///                                        See Table C-4 in Appendix C of the SAS Specification
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPB1SendCurrentPlayerDenomParser : SasLongPollParser<LongPollReadSingleValueResponse<byte>, LongPollData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LPB1SendCurrentPlayerDenomParser class
        /// </summary>
        public LPB1SendCurrentPlayerDenomParser() : base(LongPoll.SendCurrentPlayerDenominations)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = Handler(Data);

            return response != null ? new List<byte>(command) { response.Data } : null;
        }
    }
}
