namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Current Hopper Status
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           4F        Send current hopper status command
    /// 
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           4F        Send current hopper status command
    /// Length           1         02, 06      Number of bytes following, not including CRC
    ///                                        02 = only status and % full
    ///                                        06 = status, % full and level
    /// Status           1         00-FF       Hopper status (see Table 7.19b for status codes)
    /// % Full           1         00-64, FF   Current hopper level as 0-100%, or FF if unable to
    ///                                        detect hopper level percentage
    /// Level            4 BCD     XXXXXXXX    Current hopper level in number of coins/tokens, only if
    ///                                        EGM able to detect (see length byte, above)
    /// CRC              2         0000-FFFF   16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP4FSendCurrentHopperStatusParser : SasLongPollParser<LongPollHopperStatusResponse, LongPollData>
    {
        private const byte ResponseLength = 0x06;

        /// <summary>
        ///     Initializes a new instance of the LP4FSendCurrentHopperStatusParser class.
        /// </summary>
        public LP4FSendCurrentHopperStatusParser()
            : base(LongPoll.SendCurrentHopperStatus)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = command.ToList();
            var response = Handler(Data);

            result.Add(ResponseLength);
            result.Add((byte)response.Status);
            result.Add(response.PercentFull);
            result.AddRange(Utilities.ToBcd((ulong)response.Level, SasConstants.Bcd8Digits));

            return result;
        }
    }
}