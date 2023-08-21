namespace Aristocrat.Sas.Client.LPParsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Validation Meters Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           50        Send Validation Meters
    /// Validation type  1         01-82       Validation type from Table 15.13c on page 15-24 of the SAS Protocol spec
    ///                                        00 = Cashable ticket from cash out or win, no hand pay lockup
    ///                                        01 = Restricted promotional ticket from cash out
    ///                                        02 = Cashable ticket from AFT transfer
    ///                                        03 = Restricted ticket from AFT transfer
    ///                                        04 = Debit ticket from AFT transfer
    ///                                        10 = Canceled Credit hand pay (receipt printed)
    ///                                        20 = Jackpot hand pay (receipt printed)
    ///                                        40 = Canceled Credit hand pay (no receipt)
    ///                                        60 = Jackpot hand pay (no receipt)
    ///                                        80 = Cashable ticket redeemed
    ///                                        81 = Restricted Promotional ticket redeemed
    ///                                        82 = Nonrestricted promotional ticket redeemed
    /// CRC              2        0000-FFFF    16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           50        Send Validation Meters
    /// Validation type  1         01-82       Validation type from Table 15.13c on page 15-24 of the SAS Protocol spec
    /// Tot Validations 4 BCD       XXXX       Total validations of the requested type
    /// Cumulative amt  5 BCD      XXXXX       Cumulative amount in cents
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP50SendValidationMetersParser : SasLongPollParser<SendValidationMetersResponse, LongPollSingleValueData<TicketValidationType>>
    {
        private const int ValidationTypeIndex = 2;

        /// <inheritdoc />
        public LP50SendValidationMetersParser()
            : base(LongPoll.SendValidationMeters)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var commandData = command.ToArray();
            var ticketValidationType = (int)commandData[ValidationTypeIndex];
            if (!Enum.IsDefined(typeof(TicketValidationType), ticketValidationType) ||
                ticketValidationType == (int)TicketValidationType.None)
            {
                return NackLongPoll(commandData);
            }

            var response = commandData.Take(ValidationTypeIndex + 1).ToList();
            var result = Handler(new LongPollSingleValueData<TicketValidationType>((TicketValidationType)ticketValidationType));
            response.AddRange(Utilities.ToBcd((ulong)result.ValidationCount, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBcd((ulong)result.ValidationTotalAmount, SasConstants.Bcd10Digits));

            return response;
        }
    }
}