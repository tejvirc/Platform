namespace Aristocrat.Monaco.Accounting.Handpay
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Application.Contracts;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Handpay;
    using Contracts.Vouchers;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;

    /// <summary>
    ///     This class provides methods to fill in ticket information
    /// </summary>
    public static class HandpayTicketsCreator
    {
        private const string NoValidationNumber = "----------------------";

        // Date Time format string
        private static readonly DateTimeFormatInfo DateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

        /// <summary>
        ///     Creates the canceled credits ticket with a barcode.
        /// </summary>
        /// <param name="transaction">The canceled credits transaction.</param>
        /// <returns>A canceled credits (handpay) ticket with a barcode</returns>
        public static Ticket CreateCanceledCreditsTicket(HandpayTransaction transaction)
        {
            var ticket = new Ticket();

            using (var scope = new CultureScope(CultureFor.PlayerTicket))
            {
                // load the existing values from the property provider
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

                var dollarAmount =
                    VoucherTicketsCreator.GetDollarAmount(transaction.CashableAmount + transaction.PromoAmount);

                ticket["ticket type"] = "handpay offset";
                ticket["title"] = VoucherExtensions.PrefixToTitle(transaction.HostOnline) + scope.GetString(ResourceKeys.HandpayTicket);

                ticket["serial id"] = propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);

                ticket["machine id"] = propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0)
                    .ToString(CultureInfo.CurrentCulture);
                ticket["machine id 2"] = $"{scope.GetString(ResourceKeys.MachineNumber)}: {ticket["machine id"]}";
                ticket["machine id 3"] = $"{scope.GetString(ResourceKeys.MachineNumber2)} # {ticket["machine id"]}";

                var visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride);
                ticket["zone"] = (string)propertiesManager.GetProperty(visible ? ApplicationConstants.Zone : "", string.Empty);

                visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPageBankOverride);
                ticket["bank"] = (string)propertiesManager.GetProperty(visible ? ApplicationConstants.Bank : "", string.Empty);

                visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPagePositionOverride);
                ticket["position"] = (string)propertiesManager.GetProperty(visible ? ApplicationConstants.Position : "", string.Empty);

                var timeService = ServiceManager.GetInstance().GetService<ITime>();
                var localTime = timeService.GetLocationTime(transaction.TransactionDateTime);
                ticket["datetime"] = localTime.ToString("G", CultureInfo.CurrentCulture);

                var localeDateFormat = propertiesManager.GetValue(
                    ApplicationConstants.LocalizationPlayerTicketDateFormat,
                    ApplicationConstants.DefaultPlayerTicketDateFormat);
                ticket["locale datetime"] = localTime.ToString($"{localeDateFormat} {ApplicationConstants.DefaultTimeFormat}");

                ticket["establishment name"] =
                    (string)propertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty);
                ticket["location address"] =
                    (string)propertiesManager.GetProperty(PropertyKey.TicketTextLine3, string.Empty);
                ticket["location name"] =
                    (string)propertiesManager.GetProperty(PropertyKey.TicketTextLine2, string.Empty);
                ticket["validation label"] = scope.GetString(ResourceKeys.ValidationTitle);
                ticket["validation label 2"] = scope.GetString(ResourceKeys.ValidationTitle2);

                AddDollarAmount(ref ticket, dollarAmount);
                ticket["validation number"] = NoValidationNumber;
                ticket["validation number alt"] = VoucherExtensions.GetValidationStringWithHyphen(transaction.Barcode);
                ticket["sequence number"] = transaction.LogSequence.ToString(CultureInfo.InvariantCulture);

                var machineId = propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
                var assetId = machineId == 0 ? string.Empty : machineId.ToString(CultureInfo.CurrentCulture);
                ticket["asset id 2"] = $"{scope.GetString(ResourceKeys.Asset)}# {assetId}";
            }

            return ticket;
        }

        /// <summary>
        ///     Creates a reprint of a canceled credits ticket with a barcode.
        /// </summary>
        /// <param name="transaction">The canceled credits transaction being printed</param>
        /// <returns>A ticket object with settings for a canceled credits reprint ticket filled in</returns>
        public static Ticket CreateCanceledCreditsReprintTicket(HandpayTransaction transaction)
        {
            var ticket = CreateCanceledCreditsTicket(transaction);
            ticket["title"] =
                VoucherExtensions.PrefixToTitle(transaction.HostOnline) + Localizer.For(CultureFor.PlayerTicket).FormatString(ResourceKeys.CancelCreditsHandpayReprintTicket, ticket["title"]);
            return ticket;
        }

        /// <summary>
        ///     Creates the jackpot handpay ticket with a barcode.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns>A jackpot handpay ticket with a barcode</returns>
        public static Ticket CreateGameWinTicket(HandpayTransaction transaction)
        {
            // load the existing values from the property provider
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var ticket = CreateCanceledCreditsReceiptTicket(transaction);

            if (transaction.TicketData != null)
            {
                return ticket;
            }

            var titleJackpotReceipt = propertiesManager.GetValue(AccountingConstants.TitleJackpotReceipt, string.Empty);
            if (string.IsNullOrEmpty(titleJackpotReceipt))
            {
                titleJackpotReceipt = Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.JackpotHandpayTicket);
            }

            ticket["ticket type"] = transaction.Validated ? "jackpot" : "jackpot no barcode";
            ticket["title"] = VoucherExtensions.PrefixToTitle(transaction.HostOnline) + titleJackpotReceipt;

            ticket["validation number"] = VoucherExtensions.GetValidationStringWithHyphen(transaction.Barcode);
            ticket["barcode"] = transaction.Barcode?.Replace("-", string.Empty);

            return ticket;
        }

        /// <summary>
        ///     Creates a reprint of a jackpot handpay ticket with a barcode.
        /// </summary>
        /// <param name="transaction">The jackpot handpay transaction being printed</param>
        /// <returns>A ticket object with settings for a jackpot handpay reprint ticket filled in</returns>
        public static Ticket CreateGameWinReprintTicket(HandpayTransaction transaction)
        {
            var ticket = CreateGameWinTicket(transaction);

            if (transaction.TicketData != null)
            {
                ticket["title"] = $"{ticket["title"]} {Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.ReprintLabel)}";
            }
            else
            {
                ticket["title"] = Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.JackpotHandpayReprintTicket);
            }

            return ticket;
        }

        /// <summary>
        ///     Creates the bonus handpay ticket with a barcode.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns>A jackpot handpay ticket with a barcode</returns>
        public static Ticket CreateBonusPayTicket(HandpayTransaction transaction)
        {
            var ticket = CreateGameWinTicket(transaction);

            var dollarAmount = VoucherTicketsCreator.GetDollarAmount(transaction.CashableAmount) +
                               VoucherTicketsCreator.GetDollarAmount(transaction.PromoAmount);
            AddDollarAmount(ref ticket, dollarAmount);
            ticket["value in words 2"] = ticket["value in words 2"] + " " +
                                         Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.NoCashValue); 

            return ticket;
        }

        /// <summary>
        ///     Creates a reprint of a bonus handpay ticket with a barcode.
        /// </summary>
        /// <param name="transaction">The bonus handpay transaction being printed</param>
        /// <returns>A ticket object with settings for a jackpot handpay reprint ticket filled in</returns>
        public static Ticket CreateBonusPayReprintTicket(HandpayTransaction transaction)
        {
            var ticket = CreateBonusPayTicket(transaction);
            ticket["title"] = Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.JackpotHandpayReprintTicket);

            return ticket;
        }

        /// <summary>
        ///     Create a canceled credits receipt ticket
        /// </summary>
        /// <param name="transaction">The canceled credits transaction being printed</param>
        /// <returns>A ticket object with settings for a canceled credits receipt ticket filled in</returns>
        public static Ticket CreateCanceledCreditsReceiptTicket(HandpayTransaction transaction)
        {
            // load the existing values from the property provider
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var ticket = new Ticket();

            if (transaction.TicketData != null)
            {
                foreach (var data in transaction.TicketData)
                {
                    ticket[data.Key] = data.Value;
                }

                return ticket;
            }

            using (var scope = new CultureScope(CultureFor.PlayerTicket))
            {
                var dollarAmount =
                    VoucherTicketsCreator.GetDollarAmount(transaction.CashableAmount + transaction.PromoAmount);

                ticket["ticket type"] = transaction.Validated ? "handpay receipt" : "handpay receipt no barcode";

                var titleCancelReceipt = propertiesManager.GetValue(AccountingConstants.TitleCancelReceipt, string.Empty);
                if (string.IsNullOrEmpty(titleCancelReceipt))
                {
                    titleCancelReceipt = scope.GetString(ResourceKeys.HandpayReceipt);
                }
                ticket["title"] = VoucherExtensions.PrefixToTitle(transaction.HostOnline) + titleCancelReceipt;

                ticket["serial id"] = propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
                ticket["terminal number"] = scope.GetString(ResourceKeys.TerminalNumber) + ": " +
                                            propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0)
                                                .ToString(CultureInfo.CurrentCulture);
                var machineId = propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
                var assetId = machineId == 0 ? string.Empty : machineId.ToString(CultureInfo.CurrentCulture);
                ticket["machine id"] = machineId.ToString(CultureInfo.CurrentCulture);
                ticket["machine id 2"] = $"{scope.GetString(ResourceKeys.MachineNumber)}: {ticket["machine id"]}";
                ticket["machine id 3"] = $"{scope.GetString(ResourceKeys.MachineNumber2)}# {ticket["machine id"]}";
                ticket["machine id 4"] = machineId.ToString("D6");

                ticket["asset id 2"] = $"{scope.GetString(ResourceKeys.Asset)}# {assetId}";

                var visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride);
                ticket["zone"] = (string)propertiesManager.GetProperty(visible ? ApplicationConstants.Zone : "", string.Empty);

                visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPageBankOverride);
                ticket["bank"] = (string)propertiesManager.GetProperty(visible ? ApplicationConstants.Bank : "", string.Empty);

                visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPagePositionOverride);
                ticket["position"] = (string)propertiesManager.GetProperty(visible ? ApplicationConstants.Position : "", string.Empty);

                var timeService = ServiceManager.GetInstance().GetService<ITime>();
                var localTime = timeService.GetLocationTime(transaction.TransactionDateTime);
                ticket["datetime"] = localTime.ToString(
                    DateTimeFormat.ShortDatePattern + "    " + ApplicationConstants.DefaultTimeFormat,
                    CultureInfo.CurrentCulture);

                var localeDateFormat = propertiesManager.GetValue(
                    ApplicationConstants.LocalizationPlayerTicketDateFormat,
                    ApplicationConstants.DefaultPlayerTicketDateFormat);
                ticket["locale datetime"] = localTime.ToString($"{localeDateFormat} {ApplicationConstants.DefaultTimeFormat}");

                var dateFormat = localTime.ToString("MMM d", CultureInfo.CurrentCulture);
                if (!CultureInfo.CurrentCulture.Name.Equals("en-US"))
                {
                    dateFormat = localTime.ToString("d MMM", CultureInfo.CurrentCulture);
                    var period = dateFormat.IndexOf('.');
                    if (period > 0)
                    {
                        dateFormat = dateFormat.Remove(period, 1);
                    }
                }

                ticket["alternate date time"] = scope.GetString(ResourceKeys.Date) + ": " + dateFormat
                                                + localTime.ToString(", yyyy", CultureInfo.CurrentCulture)
                                                + " " + scope.GetString(ResourceKeys.TimeLabel) + ": " + localTime.ToString(
                                                    "H:mm:ss",
                                                    CultureInfo.CurrentCulture);

                ticket["establishment name"] =
                    (string)propertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty);
                ticket["location address"] =
                    (string)propertiesManager.GetProperty(PropertyKey.TicketTextLine3, string.Empty);
                ticket["location name"] =
                    (string)propertiesManager.GetProperty(PropertyKey.TicketTextLine2, string.Empty);
                ticket["full address"] =
                    (string)propertiesManager.GetProperty(PropertyKey.TicketTextLine2, string.Empty) +
                    " " +
                    (string)propertiesManager.GetProperty(PropertyKey.TicketTextLine3, string.Empty);

                ticket["validation label"] = scope.GetString(ResourceKeys.ValidationTitle);
                ticket["validation label 2"] = scope.GetString(ResourceKeys.ValidationTitle2);

                AddDollarAmount(ref ticket, dollarAmount);
                ticket["value in words 2"] = ticket["value in words 2"] + " " + scope.GetString(ResourceKeys.NoCashValue);

                var validationNumber = VoucherExtensions.GetValidationStringWithHyphen(transaction.Barcode);
                ticket["validation number"] = validationNumber;
                ticket["validation number alt"] = validationNumber;

                ticket["redemption text"] = transaction.Expiration == VoucherTicketsCreator.MaxExpirationDays ||
                                            transaction.Expiration <= 0
                    ? scope.GetString(ResourceKeys.NeverExpires)
                    : VoucherTicketsCreator.DetermineRedemptionText(
                        transaction.TransactionDateTime.AddDays(transaction.Expiration),
                        transaction.TransactionDateTime, localeDateFormat);

                ticket["redemption text 2"] = transaction.Expiration == VoucherTicketsCreator.MaxExpirationDays ||
                                            transaction.Expiration <= 0
                    ? scope.GetString(ResourceKeys.NeverExpires)
                    : VoucherTicketsCreator.DetermineRedemptionText(
                        transaction.TransactionDateTime.AddDays(transaction.Expiration),
                        transaction.TransactionDateTime, localeDateFormat, false);

                ticket["barcode"] = transaction.Barcode?.Replace("-", string.Empty);
                ticket["barcode2"] = transaction.Barcode?.Replace("-", string.Empty);

                var sequenceNumber = transaction.ReceiptSequence != 0
                    ? transaction.ReceiptSequence.ToString().PadLeft(4, '0')
                    : scope.GetString(ResourceKeys.StatusError);

                if (VoucherTicketsCreator.ConvertExpirationDate(transaction.Expiration) is DateTime convertExpirationDate)
                {
                    ticket["expiry date"] = VoucherTicketsCreator.GetExpiryDate(convertExpirationDate, ExpiryDateText.Version1, localeDateFormat);
                    ticket["expiry date 2"] = VoucherTicketsCreator.GetExpiryDate(convertExpirationDate, ExpiryDateText.Version2, localeDateFormat);
                    ticket["expiry date 3"] = VoucherTicketsCreator.GetExpiryDate(convertExpirationDate, ExpiryDateText.Version3, localeDateFormat);
                    ticket["expiry date 4"] = VoucherTicketsCreator.GetExpiryDate(convertExpirationDate, ExpiryDateText.Version4, localeDateFormat);
                }
                else
                {
                    ticket["expiry date"] = VoucherTicketsCreator.GetExpiryDate(transaction.Expiration, localTime, ExpiryDateText.Version1, localeDateFormat);
                    ticket["expiry date 2"] = VoucherTicketsCreator.GetExpiryDate(transaction.Expiration, localTime, ExpiryDateText.Version2, localeDateFormat);
                    ticket["expiry date 3"] = VoucherTicketsCreator.GetExpiryDate(transaction.Expiration, localTime, ExpiryDateText.Version3, localeDateFormat);
                    ticket["expiry date 4"] = VoucherTicketsCreator.GetExpiryDate(transaction.Expiration, localTime, ExpiryDateText.Version4, localeDateFormat);
                }

                ticket["vlt sequence number"] = scope.GetString(ResourceKeys.TicketNumber) + " : " + sequenceNumber;
                ticket["vlt sequence number alt"] = scope.GetString(ResourceKeys.TicketSequenceNo) + ": " + sequenceNumber;
                ticket["ticket number 2"] = scope.GetString(ResourceKeys.TicketNumber) + " : " + sequenceNumber;
                ticket["ticket number 3"] = scope.GetString(ResourceKeys.Ticket) + "# " + sequenceNumber;
                ticket["ticket number 4"] = scope.GetString(ResourceKeys.Ticket).ToUpper() + "# " + sequenceNumber;

                ticket["alternate sequence number"] = scope.GetString(ResourceKeys.ReceiptSequenceNumber) + ": " + sequenceNumber;
                ticket["alternate sequence number 2"] = scope.GetString(ResourceKeys.VoucherText) + "# " + sequenceNumber;

                ticket["serial2"] = "Asset: " + propertiesManager.GetValue(
                    ApplicationConstants.SerialNumber,
                    string.Empty);

                ticket["serial3"] =
                    $"{scope.GetString(ResourceKeys.MachineNumber)}:{ticket["serial id"]} {scope.GetString(ResourceKeys.AssetNumber)}:{ticket["machine id"]}";

             
            }

            return ticket;
        }

        /// <summary>
        ///     Create a canceled credit receipt reprint ticket
        /// </summary>
        /// <param name="transaction">The canceled credits transaction being printed</param>
        /// <returns>A ticket object with settings for a canceled credits receipt reprint ticket filled in</returns>
        public static Ticket CreateCanceledCreditsReceiptReprintTicket(HandpayTransaction transaction)
        {
            var ticket = CreateCanceledCreditsReceiptTicket(transaction);
            ticket["title"] = 
                Localizer.For(CultureFor.PlayerTicket).FormatString(ResourceKeys.CancelCreditsHandpayReprintTicket, ticket["title"]);
            return ticket;
        }

        private static void AddDollarAmount(ref Ticket ticket, decimal dollarAmount)
        {
            var dollarAmountString = dollarAmount.FormattedCurrencyStringForVouchers();

            ticket["value"] = Regex.Replace(dollarAmountString, @"[^\u0000-\u007F]+", " "); // Replace any unprintable characters with a space.
            ticket["value 2"] = dollarAmountString;

            var lineLength = ServiceManager.GetInstance().GetService<IPrinter>().GetCharactersPerLine(true, 0);
            var words = TicketCurrencyExtensions.ConvertCurrencyToWrappedWords(dollarAmount, lineLength);
            ticket["value in words 1"] = words[0];
            ticket["value in words 2"] = words[1];

            ticket["value in words with newline"] = words[0];
            if (!string.IsNullOrEmpty(words[0]) && !string.IsNullOrEmpty(words[1]))
            {
                ticket["value in words with newline"] += "\r\n" + words[1];
            }
        }
    }
}
