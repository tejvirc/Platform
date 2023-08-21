// ReSharper disable once CheckNamespace
namespace Aristocrat.Monaco.Mgam.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Data.Models;

    /// <summary>
    ///     Voucher method extensions.
    /// </summary>
    public static class VoucherExtensions
    {
        private const string TicketType = "ticket type";
        private const string ValidationNumber = "validation number";
        private const string EstablishmentName = "establishment name";
        private const string LocationName = "location name";
        private const string Title = "title";
        private const string Barcode = "barcode";
        private const string TicketNumber2 = "ticket number 2";
        private const string PaperStatus = "paper status";
        private const string Datetime = "datetime";
        private const string License = "license";
        private const string Online = "online";
        private const string Value = "value";
        private const string ValueInWords1 = "value in words 1";
        private const string ValidationNumberAlt = "validation number alt";
        private const string Coupon = "coupon";
        private const string ExpiryDate2 = "expiry date 2";
        private const string Asset = "asset";
        private const string Cashout = "cashout";
        private const string DataUnavailable = "Data Unavailable";
        private const string OfflineCredit = "OFFLINE CREDIT";
        private const string OfflineGamePlay = "OFFLINE PRIZE";
        private const string Offline = "OFFLINE";
        private const string Cash = "cash";
        private const string Cashable = "Cashable";
        private const string CashableNonCashableCoupon = "Cashable & Non-Cashable Coupon";
        private const string NonCashableCoupon = "Non-Cashable Coupon";

        /// <summary>
        ///     Validate and set missing voucher data.
        /// </summary>
        /// <param name="voucher">Voucher data.</param>
        public static void Validate(this Voucher voucher)
        {
            voucher.OfflineReason = VoucherOutOfflineReason.None;

            foreach (var property in voucher.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    var value = (string)property.GetValue(voucher);
                    if (string.IsNullOrEmpty(value))
                    {
                        property.SetValue(voucher, DataUnavailable);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if this voucher is ready for printing.
        /// </summary>
        /// <returns>True if there is a barcode set.</returns>
        public static bool CanPrint(this Voucher voucher)
        {
            return !(string.IsNullOrEmpty(voucher.VoucherBarcode) ||
                     voucher.VoucherBarcode.Equals(DataUnavailable));
        }

        /// <summary>
        ///     Converts Voucher to voucher out transaction.
        /// </summary>
        /// <param name="voucher">Voucher.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="type">Account type.</param>
        /// <param name="reason">Reason.</param>
        /// <returns>Voucher Out Transaction.</returns>
        public static VoucherOutTransaction ToTransaction(
            this Voucher voucher,
            long amount,
            AccountType type,
            TransferOutReason reason)
        {
            if (!voucher.CanPrint())
            {
                return null;
            }

            var transaction = new VoucherOutTransaction(
                1,
                DateTime.UtcNow,
                amount,
                type,
                voucher.VoucherBarcode,
                45,
                string.Empty)
            {
                Reason = reason, HostOnline = true, HostAcknowledged = true, TicketData = voucher.VoucherTicketData()
            };


            return transaction;
        }

        /// <summary>
        ///     Gets the voucher ticket data.
        /// </summary>
        /// <param name="voucher"><see cref="Voucher"/>.</param>
        /// <returns>Ticket data.</returns>
        public static Dictionary<string, string> VoucherTicketData(this Voucher voucher)
        {
            var ticketData =
                new Dictionary<string, string>
                {
                    [TicketType] = Cashout,
                    [ValidationNumber] = GetValidationStringWithHyphen(voucher.VoucherBarcode),
                    [EstablishmentName] = voucher.CasinoName,
                    [LocationName] = voucher.CasinoAddress,
                    [Title] = voucher.VoucherType.ToUpper(),
                    [Barcode] = voucher.VoucherBarcode,
                    [TicketNumber2] = voucher.DeviceId,
                    [PaperStatus] = voucher.Expiration,
                    [Datetime] = voucher.Date,
                    [License] = voucher.Time,
                    [Online] = $"{GetOfflineText(voucher)}",
                    [Value] = voucher.TotalAmount,
                    [ValueInWords1] = voucher.AmountLongForm,
                    [ValidationNumberAlt] = GetValidationStringWithHyphen(voucher.VoucherBarcode)
                };


            if (!voucher.VoucherType.ToLower().Contains(Coupon))
            {
                return ticketData;
            }

            ticketData[ExpiryDate2] = voucher.CashAmount.ToLower().StartsWith(Cash)
                ? voucher.CashAmount
                : $"Cash: {voucher.CashAmount}";
            ticketData[Asset] = voucher.CouponAmount.ToLower().StartsWith(Coupon)
                ? voucher.CouponAmount
                : $"Coupon: {voucher.CouponAmount}";

            return ticketData;
        }

        /// <summary>
        ///     Gets Log Display Type
        /// </summary>
        /// <param name="credits"><see cref="VoucherAmount"/></param>
        /// <returns>String with transaction log display type.</returns>
        public static string GetLogDisplayType(VoucherAmount credits)
        {
            if(credits.CashAmount > 0 && credits.PromoAmount == 0)
            {
                return Cashable;
            }

            if(credits.CashAmount > 0 && credits.PromoAmount > 0)
            {
                return CashableNonCashableCoupon;
            }

            if(credits.PromoAmount > 0)
            {
                return NonCashableCoupon;
            }

            return string.Empty;
        }

        private static string GetOfflineText(Voucher voucher)
        {
            switch (voucher.OfflineReason)
            {
                case VoucherOutOfflineReason.RequestPlay:
                    return OfflineGamePlay;
                case VoucherOutOfflineReason.Credit:
                    return OfflineCredit;
                case VoucherOutOfflineReason.Cashout:
                    return Offline;
                default:
                    return string.Empty;
            }
        }
  
        private static string GetValidationStringWithHyphen(string barcode)
        {
            if (barcode.Contains("-"))
            {
                return barcode;
            }

            // Insert hyphens to separate the validation
            return Regex.Replace( barcode, ".{1,4}", "$0-", RegexOptions.None).TrimEnd('-');
        }
    }
}