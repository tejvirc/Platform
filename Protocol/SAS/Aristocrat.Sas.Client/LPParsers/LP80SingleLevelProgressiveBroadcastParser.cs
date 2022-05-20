namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;
    using LongPollAck = LongPollDataClasses.LongPollReadSingleValueResponse<bool>;

    /// <summary>
    ///     This handles the Send single Progressive BroadCast Value.
    /// </summary>
    /// <remarks>
    ///     The command is as follows:
    ///     Field          Bytes       Value             Description
    ///     Address          1         00-7F       Gaming Machine Address
    ///     Command          1           80        progressive broadcast
    ///     Group            1         01-FF       Group Id for this broadcast
    ///     Level            1         01-20       Progressive level
    ///     Amount         5 BCD                   Level Amount in units of cents
    ///     CRC            2 binary                16-bit CRC
    ///     Response
    ///     Ack or Nack
    /// </remarks>
    [Sas(SasGroup.Progressives)]
    public class LP80SingleLevelProgressiveBroadcastParser :
        SasLongPollParser<LongPollAck, SingleLevelProgressiveBroadcastData>
    {
        private const int GroupIdOffset = 2;
        private const int GroupFieldLength = 1;
        private const int LevelOffset = 3;
        private const int LevelFieldLength = 1;
        private const int LevelAmountOffset = 4;

        /// <summary>
        ///     Instantiates a new instance of the LP80SingleLevelProgressiveBroadcastParser class
        /// </summary>
        public LP80SingleLevelProgressiveBroadcastParser()
            : base(LongPoll.SingleLevelProgressiveBroadcastValue)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            Data.ProgId = (int)Utilities.FromBinary(longPoll, GroupIdOffset, GroupFieldLength);
            Data.LevelId = (int)Utilities.FromBinary(longPoll, LevelOffset, LevelFieldLength);

            var (levelAmount, valid) = Utilities.FromBcdWithValidation(longPoll,
                LevelAmountOffset,
                SasConstants.Bcd10Digits);

            if (!valid)
            {
                Logger.Debug("Level Amount is not a valid BCD number");
                return NackLongPoll(command);
            }

            Data.LevelAmount = (long)levelAmount;
            return Handle(Data).Data ? AckLongPoll(command) : NackLongPoll(command);
        }
    }
}