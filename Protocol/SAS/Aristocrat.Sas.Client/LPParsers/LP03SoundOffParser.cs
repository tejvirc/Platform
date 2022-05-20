namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    /// This handles the Sound Off Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           03        Sound Off
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP03SoundOffParser : SasLongPollParser<LongPollResponse, LongPollSingleValueData<SoundActions>>
    {
        /// <summary>
        /// Instantiates a new instance of the LP03SoundOffParser class
        /// </summary>
        public LP03SoundOffParser() : base(LongPoll.SoundOff)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Data.Value = SoundActions.AllOff;
            Handle(Data);
            return AckLongPoll(command);
        }
    }
}
