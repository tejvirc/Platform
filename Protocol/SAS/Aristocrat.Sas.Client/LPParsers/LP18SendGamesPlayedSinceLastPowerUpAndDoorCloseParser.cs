namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Games Since Power Up And Last Door Close Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           18        Send Games Since Power Up And Last Door Close
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           18        Send Games Since Power Up And Last Door Close
    /// Power up      2 BCD       0000-9999    two byte BCD games played since last power up
    /// door close    2 BCD       0000-9999    two byte BCD games played since last door close
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP18SendGamesPlayedSinceLastPowerUpAndDoorCloseParser : SasLongPollParser<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP18SendGamesPlayedSinceLastPowerUpAndDoorCloseParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP18SendGamesPlayedSinceLastPowerUpAndDoorCloseParser(SasClientConfiguration configuration)
            : base(LongPoll.SendGamesSincePowerUpLastDoorMeter)
        {
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.GamesSinceLastPowerUp, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.GamesSinceLastDoorClose, MeterType.Lifetime));
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll send Games Played Since Last Power Up And Door Close");
            var result = Handle(Data).Meters;

            var response = new List<byte>();
            response.AddRange(command);

            // get games played since last power up meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.GamesSinceLastPowerUp].MeterValue, SasConstants.Bcd4Digits));

            // get games played since last door close meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.GamesSinceLastDoorClose].MeterValue, SasConstants.Bcd4Digits));

            return response;
        }
    }
}
