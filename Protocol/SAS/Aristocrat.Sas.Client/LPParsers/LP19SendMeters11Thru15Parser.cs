namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Meters 11-15 Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           19        Send Meters 10-15
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           19        Send Meters 10-15
    /// Total Coin in  4 BCD 00000000-99999999 four byte BCD coin in meter value
    /// Total Coin out 4 BCD 00000000-99999999 four byte BCD coin out meter value
    /// Total Drop     4 BCD 00000000-99999999 four byte BCD drop meter value
    /// Total Jackpot  4 BCD 00000000-99999999 four byte BCD jackpot meter value
    /// Total GP       4 BCD 00000000-99999999 four byte BCD Games Played meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP19SendMeters11Through15Parser : SasLongPollParser<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP19SendMeters11Through15Parser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP19SendMeters11Through15Parser(SasClientConfiguration configuration)
            : base(LongPoll.SendMeters11Thru15)
        {
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinIn, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinOut, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalDrop, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalJackpot, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.GamesPlayed, MeterType.Lifetime));
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll send meters 11-15");
            var result = Handle(Data).Meters;

            var response = new List<byte>();
            response.AddRange(command);

            // get coin in meter and convert to BCD
            var coinIn = result[SasMeters.TotalCoinIn].MeterValue;
            response.AddRange(Utilities.ToBcd(coinIn, SasConstants.Bcd8Digits));

            // get coin out meter and convert to BCD
            var coinOut = result[SasMeters.TotalCoinOut].MeterValue;
            response.AddRange(Utilities.ToBcd(coinOut, SasConstants.Bcd8Digits));

            // get drop meter and convert to BCD
            var drop = result[SasMeters.TotalDrop].MeterValue;
            response.AddRange(Utilities.ToBcd(drop, SasConstants.Bcd8Digits));

            // get jackpot meter and convert to BCD
            var jackpot = result[SasMeters.TotalJackpot].MeterValue;
            response.AddRange(Utilities.ToBcd(jackpot, SasConstants.Bcd8Digits));

            // get games played meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.GamesPlayed].MeterValue, SasConstants.Bcd8Digits));

            return response;
        }
    }
}
