namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Shutdown Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           01        Shutdown
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP01ShutdownParser : SasLongPollParser<LongPollResponse, LongPollSingleValueData<byte>>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP01ShutdownHandler class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP01ShutdownParser(SasClientConfiguration configuration)
            : base(LongPoll.Shutdown)
        {
            Data.Value = configuration.ClientNumber;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Handle(Data);
            return AckLongPoll(command);
        }
    }
}