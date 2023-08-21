namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Cumulative Progressive Win
    /// </summary>
    /// <remarks>
    ///     The command is as follows:
    ///     Field          Bytes       Value             Description
    ///     Address          1         01-7F       Gaming Machine Address
    ///     Command          1           83        Send cumulative progressive Wins
    ///     Game Number      2BCD    0000-9999     Game Number (0000= gaming Machine)
    ///     CRC              2       0000-FFFF     16-bit CRC
    ///     Response
    ///     Address          1         01-7F       Gaming Machine Address
    ///     Command          1           83        Send cumulative progressive Win
    ///     Game number    2 BCD                   Game number for currently enabled game
    ///     Cumulative Progressive Win    4 BCD    4-byte BCD meter in SAS accounting denom units
    ///     CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP83SendCumulativeProgressiveWinParser :
        SasLongPollParser<SendCumulativeProgressiveWinResponse, SendCumulativeProgressiveWinData>
    {
        private const int GameNumberIndex = 2;
        private const int GameNumberLength = 2;

        /// <summary>
        ///     Instantiates a new instance of the LP83SendCumulativeProgressiveWinParser class
        /// </summary>
        public LP83SendCumulativeProgressiveWinParser(SasClientConfiguration configuration)
            : base(LongPoll.SendCumulativeProgressiveWins)
        {
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var (gameNumber, validGameNumber) = Utilities.FromBcdWithValidation(
                command.ToArray(),
                GameNumberIndex,
                GameNumberLength);
            if (!validGameNumber)
            {
                Logger.Debug("Send Cumulative Win Amount: Game Number is not a valid BCD number");
                return NackLongPoll(command);
            }

            Data.GameId = (int)gameNumber;
            Logger.Debug($"LP83 Get Cumulative Win Amount for Game {Data.GameId}");

            var result = Handle(Data);

            var response = command.Take(GameNumberIndex + GameNumberLength).ToList();

            // get the meter and convert to BCD
            var meter = result.MeterValue;
            response.AddRange(Utilities.ToBcd(meter, SasConstants.Bcd8Digits));

            return response;
        }
    }
}