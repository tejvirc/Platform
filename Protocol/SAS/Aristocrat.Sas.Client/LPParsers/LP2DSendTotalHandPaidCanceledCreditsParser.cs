namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Startup Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2D        Send Total Hand Paid Canceled Credits
    /// Game Id          2        0000-FFFF    Game Id
    /// CRC              2        0000-FFFF    16-bit CRC
    ///
    /// The command response is as follows:
    /// Field          Bytes       Value            Description
    /// Address          1         01-7F            Gaming Machine Address
    /// Command          1           2D             Send Total Hand Paid Canceled Credits
    /// Game Id          2        0000-FFFF         Game Id
    /// Meter Value      4        00000000-FFFFFFFF Total Hand Paid Canceled Credits value
    /// CRC              2        0000-FFFF         16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP2DSendTotalHandPaidCanceledCreditsParser :
        SasLongPollParser<SendTotalHandPaidCanceledCreditsDataResponse, SendTotalHandPaidCanceledCreditsData>
    {
        private const int GameNumberLength = 2;
        private const int GameNumberIndex = 2;

        /// <inheritdoc />
        public LP2DSendTotalHandPaidCanceledCreditsParser(SasClientConfiguration configuration)
            : base(LongPoll.SendTotalHandPaidCanceledCredits)
        {
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var (gameNumber, validGameNumber) = Utilities.FromBcdWithValidation(command.ToArray(), GameNumberIndex, GameNumberLength);
            if (!validGameNumber)
            {
                Logger.Debug("Send Total Hand Paid Canceled Credits: Game Number is not a valid BCD number");
                return NackLongPoll(command);
            }

            Data.GameId = (int)gameNumber;
            Logger.Debug($"LP2D Get Total Hand Paid Canceled Credits for Game {Data.GameId}");

            var result = Handle(Data);

            if (!result.Succeeded)
            {
                // Just ignore this as we only support all game meters currently
                // NOTE from SAS Specification Section 7.6.3:
                // Send Total Hand Paid Cancelled Credits is defined as a multi-game poll. However, a
                // gaming machine is not required to track cancelled credits for specific game numbers.
                // If a gaming machine only tracks cancelled credits at the gaming machine level, it
                // must ignore long poll 2D with a game number other than 0000.
                return null;
            }
            
            // Take everything up to the game number (The game number index plus its length)
            var response = command.Take(GameNumberIndex + GameNumberLength).ToList();

            // get the meter and convert to BCD
            var meter = result.MeterValue;
            response.AddRange(Utilities.ToBcd(meter, SasConstants.Bcd8Digits));

            return response;
        }
    }
}
