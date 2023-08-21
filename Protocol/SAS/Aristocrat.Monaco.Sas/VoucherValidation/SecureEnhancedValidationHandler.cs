namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Base;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Storage.Models;
    using Storage.Repository;
    using Ticketing;
    using TaskExtensions = Common.TaskExtensions;

    /// <summary>
    ///     Handles the validation for Secure Enhanced
    /// </summary>
    public class SecureEnhancedValidationHandler : BaseValidationHandler, IDisposable
    {
        private const double ValidationConfigurationTimer = 15000.0; // 15 seconds
        private const int SequenceLength = 3;
        private const int IdLength = 3;
        private const int CrcTakeLength = 2;
        private const int ValidationArraySize = SequenceLength + IdLength;
        private const string ValidationId = "00";

        private readonly ISasDisableProvider _disableProvider;
        private readonly SasExceptionTimer _validationTimer;
        private bool _disposed;

        /// <summary>
        ///     Creates the SecureEnhancedValidationHandler instance
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="transactionHistory">The transaction history</param>
        /// <param name="disableProvider">The disable provider</param>
        /// <param name="ticketingCoordinator">The ticketing coordinator</param>
        /// <param name="exceptionHandler"></param>
        /// <param name="validationProvider"></param>
        public SecureEnhancedValidationHandler(
            IPropertiesManager propertiesManager,
            ITransactionHistory transactionHistory,
            ISasDisableProvider disableProvider,
            ITicketingCoordinator ticketingCoordinator,
            ISasExceptionHandler exceptionHandler,
            IStorageDataProvider<ValidationInformation> validationProvider)
            : base(
                SasValidationType.SecureEnhanced,
                ticketingCoordinator,
                transactionHistory,
                propertiesManager,
                validationProvider)
        {
            if (exceptionHandler == null)
            {
                throw new ArgumentNullException(nameof(exceptionHandler));
            }

            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
            _validationTimer = new SasExceptionTimer(
                exceptionHandler,
                GetValidationConfigurationException,
                ValidationNotConfigured,
                ValidationConfigurationTimer);
        }

        /// <inheritdoc />
        public override bool CanValidateTicketOutRequest(ulong amount, TicketType ticketType)
        {
            return !ValidationNotConfigured() && base.CanValidateTicketOutRequest(amount, ticketType);
        }

        /// <inheritdoc />
        public override async Task<TicketOutInfo> HandleTicketOutValidation(ulong amount, TicketType ticketType)
        {
            if (ValidationNotConfigured() ||
                !CanValidate(ticketType) ||
                await AnyTransactionPendingValidation())
            {
                return null;
            }

            var ticketExpirationDate = GetTicketExpirationDate(ticketType);
            if (!ValidExpirationDate(ticketType, ticketExpirationDate))
            {
                return null;
            }

            var validationInformation = ValidationProvider.GetData();
            var id = validationInformation.MachineValidationId;

            var barcode = GenerateBarcode(id, GetNextSequenceNumber(validationInformation));
            Logger.Debug($"Generated Barcode {barcode}");

            return BuildTicketOut(amount, ticketType, ticketExpirationDate, barcode);
        }

        /// <inheritdoc />
        public override Task HandleTicketOutCompleted(VoucherOutTransaction transaction) => UpdateBarcodeSequence(transaction);

        /// <inheritdoc />
        public override Task<TicketOutInfo> HandleHandPayValidation(ulong amount, HandPayType type)
        {
            var validationInformation = ValidationProvider.GetData();
            var id = validationInformation.MachineValidationId;
            var sequenceNumber = validationInformation.SequenceNumber;

            var barcode = string.Empty;
            if (PropertiesManager.GetValue(AccountingConstants.ValidateHandpays, false))
            {
                barcode = GenerateBarcode(id, ++sequenceNumber);
                Logger.Debug($"Generated Barcode {barcode}");
            }

            var handPayTicketType = GetHandPayTicketType(type);
            return Task.FromResult(
                BuildTicketOut(amount, handPayTicketType, GetTicketExpirationDate(handPayTicketType), barcode));
        }

        /// <inheritdoc />
        public override Task HandleHandpayCompleted(HandpayTransaction transaction) => UpdateBarcodeSequence(transaction);

        /// <inheritdoc />
        public override void Initialize()
        {
            if (ValidationNotConfigured())
            {
                TaskExtensions.FireAndForget(_disableProvider.Disable(
                    SystemDisablePriority.Normal,
                    DisableState.ValidationIdNeeded));
                _validationTimer.StartTimer(true);
            }

            var lastValidatedTransaction = TransactionHistory.RecallTransactions()
                .FirstOrDefault(x => x is HandpayTransaction || x is VoucherOutTransaction);
            TaskExtensions.RunOnCurrentThread(() => UpdateBarcodeSequence(lastValidatedTransaction));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes the unmanaged resources
        /// </summary>
        /// <param name="disposing">Whether or not to dispose the resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _validationTimer.Dispose();
            }

            _disposed = true;
        }

        private async Task UpdateBarcodeSequence(ITransaction lastValidatedTransaction)
        {
            var validationInformation = ValidationProvider.GetData();
            var incrementNumber = false;
            var id = validationInformation.MachineValidationId;
            var sequenceNumber = GetNextSequenceNumber(validationInformation);
            var currentBarcode = GenerateBarcode(id, sequenceNumber);

            switch (lastValidatedTransaction)
            {
                case HandpayTransaction handpayTransaction:
                    incrementNumber = handpayTransaction.Barcode == currentBarcode && !handpayTransaction.IsCreditType();
                    break;
                case VoucherOutTransaction voucherOutTransaction:
                    incrementNumber = voucherOutTransaction.Barcode == currentBarcode;
                    break;
            }

            if (incrementNumber)
            {
                validationInformation.SequenceNumber = sequenceNumber;
                await ValidationProvider.Save(validationInformation);
                Logger.Debug($"Incrementing the barcode sequence number to {sequenceNumber}");
            }
        }

        private static long GetNextSequenceNumber(ValidationInformation validationInformation)
        {
            var sequenceNumber = validationInformation.SequenceNumber;
            return ++sequenceNumber % (SasConstants.MaxValidationSequenceNumber + 1);
        }

        private bool ValidationNotConfigured() => !ValidationProvider.GetData().ValidationConfigured;

        private GeneralExceptionCode? GetValidationConfigurationException() => ValidationNotConfigured()
            ? (GeneralExceptionCode?)GeneralExceptionCode.ValidationIdNotConfigured
            : null;

        private static byte[] SecureEnhancedStep1(long machineId, long sequenceNumber)
        {
            // Step 1 get array A
            var arrayA = new byte[ValidationArraySize];
            Array.Copy(BitConverter.GetBytes(sequenceNumber), 0, arrayA, 0, SequenceLength);
            Array.Copy(BitConverter.GetBytes(machineId), 0, arrayA, SequenceLength, IdLength);

            return arrayA;
        }

        private static byte[] SecureEnhancedStep2(IReadOnlyList<byte> arrayA)
        {
            // Step 2 get array B
            var arrayB = new byte[ValidationArraySize];
            arrayB[0] = arrayA[0];
            arrayB[1] = arrayA[1];
            arrayB[2] = (byte)(arrayA[0] ^ arrayA[2]);
            arrayB[3] = (byte)(arrayA[1] ^ arrayA[3]);
            arrayB[4] = (byte)(arrayA[0] ^ arrayA[4]);
            arrayB[5] = (byte)(arrayA[1] ^ arrayA[5]);

            return arrayB;
        }

        private static byte[] SecureEnhancedStep3(IReadOnlyList<byte> arrayB)
        {
            // Step 3 get array C
            var arrayC = new byte[ValidationArraySize];
            var startIndex = 0;
            var crc = BitConverter.GetBytes(Utilities.GenerateCrc(arrayB.Take(CrcTakeLength).ToArray(), CrcTakeLength));
            Array.Copy(crc, 0, arrayC, startIndex, crc.Length);
            startIndex = crc.Length;
            crc = BitConverter.GetBytes(Utilities.GenerateCrc(arrayB.Skip(startIndex).Take(CrcTakeLength).ToArray(), CrcTakeLength));
            Array.Copy(crc, 0, arrayC, startIndex, crc.Length);
            startIndex += crc.Length;
            crc = BitConverter.GetBytes(Utilities.GenerateCrc(arrayB.Skip(startIndex).Take(CrcTakeLength).ToArray(), CrcTakeLength));
            Array.Copy(crc, 0, arrayC, startIndex, crc.Length);

            return arrayC;
        }

        private static Tuple<byte[], byte[]> SecureEnhancedStep4(byte[] arrayC)
        {
            // Step 4 convert array C into BCD digits
            const int bcdSize = 3;
            var binToBcd = new byte[sizeof(uint)];
            Array.Copy(arrayC, 0, binToBcd, 0, bcdSize);
            var lowerHalf = BitConverter.ToUInt32(binToBcd, 0);
            Array.Copy(arrayC, bcdSize, binToBcd, 0, bcdSize);
            var upperHalf = BitConverter.ToUInt32(binToBcd, 0);

            var lowerBytes = Utilities.ToBcd(lowerHalf, SasConstants.Bcd8Digits);
            var upperBytes = Utilities.ToBcd(upperHalf, SasConstants.Bcd8Digits);

            return new Tuple<byte[], byte[]>(lowerBytes, upperBytes);
        }

        private static string SecureEnhancedStep5(byte[] lowerBytes, byte[] upperBytes)
        {
            // Step 5 convert the BCD digits into the barcode
            const int upperBcdMask = 0xF0;
            const int lowerBcdMask = 0x0F;
            const int bcdShift = 4;
            // Get the sum of the BCD values
            var v7 = lowerBytes.Sum(b => (b & lowerBcdMask) + ((b & upperBcdMask) >> bcdShift));
            var v15 = upperBytes.Sum(x => (x & lowerBcdMask) + ((x & upperBcdMask) >> bcdShift));

            const int modSize = 5;
            // Set the upper byte for both lower and upper halves to the calculated values
            lowerBytes[0] |= (byte)((v7 % modSize) << (bcdShift + 1));
            upperBytes[0] |= (byte)((v15 % modSize) << (bcdShift + 1));

            return BitConverter.ToString(upperBytes).Replace("-", string.Empty) +
                   BitConverter.ToString(lowerBytes).Replace("-", string.Empty);
        }

        private static string GenerateBarcode(long machineId, long sequenceNumber)
        {
            // The following algorithm comes from the SAS 6.03 Specification Section 15.14 Secure Enhanced Validation Algorithm
            var arrayA = SecureEnhancedStep1(machineId, sequenceNumber);
            var arrayB = SecureEnhancedStep2(arrayA);
            var arrayC = SecureEnhancedStep3(arrayB);
            var (lowerBytes, upperBytes) = SecureEnhancedStep4(arrayC);
            var digitsV = SecureEnhancedStep5(lowerBytes, upperBytes);

            return ValidationId + digitsV;
        }
    }
}