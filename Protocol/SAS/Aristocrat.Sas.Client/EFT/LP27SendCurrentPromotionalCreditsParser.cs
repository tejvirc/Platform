namespace Aristocrat.Sas.Client.EFT
{
    using System.Collections.Generic;
    using System.Linq;
    using Eft.Response;

    /// <summary>
    ///     This handles the request of current promotional credits
    /// </summary>
    /// <remarks>
    /// The command is as follows (Slot Accounting System version 5.02, Section 8.11.2):
    /// Field           Bytes       Value               Description
    /// Address         1           01-7F               Gaming Machine Address
    /// Command         1           27                  Request current promotional credits
    /// 
    /// Response
    /// Field           Bytes       Value               Description
    /// Address         1           01-7F               Gaming Machine Address
    /// Command         1           27                  Request current promotional credits
    /// Promotional     4 BCD       XXXX                Current number of promitional credit
    /// Credits                                         in units of credits sent MSB first
    /// CRC             2           0000-FFFF           CCITT 16-bit CRC sent LSB first
    /// </remarks>
    [Sas(SasGroup.Eft)]
    public class LP27SendCurrentPromotionalCreditsParser : SasLongPollParser<EftSendCurrentPromotionalCreditsResponse, LongPollData>
    {
        /// <summary>
        ///     Initializes a new instance of the LP27SendCurrentPromotionalCreditsParser class.
        /// </summary>
        public LP27SendCurrentPromotionalCreditsParser() : base(LongPoll.EftSendCurrentPromotionalCredits)
        {
        }

        /// <summary>
        ///     Request current promotional credits
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = Handler(Data);
            var result = command.ToList();
            result.AddRange(Utilities.ToBcd(response.CurrentPromotionalCredits, SasConstants.Bcd8Digits));
            return result;
        }
    }
}