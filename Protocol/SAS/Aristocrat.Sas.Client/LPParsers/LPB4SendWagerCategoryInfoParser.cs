namespace Aristocrat.Sas.Client.LPParsers
{
    using LongPollDataClasses;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     This handles the Send Wager Category Information Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B4        Send Wager Category Information
    /// Game Number    2 BCD        XXXX       Game number (0000 = gaming machine)
    /// Wager Category 2 BCD        XXXX       Wager category (0000 - total coin in for game N)
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B4        Send Wager Category Information
    /// Length           1         09-nn       Number of bytes following, not including CRC
    /// Game Number    2 BCD        XXXX       Game number
    /// Wager Category 2 BCD        XXXX       Wager category
    /// Payback %      4 ASCII      ??.??      Theoretical payback percentage for the wager category for game n.
    ///                                        The decimal is implied and not transmitted.
    /// Coin in size     1         00-09       Coin In Meter size in number of bytes
    /// Coin In        x BCD                   Total coin in meter value (0-9 bytes)
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPB4SendWagerCategoryInfoParser : SasLongPollParser<LongPollSendWagerResponse, LongPollReadWagerData>
    {
        private const int CommandAndGameIdSize = 2; // the size of the command list to include in the response
        private const int GameIdPos = 2; // the pos in the command list where the game id starts
        private const int GameIdByteSize = 2; // game id size in bytes
        private const int WagerCatPos = 4; // pos in command list where wager category starts
        private const int WagerCatByteSize = 2; // size of wager category in bytes
        private const byte NumBytesWagerSection = 9; // default size of output data minus the size of coin-in data
        private const byte MaxCoinInLength = 9; // from documentation

        /// <summary>
        ///     Instantiates a new instance of the LPB4SendWagerCategoryInfoParser class
        /// </summary>
        public LPB4SendWagerCategoryInfoParser(SasClientConfiguration configuration) : base(LongPoll.SendWagerCategoryInformation)
        {
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var numBytesCoinIn = 0;
            var percentString = "0000";
            var result = command.Take(CommandAndGameIdSize).ToList();

            // get the game ID from the command list
            var (gameId, validGame) = Utilities.FromBcdWithValidation(command.ToArray(), GameIdPos, GameIdByteSize);
            if (!validGame)
            {
                Logger.Debug("Game Id not valid BCD. NACKing send game N configuration long poll");
                return NackLongPoll(command);
            }
            // get the wager category from the command list
            var (wagerCat, validCat) = Utilities.FromBcdWithValidation(command.ToArray(), WagerCatPos, WagerCatByteSize);
            if (!validCat)
            {
                Logger.Debug("Wager Category not valid BCD. NACKing send game N configuration long poll");
                return NackLongPoll(command);
            }

            // package data and call handler.
            Data.GameId = (int)gameId;
            Data.WagerCategory = (int) wagerCat;

            var response = Handler(Data);

            // for value not found condition. percentString will be all 0000 and the coinIn size will be 0
            // if percent is 0 this indicates that a wager category was not found so set size of coins to 0
            // since coin info will not be sent.
            if (response.IsValid)
            {
                percentString = $"{response.PaybackPercentage:####}";
                // make sure length does not exceed sas spec
                numBytesCoinIn = (byte)(response.CoinInMeterLength < MaxCoinInLength ? response.CoinInMeterLength : MaxCoinInLength);
            }

            result.Add((byte)(NumBytesWagerSection + numBytesCoinIn));
            result.AddRange(Utilities.ToBcd(gameId, SasConstants.Bcd4Digits));
            result.AddRange(Utilities.ToBcd(wagerCat, SasConstants.Bcd4Digits));

            var percentBytes = Encoding.ASCII.GetBytes(percentString);
            result.AddRange(percentBytes);
            result.Add((byte)numBytesCoinIn);
            // if the percent was not found then don't add the data to the response. 
            if (response.IsValid)
                result.AddRange(Utilities.ToBcd((ulong)response.CoinInMeter, numBytesCoinIn));

            return result;
        }
    }
}
