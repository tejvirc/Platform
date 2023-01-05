namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Globalization;
    using System.Text;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;

    /// <summary>Class that creates an Aft receipt.</summary>
    public static class SasAftReceiptCreator
    {
        /// <summary>Creates an Aft out receipt.</summary>
        /// <param name="aftData">Data about the Aft.</param>
        /// <param name="registrationProvider">The registration provider</param>
        /// <returns>A ticket object with settings for an Aft receipt ticket filled in.</returns>
        public static Ticket CreateAftReceipt(AftData aftData, IAftRegistrationProvider registrationProvider)
        {
            return new SasAftReceiptFactory(aftData, registrationProvider).Receipt();
        }

        /// <summary>
        ///     Class for Sas In-House AFT Receipts
        /// </summary>
        private class SasInHouseAftReceipt : SasAftReceipt
        {
            private readonly bool _transferIn;
            private readonly string _restrictedAmountLabel;

            /// <summary>
            ///     Create new instance of SasInHouseAftReceipt
            /// </summary>
            /// <param name="aftData">AFT Data</param>
            /// <param name="transferIn">True if transferring in to the game</param>
            /// <param name="transferDescription">Transfer Description (Table 8.11.2)</param>
            /// <param name="totalCashableAmountLabel">Total Cashable Amount Label (Table 8.11.2)</param>
            /// <param name="restrictedAmountLabel">Restricted Amount Label (Table 8.11.2)</param>
            public SasInHouseAftReceipt(
                AftData aftData,
                bool transferIn,
                string transferDescription,
                string totalCashableAmountLabel,
                string restrictedAmountLabel = "")
                : base(aftData, transferDescription, totalCashableAmountLabel)
            {
                _transferIn = transferIn;
                _restrictedAmountLabel = restrictedAmountLabel;
            }

            /// <inheritdoc />
            protected override void Line11() => AddLine();

            /// <inheritdoc />
            protected override void Line12() => AddLine(AftData.ReceiptData.PatronName);

            /// <inheritdoc />
            protected override void Line13()
            {
                if (AftData.ReceiptData.PatronAccount.Length > 0)
                {
                    AddLine(
                        Localizer.For(CultureFor.Player).GetString(ResourceKeys.PatronAccountLabel),
                        AftData.ReceiptData.PatronAccount);
                }
            }

            /// <inheritdoc />
            protected override void Line17()
            {
                if (AftData.RestrictedAmount > 0)
                {
                    AddLine(
                        _restrictedAmountLabel,
                        GetDollarAmount(AftData.RestrictedAmount).FormattedCurrencyString());
                }
                else
                {
                    AddLine();
                }
            }

            /// <inheritdoc />
            protected override void Line18() => AddLine();

            /// <inheritdoc />
            protected override void Line19()
            {
                var total = AftData.CashableAmount + AftData.RestrictedAmount + AftData.NonRestrictedAmount;
                if (_transferIn)
                {
                    if (total <= AftData.AccountBalance)
                    {
                        AddLine(
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.AccountBalanceLabel),
                            GetDollarAmount(AftData.AccountBalance - total).FormattedCurrencyString());
                    }
                    else
                    {
                        AddLine();
                    }
                }
                else
                {
                    if (AftData.ReceiptData.AccountBalance > 0)
                    {
                        AddLine(
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.AccountBalanceLabel),
                            GetDollarAmount(AftData.AccountBalance + total).FormattedCurrencyString());
                    }
                    else
                    {
                        AddLine();
                    }
                }
            }

            /// <inheritdoc />
            protected override void Line21() =>
                AddLine(
                    (string)PropertiesManager.GetProperty(
                        SasProperties.AftTransferReceiptInHouseLine1,
                        Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.InHouse1)));

            /// <inheritdoc />
            protected override void Line22() =>
                AddLine(
                    (string)PropertiesManager.GetProperty(
                        SasProperties.AftTransferReceiptInHouseLine2,
                        Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.InHouse2)));

            /// <inheritdoc />
            protected override void Line23() =>
                AddLine(
                    (string)PropertiesManager.GetProperty(
                        SasProperties.AftTransferReceiptInHouseLine3,
                        Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.InHouse3)));

            /// <inheritdoc />
            protected override void Line24() =>
                AddLine(
                    (string)PropertiesManager.GetProperty(
                        SasProperties.AftTransferReceiptInHouseLine4,
                        Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.InHouse4)));
        }

        /// <summary>
        ///     Class for Sas Debit AFT Receipts
        /// </summary>
        private class SasDebitAftReceipt : SasAftReceipt
        {
            private readonly long _posId;

            /// <summary>
            ///     Create new instance of SasDebitAftReceipt
            /// </summary>
            /// <param name="aftData">AFT Data</param>
            /// <param name="transferDescription">Transfer Description (Table 8.11.2)</param>
            /// <param name="totalCashableAmountLabel">Total Cashable Amount Label (Table 8.11.2)</param>
            /// <param name="posId">The posId</param>
            public SasDebitAftReceipt(
                AftData aftData,
                string transferDescription,
                string totalCashableAmountLabel,
                long posId)
                : base(aftData, transferDescription, totalCashableAmountLabel)
            {
                _posId = posId;
            }

            /// <inheritdoc />
            protected override void Line11() => AddLine(_posId.ToString());

            /// <inheritdoc />
            protected override void Line12() => AddLine();

            /// <inheritdoc />
            protected override void Line13() => AddLine(
                Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PatronAccountLabel),
                AftData.ReceiptData.DebitCardNumber);

            /// <inheritdoc />
            protected override void Line17() => AddLine();

            /// <inheritdoc />
            protected override void Line18() => AddLine(
                Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.TransactionFeeLabel),
                GetDollarAmount(AftData.ReceiptData.TransactionFee).FormattedCurrencyString());

            /// <inheritdoc />
            protected override void Line19()
            {
                if (AftData.ReceiptData.DebitAmount == 0)
                {
                    var fee = AftData.ReceiptData.TransactionFee;
                    if (fee > 0)
                    {
                        var cash = AftData.CashableAmount + AftData.NonRestrictedAmount;
                        AddLine(
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.TotalDebitLabel),
                            GetDollarAmount(cash + fee).FormattedCurrencyString());
                    }
                    else
                    {
                        AddLine();
                    }
                }
                else
                {
                    AddLine(
                        Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.TotalDebitLabel),
                        GetDollarAmount(AftData.ReceiptData.DebitAmount).FormattedCurrencyString());
                }
            }

            /// <inheritdoc />
            protected override void Line21() => AddLine(
                (string)PropertiesManager.GetProperty(
                    SasProperties.AftTransferReceiptDebitLine1,
                    Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Debit1)));

            /// <inheritdoc />
            protected override void Line22() => AddLine(
                (string)PropertiesManager.GetProperty(
                    SasProperties.AftTransferReceiptDebitLine2,
                    Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Debit2)));

            /// <inheritdoc />
            protected override void Line23() => AddLine(
                (string)PropertiesManager.GetProperty(
                    SasProperties.AftTransferReceiptDebitLine3,
                    Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Debit3)));

            /// <inheritdoc />
            protected override void Line24() => AddLine(
                (string)PropertiesManager.GetProperty(
                    SasProperties.AftTransferReceiptDebitLine4,
                    Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Debit4)));
        }

        /// <summary>
        ///     Base class for Sas AFT Receipts
        /// </summary>
        private abstract class SasAftReceipt
        {
            private static readonly object Lock = new object();
            private readonly StringBuilder _leftField = new StringBuilder();
            private readonly StringBuilder _centerField = new StringBuilder();
            private readonly StringBuilder _rightField = new StringBuilder();

            private readonly Ticket _ticket;
            private readonly string _transferDescription;
            private readonly string _totalCashableAmountLabel;

            /// <summary>
            ///     Create new instance of SasAftReceipt
            /// </summary>
            /// <param name="aftData">AFT Data</param>
            /// <param name="transferDescription">The description for this transfer</param>
            /// <param name="totalCashableAmountLabel">The label for this total cashable amount</param>
            protected SasAftReceipt(AftData aftData, string transferDescription, string totalCashableAmountLabel)
            {
                PropertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

                AftData = aftData;
                _transferDescription = transferDescription;
                _totalCashableAmountLabel = totalCashableAmountLabel;

                _ticket = new Ticket();
            }

            /// <summary>
            ///     Retrieves the receipt
            /// </summary>
            /// <returns>A Sas AFT receipt</returns>
            public Ticket Receipt()
            {
                _ticket["ticket type"] = "text";
                lock (Lock)
                {
                    Line1();
                    Line2();
                    Line3();
                    Line4();
                    Line5();
                    Line6();
                    Line7();
                    Line8();
                    Line9();
                    Line10();
                    Line11();
                    Line12();
                    Line13();
                    Line14();
                    Line15();
                    Line16();
                    Line17();
                    Line18();
                    Line19();
                    Line20();
                    Line21();
                    Line22();
                    Line23();
                    Line24();

                    _ticket["left"] = _leftField.ToString();
                    _ticket["center"] = _centerField.ToString();
                    _ticket["right"] = _rightField.ToString();
                }

                return _ticket;
            }

            /// <summary>
            ///     The AFT Data
            /// </summary>
            protected AftData AftData { get; }

            /// <summary>
            ///     The Properties Manager
            /// </summary>
            protected IPropertiesManager PropertiesManager { get; }

            /// <summary>
            ///     Generate Line 11 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 11: Blank (in-house) or POS ID (debit)
            ///     Source: Debit = POS ID from long poll 73
            /// </remarks>
            protected abstract void Line11();

            /// <summary>
            ///     Generate Line 12 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 12: Patron name (in-house) or blank (debit)
            ///     Source: In-house = long poll 72 print data (ASCII text as received, or blank)
            /// </remarks>
            protected abstract void Line12();

            /// <summary>
            ///     Generate Line 13 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 13: Patron acct# (in-house) or Debit card# (debit)
            ///     Source: In-house = “Acct: ” followed by long poll 72 print data
            ///             Debit = “Acct: X” followed by long poll 72 print data
            /// </remarks>
            protected abstract void Line13();

            /// <summary>
            ///     Generate Line 17 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 17: Restricted transfer amount (in-house) or blank (debit)
            ///     Source: In-house = descriptive text based on transfer type (see Table 8.11.2), followed by
            ///             restricted transfer amount from long poll 72 response
            ///             (leave line blank if restricted amount is zero)
            /// </remarks>
            protected abstract void Line17();

            /// <summary>
            ///     Generate Line 18 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 18: Blank (in-house) or transaction fee (debit)
            ///     Source: Debit = “Transaction Fee” followed by long poll 72 print data, or blank
            /// </remarks>
            protected abstract void Line18();

            /// <summary>
            ///     Generate Line 19 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 19: Account balance (in-house) or total debit (debit)
            ///     Source: In-house = “Acct Bal” followed by sum or difference of long poll 72 print data and
            ///             total transfer amount, or blank
            ///             Debit = “Total Debit” followed by long poll 72 print data, or calculated total (debit
            ///             transfer amount plus fee) if total is not provided but transaction fee is provided, or
            ///             blank
            /// </remarks>
            protected abstract void Line19();

            /// <summary>
            ///     Generate Line 21 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 21: In-house text 1 (in-house) or debit text 1 (debit)
            ///     Source: Long poll 75 data (ASCII text as received, or blank)
            /// </remarks>
            protected abstract void Line21();

            /// <summary>
            ///     Generate Line 22 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 22: In-house text 2 (in-house) or debit text 2 (debit)
            ///     Source: Long poll 75 data (ASCII text as received, or blank)
            /// </remarks>
            protected abstract void Line22();

            /// <summary>
            ///     Generate Line 23 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 23: In-house text 3 (in-house) or debit text 3 (debit)
            ///     Source: Long poll 75 data (ASCII text as received, or blank)
            /// </remarks>
            protected abstract void Line23();

            /// <summary>
            ///     Generate Line 24 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 24: In-house text 4 (in-house) or debit text 4 (debit)
            ///     Source: Long poll 75 data (ASCII text as received, or blank)
            /// </remarks>
            protected abstract void Line24();

            /// <summary>
            ///     Adds a line to the receipt
            /// </summary>
            /// <param name="label">A left-justified item label</param>
            /// <param name="value">A right-justified item value</param>
            protected void AddLine(string label, string value)
            {
                _leftField.AppendLine(label);
                _centerField.AppendLine(null);
                _rightField.AppendLine(value);
            }

            /// <summary>
            ///     Adds a line to the receipt
            /// </summary>
            /// <param name="value">A center justified value</param>
            protected void AddLine(string value)
            {
                _leftField.AppendLine(null);
                _centerField.AppendLine(value);
                _rightField.AppendLine(null);
            }

            /// <summary>
            ///     Adds a blank line to the receipt
            /// </summary>
            protected void AddLine()
            {
                _leftField.AppendLine(null);
                _centerField.AppendLine(null);
                _rightField.AppendLine(null);
            }

            /// <summary>Converts the transaction amount from millicent into dollar</summary>
            /// <param name="amount">The amount to convert from millicent to dollar</param>
            /// <returns>The transaction amount in dollars.</returns>
            protected decimal GetDollarAmount(ulong amount)
            {
                var multiplier =
                    Convert.ToDecimal(PropertiesManager.GetProperty(AftConstants.CurrencyMultiplierKey, 0));
                return amount / multiplier;
            }

            /// <summary>
            ///     Generate Line 1 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 1: Location
            ///     Source: Operator entry or long poll 75 data
            /// </remarks>
            private void Line1()
            {
                AddLine(
                    (string)PropertiesManager.GetProperty(
                        SasProperties.AftTransferReceiptLocationLine,
                        Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Location)));
            }

            /// <summary>
            ///     Generate Line 2 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 2: Address1
            ///     Source: Operator entry or long poll 75 data
            /// </remarks>
            private void Line2()
            {
                AddLine(
                    (string)PropertiesManager.GetProperty(
                        SasProperties.AftTransferReceiptAddressLine1,
                        Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Address1)));
            }

            /// <summary>
            ///     Generate Line 3 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 3: Address2
            ///     Source: Operator entry or long poll 75 data
            /// </remarks>
            private void Line3()
            {
                AddLine(
                    (string)PropertiesManager.GetProperty(
                        SasProperties.AftTransferReceiptAddressLine2,
                        Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Address2)));
            }

            /// <summary>
            ///     Generate Line 4 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 4: Blank
            /// </remarks>
            private void Line4() => AddLine();

            /// <summary>
            ///     Generate Line 5 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 5: Transfer description
            ///     Source: Long poll 72 transfer type (see Table 8.11.2)
            /// </remarks>
            private void Line5() => AddLine(_transferDescription);

            /// <summary>
            ///     Generate Line 6 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 6: Transfer source/destination
            ///     Source: Long poll 72 print data (ASCII text as received, or blank)
            /// </remarks>
            private void Line6()
            {
                AddLine(AftData.ReceiptData.TransferSource);
            }

            /// <summary>
            ///     Generate Line 7 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 7: Blank
            /// </remarks>
            private void Line7() => AddLine();

            /// <summary>
            ///     Generate Line 8 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 8: Date and time
            ///     Source: Long poll 72 receipt data, or date and time transfer completed if not specified by host
            /// </remarks>
            private void Line8()
            {
                // check if a date was provided by LP72
                var time = AftData.ReceiptData.ReceiptTime;
                if (time == DateTime.MinValue)
                {
                    // no LP72 date provided, so use actual transaction date/time
                    time = AftData.TransactionDateTime;
                }

                var dateFormat = PropertiesManager.GetProperty(
                    ApplicationConstants.LocalizationPlayerTicketDateFormat,
                    ApplicationConstants.DefaultDateFormat);
                var dateTime = time.ToString(
                    $"{dateFormat} {ApplicationConstants.DefaultTimeFormat}",
                    CultureInfo.InvariantCulture);
                AddLine(dateTime);
            }

            /// <summary>
            ///     Generate Line 9 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 9: Blank
            /// </remarks>
            private void Line9() => AddLine();

            /// <summary>
            ///     Generate Line 10 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 10: Asset number
            ///     Source: Set in gaming machine
            /// </remarks>
            private void Line10()
            {
                AddLine(
                    Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.AssetNumberLabel),
                    PropertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0).ToString());
            }

            /// <summary>
            ///     Generate Line 14 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 14: Blank
            /// </remarks>
            private void Line14() => AddLine();

            /// <summary>
            ///     Generate Line 15 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 15: Transaction ID
            ///     Source: Long poll 72 transaction ID
            /// </remarks>
            private void Line15()
            {
                AddLine(AftData.TransactionId);
            }

            /// <summary>
            ///     Generate Line 16 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 16: Total cashable transfer amount
            ///     Source: Descriptive text based on transfer type (see Table 8.11.2), followed by total of
            ///             cashable and non-restricted transfer amounts from long poll 72 response
            ///             (leave line blank if total cashable amount is zero)
            /// </remarks>
            private void Line16()
            {
                var cash = AftData.CashableAmount + AftData.NonRestrictedAmount;
                if (cash > 0)
                {
                    AddLine(_totalCashableAmountLabel, GetDollarAmount(cash).FormattedCurrencyString());
                }
                else
                {
                    AddLine();
                }
            }

            /// <summary>
            ///     Generate Line 20 of the receipt
            /// </summary>
            /// <remarks>
            ///     Line 20: Blank
            /// </remarks>
            private void Line20() => AddLine();
        }

        /// <summary>
        ///     A factory for producing Sas AFT Receipts
        /// </summary>
        private class SasAftReceiptFactory
        {
            private readonly AftData _aftData;
            private readonly IAftRegistrationProvider _aftRegistration;

            /// <summary>
            ///     Create a new instance of SasAftReceiptFactory
            /// </summary>
            /// <param name="aftData"></param>
            /// <param name="aftRegistration">The aft registration provider</param>
            public SasAftReceiptFactory(AftData aftData, IAftRegistrationProvider aftRegistration)
            {
                _aftData = aftData;
                _aftRegistration = aftRegistration;
            }

            /// <summary>
            ///     Retrieves the receipt
            /// </summary>
            /// <returns>A Sas AFT receipt</returns>
            public Ticket Receipt()
            {
                // The following values are from Table 8.11.2 Transfer Descriptive Text
                switch (_aftData.TransferType)
                {
                    case AftTransferType.HostToGameInHouse:
                        return new SasInHouseAftReceipt(
                            _aftData,
                            true,
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.TransferToGame),
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.CashInText),
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PromoIn)).Receipt();
                    case AftTransferType.HostToGameInHouseTicket:
                        return new SasInHouseAftReceipt(
                            _aftData,
                            true,
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.TransferToGame),
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.CashTicket),
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PromoTicket)).Receipt();
                    case AftTransferType.HostToGameDebit:
                        return new SasDebitAftReceipt(
                            _aftData,
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.DebitCardWithdrawal),
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.DebitIn),
                            _aftRegistration.PosId).Receipt();
                    case AftTransferType.HostToGameDebitTicket:
                        return new SasDebitAftReceipt(
                            _aftData,
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.DebitCardWithdrawal),
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.DebitTicket),
                            _aftRegistration.PosId).Receipt();
                    case AftTransferType.GameToHostInHouse:
                        return new SasInHouseAftReceipt(
                            _aftData,
                            false,
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.TransferFromGame),
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.CashOutText),
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PromoOut)).Receipt();
                    case AftTransferType.GameToHostInHouseWin:
                        return new SasInHouseAftReceipt(
                            _aftData,
                            false,
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.TransferFromGame),
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.CashOutText)).Receipt();
                    default:
                        return null;
                }
            }
        }
    }
}