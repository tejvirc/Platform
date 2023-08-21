namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     The secure enhanced validation data required by the host
    /// </summary>
    [Serializable]
    public class EnhancedValidationData : ISerializable
    {
        private const int ValidationIdLength = 2;
        private long _hostSequence;
        private long _index;

        /// <summary>
        ///     Response to send to the host
        /// </summary>
        public SendEnhancedValidationInformationResponse Response { get; private set; }

        /// <summary>
        ///     Whether the transaction has been acknowledged by the host
        /// </summary>
        public bool Acknowledged { get; private set; }

        /// <summary>
        ///     The TransactionId from the history log
        /// </summary>
        public long TransactionId { get; private set; }

        /// <summary>
        ///     The HoseSequence from the history log
        /// </summary>
        public long HostSequence
        {
            get => _hostSequence;

            private set
            {
                _hostSequence = value;
                _index = GetIndex(value);
            }
        }

        /// <summary>
        ///     Indicates whether the data needs to have its processing handlers filled.
        ///     Used in deserialization.
        /// </summary>
        public bool NeedsProcessingHandlers { get; }

        private static readonly IDictionary<AccountType, TicketValidationType> VoucherOutValidationTypes =
            new Dictionary<AccountType, TicketValidationType>
            {
                { AccountType.Cashable, TicketValidationType.CashableTicketFromCashOutOrWin },
                { AccountType.NonCash, TicketValidationType.RestrictedPromotionalTicketFromCashOut },
                { AccountType.Promo, TicketValidationType.CashableTicketFromCashOutOrWin }
            };

        /// <summary>
        ///     Creates a default instance of the <see cref="EnhancedValidationData"/>
        /// </summary>
        public EnhancedValidationData()
        {
            Response = FailedResponse();
            Acknowledged = true;
            TransactionId = -1;
            HostSequence = 0;
        }

        /// <summary>
        ///     Creates an instance of the <see cref="EnhancedValidationData"/> based on an ITransaction
        /// </summary>
        /// <param name="transaction">An <see cref="ITransaction"/> from which to generate the data</param>
        public EnhancedValidationData(ITransaction transaction) : this()
        {
            if (transaction == null)
            {
                return;
            }

            HostSequence = (transaction as IAcknowledgeableTransaction)?.HostSequence ?? 0;

            if (HostSequence == 0)
            {
                return;
            }

            Response = GetSendEnhancedValidationInformationResponseFromTransaction(transaction);
            Acknowledged = false;
            TransactionId = transaction.TransactionId;
        }

        /// <summary>
        ///     Creates an instance of the <see cref="EnhancedValidationData"/> from serialization info
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected EnhancedValidationData(SerializationInfo info, StreamingContext context)
        {
            HostSequence = info.GetInt64("HostSequence");
            Acknowledged = info.GetBoolean("Acknowledged");
            TransactionId = info.GetInt64("TransactionId");
            Response = new SendEnhancedValidationInformationResponse
            {
                ValidationType = info.GetInt32("Response.ValidationType"),
                Index = info.GetInt64("Response.Index"),
                ValidationDate = info.GetDateTime("Response.ValidationDate"),
                ValidationNumber = info.GetUInt64("Response.ValidationNumber"),
                Amount = info.GetInt64("Response.Amount"),
                TicketNumber = info.GetInt64("Response.TicketNumber"),
                ValidationSystemId = info.GetUInt64("Response.ValidationSystemId"),
                ExpirationDate = info.GetUInt32("Response.ExpirationDate"),
                PoolId = info.GetUInt16("Response.PoolId"),
                Successful = info.GetBoolean("Response.Successful"),
                Handlers = new HostAcknowledgementHandler()
            };

            NeedsProcessingHandlers = info.GetBoolean("NeedsProcessingHandlers");
        }

        /// <summary>
        ///     Fills a serialization info with <see cref="EnhancedValidationData"/> items
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("HostSequence", HostSequence);
            info.AddValue("Acknowledged", Acknowledged);
            info.AddValue("TransactionId", TransactionId);
            info.AddValue("Response.ValidationType", Response.ValidationType);
            info.AddValue("Response.Index", Response.Index);
            info.AddValue("Response.ValidationDate", Response.ValidationDate);
            info.AddValue("Response.ValidationNumber", Response.ValidationNumber);
            info.AddValue("Response.Amount", Response.Amount);
            info.AddValue("Response.TicketNumber", Response.TicketNumber);
            info.AddValue("Response.ValidationSystemId", Response.ValidationSystemId);
            info.AddValue("Response.ExpirationDate", Response.ExpirationDate);
            info.AddValue("Response.PoolId", Response.PoolId);
            info.AddValue("Response.Successful", Response.Successful);
            info.AddValue("NeedsProcessingHandlers", Response.Handlers.ImpliedAckHandler != null);
        }

        /// <summary>
        ///     Gets whether the data is valid
        /// </summary>
        public bool Valid => TransactionId >= 0;

        /// <summary>
        ///     Gets a response that does not correspond to a record
        /// </summary>
        /// <returns>The response</returns>
        public static SendEnhancedValidationInformationResponse FailedResponse()
        {
            return new SendEnhancedValidationInformationResponse { Successful = false, Handlers = new HostAcknowledgementHandler() };
        }

        /// <summary>
        ///     Gets whether this record is at the specified index
        /// </summary>
        /// <param name="index">The index.  This is normally the LP4D function code 0x01-0x1F</param>
        /// <returns>True if this data is at the specified index</returns>
        public bool IsAtIndex(long index)
        {
            if (HostSequence < 1)
            {
                return false;
            }

            return index == _index;
        }

        /// <summary>
        ///     Acknowledges the data
        /// </summary>
        public void Acknowledge()
        {
            Acknowledged = true;
            Response.Handlers = new HostAcknowledgementHandler();
        }

        private SendEnhancedValidationInformationResponse GetSendEnhancedValidationInformationResponseFromTransaction(ITransaction transaction)
        {
            return transaction switch
            {
                VoucherOutTransaction voucherOutTransaction => GetSendEnhancedValidationInformationResponse(
                    voucherOutTransaction),
                HandpayTransaction handpayTransaction => GetSendEnhancedValidationInformationResponse(
                    handpayTransaction),
                _ => FailedResponse()
            };
        }

        private SendEnhancedValidationInformationResponse GetSendEnhancedValidationInformationResponse(HandpayTransaction readResults)
        {
            var (validationSystemId, validationNumber) = GetValidationInformation(readResults.Barcode);

            return new SendEnhancedValidationInformationResponse
            {
                Successful = true,
                ValidationSystemId = validationSystemId,
                TicketNumber = readResults.Printed ? readResults.ReceiptSequence : ushort.MaxValue,
                ValidationType = (int)GetHandpayValidationType(readResults),
                ExpirationDate = (uint)(readResults.Printed ? GetExpirationDate(readResults.Expiration) : 0),
                ValidationDate = readResults.TransactionDateTime.ToLocalTime(), // We store in UTC but want local time here
                Amount = (readResults.CashableAmount + readResults.NonCashAmount + readResults.PromoAmount).MillicentsToCents(),
                Index = _index,
                ValidationNumber = validationNumber,
                Handlers = new HostAcknowledgementHandler()
            };
        }

        private SendEnhancedValidationInformationResponse GetSendEnhancedValidationInformationResponse(
            VoucherOutTransaction readResults)
        {
            var (validationSystemId, validationNumber) = GetValidationInformation(readResults.Barcode);

            return new SendEnhancedValidationInformationResponse
            {
                Successful = true,
                ValidationSystemId = validationSystemId,
                TicketNumber = readResults.VoucherSequence,
                ValidationType = (int)VoucherOutValidationTypes[readResults.TypeOfAccount],
                PoolId = (ushort)readResults.ReferenceId,
                ValidationDate = readResults.TransactionDateTime.ToLocalTime(), // We store in UTC but want local time here
                ExpirationDate = (uint)GetExpirationDate(readResults.Expiration),
                Amount = readResults.Amount.MillicentsToCents(),
                Index = _index,
                ValidationNumber = validationNumber,
                Handlers = new HostAcknowledgementHandler()
            };
        }

        private static (ulong, ulong) GetValidationInformation(string barcode)
        {
            ulong validationSystemId = 0;
            ulong validationNumber = 0;
            if (!string.IsNullOrEmpty(barcode) &&
                barcode.Length > ValidationIdLength &&
                barcode.All(char.IsDigit))
            {
                validationSystemId = ulong.Parse(
                    barcode.Substring(0, ValidationIdLength),
                    NumberStyles.Integer);
                validationNumber = ulong.Parse(barcode.Substring(ValidationIdLength), NumberStyles.Integer);
            }

            return (validationSystemId, validationNumber);
        }

        private static int GetExpirationDate(int expirationDate)
        {
            // SAS wants zero as never expires as 9999 and not 0
            return expirationDate == 0 ? SasConstants.MaxTicketExpirationDays : expirationDate;
        }

        private long GetIndex(long hostSequence)
        {
            // SAS wants 1 based indexes only from 1 - 31.
            return ((hostSequence - 1) % (SasConstants.MaxValidationIndex)) + 1;
        }

        private static TicketValidationType GetHandpayValidationType(HandpayTransaction readResults)
        {
            if (readResults.HandpayType == HandpayType.CancelCredit)
            {
                return readResults.Printed
                    ? TicketValidationType.HandPayFromCashOutReceiptPrinted
                    : TicketValidationType.HandPayFromCashOutNoReceipt;
            }

            return readResults.Printed
                ? TicketValidationType.HandPayFromWinReceiptPrinted
                : TicketValidationType.HandPayFromWinNoReceipt;
        }
    }
}