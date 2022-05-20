namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Exit Maintenance Mode Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           0B        Exit Maintenance Mode
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP0BExitMaintenanceModeParser : SasLongPollParser<LongPollResponse, LongPollSingleValueData<bool>>
    {
        /// <inheritdoc />
        public LP0BExitMaintenanceModeParser()
            : base(LongPoll.ExitMaintenanceMode)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Handler(new LongPollSingleValueData<bool>(false));
            return AckLongPoll(command);
        }
    }
}