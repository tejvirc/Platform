namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Game N Meters Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           52        Send Game N Meters
    /// Game Number    2 BCD      0000-9999    Game Number
    /// CRC              2        0000-FFFF    16-bit CRC
    /// 
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           52        Send Game N Meters
    /// Game Number    2 BCD      0000-9999    Game Number
    /// Total Coin in  4 BCD 00000000-99999999 four byte BCD coin in meter value for game N
    /// Total Coin out 4 BCD 00000000-99999999 four byte BCD coin out meter value for game N
    /// Total Jackpot  4 BCD 00000000-99999999 four byte BCD jackpot meter value for game N
    /// Total GP       4 BCD 00000000-99999999 four byte BCD Games Played meter value for game N
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP52SendGameNMetersParser : SasLongPollParser<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersGameNData>
    {
        private const int GameNumberIndex = 2;
        private const int GameNumberLength = 2;

        /// <summary>
        ///     Instantiates a new instance of the LP52SendGameNMetersParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP52SendGameNMetersParser(SasClientConfiguration configuration) : base(LongPoll.SendGameNMeters)
        {
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinIn, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinOut, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalJackpot, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.GamesPlayed, MeterType.Lifetime));
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll(52) Send Game N Meter");
            var longPoll = command.ToArray();

            var (id, valid) = Utilities.FromBcdWithValidation(longPoll, GameNumberIndex, GameNumberLength);

            if (!valid)
            {
                Logger.Debug("Game Id not valid BCD. NACKing Send Game N Meters long poll.");
                return NackLongPoll(command);
            }

            Data.GameNumber = (int)id;

            var result = Handle(Data).Meters;

            var response = new List<byte>(command.Take(4).ToList());

            // get game N coin in meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.TotalCoinIn].MeterValue, SasConstants.Bcd8Digits));

            // get game N coin out meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.TotalCoinOut].MeterValue, SasConstants.Bcd8Digits));

            // get game N jackpot meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.TotalJackpot].MeterValue, SasConstants.Bcd8Digits));

            // get game N played meter and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.GamesPlayed].MeterValue, SasConstants.Bcd8Digits));

            return response;
        }
    }
}
