namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Game Delay Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2E        Game Delay
    /// Buffer Amount    2       0000-FFFF     Delay time in units of 100ms
    /// CRC              2       0000-FFFF     16-bit CRC
    /// 
    /// Response
    /// ACK or NACK according to SAS document.
    /// </remarks>
    [Sas(SasGroup.LegacyBonus)]
    public class LP2EGameDelayParser : SasLongPollParser<LongPollReadSingleValueResponse<bool>, LongPollSingleValueData<uint>>
    {
        private const int DelayIndex = 2;
        private const int DelaySize = 2;

        /// <summary>
        ///     Instantiates a new instance of the LPB7SetMachineNumberParser class
        /// </summary>
        public LP2EGameDelayParser()
            : base(LongPoll.DelayGame)
        {
        }

        /// <summary>
        ///     Parse 2E LP command for game delay
        /// </summary>
        /// <remarks>
        ///     This LP does not return any data to SAS. It merely initiates a game delay
        /// </remarks>
        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var info = command.ToArray();
            Data.Value = Utilities.FromBinary(info, DelayIndex, DelaySize);

            return Handle(Data).Data ? AckLongPoll(command) : NackLongPoll(command);
        }
    }
}