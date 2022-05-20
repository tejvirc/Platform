namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    /// This handles the Game Play Sounds Disabled Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           05        Game Play Sound Disabled
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP05GamePlaySoundDisabledParser : SasLongPollParser<LongPollResponse, LongPollSingleValueData<SoundActions>>
    {
        /// <summary>
        /// Instantiates a new instance of the LP05GamePlaySoundDisabledParser class
        /// </summary>
        public LP05GamePlaySoundDisabledParser() : base(LongPoll.GameSoundsDisable)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Data.Value = SoundActions.GameOff;
            Handle(Data);
            return AckLongPoll(command);
        }
    }
}
