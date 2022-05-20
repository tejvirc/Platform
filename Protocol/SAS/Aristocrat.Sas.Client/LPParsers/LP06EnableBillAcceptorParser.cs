namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Enable Bill Acceptor Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           06        Enable Bill Acceptor
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP06EnableBillAcceptorParser : SasLongPollParser<EnableDisableResponse, EnableDisableData>
    {
        /// <summary>
        /// Instantiates a new instance of the LP06EnableBillAcceptorParser class
        /// </summary>
        public LP06EnableBillAcceptorParser() : base(LongPoll.EnableBillAcceptor)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Data.Enable = true;
            var result = Handle(Data);

            if (!result.Succeeded)
            {
                Logger.Debug("Enable failed. NACKing Enable Bill Acceptor long poll");
                return NackLongPoll(command);
            }

            return AckLongPoll(command);
        }
    }
}
