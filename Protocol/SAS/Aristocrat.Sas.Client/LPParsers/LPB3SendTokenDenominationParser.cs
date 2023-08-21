namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Token Denomination Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B3        Send Token Denomination
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B3        Send Token Denomination
    /// Token            1         00-3F       Binary number representing the token denomination.
    ///                                        See Table C-4 in Appendix C of the SAS Spec
    ///                                        00 = no configured token value
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPB3SendTokenDenominationParser : SasLongPollParser<LongPollReadSingleValueResponse<byte>, LongPollData>
    {
        /// <inheritdoc />
        public LPB3SendTokenDenominationParser()
            : base(LongPoll.SendTokenDenomination)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = Handle(Data);

            return new List<byte>(command) { result.Data };
        }
    }
}
