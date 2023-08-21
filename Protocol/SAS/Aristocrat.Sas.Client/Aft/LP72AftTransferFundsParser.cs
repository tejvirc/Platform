namespace Aristocrat.Sas.Client.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the AFT Transfer Funds Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           72        AFT Transfer Funds
    /// Length           1         01-nn       Number of bytes following, not including CRC
    /// Transfer Code    1           nn        00 - Transfer Request, full transfer only
    ///                                        01 - Transfer Request, partial transfers allowed
    ///                                        80 - Cancel transfer request
    /// Transaction Index 1          00        Only "current" transaction may be initiated
    /// Transfer Type    1           nn        Transfer type
    ///                                        00 - Transfer in-house amount from host to gaming machine
    ///                                        10 - Transfer bonus coin out win from host to gaming machine
    ///                                        11 - Transfer bonus jackpot win from host to gaming machine (force handpay lockup)
    ///                                        20 - Transfer in-house amount from host to ticket
    ///                                        40 - Transfer debit amount from host to gaming machine
    ///                                        60 - Transfer debit amount from host to ticket
    ///                                        80 - Transfer in-house amount from gaming machine to host
    ///                                        90 - Transfer win amount (in-house) from gaming machine to host
    /// Cashable Amount 5 BCD     XXXXX        Cashable transfer amount in cents
    /// Restricted Amount 5 BCD   XXXXX        Restricted transfer amount in cents
    /// Non-restricted amt 5 BCD  XXXXX        Non-Restricted transfer amount in cents
    /// Transfer flags   1        00-FF        Bit  Description
    ///                                         0   Host cashout enable control. 1 = set enable to bit 1 state
    ///                                         1   Host cashout enable (ignore if bit 0 is 0)
    ///                                         2   Host cashout mode. 0=soft, 1=hard (ignore is bit 0 is 0)
    ///                                         3   Cashout from gaming machine request
    ///                                         4   Lock After Transfer request (see Section 8.9 of SAS spec)
    ///                                         5   Use custom ticket data from long poll 76
    ///                                         6   Accept transfer only if locked
    ///                                         7   Transaction Receipt request
    /// Asset Number    4       nnnnnnnn       Gaming machine asset number or house ID
    /// Registration key 20     nn...          Registration key (0 = registration not required)
    /// Transaction Id  1        01-0x14       Length of message transaction ID
    /// Length
    /// Transaction Id  x        ???           Transaction ID ASCII text (1-20 characters)
    /// Expiration      4        XXXX          Expiration date in MMDDYYYY format or 0000NNNN days format
    /// Pool ID         2      0000-FFFF       Restricted Pool ID
    /// Receipt data    1         nn           Number of bytes of receipt data following
    /// Length                                 (Length zero if no data provided.Data may be provided even if no receipt is requested.
    ///                                        Note that maximum overall message length must not be exceeded.)
    /// Receipt Data    X         ???          Transaction Receipt data. (see SAS Spec Table 8.3f)
    /// Lock Timeout    2      0000-9999       Lock expiration time in hundredths of a second. Only used for Lock After Transfer Request
    /// CRC             2      0000-FFFF       16-bit CRC
    ///
    /// ===============================================================================================================================
    /// AFT transfer funds Interrogation only poll
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           72        AFT Transfer Funds
    /// Length           1           02        Number of bytes following, not including CRC
    /// Transfer Code    1          FE,FF      Identify poll as interrogation request
    /// Transaction Index 1          00        current or most recent transaction
    ///                             01-7F      absolute history buffer position
    ///                             81-FF      relative history index
    /// CRC             2         0000-FFFF    16-bit CRC
    ///
    /// ===============================================================================================================================
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           72        AFT Transfer Funds
    /// Length           1         02-nn       Number of bytes following, not including CRC
    /// Transaction      1         00-FF       Specific transaction history buffer position. 0=current or most recent transaction, not in history buffer
    /// buffer position
    /// Transfer Status  1          nn         Gaming machine transfer status code
    ///                                        00 - Full transfer successful
    ///                                        01 - partial transfer successful
    ///                                        40 - Transfer pending (not complete)
    ///                                        80 - Transfer canceled by host
    ///                                        81 - Transaction ID not unique
    ///                                        82 - Not a valid function
    ///                                        83 - Not a valid transfer amount or expiration
    ///                                        84 - Transfer amount exceeds gaming machine limit
    ///                                        85 - Transfer amount not an even multiple of gaming machine denomination
    ///                                        86 - Gaming machine unable to perform partial transfers to the host
    ///                                        87 - Gaming machine unable to perform transfers at this time (tilt, disabled, etc)
    ///                                        88 - Gaming machine not registered
    ///                                        89 - Registration key doesn't match
    ///                                        8A - No POS ID (required for Debit transfers)
    ///                                        8B - No won credits available for cashout
    ///                                        8C - No gaming machine denomination set
    ///                                        8D - Expiration not valid for transfer to ticket (already expired)
    ///                                        8E - transfer to ticket device not available
    ///                                        8F - Unable to accept transfer due to existing restricted amounts from a different pool
    ///                                        90 - Unable to print transaction receipt
    ///                                        91 - Insufficient data to print transaction receipt
    ///                                        92 - Transaction receipt not allowed for specified transfer type
    ///                                        93 - Asset number zero or does not match
    ///                                        94 - Gaming machine not locked
    ///                                        95 - Transaction ID not valid
    ///                                        9F - Unexpected Error
    ///                                        C0 - Not compatible with current transfer in progress
    ///                                        C1 - Unsupported transfer code
    ///                                        FF - no transfer information available
    /// Receipt Status   1         nn          Transaction Receipt status
    ///                                        00 - printed
    ///                                        20 - printing in progress (not complete)
    ///                                        40 - Receipt pending (not complete)
    ///                                        FF - no receipt requested or receipt not printed
    /// Transfer Type    1         nn          Transfer Type
    ///                                        00 - Transfer in-house amount from host to gaming machine
    ///                                        10 - Transfer bonus coin out win from host to gaming machine
    ///                                        11 - Transfer bonus jackpot win from host to gaming machine (force handpay lockup)
    ///                                        20 - Transfer in-house amount from host to ticket
    ///                                        40 - Transfer debit amount from host to gaming machine
    ///                                        60 - Transfer debit amount from host to ticket
    ///                                        80 - Transfer in-house amount from gaming machine to host
    ///                                        90 - Transfer win amount (in-house) from gaming machine to host
    /// Cashable Amount 5 BCD     XXXXX        Actual or pending Cashable transfer amount in cents
    /// Restricted Amount 5 BCD   XXXXX        Actual or pending Restricted transfer amount in cents
    /// Non-restricted amt 5 BCD  XXXXX        Actual or pending Non-Restricted transfer amount in cents
    /// Transfer flags   1        00-FF        Bit  Description
    ///                                         0   0 = cashout to host forced by gaming machine, 1 = cashout to host controllable by host
    ///                                         1   0 = cashout to host currently disabled, 1 = cashout to host currently enabled
    ///                                         2   0 = host cashout mode currently soft, 1 = host cashout mode currently hard
    ///                                         3   0 = host didn't request cashout, 1 = host requested cashout
    ///                                         4   0 = no lock after AFT request, 1 = lock after AFT request
    ///                                         5   custom ticket data requested
    ///                                         6   0 = host did not require lock, 1 = host requested transfer only if locked
    ///                                         7   Transaction Receipt request
    /// Asset Number     4       nnnnnnnn      Gaming machine asset number or house ID
    /// Transaction Id   1       01-0x14       Length of message transaction ID
    /// Length
    /// Transaction Id   x       ???           Transaction ID ASCII text (1-20 characters)
    /// Transaction Date 4        XXXX         Date transaction completed in MMDDYYYY format
    /// Transaction Time 3         XXX         Time transaction completed in HHMMSS 24 hour format
    /// Expiration       4        XXXX         Expiration date in MMDDYYYY format or 0000NNNN days format
    /// Pool ID          2      0000-FFFF      Restricted Pool ID
    /// Cumulative cash  1        00-09        Length of cumulative cashable amount meter for transfer type, after transfer complete
    /// amt meter size                         (0 until transfer complete)
    /// Cumulative cash  x         ???         Cumulative cashable amount meter for transfer type, in cents, 0-9 bytes
    /// amt meter
    /// Cumulative restricted  1  00-09        Length of cumulative restricted amount meter for transfer type, after transfer complete
    /// amt meter size                         (0 until transfer complete)
    /// Cumulative restricted  x   ???         Cumulative restricted amount meter for transfer type, in cents, 0-9 bytes
    /// amt meter
    /// Cumulative non-restrict 1 00-09        Length of cumulative non-restricted amount meter for transfer type, after transfer complete
    /// amt meter size                         (0 until transfer complete)
    /// Cumulative non-restrict x  ???         Cumulative non-restricted amount meter for transfer type, in cents, 0-9 bytes
    /// amt meter
    /// CRC              2      0000-FFFF      16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Aft)]
    public class LP72AftTransferFundsParser : SasLongPollParser<AftResponseData, AftTransferData>
    {
        private const byte AssetNumberLength = 4;
        private const byte PoolIdLength = 2;
        private const int LengthOffset = 2;
        private const int TransferCodeOffset = 3;
        private const int TransactionIndexOffset = 4;
        private const int TransferTypeOffset = 5;
        private const int CashableAmountOffset = 6;
        private const int RestrictedAmountOffset = 11;
        private const int NonRestrictedAmountOffset = 16;
        private const int TransferFlagsOffset = 21;
        private const int AssetNumberOffset = 22;
        private const int RegistrationKeyOffset = 26;
        private const int TransactionIdLengthOffset = 46;
        private const int TransactionIdOffset = 47;
        private const int RegistrationKeySize = 20;
        private const int BytesNotIncludedInLength = 5;
        private const byte MeterDataLength = 8;
        private const long CurrencyMultiplier = 1000;
        private const int MaxTransferSourceDestinationLength = 22;
        private const int DateTimeLength = 7;
        private const int MaxPatronNameLength = 22;
        private const int MaxPatronAccountNumberLength = 16;
        private const int MinReceiptFieldLength = 2;

        /// <inheritdoc />
        public LP72AftTransferFundsParser()
            : base(LongPoll.AftTransferFunds)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            var (data, errorCode) = GetAftTransferDataFromCommand(longPoll);
            if (errorCode.HasValue)
            {
                return GenerateLongPollResponse(command, new AftResponseData
                {
                    TransferStatus = errorCode.Value,
                    ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
                });
            }

            var responseData = Handler(data);
            Handlers = responseData.Handlers;
            return GenerateLongPollResponse(command, responseData);
        }

        private static (AftTransferData data, AftTransferStatusCode? errorCode) GetAftTransferDataFromCommand(byte[] longPoll)
        {
            var data = new AftTransferData();
            // check if the length reported in the length byte is equal to
            // the actual length. Don't include the address + command + length + crc bytes
            if (longPoll.Length - BytesNotIncludedInLength != longPoll[LengthOffset])
            {
                return (data, AftTransferStatusCode.NotAValidTransferFunction);
            }

            data.TransferCode = (AftTransferCode)longPoll[TransferCodeOffset];
            if (data.TransferCode == AftTransferCode.CancelTransferRequest)
            {
                // no more data for a cancel, so exit early
                return (data, null);
            }

            data.TransactionIndex = longPoll[TransactionIndexOffset];
            if (data.TransferCode == AftTransferCode.InterrogationRequest ||
                data.TransferCode == AftTransferCode.InterrogationRequestStatusOnly)
            {
                // no more data for an interrogate, so exit early
                return (data, null);
            }

            data.TransferType = (AftTransferType)longPoll[TransferTypeOffset];
            var (cashableAmount, validCashable) = Utilities.FromBcdWithValidation(longPoll, CashableAmountOffset, SasConstants.Bcd10Digits);
            if (!validCashable)
            {
                Logger.Debug("AFT Transfer Cashable Amount is not a valid BCD number");
                return (data, AftTransferStatusCode.NotAValidTransferAmountOrExpirationDate);
            }

            data.CashableAmount = cashableAmount;
            var (restrictedAmount, validRestricted) = Utilities.FromBcdWithValidation(longPoll, RestrictedAmountOffset, SasConstants.Bcd10Digits);
            if (!validRestricted)
            {
                Logger.Debug("AFT Transfer Restricted Amount is not a valid BCD number");
                return (data, AftTransferStatusCode.NotAValidTransferAmountOrExpirationDate);
            }

            data.RestrictedAmount = restrictedAmount;
            var (nonRestrictedAmount, validNonRestricted) = Utilities.FromBcdWithValidation(longPoll, NonRestrictedAmountOffset, SasConstants.Bcd10Digits);
            if (!validNonRestricted)
            {
                Logger.Debug("AFT Transfer Non-Restricted Amount is not a valid BCD number");
                return (data, AftTransferStatusCode.NotAValidTransferAmountOrExpirationDate);
            }

            data.NonRestrictedAmount = nonRestrictedAmount;
            data.TransferFlags = (AftTransferFlags)longPoll[TransferFlagsOffset];
            data.AssetNumber = Utilities.FromBinary(longPoll, AssetNumberOffset, AssetNumberLength);
            Array.Copy(longPoll, RegistrationKeyOffset, data.RegistrationKey, 0, RegistrationKeySize);
            var transactionIdLength = longPoll[TransactionIdLengthOffset];
            if (transactionIdLength == 0)
            {
                Logger.Debug("Invalid transaction ID.  We must have at least one character for the transaction ID");
                return (data, AftTransferStatusCode.TransactionIdNotValid);
            }

            if (transactionIdLength + TransactionIdOffset + SasConstants.NumberOfCrcBytes + 1 > longPoll.Length)
            {
                Logger.Debug("Invalid transaction ID length");
                return (data, AftTransferStatusCode.TransactionIdNotValid);
            }

            data.TransactionId = Encoding.ASCII.GetString(longPoll, TransactionIdOffset, transactionIdLength);

            // for the rest of the data we can't use fixed offsets since the TransactionId is variable length
            var offset = (uint)(TransactionIdLengthOffset + transactionIdLength + 1);
            var (expiration, validExpiration) = Utilities.FromBcdWithValidation(longPoll, offset, SasConstants.Bcd8Digits);
            if (!validExpiration)
            {
                Logger.Debug("AFT Transfer Expiration is not a valid BCD number");
                return (data, AftTransferStatusCode.NotAValidTransferAmountOrExpirationDate);
            }

            data.Expiration = (uint)expiration;
            offset += SasConstants.Bcd8Digits;

            data.PoolId = (ushort)Utilities.FromBinary(longPoll, offset, PoolIdLength);
            offset += PoolIdLength;

            AftTransferStatusCode? errorCode;
            (data.ReceiptData, errorCode) = ParseReceiptData(longPoll, offset);
            return (data, errorCode);
        }

        private static (AftReceiptData receiptData, AftTransferStatusCode? errorCode) ParseReceiptData(byte[] longPoll, uint offset)
        {
            var receiptData = new AftReceiptData();
            var receiptDataEnd = longPoll[offset] + offset;
            Logger.Debug($"ParseReceiptData: receipt data ends at 0x{receiptDataEnd:X2} and offset is 0x{offset:X2}");
            if (receiptDataEnd + SasConstants.NumberOfCrcBytes > longPoll.Length)
            {
                Logger.Debug("Invalid receipt data length. Receipt data longer than message");
                return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
            }

            offset++;

            // parse any Receipt Data
            while (offset < receiptDataEnd)
            {
                // offset is pointing to the code entry, so at minimum we should have 1 more byte
                if (receiptDataEnd - offset < MinReceiptFieldLength - 1)
                {
                    Logger.Debug("Invalid receipt data length, not enough bytes left");
                    return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                }

                var valid = true;
                var code = (ReceiptField)longPoll[offset++];
                uint dataLength = longPoll[offset++];
                Logger.Debug($"ParseReceiptData: code is {code} 0x{(byte)code:X2} dataLength is 0x{dataLength:X2} offset is 0x{offset:X2}");

                if (dataLength + SasConstants.NumberOfCrcBytes > longPoll.Length)
                {
                    Logger.Debug($"Invalid receipt data length {dataLength}. Receipt data longer than remaining message");
                    return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                }

                switch (code)
                {
                    case ReceiptField.TransferSourceDestination:
                        if (dataLength > MaxTransferSourceDestinationLength)  // max 22 ASCII characters. See Table 8.3f in the Sas spec.
                        {
                            return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                        }

                        (receiptData.TransferSource, valid) = Utilities.FromBytesToString(longPoll, offset, dataLength);
                        Logger.Debug($"Ticket data TransferSourceDestination='{receiptData.TransferSource}'");
                        break;

                    case ReceiptField.DateAndTime:
                        if (dataLength != DateTimeLength)  // 7 BCD bytes. See Table 8.3f in the Sas spec.
                        {
                            return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                        }

                        ulong dateTime;
                        (dateTime, valid) = Utilities.FromBcdWithValidation(longPoll, offset, (int)dataLength);
                        receiptData.ReceiptTime = Utilities.FromSasDateTime(dateTime);
                        Logger.Debug($"Ticket data Receipt time='{receiptData.ReceiptTime}'");
                        break;

                    case ReceiptField.PatronName:
                        if (dataLength > MaxPatronNameLength)  // max 22 ASCII characters. See Table 8.3f in the Sas spec.
                        {
                            return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                        }

                        (receiptData.PatronName, valid) = Utilities.FromBytesToString(longPoll, offset, dataLength);
                        Logger.Debug($"Ticket data Patron Name='{receiptData.PatronName}'");
                        break;

                    case ReceiptField.PatronAccountNumber:
                        if (dataLength > MaxPatronAccountNumberLength)  // max 16 ASCII characters. See Table 8.3f in the Sas spec.
                        {
                            return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                        }

                        (receiptData.PatronAccount, valid) = Utilities.FromBytesToString(longPoll, offset, dataLength);
                        Logger.Debug($"Ticket data Patron Account='{receiptData.PatronAccount}'");
                        break;

                    case ReceiptField.AccountBalance:  // in cents. value BEFORE transaction
                        if (dataLength != SasConstants.Bcd10Digits)  // 5 BCD bytes. See Table 8.3f in the Sas spec.
                        {
                            return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                        }

                        // platform expects this in millicents, so convert it.
                        (receiptData.AccountBalance, valid) = Utilities.FromBcdWithValidation(longPoll, offset, (int)dataLength);
                        receiptData.AccountBalance *= CurrencyMultiplier;
                        Logger.Debug($"Ticket data Account Balance='{receiptData.AccountBalance}'");
                        break;

                    case ReceiptField.DebitCardNumber:
                        if (dataLength != SasConstants.Bcd4Digits)  // 2 BCD bytes. See Table 8.3f in the Sas spec.
                        {
                            return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                        }

                        ulong accountNumber;
                        (accountNumber, valid) = Utilities.FromBcdWithValidation(longPoll, offset, (int)dataLength);
                        receiptData.DebitCardNumber = "xxxxxxxxxxxx" + accountNumber.ToString("D4");
                        Logger.Debug($"Ticket data debit card last 4 digits='{receiptData.DebitCardNumber}'");
                        break;

                    case ReceiptField.TransactionFee: // in cents
                        if (dataLength != SasConstants.Bcd10Digits)  // 5 BCD bytes. See Table 8.3f in the Sas spec.
                        {
                            return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                        }

                        (receiptData.TransactionFee, valid) = Utilities.FromBcdWithValidation(longPoll, offset, (int)dataLength);
                        Logger.Debug($"Ticket data Transaction Fee='{receiptData.TransactionFee:D10}'");
                        break;

                    case ReceiptField.TotalDebitAmount: // in cents
                        if (dataLength != SasConstants.Bcd10Digits)  // 5 BCD bytes. See Table 8.3f in the Sas spec.
                        {
                            return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                        }

                        (receiptData.DebitAmount, valid) = Utilities.FromBcdWithValidation(longPoll, offset, (int)dataLength);
                        Logger.Debug($"Ticket data Debit Amount='{receiptData.DebitAmount:D10}'");
                        break;
                }

                offset += dataLength;
                if (!valid)
                {
                    return (receiptData, AftTransferStatusCode.NotAValidTransferFunction);
                }
            }

            return (receiptData, null);
        }

        private Collection<byte> GenerateLongPollResponse(IReadOnlyCollection<byte> command, AftResponseData responseData)
        {
            Logger.Debug($"generating response for {responseData.TransferStatus}");
            var response = command.Take(SasConstants.MinimumBytesForLongPoll).ToList();
            switch (responseData.TransferStatus)
            {
                case AftTransferStatusCode.UnsupportedTransferCode:
                case AftTransferStatusCode.TransferCanceledByHost:
                case AftTransferStatusCode.NotCompatibleWithCurrentTransfer:
                {
                    Logger.Debug("Unsupported transfer");
                    response.AddRange(GenerateShortVersionOfResponse(responseData));
                    break;
                }

                case AftTransferStatusCode.NoTransferInfoAvailable:
                {
                    const byte responseLength = 0x03;
                    response.AddRange(new List<byte>
                    {
                        responseLength,
                        responseData.TransactionIndex,
                        (byte)responseData.TransferStatus,
                        responseData.ReceiptStatus
                    });
                    break;
                }

                case AftTransferStatusCode.FullTransferSuccessful:
                case AftTransferStatusCode.PartialTransferSuccessful:
                {
                    Logger.Debug("Full or Partial transfer successful");
                    response.AddRange(GenerateLongVersionOfResponse(responseData));
                    break;
                }

                case AftTransferStatusCode.TransferPending:
                {
                    Logger.Debug("transfer pending successful");
                    response.AddRange(GenerateLongVersionOfResponse(responseData));
                    break;
                }

                case AftTransferStatusCode.AssetNumberZeroOrDoesNotMatch:
                case AftTransferStatusCode.ExpirationNotValidForTransferToTicket:
                case AftTransferStatusCode.GamingMachineNotLocked:
                case AftTransferStatusCode.GamingMachineUnableToPerformPartial:
                case AftTransferStatusCode.GamingMachineNotRegistered:
                case AftTransferStatusCode.GamingMachineUnableToPerformTransfer:
                case AftTransferStatusCode.InsufficientDataToPrintTransactionReceipt:
                case AftTransferStatusCode.NoGamingMachineDenominationSet:
                case AftTransferStatusCode.NoPosId:
                case AftTransferStatusCode.NoWonCreditsAvailableForCashOut:
                case AftTransferStatusCode.NotAValidTransferAmountOrExpirationDate:
                case AftTransferStatusCode.RegistrationKeyDoesNotMatch:
                case AftTransferStatusCode.NotAValidTransferFunction:
                case AftTransferStatusCode.TransactionIdNotUnique:
                case AftTransferStatusCode.TransactionIdNotValid:
                case AftTransferStatusCode.TransactionReceiptNotAllowedForTransferType:
                case AftTransferStatusCode.TransferAmountExceedsGameLimit:
                case AftTransferStatusCode.TransferAmountNotEvenMultiple:
                case AftTransferStatusCode.TransferToTicketDeviceNotAvailable:
                case AftTransferStatusCode.UnableToAcceptTransferDueToExistingRestrictedAmounts:
                case AftTransferStatusCode.UnableToPrintTransactionReceipt:
                case AftTransferStatusCode.UnexpectedError:
                {
                    Logger.Debug($"responding with {responseData.TransferStatus}");
                    response.AddRange(GenerateLongVersionOfResponse(responseData));
                    break;
                }

                default:
                {
                    Logger.Debug("default transfer status");
                    return NackLongPoll(command);
                }
            }

            return new Collection<byte>(response);
        }

        private static IEnumerable<byte> GenerateShortVersionOfResponse(AftResponseData responseData)
        {
            const byte responseLength = 0x02;
            return new List<byte>
            {
                responseLength, responseData.TransactionIndex, (byte)responseData.TransferStatus
            };
        }

        private static IEnumerable<byte> GenerateLongVersionOfResponse(AftResponseData responseData)
        {
            var response = new List<byte>();
            var transactionLength = responseData.TransactionId?.Length ?? 0;
            response.Add(responseData.TransactionIndex);
            response.Add((byte)responseData.TransferStatus);
            response.Add(responseData.ReceiptStatus);
            response.Add((byte)responseData.TransferType);
            response.AddRange(Utilities.ToBcd(responseData.CashableAmount, SasConstants.Bcd10Digits));
            response.AddRange(Utilities.ToBcd(responseData.RestrictedAmount, SasConstants.Bcd10Digits));
            response.AddRange(Utilities.ToBcd(responseData.NonRestrictedAmount, SasConstants.Bcd10Digits));
            response.Add((byte)responseData.TransferFlags);
            response.AddRange(Utilities.ToBinary(responseData.AssetNumber, AssetNumberLength));
            response.Add((byte)transactionLength);
            if (!string.IsNullOrEmpty(responseData.TransactionId))
            {
                response.AddRange(Encoding.ASCII.GetBytes(responseData.TransactionId));
            }

            var transferComplete = IsTransferComplete(responseData);

            // From SAS Spec pg 8-16 paragraph 6 - The date and time fields should be all zero while
            // the transfer is pending. Once the transfer is complete, whether successful or failed, the
            // date and time fields must indicate the time, according to the gaming machine clock, that
            // the transfer completed.
            response.AddRange(
                transferComplete ? Utilities.ToSasDateTime(responseData.TransactionDateTime) : new byte[DateTimeLength]);

            response.AddRange(Utilities.ToBcd(responseData.Expiration, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBinary(responseData.PoolId, PoolIdLength));

            // From Sas spec pg 8-17 paragraph 5 - While the transfer is pending, the cumulative amount
            // meters are reported with a size byte of zero. Once the transfer is complete, the
            // cumulative amount meters report the total meters for the transfer type just completed.
            var meterLength = transferComplete ? MeterDataLength : (byte)0;
            response.Add(meterLength);
            if (meterLength > 0)
            {
                response.AddRange(
                    Utilities.ToBcd((ulong)responseData.CumulativeCashableAmount, SasConstants.Bcd16Digits));
            }

            response.Add(meterLength);
            if (meterLength > 0)
            {
                response.AddRange(
                    Utilities.ToBcd((ulong)responseData.CumulativeRestrictedAmount, SasConstants.Bcd16Digits));
            }

            response.Add(meterLength);
            if (meterLength > 0)
            {
                response.AddRange(
                    Utilities.ToBcd((ulong)responseData.CumulativeNonRestrictedAmount, SasConstants.Bcd16Digits));
            }

            var responseCount = response.Count;
            response.Insert(0, (byte)responseCount);
            return response;
        }

        private static bool IsTransferComplete(AftResponseData responseData)
        {
            return responseData.TransferStatus != AftTransferStatusCode.TransferPending;
        }
    }
}