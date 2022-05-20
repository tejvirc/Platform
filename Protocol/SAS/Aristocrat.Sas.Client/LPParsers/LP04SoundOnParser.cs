namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    /// This handles the Sound On Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           04        Sound On
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP04SoundOnParser : SasLongPollParser<LongPollResponse, LongPollSingleValueData<SoundActions>>
    {
        /// <summary>
        /// Instantiates a new instance of the LP04SoundOnParser class
        /// </summary>
        public LP04SoundOnParser() : base(LongPoll.SoundOn)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Data.Value = SoundActions.AllOn;
            Handle(Data);
            return AckLongPoll(command);
        }
    }
}
