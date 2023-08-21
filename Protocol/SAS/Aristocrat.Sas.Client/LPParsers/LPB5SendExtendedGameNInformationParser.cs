namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Extended Game N Information Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B5        Send Extended Game N Information
    /// Game Number    2 BCD        XXXX       Game number (0000 = gaming machine)
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B5        Send Extended Game N Information
    /// Game Number    2 BCD        XXXX       Game number
    /// Max bet        2 BCD                   Max bet for Game N, in units of game credits
    /// Progressive Group 1                    SAS progressive group for game N
    /// Progressive Lvls  4                    SAS progressive levels enabled for game N.
    ///                                        (lsb= level 1, msb=level 32, one bit for each SAS progressive level enabled)
    /// Game Name Length 1          00-14      Length of game N name text
    /// Game Name      n ASCII                 Game name or family
    /// Paytable Name Length 1      00-14      Length of game N paytable name text
    /// Paytable Name  n ASCII                 Optional ASCII name of the paytable of collection of paytables
    /// Wager categories 2BCD                  Number of wager categories supported
    /// Num Player Denoms 1                    Number of player denominations that game N can be configured for.
    /// Player Denom 1  1           00-FF      First player denom
    ///            variable                    Additional player denoms
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPB5SendExtendedGameNInformationParser : SasLongPollMultiDenomAwareParser<LongPollExtendedGameNInformationResponse, LongPollExtendedGameNInformationData>
    {
        private const int AddressAndCommandLength = 2;
        private const int GameNumberIndex = 2;
        private const int GameNumberLength = 2;
        private const int MaxBetLength = 2;
        private const int ProgressiveLevelsLength = 4;
        private const int WagerCategoriesLength = 2;
        private const int MaxGameNameLength = 20;
        private const int MaxPaytableNameLength = 20;
        private const int MaxDataLength = 0xFF;

        /// <summary>
        ///     Instantiates a new instance of the LPB5SendExtendedGameNInformationParser class
        /// </summary>
        public LPB5SendExtendedGameNInformationParser() : base(LongPoll.SendExtendedGameNInformation)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command) => Parse(command, 0, false);

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom) => Parse(command, denom, true);

        /// <summary>
        ///     Handles the parsing of the long poll, being aware of multi-denom-awareness.
        /// </summary>
        /// <param name="command">Byte collection representing the long poll received</param>
        /// <param name="denom">Desired denomination represented in cents</param>
        /// <param name="multiDenomPoll">Whether or not to treat this as a multi-denom poll</param>
        /// <returns>Long poll response, or null if there is no response</returns>
        private IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom, bool multiDenomPoll)
        {
            var longPoll = command.ToArray();
            var (id, valid) = Utilities.FromBcdWithValidation(longPoll, GameNumberIndex, GameNumberLength);

            if (!valid)
            {
                Logger.Debug("Game Id not valid BCD. NACKing send game N configuration long poll");
                return multiDenomPoll
                    ? GenerateMultiDenomAwareError(command.First(), MultiDenomAwareErrorCode.ImproperlyFormatted)
                    : NackLongPoll(command);
            }

            Data.GameNumber = (uint)id;
            Data.TargetDenomination = denom;
            Data.MultiDenomPoll = multiDenomPoll;
            var handlerResponse = Handle(Data);
            if (handlerResponse is null)
            {
                return multiDenomPoll
                    ? GenerateMultiDenomAwareError(command.First(), MultiDenomAwareErrorCode.SpecificDenomNotSupported)
                    : NackLongPoll(command);
            }

            if (multiDenomPoll && handlerResponse.ErrorCode != MultiDenomAwareErrorCode.NoError)
            {
                return GenerateMultiDenomAwareError(command.First(), handlerResponse.ErrorCode);
            }

            var gameNameLength = System.Math.Min(handlerResponse.GameName?.Length ?? 0, MaxGameNameLength);
            var payTableNameLength = System.Math.Min(handlerResponse.PaytableName?.Length ?? 0, MaxPaytableNameLength);

            var responseData = new List<byte>();
            responseData.AddRange(Utilities.ToBcd(Data.GameNumber, GameNumberLength));
            responseData.AddRange(Utilities.ToBcd((ulong)handlerResponse.MaxBet, MaxBetLength));
            responseData.Add(handlerResponse.ProgressiveGroup);
            responseData.AddRange(Utilities.ToBinary(handlerResponse.ProgressiveLevels, ProgressiveLevelsLength));
            responseData.Add((byte)gameNameLength);
            if (gameNameLength > 0 && handlerResponse.GameName != null)
            {
                responseData.AddRange(Encoding.ASCII.GetBytes(handlerResponse.GameName.Substring(0, gameNameLength)));
            }

            responseData.Add((byte)payTableNameLength);
            if (payTableNameLength > 0 && handlerResponse.PaytableName != null)
            {
                responseData.AddRange(Encoding.ASCII.GetBytes(handlerResponse.PaytableName.Substring(0, payTableNameLength)));
            }

            responseData.AddRange(Utilities.ToBcd((ulong)handlerResponse.NumberOfWagerCategories, WagerCategoriesLength));

            var numberOfDenominations = System.Math.Min(handlerResponse.Denominations.Count, MaxNumberOfDenominations(responseData.Count + 1));
            responseData.Add((byte)numberOfDenominations);
            responseData.AddRange(handlerResponse.Denominations.Take(numberOfDenominations));

            // take address and command bytes
            var response = command.Take(AddressAndCommandLength).ToList();
            response.Add((byte)responseData.Count);
            response.AddRange(responseData);

            return response;
        }

        private static int MaxNumberOfDenominations(int dataLength) => (dataLength >= MaxDataLength ? 0 : MaxDataLength - dataLength);
    }
}
