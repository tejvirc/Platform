namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Selected Game Number Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           55        Send Selected Game Number
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           55        Send Selected Game Number
    /// Game Number    2 BCD        XXXX       Game number
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP55SendSelectedGameNumberParser : SasLongPollParser<LongPollReadSingleValueResponse<int>, LongPollData>
    {
        /// <inheritdoc />
        public LP55SendSelectedGameNumberParser()
            : base(LongPoll.SendSelectedGameNumber)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = new List<byte>(command);
            var result = Handler(Data);

            response.AddRange(Utilities.ToBcd((ulong) result.Data, SasConstants.Bcd4Digits));
            return response;
        }
    }
}