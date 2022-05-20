namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;
    using LongPollAck = LongPollDataClasses.LongPollReadSingleValueResponse<bool>;

    /// <summary>
    ///     This handles the extended Progressive BroadCast Value.
    /// </summary>
    /// <remarks>
    ///     The command is as follows:
    ///     Field          Bytes       Value             Description
    ///     Address          1         00-7F       Gaming Machine Address
    ///     Command          1           86        Multiple progressive broadcast
    ///     Group            1         01-FF       Group Id for this broadcast
    ///     Level            1         01-20       Progressive level
    ///     Amount         5 BCD                   Level Amount in units of cents
    ///     Base           5 BCD                   Base/Reset Amount in units of cents
    ///     Contribution         5 BCD             contribution rate specified expressed as whole number with 4 decimal places
    ///     variable                               Optional additional level/amount/contribution data pairs
    ///     CRC            2 binary                16-bit CRC
    ///     Response
    ///     Ack or Nack
    /// </remarks>
    [Sas(SasGroup.Progressives)]
    public class LP7AExtendedProgressiveBroadcastParser :
        SasLongPollParser<LongPollAck, ExtendedProgressiveBroadcastData>
    {
        private const int CommandHeaderLength = 3;
        private const int LengthOffset = 2;
        private const int GroupIdOffset = 3;
        private const int LevelOffset = 4;
        private const int MaxMsgLength = 0xE1;
        private const int LevelAmountOffset = 5;
        private const int BaseAmountOffset = 10;
        private const int ContributionRateOffset = 15;
        private const int ProgressiveLevelDataSize = 14;
        private const int MinimumLengthSupported = 0x0F;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LP7AExtendedProgressiveBroadcastParser"/> class.
        /// </summary>
        public LP7AExtendedProgressiveBroadcastParser()
            : base(LongPoll.ExtendedProgressiveBroadcast)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            if (command.Count <= LengthOffset)
            {
                return NackLongPoll(command);
            }

            var longPoll = command.ToArray();
            var length = (int)Utilities.FromBinary(longPoll, LengthOffset, SasConstants.FieldLength);

            if (length < MinimumLengthSupported || length > MaxMsgLength ||
                length < command.Count - (SasConstants.NumberOfCrcBytes + CommandHeaderLength))
            {
                return NackLongPoll(command);
            }

            Data.ProgId = (int)Utilities.FromBinary(longPoll, GroupIdOffset, SasConstants.FieldLength);

            var parseResponse = ParseProgressiveLevel(longPoll, --length);
            if (parseResponse)
            {
                Handle(Data);
            }
            else
            {
                return NackLongPoll(command);
            }

            // If we don't process the request just ignore the poll
            return Handle(Data).Data ? AckLongPoll(command) : null;
        }

        private bool ParseProgressiveLevel(byte[] longPoll, int length)
        {
            var numOfLevels = length / ProgressiveLevelDataSize;
            var nextLevelOffset = 0;
            if (numOfLevels <= 0 || length % ProgressiveLevelDataSize != 0)
            {
                return false;
            }

            var progressiveLevelData = new Dictionary<int, ExtendedLevelData>();
            for (var i = 0; i < numOfLevels; i++)
            {
                var levelData = new ExtendedLevelData();
                var levelId =
                    (int)Utilities.FromBinary(
                        longPoll,
                        (uint)(LevelOffset + nextLevelOffset),
                        SasConstants.Bcd2Digits);

                var (levelAmount, validLevelAmount) = Utilities.FromBcdWithValidation(
                    longPoll,
                    (uint)(LevelAmountOffset + nextLevelOffset),
                    SasConstants.Bcd10Digits);
                if (!validLevelAmount)
                {
                    Logger.Debug("Level Amount is not a valid BCD number");
                    return false;
                }

                levelData.Amount = (long)levelAmount;
                var (baseAmount, validBaseAmount) = Utilities.FromBcdWithValidation(
                    longPoll,
                    (uint)(BaseAmountOffset + nextLevelOffset),
                    SasConstants.Bcd10Digits);
                if (!validBaseAmount)
                {
                    Logger.Debug("Base Amount is not a valid BCD number");
                    return false;
                }

                levelData.ResetValue = (long)baseAmount;
                var (contributionRate, validContributionRate) = Utilities.FromBcdWithValidation(
                    longPoll,
                    (uint)(ContributionRateOffset + nextLevelOffset),
                    SasConstants.Bcd6Digits);

                if (!validContributionRate)
                {
                    Logger.Debug("Contribution Rate is not a valid BCD number");
                    return false;
                }

                levelData.ContributionRate = (int)contributionRate;
                nextLevelOffset += ProgressiveLevelDataSize;
                progressiveLevelData.Add(levelId, levelData);
            }

            Data.LevelInfo = progressiveLevelData;

            return true;
        }
    }
}