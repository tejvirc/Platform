namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Number of Games Implemented command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           51        Send Number of Games Implemented
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           51        Send number of games implemented
    /// # of Games     2 BCD      0000-9999    Total number of games implemented
    /// CRC              2        0000-FFFF    16-bit CRC
    /// 
    /// Note: In response to long poll 51, gaming machines must send the total number of implemented games, not the number of games currently available to the player.
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP51SendNumberOfGamesImplementedParser : SasLongPollParser<LongPollReadSingleValueResponse<int>, LongPollData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP51SendNumberOfGamesImplementedParser class
        /// </summary>
        public LP51SendNumberOfGamesImplementedParser() : base(LongPoll.SendNumberOfGames)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = new List<byte>(command);
            var result = Handle(Data);

            response.AddRange(Utilities.ToBcd((ulong)result.Data, SasConstants.Bcd4Digits));

            return response;
        }
    }
}
