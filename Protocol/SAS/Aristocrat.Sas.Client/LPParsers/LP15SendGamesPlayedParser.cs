﻿namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Games Played Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           15        Send Games Played Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           15        Send Games Played Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD games played meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP15SendGamesPlayedParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP15SendGamesPlayedParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP15SendGamesPlayedParser(SasClientConfiguration configuration)
            : base(LongPoll.SendGamesPlayedMeter, SasMeters.GamesPlayed, MeterType.Lifetime, configuration, true)
        {
        }
    }
}
