namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Storage.Models;
    using Storage.Repository;
    using Ticketing;

    /// <inheritdoc />
    public class NoValidationHandler : BaseValidationHandler
    {
        /// <inheritdoc />
        public NoValidationHandler(
            ITicketingCoordinator ticketingCoordinator,
            ITransactionHistory transactionHistory,
            IPropertiesManager propertiesManager,
            IStorageDataProvider<ValidationInformation> validationProvider)
            : base(
                SasValidationType.None,
                ticketingCoordinator,
                transactionHistory,
                propertiesManager,
                validationProvider)
        {
        }

        /// <inheritdoc />
        public override Task<TicketOutInfo> HandleTicketOutValidation(ulong amount, TicketType ticketType)
        {
            if (!CanValidate(ticketType))
            {
                return Task.FromResult((TicketOutInfo)null);
            }

            var ticketExpirationDate = GetTicketExpirationDate(ticketType);
            if (!ValidExpirationDate(ticketType, ticketExpirationDate))
            {
                return Task.FromResult((TicketOutInfo)null);
            }

            var ticketDateTime = DateTime.UtcNow;
            var barcode = GenerateBarcode(amount, ticketDateTime);
            Logger.Debug($"Generated Barcode {barcode}");

            var ticketOutInfo = BuildTicketOut(amount, ticketType, ticketExpirationDate, barcode);
            ticketOutInfo.Time = ticketDateTime;

            return Task.FromResult(ticketOutInfo);
        }

        /// <inheritdoc />
        public override Task HandleTicketOutCompleted(VoucherOutTransaction transaction) => Task.CompletedTask;

        /// <inheritdoc />
        public override Task HandleHandpayCompleted(HandpayTransaction transaction) => Task.CompletedTask;

        /// <inheritdoc />
        public override Task<TicketOutInfo> HandleHandPayValidation(ulong amount, HandPayType type)
        {
            var ticketDateTime = DateTime.UtcNow;
            var barcode = string.Empty;
            if (PropertiesManager.GetValue(AccountingConstants.ValidateHandpays, false))
            {
                barcode = GenerateBarcode(amount, ticketDateTime);
                Logger.Debug($"Generated Barcode {barcode}");
            }

            var handPayTicketType = GetHandPayTicketType(type);
            var ticketOutInfo = BuildTicketOut(
                amount,
                handPayTicketType,
                GetTicketExpirationDate(handPayTicketType),
                barcode);
            ticketOutInfo.Time = ticketDateTime;
            return Task.FromResult(ticketOutInfo);
        }

        /// <inheritdoc />
        public override void Initialize()
        {
        }

        /// <summary>
        ///     Generate a standard validation barcode. See SAS 5.02 15.11
        /// </summary>
        /// <param name="millicents">The amount in millicents.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <returns>The barcode value.</returns>
        public static string GenerateBarcode(ulong millicents, DateTime transactionDateTime)
        {
            // Data setup - Create arrays for amount and time
            var amount = ((long)millicents).MillicentsToCents();
            var amountBcd = Utilities.ToBcd((ulong)amount, SasConstants.Bcd6Digits);
            int[] timeArray = { transactionDateTime.Hour, transactionDateTime.Minute, transactionDateTime.Second };

            var validationNumberHex = StandardValidationStep1(amountBcd, timeArray);
            StandardValidationStep2(validationNumberHex);
            return StandardValidationStep3(validationNumberHex);
        }

        private static byte[] StandardValidationStep1(byte[] amountBcd, int[] timeArray)
        {
            // BCD addition with carry.  Byte 0 of BCD amount is LSB.  Byte 2 is MSB
            const int bcdArrayLength = 3;

            byte carryBcd = 0;
            var validationNumberHex = new byte[bcdArrayLength + 1];

            for (var i = bcdArrayLength; i >= 1; i--)
            {
                var (amountInt, _) = Utilities.FromBcdWithValidation(amountBcd, (uint)i - 1, SasConstants.Bcd2Digits);
                var (carryInt, _) = Utilities.FromBcdWithValidation(new[] { carryBcd }, 0, SasConstants.Bcd2Digits);

                var sum = amountInt + (ulong)timeArray[i - 1] + carryInt;
                var sumBcd = Utilities.ToBcd(sum, SasConstants.Bcd4Digits);

                carryBcd = sumBcd[0];
                validationNumberHex[bcdArrayLength - i] = sumBcd[1];
            }

            return validationNumberHex;
        }

        private static void StandardValidationStep2(byte[] validationNumberHex)
        {
            // Copy LSB of result to 4th byte of result
            if (validationNumberHex.Length >= 4)
            {
                validationNumberHex[3] = validationNumberHex[0];
            }
        }

        private static string StandardValidationStep3(byte[] validationNumberHex)
        {
            // Treat 4 byte result as base 16 and convert to BCD
            var validationNumberInt = Utilities.FromBinary(validationNumberHex, 0, SasConstants.Bcd8Digits);
            var validationNumberBcd = Utilities.ToBcd(validationNumberInt, SasConstants.Bcd8Digits);
            return string.Join(string.Empty, validationNumberBcd.Select(v => v.ToString("X")));
        }
    }
}