namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;
    using LongPollAck = LongPollDataClasses.LongPollReadSingleValueResponse<bool>;

    /// <summary>
    ///     This handles the Send Multiple Progressive BroadCast Value.
    /// </summary>
    /// <remarks>
    ///     The command is as follows:
    ///     Field          Bytes       Value             Description
    ///     Address          1         00-7F       Gaming Machine Address
    ///     Command          1           86        Multiple progressive broadcast
    ///     Length           1         07-C1       Length of the data to follow, not including the message CRC
    ///     Group            1         01-FF       Group Id for this broadcast
    ///     Level            1         01-20       Progressive level
    ///     Amount         5 BCD                   Level Amount in units of cents
    ///     variable                               Optional additional level/amount pairs
    ///     CRC            2 binary                16-bit CRC
    ///     Response
    ///     Ack or Nack
    /// </remarks>
    [Sas(SasGroup.Progressives)]
    public class LP86MultipleLevelProgressiveBroadcastParser :
        SasLongPollParser<LongPollAck, MultipleLevelProgressiveBroadcastData>
    {
        private const int MessageFieldLength = 1;
        private const int GroupIdOffset = 3;
        private const int GroupFieldLength = 1;
        private const int LengthOffset = 2;
        private const int LevelFieldLength = 1;
        private const int LevelOffset = 4;
        private const int LevelAmountOffset = 5;
        private const int MaxMsgLength = 193;
        private const int LevelDetailSize = 6;

        /// <summary>
        ///     Instantiates a new instance of the LP86MultipleLevelProgressiveBroadcastParser class
        /// </summary>
        public LP86MultipleLevelProgressiveBroadcastParser()
            : base(LongPoll.MultipleLevelProgressiveBroadcastValues)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            var length = (int)Utilities.FromBinary(longPoll, LengthOffset, MessageFieldLength);

            if (length < LevelDetailSize + 1)
            {
                Logger.Debug("Message Length is not valid it should be minimum 7");
                return NackLongPoll(command);
            }

            Data.ProgId = (int)Utilities.FromBinary(longPoll, GroupIdOffset, GroupFieldLength);

            if (Data.ProgId <= 0)
            {
                Logger.Debug("Group id is not valid");
                return NackLongPoll(command);
            }

            var parseResponse = ParseProgressiveLevel(longPoll, length);
            if (!parseResponse)
            {
                return NackLongPoll(command);
            }

            return Handle(Data).Data ? AckLongPoll(command) : NackLongPoll(command);
        }

        private bool ParseProgressiveLevel(byte[] longPoll, int length)
        {
            var numOfLevels = (length - 1) / LevelDetailSize;
            var nextLevelDetailOffset = 0;

            if (length > MaxMsgLength)
            {
                Logger.Debug("Invalid length for level details in LP86");
                return false;
            }

            var levels = new Dictionary<int, long>();
            for (var i = 0; i < numOfLevels; i++)
            {
                var levelId = (int)Utilities.FromBinary(longPoll,
                        (uint)(LevelOffset + nextLevelDetailOffset),
                        LevelFieldLength);

                var (levelAmount, validLevelAmount) = Utilities.FromBcdWithValidation(longPoll,
                    (uint)(LevelAmountOffset + nextLevelDetailOffset),
                    SasConstants.Bcd10Digits);

                if (!validLevelAmount)
                {
                    Logger.Debug("Level Amount is not a valid BCD number");
                    return false;
                }

                nextLevelDetailOffset = nextLevelDetailOffset + LevelDetailSize;
                levels[levelId] = (long)levelAmount;
            }

            Data.LevelInfo = levels;
            return true;
        }
    }
}