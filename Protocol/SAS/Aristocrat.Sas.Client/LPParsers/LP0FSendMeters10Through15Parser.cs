namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    /// This handles the Send Meters 10-15 Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           0F        Send Meters 10-15
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           0F        Send Meters 10-15
    /// Total CC       4 BCD 00000000-99999999 four byte BCD canceled credits meter value
    /// Total Coin in  4 BCD 00000000-99999999 four byte BCD coin in meter value
    /// Total Coin out 4 BCD 00000000-99999999 four byte BCD coin out meter value
    /// Total Drop     4 BCD 00000000-99999999 four byte BCD drop meter value
    /// Total Jackpot  4 BCD 00000000-99999999 four byte BCD jackpot meter value
    /// Total GP       4 BCD 00000000-99999999 four byte BCD Games Played meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP0FSendMeters10Through15Parser : SasLongPollParser<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP0FSendMeters10Through15Parser class
        /// </summary>
        /// <param name="clientConfiguration">The configuration for the client</param>
        public LP0FSendMeters10Through15Parser(SasClientConfiguration clientConfiguration)
            : base(LongPoll.SendMeters10Thru15)
        {
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCanceledCredits, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinIn, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinOut, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalDrop, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalJackpot, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.GamesPlayed, MeterType.Lifetime));
            Data.AccountingDenom = clientConfiguration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll send meters 10-15");
            var result = Handle(Data).Meters;

            var response = new List<byte>();
            response.AddRange(command);

            // get canceled credits meter and convert to BCD
            var canceledCredits = result[SasMeters.TotalCanceledCredits].MeterValue;
            response.AddRange(Utilities.ToBcd(canceledCredits, SasConstants.Bcd8Digits));

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
