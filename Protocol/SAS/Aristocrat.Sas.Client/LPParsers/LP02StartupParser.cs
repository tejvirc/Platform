namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Startup Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           02        Startup
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP02StartupParser : SasLongPollParser<LongPollResponse, LongPollSASClientConfigurationData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP02StartupParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP02StartupParser(SasClientConfiguration configuration)
            : base(LongPoll.Startup)
        {
            Data.ClientConfiguration = configuration;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LP02 Enable System");
            Handle(Data);
            return AckLongPoll(command);
        }
    }
}