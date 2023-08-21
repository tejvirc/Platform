namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Cash Out Ticket Information
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           3D        Send Cash Out Ticket Information
    /// 
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           3D        Send Cash Out Ticket Information
    /// Validation #     4 BCD     XXXX        Standard validation number
    ///                                        (calculated by the gaming machine)
    /// Ticket Amount    5 BCD     XXXXX       Ticket amount in units of cents
    /// CRC              2         0000-FFFF   16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class
        LP3DSendCashOutTicketInformationParser : SasLongPollParser<LongPollSendCashOutTicketInformationResponse,
            LongPollData>
    {
        /// <summary>
        ///     Initializes a new instance of the LP3DSendCashOutTicketInformationParser class.
        /// </summary>
        public LP3DSendCashOutTicketInformationParser()
            : base(LongPoll.SendCashOutTicketInformation)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = command.ToList();
            var response = Handler(Data);

            if (response == null)
            {
                return null;
            }

            result.AddRange(Utilities.ToBcd((ulong)response.ValidationNumber, SasConstants.Bcd8Digits));
            result.AddRange(Utilities.ToBcd((ulong)response.TicketAmount, SasConstants.Bcd10Digits));

            return result;
        }
    }
}