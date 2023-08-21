namespace Aristocrat.Sas.Client.LPParsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Extended Validation Status Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           7B        Extended Validation Status
    /// Length           1           08        Length of bytes following, not including the CRC
    /// Control Mask     2         table       See Table 15.2c in the SAS Protocol Spec on page 15-5
    /// Control States   2         table       Bit=1 to enable function, 0 to disable function if corresponding Control Mask bit is 1
    /// Expiration     2 BCD                   Number of days until cashable/handpay expiration. 0000=do not change existing expiration value, 9999=never expires
    /// Restricted exp 2 BCD                   Default number of days before restricted tickets expire. 0000=do not change, 9999=never expires
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           7B        Extended Validation Status
    /// Length           1           0A        Length of bytes following, not including the CRC
    /// Asset Number     4                     Machine Asset Number or house id
    /// Status bits      2                     Bit=1 if function enabled, 0 if function disabled. See Table 15.2c
    /// Expiration     2 BCD                   Number of days until cashable/handpay expiration. 0000=do not change existing expiration value, 9999=never expires
    /// Restricted exp 2 BCD                   Default number of days before restricted tickets expire. 0000=do not change, 9999=never expires
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Validation)]
    public class LP7BExtendedValidationStatusParser : SasLongPollParser<ExtendedValidationStatusResponse, ExtendedValidationStatusData>
    {
        private const int CommandLength = 8;
        private const int DataStartIndex = 2;
        private const int ControlMaskIndex = 3;
        private const int ControlStatesIndex = 5;
        private const int ControlLength = 2;
        private const int CashableExpirationIndex = 7;
        private const int RestrictedExpirationIndex = 9;
        private const byte ResponseLength = 10;
        private const int AssertNumberLength = 4;

        private static readonly uint ValidFlags = Enum.GetValues(typeof(ValidationControlStatus))
            .Cast<ValidationControlStatus>().Aggregate((uint)0, (current, value) => current | (uint)value);

        /// <inheritdoc />
        public LP7BExtendedValidationStatusParser()
            : base(LongPoll.ExtendedValidationStatus)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var commandData = command.ToArray();

            if (commandData[DataStartIndex] != CommandLength)
            {
                return NackLongPoll(command);
            }

            var controlMask = Utilities.FromBinary(
                commandData,
                ControlMaskIndex,
                ControlLength);
            var controlStates = Utilities.FromBinary(
                commandData,
                ControlStatesIndex,
                ControlLength);
            var (cashableExpiration, validCashableExpiration) = Utilities.FromBcdWithValidation(
                commandData,
                CashableExpirationIndex,
                SasConstants.Bcd4Digits);
            var (restrictedExpiration, validRestrictedExpiration) = Utilities.FromBcdWithValidation(
                commandData,
                RestrictedExpirationIndex,
                SasConstants.Bcd4Digits);

            if (!(validCashableExpiration && validRestrictedExpiration))
            {
                return NackLongPoll(command);
            }

            // We need to ignore any invalid bits set and just return zeros for those
            Data.ControlMask = (ValidationControlStatus)(controlMask & ValidFlags);
            Data.ControlStatus = (ValidationControlStatus)(controlStates & ValidFlags);
            Data.CashableExpirationDate = (int)cashableExpiration;
            Data.RestrictedExpirationDate = (int)restrictedExpiration;

            var result = Handler(Data);

            var response = command.Take(DataStartIndex).ToList();
            response.Add(ResponseLength);
            response.AddRange(Utilities.ToBinary((uint)result.AssertNumber, AssertNumberLength));
            response.AddRange(Utilities.ToBinary((uint)result.ControlStatus, ControlLength));
            response.AddRange(Utilities.ToBcd((ulong)result.CashableExpirationDate, SasConstants.Bcd4Digits));
            response.AddRange(Utilities.ToBcd((ulong)result.RestrictedExpirationDate, SasConstants.Bcd4Digits));

            return response;
        }
    }
}