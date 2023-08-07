namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Application.Contracts;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using log4net;
    using Vouchers;

    /// <summary>
    ///     Defines choices for the expiration text formats on tickets
    /// </summary>
    public enum ExpiryDateText
    {
        /// <summary>
        ///     Begins with: Expiry Date :
        /// </summary>
        Version1,

        /// <summary>
        ///     Begins with: Ticket Void After, or is: TICKET NEVER EXPIRES
        /// </summary>
        Version2,

        /// <summary>
        ///     Is a date formatted based on localization settings, or is Never Expires
        /// </summary>
        Version3,

        /// <summary>
        ///     Begins with: Expires: (localized date format), or is Never Expires
        /// </summary>
        Version4
    };

    /// <summary>
    ///     This class provides methods to fill in ticket information
    /// </summary>
    public static class VoucherTicketsCreator
    {
        /// <summary>The max expiration days for a ticket</summary>
        public const int MaxExpirationDays = 9999;

        private static readonly Regex ReplaceAsciiRegex = new Regex(@"[^\u0000-\u007F]+");

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly DateTimeFormatInfo DateTimeFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;

        /// <summary>
        ///     Create a cashout reprint ticket
        /// </summary>
        /// <param name="transaction">The transaction for the reprint ticket</param>
        /// <returns>A ticket object with settings for a cashout reprint ticket filled in</returns>
        public static Ticket CreateCashOutReprintTicket(VoucherOutTransaction transaction)
        {
            using (var scope = new CultureScope(CultureFor.PlayerTicket))
            {
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

                var titleOverride = propertiesManager.GetValue(
                    AccountingConstants.ReprintLoggedVoucherTitleOverride,
                    false);

                var ticket = CreateCashOutTicket(transaction);

                if (titleOverride)
                {
                    propertiesManager.SetProperty(
                        AccountingConstants.TicketTitleCashReprint,
                         scope.GetString(ResourceKeys.CashoutTicketReprint));

                    ticket["title"] = propertiesManager.GetValue(
                        AccountingConstants.TicketTitleCashReprint,
                         scope.GetString(ResourceKeys.CashoutTicketReprint));
                }

                var allowCashWinTicket = propertiesManager.GetValue(AccountingConstants.AllowCashWinTicket, false);
                if (allowCashWinTicket && transaction.Reason == TransferOut.TransferOutReason.CashWin)
                {
                    ticket["title"] = titleOverride
                        ? scope.GetString(ResourceKeys.CashWinTicketReprint)
                        : scope.GetString(ResourceKeys.CashWinTicket);
                }

                if (transaction.TicketData != null)
                {
                    ticket["title"] = $"{ticket["title"]} {scope.GetString(ResourceKeys.ReprintLabel)}";
                }

                return ticket;
            }
        }

        /// <summary>
        ///     Create a restricted cashout ticket
        /// </summary>
        /// <param name="transaction">The transaction for the cashout ticket</param>
        /// <returns>A ticket object with settings for a cashout ticket filled in</returns>
        public static Ticket CreateCashOutRestrictedTicket(VoucherOutTransaction transaction)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var ticket = CreateCashOutTicket(transaction, false, false, true);

            ticket["title"] = propertiesManager.GetValue(
                AccountingConstants.TicketTitleNonCash,
                string.Empty);

            if (string.IsNullOrEmpty(ticket["title"]))
            {
                ticket["title"] = Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PlayableOnly);
            }

            return ticket;
        }

        /// <summary>
        ///     Create a restricted cashout reprint ticket
        /// </summary>
        /// <param name="transaction">The transaction for the reprint ticket</param>
        /// <returns>A ticket object with settings for a cashout reprint ticket filled in</returns>
        public static Ticket CreateCashOutRestrictedReprintTicket(VoucherOutTransaction transaction)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var ticket = CreateCashOutTicket(transaction, false, false, true);

            var titleOverride = propertiesManager.GetValue(
                AccountingConstants.ReprintLoggedVoucherTitleOverride,
                false);

            if (titleOverride)
            {
                propertiesManager.SetProperty(
                    AccountingConstants.TicketTitleNonCashReprint,
                    Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PlayableOnlyReprint));

                ticket["title"] = propertiesManager.GetValue(
                    AccountingConstants.TicketTitleNonCashReprint,
                    Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PlayableOnlyReprint));
            }
            else
            {
                ticket["title"] = propertiesManager.GetValue(
                    AccountingConstants.TicketTitleNonCash,
                    string.Empty);

                if (string.IsNullOrEmpty(ticket["title"]))
                {
                    ticket["title"] = Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PlayableOnly);
                }
            }

            return ticket;
        }

        /// <summary>Determines the correct redemption text based on the number of days until expiration</summary>
        /// <remarks>Changes here must also be made to the same resource files in Canceled Credits Ticket Creator</remarks>
        /// <param name="expirationDate">The date of the expiration.</param>
        /// <param name="localTime">The local time.</param>
        /// <param name="localeDateFormat">The date format for the current locale</param>
        /// <param name="prefixExpirationText">If true, it will print VOID AFTER X DAYS, otherwise it just print X DAYS</param>
        /// <param name="preserveDate">If true, preserve the date rather than translate to number of days</param>
        /// <returns>The redemption text</returns>
        public static string DetermineRedemptionText(DateTime expirationDate, DateTime localTime, string localeDateFormat, bool prefixExpirationText = true, bool preserveDate = false)
        {
            string redemptionText;
            var daysTillExpiration = (expirationDate - localTime).Days;

            using (var scope = new CultureScope(CultureFor.PlayerTicket))
            {
                if (daysTillExpiration <= 0)
                {
                    Logger.Debug($"DetermineRedemptionText deciphered '{daysTillExpiration}' as never expires");

                    redemptionText = scope.GetString(ResourceKeys.NeverExpires);
                }
                else if (preserveDate)
                {
                    Logger.Debug($"DetermineRedemptionText deciphered '{daysTillExpiration}' as a date");

                    redemptionText =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} {1}",
                            scope.GetString(ResourceKeys.VoucherTicketsCreatorRedemptionString1),
                            expirationDate.ToString(localeDateFormat));
                }
                else
                {
                    Logger.Debug($"DetermineRedemptionText deciphered '{daysTillExpiration}' as number of days");

                    redemptionText = prefixExpirationText ?
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} {1} {2}",
                            scope.GetString(ResourceKeys.VoucherTicketsCreatorRedemptionString1),
                            daysTillExpiration.ToString(CultureInfo.CurrentCulture),
                            scope.GetString(ResourceKeys.VoucherTicketsCreatorRedemptionString2)) :
                        // Some printer templates will truncate the redemption text if it is too long, so remove the first part in this case
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} {1}",
                            daysTillExpiration.ToString(CultureInfo.CurrentCulture),
                            scope.GetString(ResourceKeys.VoucherTicketsCreatorRedemptionString2));
                }
            }

            Logger.Debug($"DetermineRedemptionText returned '{redemptionText}'");
            return redemptionText;
        }

        /// <summary>
        ///     Create a cashout ticket
        /// </summary>
        /// <param name="transaction">The transaction for the cashout ticket</param>
        /// <param name="largeWin">Indicates a large win if true.</param>
        /// <param name="voidTicket">Whether the ticket should be printed as void demo ticket</param>
        /// <param name="restrictedTicket">Whether the ticket should be printed as a restricted ticket</param>
        /// <returns>A ticket object with settings for a cashout ticket filled in</returns>
        public static Ticket CreateCashOutTicket(VoucherOutTransaction transaction, bool largeWin = false, bool voidTicket = false, bool restrictedTicket = false)
        {
            Logger.DebugFormat("CreateTicket(transaction = '{0}')", transaction);

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
            if (printer == null)
            {
                Logger.Warn("Printer service unavailable when creating ticket...");
            }

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

                var dollarAmount = GetDollarAmount(transaction.Amount);

                ticket["title alt"] = string.Empty;

                if (largeWin)
                {
                    ticket["ticket type"] = "jackpot";
                    ticket["title jackpot top"] = VoucherExtensions.PrefixToTitle(transaction.HostOnline) + scope.GetString(ResourceKeys.JackpotTicketTitle);
                    ticket["title jackpot bottom"] = scope.GetString(ResourceKeys.JackpotTicketTitle);
                }
                else
                {
                    ticket["ticket type"] = "cashout";

                    var ticketTitleCash = voidTicket
                        ? scope.GetString(ResourceKeys.VoidDemoTicket)
                        : propertiesManager.GetValue(AccountingConstants.TicketTitleCash, scope.GetString(ResourceKeys.CashoutTicket));

                    ticket["title"] = VoucherExtensions.PrefixToTitle(transaction.HostOnline) + ticketTitleCash;
                    ticket["title 1"] = ticketTitleCash;
                    ticket["title localized"] = scope.GetString(ResourceKeys.CashoutTicket);
                }

                ticket["serial id"] = propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
                ticket["serial id alt"] = scope.GetString(ResourceKeys.SerialNo) + " " + ticket["serial id"];

                var machineId = propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
                var assetId = machineId == 0 ? string.Empty : machineId.ToString(CultureInfo.CurrentCulture);

                ticket["terminal number"] = scope.GetString(ResourceKeys.TerminalNumber) + ": " + assetId;
                ticket["machine id"] = assetId;
                ticket["machine id 2"] = $"{scope.GetString(ResourceKeys.MachineNumber)}: {ticket["machine id"]}";
                ticket["machine id 3"] = $"{scope.GetString(ResourceKeys.MachineNumber2)}# {ticket["machine id"]}";
                // Zero padded string of 6 characters
                ticket["machine id 4"] = machineId.ToString("D6");
                ticket["asset id 2"] = $"{scope.GetString(ResourceKeys.Asset)}# {assetId}";

                var visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride);
                ticket["zone"] = propertiesManager.GetValue(visible ? ApplicationConstants.Zone : "", string.Empty);

                visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPageBankOverride);
                ticket["bank"] = propertiesManager.GetValue(visible ? ApplicationConstants.Bank : "", string.Empty);

                visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPagePositionOverride);
                ticket["position"] = propertiesManager.GetValue(visible ? ApplicationConstants.Position : "", string.Empty);

                var timeService = ServiceManager.GetInstance().GetService<ITime>();
                var localTime = timeService.GetLocationTime(transaction.TransactionDateTime);
                ticket["datetime"] = localTime.ToString(DateTimeFormatInfo.ShortDatePattern) + " " +
                                     localTime.ToString(ApplicationConstants.DefaultTimeFormat);

                var localeDateFormat = propertiesManager.GetValue(
                    ApplicationConstants.LocalizationPlayerTicketDateFormat,
                    ApplicationConstants.DefaultPlayerTicketDateFormat);
                ticket["locale datetime"] = localTime.ToString($"{localeDateFormat} {ApplicationConstants.DefaultTimeFormat}");

                ticket["date"] = scope.GetString(ResourceKeys.Date) + ": " + localTime.ToString(DateTimeFormatInfo.ShortDatePattern);
                ticket["time"] = scope.GetString(ResourceKeys.TimeLabel) + ": " + localTime.ToString(ApplicationConstants.DefaultTimeFormat);

                var dateFormat = scope.GetString(ResourceKeys.Date) + ": " + localTime.ToString(DateTimeFormatInfo.MonthDayPattern);

                if (!CultureInfo.CurrentCulture.Name.Equals("en-US"))
                {
                    dateFormat = scope.GetString(ResourceKeys.Date) + ": " +
                                 localTime.ToString("d MMM", CultureInfo.CurrentCulture);
                }

                ticket["alternate date time"] = dateFormat + ", " +
                                                localTime.Year + " " + ticket["time"];

                var license = propertiesManager.GetValue(
                    ApplicationConstants.Zone,
                    scope.GetString(ResourceKeys.DataUnavailable));

                ticket["license"] = $"{scope.GetString(ResourceKeys.RetailerNumber)}: {license}"; // Resources.License
                ticket["license alt"] = $"{license}";

                ticket["online"] = string.IsNullOrEmpty(transaction.ManualVerification)
                    ? scope.GetString(ResourceKeys.Online)
                    : scope.GetString(ResourceKeys.Offline);

                ticket["establishment name"] = propertiesManager.GetValue(PropertyKey.TicketTextLine1, string.Empty);

                var dollarAmountString = dollarAmount.FormattedCurrencyStringForVouchers();
                // Replace any unprintable characters.
                ticket["value"] = ReplaceAsciiCharacters(dollarAmountString);
                ticket["value 2"] = dollarAmountString;
                ticket["value 3"] = scope.GetString(ResourceKeys.CashAmount) + " " + ticket["value"];

                var lineLength = printer?.GetCharactersPerLine(true, 0) ?? 0;
                var words = TicketCurrencyExtensions.ConvertCurrencyToWrappedWords(
                    Math.Round(dollarAmount, 2, MidpointRounding.AwayFromZero),
                    lineLength);

                ticket["value in wrapped words 1"] = words?[0];
                ticket["value in wrapped words 2"] = words?[1];

                ticket["value in words with newline"] = words?[0];
                if (!string.IsNullOrEmpty(words?[0]) && !string.IsNullOrEmpty(words[1]))
                {
                    ticket["value in words with newline"] += "\r\n" + words[1];
                }

                ticket["value in words 1"] = TicketCurrencyExtensions.ConvertCurrencyToWords(
                    Math.Round(dollarAmount, 2, MidpointRounding.AwayFromZero));
                ticket["value in words 2"] = TicketCurrencyExtensions.ConvertCurrencyToWords(
                    Math.Round(dollarAmount, 2, MidpointRounding.AwayFromZero),
                    false);

                ticket["validation label"] = scope.GetString(ResourceKeys.ValidationTitle);
                ticket["validation label 2"] = scope.GetString(ResourceKeys.ValidationTitle2);

                if (ConvertExpirationDate(transaction.Expiration) is DateTime convertExpirationDate)
                {
                    ticket["redemption text"] = DetermineRedemptionText(convertExpirationDate, localTime, localeDateFormat, true, restrictedTicket);
                    ticket["redemption text 2"] = DetermineRedemptionText(convertExpirationDate, localTime, localeDateFormat, false);

                    ticket["expiry date"] = GetExpiryDate(convertExpirationDate, ExpiryDateText.Version1, localeDateFormat);
                    ticket["expiry date 2"] = GetExpiryDate(convertExpirationDate, ExpiryDateText.Version2, localeDateFormat);
                    ticket["expiry date 3"] = GetExpiryDate(convertExpirationDate, ExpiryDateText.Version3, localeDateFormat);
                    ticket["expiry date 4"] = GetExpiryDate(convertExpirationDate, ExpiryDateText.Version4, localeDateFormat);
                }
                else
                {
                    var expiration = transaction.Expiration == MaxExpirationDays
                        ? DateTime.MinValue
                        : localTime.AddDays(transaction.Expiration);

                    ticket["redemption text"] = DetermineRedemptionText(expiration, localTime, localeDateFormat, true, restrictedTicket);
                    ticket["redemption text 2"] = DetermineRedemptionText(expiration, localTime, localeDateFormat, false);

                    ticket["expiry date"] = GetExpiryDate(transaction.Expiration, localTime, ExpiryDateText.Version1, localeDateFormat);
                    ticket["expiry date 2"] = GetExpiryDate(transaction.Expiration, localTime, ExpiryDateText.Version2, localeDateFormat);
                    ticket["expiry date 3"] = GetExpiryDate(transaction.Expiration, localTime, ExpiryDateText.Version3, localeDateFormat);
                    ticket["expiry date 4"] = GetExpiryDate(transaction.Expiration, localTime, ExpiryDateText.Version4, localeDateFormat);
                }

                ticket["location address"] = propertiesManager.GetValue(PropertyKey.TicketTextLine3, string.Empty);
                ticket["location name"] = propertiesManager.GetValue(PropertyKey.TicketTextLine2, string.Empty);

                ticket["full address"] = $"{propertiesManager.GetValue(PropertyKey.TicketTextLine2, string.Empty)} {propertiesManager.GetValue(PropertyKey.TicketTextLine3, string.Empty)}".TrimEnd(' ');

                ticket["sequence number"] = transaction.LogSequence.ToString(CultureInfo.InvariantCulture);

                var validationNumber = VoucherExtensions.GetValidationStringWithHyphen(transaction.Barcode);
                ticket["validation number"] = validationNumber;
                ticket["validation number alt"] = validationNumber;
                //ticket["validation number masked"] = VoucherExtensions.GetMaskedValidationId(validationNumber);
                ticket["barcode"] = validationNumber.Replace("-", string.Empty);

                var paperLow = printer?.PaperState != PaperStates.Full;

                ticket["paper status"] =
                    scope.GetString(ResourceKeys.PaperLevelText) + ": " +
                    (paperLow ? scope.GetString(ResourceKeys.StatusLow) : scope.GetString(ResourceKeys.StatusOk));

                ticket["paper level"] =
                    scope.GetString(ResourceKeys.PaperLevelText) + ": " +
                    (paperLow ? scope.GetString(ResourceKeys.StatusLow) : scope.GetString(ResourceKeys.StatusGood));

                var sequenceNumber = voidTicket || transaction.VoucherSequence != 0
                    ? transaction.VoucherSequence.ToString().PadLeft(4, '0')
                    : scope.GetString(ResourceKeys.StatusError);

                ticket["vlt sequence number"] =
                    scope.GetString(ResourceKeys.SequenceNumber) + ": " + sequenceNumber;
                ticket["vlt sequence number alt"] = scope.GetString(ResourceKeys.SequenceNumber) + ": " + sequenceNumber;
                ticket["ticket number 2"] = scope.GetString(ResourceKeys.SequenceNumber) + " : " + sequenceNumber;
                ticket["ticket number 3"] = scope.GetString(ResourceKeys.Ticket) + "# " + sequenceNumber;
                ticket["ticket number 4"] = scope.GetString(ResourceKeys.Ticket).ToUpper() + "# " + sequenceNumber;
                ticket["alternate sequence number"] = scope.GetString(ResourceKeys.SequenceNumber) + ": " + sequenceNumber;
                ticket["alternate sequence number 2"] = scope.GetString(ResourceKeys.VoucherText) + "# " + sequenceNumber;
                ticket["ticket number"] = transaction.VoucherSequence.ToString("D7");

                ticket["version"] = propertiesManager.GetValue(
                    KernelConstants.SystemVersion,
                    scope.GetString(ResourceKeys.NotSet));

                ticket["os version"] = ServiceManager.GetInstance().TryGetService<IOSService>()?.OsImageVersion.ToString() ?? scope.GetString(ResourceKeys.DataUnavailable);

                ticket["mac"] = MacAddressWithColon(NetworkInterfaceInfo.DefaultPhysicalAddress);

                // TODO: until the template supports it as three individual fields/regions, provide as a single field
                ticket["serial version mac"] =
                    $"{scope.GetString(ResourceKeys.AssetNumber)}:{ticket["machine id"]} {scope.GetString(ResourceKeys.Version)}:{ticket["version"]} {scope.GetString(ResourceKeys.Mac)}: {ticket["mac"]}     ";

                ticket["serial osversion mac"] =
                    $"{scope.GetString(ResourceKeys.AssetNumber)}:{ticket["machine id"]} {scope.GetString(ResourceKeys.Version)}:{ticket["os version"]} {scope.GetString(ResourceKeys.Mac)}: {ticket["mac"]}     ";

                ticket["machine version"] =
                    $"{scope.GetString(ResourceKeys.AssetNumber)}:{ticket["machine id"]} {scope.GetString(ResourceKeys.Version)}:{ticket["version"]}     ";

                ticket["asset"] =
                    $"{scope.GetString(ResourceKeys.MachineNumber)}:{ticket["serial id"]} {scope.GetString(ResourceKeys.AssetNumber)}:{ticket["machine id"]}";

                // Multiply the dollar amount by 100 to get value in cents.
                var vltCreditsString =
                    (dollarAmount * CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit).FormattedCurrencyStringForVouchers();

                ticket["vlt credits"] = vltCreditsString + " " + scope.GetString(ResourceKeys.CreditsAt) + " " + scope.GetString(ResourceKeys.OneCent);

                ticket["vendor"] = scope.GetString(ResourceKeys.Vendor);
                ticket["regulator"] = scope.GetString(ResourceKeys.Regulator);
            }

            return ticket;
        }

        /// <summary>
        ///     Create a demonstration cashout ticket
        /// </summary>
        /// <param name="transaction">The transaction for the cashout ticket</param>
        /// <param name="voidTicket">Whether the ticket should be printed as void demo ticket</param>
        /// <returns>A ticket object with settings for a cashout ticket filled in</returns>
        public static Ticket CreateDemonstrationCashOutTicket(VoucherOutTransaction transaction, bool voidTicket = false)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var ticket = CreateCashOutTicket(transaction, false, voidTicket);

            using (var scope = new CultureScope(CultureFor.PlayerTicket))
            {
                ticket["title alt"] = scope.GetString(ResourceKeys.TitleDemonstration);

                var dollarAmount = GetDollarAmount(0);
                // Replace any unprintable characters.
                ticket["value"] = ReplaceAsciiCharacters(dollarAmount.FormattedCurrencyStringForVouchers());
                ticket["value in test ticket"] = scope.GetString(ResourceKeys.TestText);

                var lineLength = ServiceManager.GetInstance().GetService<IPrinter>().GetCharactersPerLine(false, 0);
                var words = TicketCurrencyExtensions.ConvertCurrencyToWrappedWords(
                    Math.Round(dollarAmount, 2, MidpointRounding.AwayFromZero),
                    lineLength);
                ticket["value in wrapped words 1"] = words?[0];
                ticket["value in wrapped words 2"] = words?[1];
                ticket["value in words for test ticket"] = scope.GetString(ResourceKeys.NoDollarsExactly);

                ticket["serial id alt"] = scope.GetString(ResourceKeys.NotAvailable);
                ticket["license alt"] = scope.GetString(ResourceKeys.NotAvailable);
                ticket["establishment name"] = propertiesManager.GetValue(PropertyKey.TicketTextLine1, string.Empty);
                ticket["regulator"] = scope.GetString(ResourceKeys.NotAvailable);

                // For Test ticket alone get the config
                if (voidTicket)
                {
                    string testTicketType = propertiesManager.GetValue(
                        AccountingConstants.TestTicketType,
                        string.Empty);
                    if (!string.IsNullOrWhiteSpace(testTicketType))
                    {
                        ticket["ticket type"] = testTicketType;
                    }
                }
            }

            return ticket;
        }

        /// <summary>
        ///     Create a cash win ticket
        /// </summary>
        /// <param name="transaction">The transaction for the cash win ticket</param>
        /// <param name="largeWin">Indicates a large win if true.</param>
        /// <returns>A ticket object with settings for a cash win ticket filled in</returns>
        public static Ticket CreateCashWinTicket(
            VoucherOutTransaction transaction,
            bool largeWin = false)
        {
            // cash win tickets are exactly the same as cashout tickets except for the ticket title
            var ticket = CreateCashOutTicket(transaction, largeWin);
            ticket["title"] = VoucherExtensions.PrefixToTitle(transaction.HostOnline) + Localizer.For(CultureFor.PlayerTicket).GetString("CashWinTicket");
            return ticket;
        }

        /// <summary>
        ///     Create a cashout ticket
        /// </summary>
        /// <param name="transaction">The transaction for the cashout ticket</param>
        /// <returns>A ticket object with settings for a cashout ticket filled in</returns>
        public static Ticket GetTicket(VoucherOutTransaction transaction)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var isDemonstrationMode = propertiesManager.GetValue(ApplicationConstants.DemonstrationMode, false);
            if (isDemonstrationMode)
            {
                return CreateDemonstrationCashOutTicket(transaction);
            }

            var cashWin = transaction.Reason == TransferOut.TransferOutReason.CashWin;
            var largeWin = transaction.Reason == TransferOut.TransferOutReason.LargeWin;
            var allowCashWinTicket = propertiesManager.GetValue(AccountingConstants.AllowCashWinTicket, false);

            if (allowCashWinTicket && cashWin)
            {
                return CreateCashWinTicket(transaction, largeWin);
            }

            return transaction.TypeOfAccount == AccountType.NonCash
                ? CreateCashOutRestrictedTicket(transaction)
                : CreateCashOutTicket(transaction, largeWin);
        }

        /// <summary>
        ///     Returns string representation of a version of an expiration string
        /// </summary>
        /// <param name="expiration">The number of expiration days</param>
        /// <param name="localTime">The time from which to add the expiration days</param>
        /// <param name="version">The version of the expiry date string</param>
        /// <param name="localeDateFormat">The date format for the current locale</param>
        /// <returns>A string representing a variation of the expiration</returns>
        public static string GetExpiryDate(int expiration, DateTime localTime, ExpiryDateText version, string localeDateFormat)
        {
            using (var scope = new CultureScope(CultureFor.PlayerTicket))
            {
                if (expiration < 0 || expiration == MaxExpirationDays)
                {
                    switch (version)
                    {
                        case ExpiryDateText.Version1:
                            return $"{scope.GetString(ResourceKeys.ExpiryDate)} : {scope.GetString(ResourceKeys.NeverExpires)}";

                        case ExpiryDateText.Version2:
                            return scope.GetString(ResourceKeys.VoucherTicketsCreatorNeverExpires);

                        case ExpiryDateText.Version3:
                        case ExpiryDateText.Version4:
                        default:
                            return scope.GetString(ResourceKeys.NeverExpires);
                    }
                }
                else
                {
                    switch (version)
                    {
                        case ExpiryDateText.Version1:
                            return $"{scope.GetString(ResourceKeys.ExpiryDate)} : {localTime.AddDays(expiration).ToString(localeDateFormat)}";

                        case ExpiryDateText.Version2:
                            var format = $"{localeDateFormat} {ApplicationConstants.DefaultTimeFormat}";
                            return $"{scope.GetString(ResourceKeys.ExpiryDate2)} {localTime.AddDays(expiration).ToString(format)}";

                        case ExpiryDateText.Version4:
                            return $"{scope.GetString(ResourceKeys.Expires)}: {localTime.AddDays(expiration).ToString(localeDateFormat)}";

                        case ExpiryDateText.Version3:
                        default:
                            return localTime.AddDays(expiration).ToString(localeDateFormat);
                    }
                }
            }
        }

        /// <summary>
        ///     Returns string representation of a version of an expiration string
        /// </summary>
        /// <param name="expirationDate">A DateTime of the expiration date</param>
        /// <param name="version">The version of the expiry date string</param>
        /// <param name="localeDateFormat">The date format for the current locale</param>
        /// <returns>A string a variation of the expiration</returns>
        public static string GetExpiryDate(DateTime expirationDate, ExpiryDateText version, string localeDateFormat)
        {
            using (var scope = new CultureScope(CultureFor.PlayerTicket))
            {
                switch (version)
                {
                    case ExpiryDateText.Version1:
                        return $"{scope.GetString(ResourceKeys.ExpiryDate)} : {expirationDate.ToString(localeDateFormat)}";

                    case ExpiryDateText.Version2:
                        return $"{scope.GetString(ResourceKeys.ExpiryDate2)} {expirationDate.ToString(localeDateFormat)}";

                    case ExpiryDateText.Version4:
                        return $"{scope.GetString(ResourceKeys.Expires)}: {expirationDate.ToString(localeDateFormat)}";

                    case ExpiryDateText.Version3:
                    default:
                        return expirationDate.ToString(localeDateFormat);
                }
            }
        }

        /// <summary>
        ///     Converts the transaction amount from millicents into dollars
        /// </summary>
        /// <param name="amount">The amount to convert from millicents to dollars</param>
        /// <returns>The transaction amount in dollars.</returns>
        public static decimal GetDollarAmount(long amount)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var multiplier = (double)propertiesManager.GetProperty(ApplicationConstants.CurrencyMultiplierKey, null);
            return amount / (decimal)multiplier;
        }

        /// <summary>
        ///     Format Mac address like "xx:xx:xx:xx:xx:xx"
        /// </summary>
        /// <param name="address"></param>
        /// <returns>formatted Mac address</returns>
        private static string MacAddressWithColon(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return address;
            }

            var sb = new StringBuilder(address);
            var i = 2;
            while (i < sb.Length)
            {
                sb.Insert(i, ":");
                i += 3;
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Returns a DateTime representing the number of expiration days
        /// </summary>
        /// <param name="expirationDays">Number of expiration days</param>
        /// <returns>DateTime</returns>
        public static DateTime? ConvertExpirationDate(int expirationDays)
        {
            const string dateFormat = "MMddyyyy";
            var daysValue = expirationDays.ToString("D8", CultureInfo.InvariantCulture);

            return DateTime.TryParseExact(
                daysValue,
                dateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date)
                ? date
                : (DateTime?)null;
        }

        private static string ReplaceAsciiCharacters(string value, string replacementValue = " ") =>
            ReplaceAsciiRegex.Replace(value, replacementValue);
    }
}