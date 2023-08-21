namespace Aristocrat.Monaco.G2S.Handlers.Voucher
{
    using System;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Accounting.Contracts.Vouchers;
    using Aristocrat.G2S.Protocol.v21;
    using Services;
    using ClientExtensions = Aristocrat.G2S.Client.VoucherExtensions;

    /// <summary>
    ///     Extension methods for an Voucher.
    /// </summary>
    public static class VoucherExtensions
    {
        /// <summary>
        ///     Find transaction record.
        /// </summary>
        /// <typeparam name="T">ITransaction</typeparam>
        /// <param name="transactionHistory">ITransactionHistory instance.</param>
        /// <param name="transactionId">Transaction Id.</param>
        /// <returns></returns>
        public static T FindTransaction<T>(ITransactionHistory transactionHistory, long transactionId) where T : ITransaction
        {
            return transactionHistory.RecallTransactions<T>().FirstOrDefault(t => t.TransactionId == transactionId);
        }

        /// <summary>
        ///     Construct issueVoucher command.
        /// </summary>
        /// <param name="transaction">Voucher out transaction.</param>
        /// <param name="referenceProvider">The transaction reference provider</param>
        /// <returns></returns>
        public static issueVoucher GetIssueVoucher(VoucherOutTransaction transaction, ITransactionReferenceProvider referenceProvider)
        {
            return new issueVoucher
            {
                transactionId = transaction.TransactionId,
                validationId = transaction.Barcode, 
                voucherAmt = transaction.Amount,
                creditType = ToCreditType(transaction.TypeOfAccount),
                voucherSource = t_voucherSources.G2S_egmIssued,
                largeWin = transaction.Reason == TransferOutReason.LargeWin,
                voucherSequence = transaction.VoucherSequence,
                transferAmt = transaction.Amount,
                transferDateTime = transaction.TransactionDateTime,
                egmAction = t_egmVoucherActions.G2S_issued,
                egmException = 0,
                voucherSourceRef = referenceProvider.GetReferences<voucherSourceRef>(transaction).ToArray()
            };
        }

        /// <summary>
        ///     Constructs commitVoucher
        /// </summary>
        /// <param name="transactionId">Transaction Id.</param>
        /// <param name="validationId">Validation Id.</param>
        /// <param name="result">authorizeVoucher result.</param>
        /// <returns>CommitVoucher command.</returns>
        public static commitVoucher GetCommitVoucherRejected(long transactionId, string validationId, authorizeVoucher result)
        {
            return GetCommitVoucher(
                transactionId,
                validationId,
                expireDateTime: DateTime.UtcNow,
                transferDateTime: DateTime.UtcNow,
                egmAction: t_egmVoucherActions.G2S_rejected,
                egmException: (int)(result == null ?
                    EgmVoucherException.RedemptionTimedOut :
                    result.hostException == (int)HostVoucherExceptions.InProcessAtAnotherLocation ?
                        EgmVoucherException.ExceptionFromHostStacked : EgmVoucherException.ExceptionFromHostRejected));
        }

        /// <summary>
        ///     Constructs commitVoucher command.
        /// </summary>
        /// <param name="voucherInTransaction">Voucher In transaction.</param>
        /// <returns></returns>
        public static commitVoucher GetCommitVoucher(VoucherInTransaction voucherInTransaction)
        {
            return GetCommitVoucher(
                voucherInTransaction.TransactionId,
                voucherInTransaction.Barcode,
                creditType: (t_creditTypes)voucherInTransaction.TypeOfAccount,
                voucherAmt: voucherInTransaction.Amount,
                transferDateTime: voucherInTransaction.TransactionDateTime,
                egmAction: voucherInTransaction.State == VoucherState.Redeemed ? t_egmVoucherActions.G2S_redeemed : t_egmVoucherActions.G2S_rejected,
                voucherSource: t_voucherSources.G2S_egmIssued,
                largeWin: false,
                expireCredits: false,
                transferAmt: voucherInTransaction.State == VoucherState.Redeemed ? voucherInTransaction.Amount : 0,
                egmException: voucherInTransaction.Exception);
        }

        /// <summary>
        ///     Constructs a voucher log.
        /// </summary>
        /// <param name="voucherIn">Log type voucher in.</param>
        /// <param name="logSequence">Log sequence.</param>
        /// <param name="deviceId">Device Id.</param>
        /// <param name="transactionId">Transaction Id.</param>
        /// <param name="idReaderType">Idf reader type.</param>
        /// <param name="idNumber">Id number.</param>
        /// <param name="playerId">Player Id.</param>
        /// <param name="validationId">Validation Id.</param>
        /// <param name="amount">Amount.</param>
        /// <returns>Voucher log.</returns>
        public static voucherLog GetVoucherLog(bool voucherIn, long logSequence, int deviceId, long transactionId, string idReaderType, string idNumber, string playerId, string validationId, long amount)
        {
            return new voucherLog
            {
                logSequence = logSequence,
                deviceId = deviceId,
                transactionId = transactionId,
                voucherState = voucherIn ? t_voucherStates.G2S_redeemSent : t_voucherStates.G2S_issueSent,
                voucherAction = voucherIn ? t_voucherActions.G2S_redeem : t_voucherActions.G2S_issue,
                voucherSequence = 0,
                idReaderType = idReaderType,
                idNumber = idNumber,
                playerId = playerId,
                validationId = ClientExtensions.GetMaskedValidationId(validationId),
                voucherAmt = amount,
                creditType = t_creditTypes.G2S_cashable,
                voucherSource = t_voucherSources.G2S_egmIssued,
                expireCredits = false,
                expireDateTime = DateTime.Parse("2000-01-01T00:00:00.000-00:00"),
                expireDays = 0,
                hostAction = t_hostVoucherActions.G2S_egmAction,
                hostException = 0,
                transferAmt = 0,
                transferDateTime = DateTime.UtcNow,
                egmAction = t_egmVoucherActions.G2S_pending,
                egmException = 0
            };
        }

        /// <summary>
        ///     Constructs voucherLog command.
        /// </summary>
        /// <param name="voucherInTransaction">Voucher in transaction.</param>
        /// <param name="device">Device class</param>
        /// <returns>voucherLog</returns>
        public static voucherLog GetVoucherLog(VoucherInTransaction voucherInTransaction, string device)
        {
            return new voucherLog
            {
                logSequence = voucherInTransaction.LogSequence,
                deviceId = voucherInTransaction.DeviceId,
                transactionId = voucherInTransaction.TransactionId,
                voucherState = voucherInTransaction.CommitAcknowledged ? t_voucherStates.G2S_commitAcked : t_voucherStates.G2S_commitSent,
                voucherAction =  t_voucherActions.G2S_redeem,
                validationId = ClientExtensions.GetMaskedValidationId(voucherInTransaction.Barcode),
                voucherAmt = voucherInTransaction.Amount,
                creditType = ToCreditType(voucherInTransaction.TypeOfAccount),
                voucherSource = t_voucherSources.G2S_egmIssued,
                voucherSequence = voucherInTransaction.VoucherSequence,
                hostAction = voucherInTransaction.Amount > 0
                    ? t_hostVoucherActions.G2S_stack
                    : t_hostVoucherActions.G2S_reject,
                transferAmt = voucherInTransaction.Amount,
                transferDateTime = voucherInTransaction.TransactionDateTime,
                egmAction =
                    voucherInTransaction.Amount > 0
                        ? t_egmVoucherActions.G2S_redeemed
                        : t_egmVoucherActions.G2S_rejected, //// TODO: verify
                egmException = 0 //// TODO: verify
            };
        }

        /// <summary>
        ///     Converts a <see cref="VoucherOutTransaction"/> to a <see cref="voucherLog"/>
        /// </summary>
        /// <param name="this">The voucher transaction</param>
        /// <param name="referenceProvider">A <see cref="ITransactionReferenceProvider"/></param>
        /// <returns>a G2S voucher log</returns>
        public static voucherLog ToLog(this VoucherOutTransaction @this, ITransactionReferenceProvider referenceProvider)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (referenceProvider == null)
            {
                throw new ArgumentNullException(nameof(referenceProvider));
            }

            return new voucherLog
            {
                logSequence = @this.LogSequence,
                deviceId = @this.DeviceId,
                transactionId = @this.TransactionId,
                voucherState = @this.HostAcknowledged ? t_voucherStates.G2S_issueAcked : t_voucherStates.G2S_issueSent,
                voucherAction = t_voucherActions.G2S_issue,
                validationId = ClientExtensions.GetMaskedValidationId(@this.Barcode),
                voucherAmt = @this.Amount,
                creditType = ToCreditType(@this.TypeOfAccount),
                voucherSource = t_voucherSources.G2S_egmIssued,
                largeWin = @this.Reason == TransferOutReason.LargeWin,
                voucherSequence = @this.VoucherSequence,
                hostAction = t_hostVoucherActions.G2S_egmAction,
                transferAmt = @this.Amount,
                transferDateTime = @this.TransactionDateTime,
                egmAction = t_egmVoucherActions.G2S_issued,
                egmException = 0,
                voucherSourceRef = referenceProvider.GetReferences<voucherSourceRef>(@this).ToArray()
            };
        }

        private static t_creditTypes ToCreditType(this AccountType accountType)
        {
            switch (accountType)
            {
                case AccountType.Cashable:
                    return t_creditTypes.G2S_cashable;
                case AccountType.Promo:
                    return t_creditTypes.G2S_promo;
                case AccountType.NonCash:
                    return t_creditTypes.G2S_nonCash;
                default:
                    throw new ArgumentOutOfRangeException(nameof(accountType), accountType, null);
            }
        }

        private static commitVoucher GetCommitVoucher(
            long transactionId = 0,
            string validationId = null,
            long voucherAmt = 0,
            t_creditTypes creditType = t_creditTypes.G2S_cashable,
            t_voucherSources voucherSource = t_voucherSources.G2S_egmIssued,
            bool largeWin = false,
            int voucherSequence = 0,
            bool expireCredits = false,
            DateTime expireDateTime = default(DateTime),
            long transferAmt = 0,
            DateTime transferDateTime = default(DateTime),
            t_egmVoucherActions egmAction = t_egmVoucherActions.G2S_issued,
            int egmException = 0)
        {
            return new commitVoucher
            {
                transactionId = transactionId,
                validationId = validationId,
                creditType = creditType,
                voucherAmt = voucherAmt,
                transferDateTime = transferDateTime == default(DateTime) ? DateTime.Parse("2000-01-01T00:00:00.000-00:00") : transferDateTime,
                egmAction = egmAction,
                voucherSource = voucherSource,
                largeWin = largeWin,
                expireCredits = expireCredits,
                transferAmt = transferAmt,
                egmException = egmException,
                voucherSequence = voucherSequence,
                expireDateTime = expireDateTime == default(DateTime) ? DateTime.Parse("2000-01-01T00:00:00.000-00:00") : expireDateTime
            };
        }

        public static VoucherInExceptionCode ToVoucherInExceptionCode(this HostVoucherExceptions @this)
        {
            switch (@this)
            {
                case HostVoucherExceptions.Authorized:
                    return VoucherInExceptionCode.None;

                case HostVoucherExceptions.InProcessAtAnotherLocation:
                    return VoucherInExceptionCode.InProcessAtAnotherLocation;

                case HostVoucherExceptions.AlreadyRedeemed:
                    return VoucherInExceptionCode.AlreadyReedemed;

                case HostVoucherExceptions.Expired:
                    return VoucherInExceptionCode.Expired;

                case HostVoucherExceptions.NotFound:
                    return VoucherInExceptionCode.ValidationFailed;

                case HostVoucherExceptions.CannotBeRedeemedAtLocation:
                    return VoucherInExceptionCode.InvalidTicket;

                case HostVoucherExceptions.IncorrectPlayer:
                    return VoucherInExceptionCode.IncorrectPlayer;

                case HostVoucherExceptions.Denied:
                    return VoucherInExceptionCode.Other;

                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static VoucherInExceptionCode ToVoucherInExceptionCode(this EgmVoucherException @this)
        {
            switch (@this)
            {
                case EgmVoucherException.Success:
                    return VoucherInExceptionCode.None;

                case EgmVoucherException.PrinterPresentationError:
                    return VoucherInExceptionCode.PrinterError;

                case EgmVoucherException.ErrorFromHost:
                    return VoucherInExceptionCode.ValidationFailed;

                case EgmVoucherException.ExceptionFromHostRejected:
                    return VoucherInExceptionCode.ValidationFailed;

                case EgmVoucherException.ExceptionFromHostStacked:
                    return VoucherInExceptionCode.ValidationFailed;

                case EgmVoucherException.RedemptionTimedOut:
                    return VoucherInExceptionCode.TimedOut;

                case EgmVoucherException.ExceedsLimit:
                    return VoucherInExceptionCode.CreditLimitExceeded;

                case EgmVoucherException.GameStateChanged:
                    return VoucherInExceptionCode.Other;

                case EgmVoucherException.AnotherTransferInprogress:
                    return VoucherInExceptionCode.AnotherTransferInProgress;

                case EgmVoucherException.CannotMixNonCashableExpired:
                    return VoucherInExceptionCode.CannotMixNonCashableExpired;

                case EgmVoucherException.CannotMixNonCashableCredits:
                    return VoucherInExceptionCode.CannotMixNonCashableCredits;

                case EgmVoucherException.Rejected:
                    return VoucherInExceptionCode.Other;

                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public enum HostVoucherExceptions
        {
            Authorized,
            InProcessAtAnotherLocation,
            AlreadyRedeemed,
            Expired,
            NotFound,
            CannotBeRedeemedAtLocation,
            IncorrectPlayer,
            Denied = 99
        }

        public enum EgmVoucherException
        {
            Success,
            PrinterPresentationError,
            ErrorFromHost,
            ExceptionFromHostRejected,
            ExceptionFromHostStacked,
            RedemptionTimedOut,
            ExceedsLimit,
            GameStateChanged,
            AnotherTransferInprogress,
            CannotMixNonCashableExpired,
            CannotMixNonCashableCredits,
            Rejected = 99
        }
    }
}
