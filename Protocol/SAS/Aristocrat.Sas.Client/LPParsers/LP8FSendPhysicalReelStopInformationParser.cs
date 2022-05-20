namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Physical Reel Stop Information Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           8F        Send physical reel stop information
    ///
    /// Response
    /// Field          Bytes       Value         Description
    /// Address          1         01-7F         Gaming Machine Address
    /// Command          1           8F          Send physical reel stop information
    /// Stops            9       ?????????       Physical reel stop information with the left most reel sent first. Unused bytes are padded with FF.
    /// CRC              2       0000-FFFF       16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP8FSendPhysicalReelStopInformationParser : SasLongPollParser<SendPhysicalReelStopInformationResponse, LongPollData>
    {
        /// <inheritdoc />
        public LP8FSendPhysicalReelStopInformationParser()
            : base(LongPoll.SendPhysicalReelStopInformation)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = command.ToList();
            var response = Handle(Data);

            // On gaming machines with multiple hands or more than nine reels, only the
            // first nine reel stops can be reported.
            result.Add(response.Reel1);
            result.Add(response.Reel2);
            result.Add(response.Reel3);
            result.Add(response.Reel4);
            result.Add(response.Reel5);
            result.Add(response.Reel6);
            result.Add(response.Reel7);
            result.Add(response.Reel8);
            result.Add(response.Reel9);

            return result;
        }
    }
}