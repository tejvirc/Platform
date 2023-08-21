namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Meters Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1C        Send Meters
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1C        Send Meters
    /// Total Coin in  4 BCD 00000000-99999999 four byte BCD coin in meter value
    /// Total Coin out 4 BCD 00000000-99999999 four byte BCD coin out meter value
    /// Total Drop     4 BCD 00000000-99999999 four byte BCD drop meter value
    /// Total Jackpot  4 BCD 00000000-99999999 four byte BCD jackpot meter value
    /// Total GP       4 BCD 00000000-99999999 four byte BCD Games Played meter value
    /// Total GW       4 BCD 00000000-99999999 four byte BCD Games Won meter value
    /// Total SDO      4 BCD 00000000-99999999 four byte BCD Slot door opened meter value
    /// Total Reset    4 BCD 00000000-99999999 four byte BCD Power Reset meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP1CSendMetersParser : SasLongPollParser<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP1CSendMetersParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP1CSendMetersParser(SasClientConfiguration configuration)
            : base(LongPoll.SendMeters)
        {
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinIn, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinOut, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalDrop, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalJackpot, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.GamesPlayed, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.GamesWon, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.MainDoorOpened, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TopMainDoorOpened, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.PowerReset, MeterType.Lifetime));
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll Send Meters");
            var result = Handle(Data).Meters;

            var response = new List<byte>();
            response.AddRange(command);

            // get coin in meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.TotalCoinIn].MeterValue, SasConstants.Bcd8Digits));

            // get coin out meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.TotalCoinOut].MeterValue, SasConstants.Bcd8Digits));

            // get drop meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.TotalDrop].MeterValue, SasConstants.Bcd8Digits));

            // get jackpot meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.TotalJackpot].MeterValue, SasConstants.Bcd8Digits));

            // get games played meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.GamesPlayed].MeterValue, SasConstants.Bcd8Digits));

            // get games won meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.GamesWon].MeterValue, SasConstants.Bcd8Digits));

            // get slot door opened meter and convert to BCD
            var doorOpened = result[SasMeters.MainDoorOpened].MeterValue + result[SasMeters.TopMainDoorOpened].MeterValue;
            response.AddRange(Utilities.ToBcd(doorOpened, SasConstants.Bcd8Digits));

            // get power reset meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.PowerReset].MeterValue, SasConstants.Bcd8Digits));

            return response;
        }
    }
}
