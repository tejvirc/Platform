namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Disable Bill Acceptor Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           07        Disable Bill Acceptor
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP07DisableBillAcceptorParser : SasLongPollParser<EnableDisableResponse, EnableDisableData>
    {
        /// <summary>
        /// Instantiates a new instance of the LP07DisableBillAcceptorParser class
        /// </summary>
        public LP07DisableBillAcceptorParser() : base(LongPoll.DisableBillAcceptor)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Data.Enable = false;
            var result = Handle(Data);

            if (!result.Succeeded)
            {
                Logger.Debug("Disable failed. NACKing Disable Bill Acceptor long poll");
                return NackLongPoll(command);
            }

            return AckLongPoll(command);
        }
    }
}
