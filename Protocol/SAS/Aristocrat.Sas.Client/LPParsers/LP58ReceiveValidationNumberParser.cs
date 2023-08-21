namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Receive Validation Number Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           52        Receive Validation Number
    /// Validation Id  1 BCD         XX        Validation system id code (00=system validation denied)
    /// Validation #   8 BCD     XXXXXXXX      Validation number if validation not denied
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           52        Receive Validation Number
    /// Status           1                     00=command acknowledged, 80=not in cashout, 81=improper validation rejected
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Validation)]
    public class LP58ReceiveValidationNumberParser : SasLongPollParser<ReceiveValidationNumberResult, ReceiveValidationNumberData>
    {
        private const int Length = 13;
        private const int ValidationSystemIdIndex = 2;
        private const int ValidationNumberIndex = 3;

        public LP58ReceiveValidationNumberParser()
            : base(LongPoll.ReceiveValidationNumber)
        {
        }

        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var commandData = command.ToArray();
            if (commandData.Length < Length)
            {
                return NackLongPoll(command);
            }
            
            var (systemId, valid) = Utilities.FromBcdWithValidation(
                commandData,
                ValidationSystemIdIndex,
                SasConstants.Bcd2Digits);
            if (!valid)
            {
                return NackLongPoll(command);
            }

            Data.ValidationSystemId = (byte)systemId;
            (Data.ValidationNumber, valid) = Utilities.FromBcdWithValidation(
                commandData,
                ValidationNumberIndex,
                SasConstants.Bcd16Digits);
            if (!valid)
            {
                return NackLongPoll(command);
            }

            var result = Handler(Data);
            return result.ValidResponse
                ? new List<byte>
                {
                    commandData[SasConstants.SasAddressIndex],
                    commandData[SasConstants.SasCommandIndex],
                    (byte)result.Status
                }
                : null;
        }
    }
}