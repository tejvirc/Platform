namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Enhanced Validation Information Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           4D        Set Secure Enhanced validation ID
    /// Function code    1        00-1F,FF     00=read current info,
    ///                                        01-1F=validation info from buffer index n,
    ///                                        FF=look ahead at current validation info
    /// CRC              2        0000-FFFF    16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           4D        Set Secure Enhanced validation ID
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
    /// Index number     1          00-FF      Buffer position index number
    /// Date           4 BCD        XXXX       Validation Date i MMDDYYYY format
    /// Time           3 BCD         XXX       Time in HHMMSS 24 hour format
    /// Validation #   8 BCD      XXXXXXXX     Validation number (secure enhanced or system)
    /// Amount         5 BCD        XXXXX      Ticket/handpay amount in cents
    /// Ticket #         2     0000-270F,FFFF  The sequential number printed on the ticket. It starts at
    ///                                        0001, rolls over from 9999 to 0000. (FFFF for validations with no ticket)
    /// Validation     1 BCD         XX        00 = Secure Enhanced validation number calculated by gaming machine
    /// System Id                              01-99 = System ID code (indicates validation number provided by host)
    /// Expiration     4 BCD        XXXX       Expiration date printed on the ticket in MMDDYYYY format, or
    ///                                        00000001-00009998 = number of days before expiration, or
    ///                                        00009999 = ticket never expires, or 00000000 = no ticket printed or validation extensions not supported
    /// Pool Id          2       0000-FFFF     Restricted Pool Id (0000 if not restricted or Pool Id unknown)
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Validation)]
    public class LP4DSendEnhancedValidationInformationParser
        : SasLongPollParser<SendEnhancedValidationInformationResponse, SendEnhancedValidationInformation>
    {
        private const int FunctionCodeIndex = 2;
        private const int FunctionCodeLength = 1;
        private const int PoolIdLength = 2;
        private const int TicketNumberLength = 2;
        private const int ValidationTypeLength = 1;
        private const int IndexNumberLength = 1;
        private const int ZeroPaddedLength = 31;

        /// <inheritdoc />
        public LP4DSendEnhancedValidationInformationParser()
            : base(LongPoll.SendEnhancedValidationInformation)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Data.FunctionCode = (int)Utilities.FromBinary(command.ToArray(), FunctionCodeIndex, FunctionCodeLength);
            if (Data.FunctionCode > SasConstants.MaxValidationIndex &&
                Data.FunctionCode < SasConstants.LookAhead)
            {
                return NackLongPoll(command);
            }

            var result = Handle(Data);
            Handlers = result.Handlers;
            return BuildResponse(command, result);
        }

        private static IReadOnlyCollection<byte> BuildResponse(
            IReadOnlyCollection<byte> command,
            SendEnhancedValidationInformationResponse result)
        {
            var response = command.Take(FunctionCodeIndex).ToList();
            if (!result.Successful)
            {
                Logger.Debug("The read failed and the request is valid so we need return all zeros");
                response.AddRange(Enumerable.Range(0, ZeroPaddedLength).Select(x => (byte)0));
                return response;
            }

            response.AddRange(Utilities.ToBinary((uint)result.ValidationType, ValidationTypeLength));
            response.AddRange(Utilities.ToBinary((uint)result.Index, IndexNumberLength));
            response.AddRange(Utilities.ToSasDate(result.ValidationDate));
            response.AddRange(Utilities.ToSasTime(result.ValidationDate));
            response.AddRange(Utilities.ToBcd(result.ValidationNumber, SasConstants.Bcd16Digits));
            response.AddRange(Utilities.ToBcd((ulong)result.Amount, SasConstants.Bcd10Digits));
            response.AddRange(Utilities.ToBinary((uint)result.TicketNumber, TicketNumberLength));
            response.Add(Utilities.ToBcd(result.ValidationSystemId));
            response.AddRange(Utilities.ToBcd(result.ExpirationDate, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBinary(result.PoolId, PoolIdLength));

            return response;
        }
    }
}