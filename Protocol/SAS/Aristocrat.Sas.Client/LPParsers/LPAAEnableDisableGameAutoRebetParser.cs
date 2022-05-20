namespace Aristocrat.Sas.Client.LPParsers
{
    using LongPollDataClasses;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     This handles the Enable/Disable Auto Rebet Feature
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           AA        Enable/Disable Game
    /// Enable/Disable   1          00-01      00 - Disable Auto Rebet Feature, 01 - Enable Auto Rebet Feature
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LPAAEnableDisableGameAutoRebetParser : SasLongPollParser<LongPollReadSingleValueResponse<bool>,
        LongPollSingleValueData<byte>>
    {
        private const int EnableDisableByte = 2;

        /// <summary>
        ///     Instantiates a new instance of the LPAAEnableDisableGameAutoRebetParser class
        /// </summary>
        public LPAAEnableDisableGameAutoRebetParser()
            : base(LongPoll.EnableDisableGameAutoRebet)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            Data.Value = longPoll[EnableDisableByte];
            var res = Handle(Data);
            return res.Data ? AckLongPoll(command) : NackLongPoll(command);
        }
    }
}