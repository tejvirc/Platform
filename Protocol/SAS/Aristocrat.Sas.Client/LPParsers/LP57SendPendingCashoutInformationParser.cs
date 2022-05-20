namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Pending Cashout Information Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           57        Send Pending Cashout Information
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           57        Send Pending Cashout Information
    /// Cashout type     1           XX        00=Cashable Ticket, 01=Restricted Promotional Ticket, 80=Not Waiting for validation
    /// Amount         5 BCD                   cashout amount in cents
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Validation)]
    public class LP57SendPendingCashoutInformationParser : SasLongPollParser<SendPendingCashoutInformation, LongPollData>
    {
        public LP57SendPendingCashoutInformationParser()
            : base(LongPoll.SendPendingCashoutInformation)
        {
        }

        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = Handler(Data);
            if (!result.ValidResponse)
            {
                return null;
            }

            var response = command.ToList();
            response.Add((byte)result.TypeCode);
            response.AddRange(Utilities.ToBcd(result.Amount, SasConstants.Bcd10Digits));

            return response;
        }
    }
}