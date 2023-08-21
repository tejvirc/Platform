namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Enter Maintenance Mode Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           0A        Enter Maintenance Mode
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP0AEnterMaintenanceModeParser : SasLongPollParser<LongPollResponse, LongPollSingleValueData<bool>>
    {
        /// <inheritdoc />
        public LP0AEnterMaintenanceModeParser()
            : base(LongPoll.EnterMaintenanceMode)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Handler(new LongPollSingleValueData<bool>(true));
            return AckLongPoll(command);
        }
    }
}