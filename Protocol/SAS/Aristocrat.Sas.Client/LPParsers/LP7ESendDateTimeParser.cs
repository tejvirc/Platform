namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Current Date And Time Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           7E        Send Current Date And Time
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           7E        Send Current Date And Time
    /// Date           4 BCD        XXXX       Date in MMDDYYY format
    /// Time           3 BCD        XXX        Time in HHMMSS 24 hour format
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP7ESendDateTimeParser : SasLongPollParser<LongPollDateTimeResponse, LongPollDateTimeData>
    {
        /// <summary>
        /// Instantiates a new instance of the LP7ESendDateTimeParser class
        /// </summary>
        public LP7ESendDateTimeParser() : base(LongPoll.SetCurrentDateTime)
        {
        }

        /// <inheritdoc/>
        /// <summary>
        ///     Parse the command and build a return array of the data in BCD format. 
        /// </summary>
        /// <param name="command">input command array</param>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = command.ToList();
            var response = Handler(Data);
            result.AddRange(Utilities.ToSasDateTime(response.DateAndTime));

            return result;
        }
    }
}