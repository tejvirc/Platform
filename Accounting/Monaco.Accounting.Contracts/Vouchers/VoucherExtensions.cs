namespace Aristocrat.Monaco.Accounting.Contracts.Vouchers
{
    using System.Text;
    using System.Text.RegularExpressions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Extension methods for an IVoucher.
    /// </summary>
    public static class VoucherExtensions
    {
        /// <summary>The length a barcode should be.</summary>
        private const int BarcodeLength = 18;

        /// <summary>
        ///     Gets masked validation Id
        /// </summary>
        /// <param name="validationId">Validation Id</param>
        /// <returns>Masked validation Id.</returns>
        public static string GetMaskedValidationId(string validationId)
        {
            var st = new StringBuilder(validationId);
            for (var i = 0; i < validationId.Length - 4; i++)
            {
                if (validationId[i] != '-')
                {
                    st[i] = 'x';
                }
            }

            return st.ToString();
        }

        /// <summary>Creates a formatted and padded validation string.</summary>
        /// <param name="barcode">The value to turn into a validation string.</param>
        /// <returns>The formatted validation string that masks with 'x' except last 4 digits.</returns>
        public static string GetValidationString(string barcode)
        {
            if (barcode == null)
            {
                return string.Empty;
            }

            // make sure the barcode is 18 characters
            var formattedValidation = barcode.PadLeft(BarcodeLength, '0');
            var config = ServiceManager.GetInstance().GetService<IOperatorMenuConfiguration>();
            var showVoucherId = config.GetSetting(
                OperatorMenuSetting.VoucherOutLogViewModel,
                OperatorMenuSetting.VoucherIdVisible,
                true);
            var maskVoucherId = config.GetSetting(
                OperatorMenuSetting.VoucherOutLogViewModel,
                OperatorMenuSetting.VoucherIdMask,
                true);

            if (showVoucherId)
            {
                return maskVoucherId ? GetMaskedValidationId(formattedValidation) : formattedValidation;
            }

            return string.Empty;
        }

        /// <summary>Return a prefix string for title of voucher/ticket.</summary>
        /// <param name="online">The value to find out if we need to append offline to title.</param>
        /// <returns>prefix OFFLINE if online is false and required in jurisdiction.</returns>
        public static string PrefixToTitle(bool online)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var notify = propertiesManager.GetValue(AccountingConstants.VoucherOfflineNotify, false);
            if (notify && !online)
            {
                return Localizer.For(CultureFor.Player).GetString(ResourceKeys.OfflinePrefix);
            }

            return string.Empty;
        }

        /// <summary>Creates a formatted and padded validation string.</summary>
        /// <param name="barcode">The value to turn into a validation string.</param>
        /// <returns>The formatted and padded validation string.</returns>
        public static string GetValidationStringWithHyphen(string barcode)
        {
            if (barcode == null)
            {
                return string.Empty;
            }

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var barcodeLength = (int)propertiesManager.GetProperty(
                AccountingConstants.TicketBarcodeLength,
                AccountingConstants.DefaultTicketBarcodeLength);

            // make sure the barcode is the correct number of characters
            var formattedValidation = barcode.PadLeft(barcodeLength, '0');

            // Insert hyphens to separate the validation
            return Regex.Replace(formattedValidation, ".{1,4}", "$0-", RegexOptions.RightToLeft).TrimEnd('-');
        }

        /// <summary>Creates a message that tells the last voucher state.</summary>
        /// <param name="transaction">The voucher transaction</param>
        /// <returns>The details of the state.</returns>
        public static string GetStatusText(VoucherInTransaction transaction)
        {
            return GetStatusText(transaction.State);
        }

        /// <summary>Creates a message that tells the last voucher state.</summary>
        /// <param name="state">The voucher state</param>
        /// <returns>The details of the state.</returns>
        public static string GetStatusText(VoucherState state)
        {
            switch (state)
            {
                case VoucherState.Issued:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IssuedText);

                case VoucherState.Pending:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PendingText);

                case VoucherState.Redeemed:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AcceptedText);

                case VoucherState.Rejected:
                default:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RejectedText);
            }
        }

        /// <summary>Creates a message that tells the voucher rejection reason.</summary>
        /// <param name="exceptionCode">The voucher rejection exception code.</param>
        /// <returns>The details of the exception.</returns>
        public static string GetDetailsMessage(int exceptionCode)
        {
            switch ((VoucherInExceptionCode)exceptionCode)
            {
                case VoucherInExceptionCode.CreditLimitExceeded:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CreditLimitExceeded);

                case VoucherInExceptionCode.ZeroAmount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ZeroAmount);

                case VoucherInExceptionCode.LaundryLimitExceeded:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SessionLimitReached);

                case VoucherInExceptionCode.VoucherInLimitExceeded:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherLimitExceeded);

                case VoucherInExceptionCode.InvalidTicket:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidVoucher);

                case VoucherInExceptionCode.ValidationFailed:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ValidationFailed);

                case VoucherInExceptionCode.AlreadyReedemed:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AlreadyRedeemed);

                case VoucherInExceptionCode.Expired:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Expired);

                case VoucherInExceptionCode.TimedOut:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TimedOut);

                case VoucherInExceptionCode.InProcessAtAnotherLocation:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InProcessAtAnotherLocation);

                case VoucherInExceptionCode.IncorrectPlayer:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IncorrectPlayer);

                case VoucherInExceptionCode.PrinterError:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrinterError);

                case VoucherInExceptionCode.AnotherTransferInProgress:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AnotherTransferInProgress);

                case VoucherInExceptionCode.CannotMixNonCashableExpired:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CannotMixNonCashableExpired);

                case VoucherInExceptionCode.CannotMixNonCashableCredits:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CannotMixNonCashableCredits);

                case VoucherInExceptionCode.PlayerCardMustBeInserted:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PlayerCardMustBeInserted);

                case VoucherInExceptionCode.NoteAcceptorFailure:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptorFailure);

                case VoucherInExceptionCode.PowerFailure:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PowerFailure);

                case VoucherInExceptionCode.None:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AcceptedText);

                case VoucherInExceptionCode.Other:
                default:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ValidationFailed);
            }
        }
    }
}