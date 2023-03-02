namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;

    /// <summary>
    ///     Accounting Constants
    /// </summary>
    public static class AccountingConstants
    {
        /// <summary>
        ///     The maximum value that can be added to the credit meter via the note or coin acceptor in the current period
        /// </summary>
        public const string MaxTenderInLimit = "System.MaxTenderInLimit";

        /// <summary>
        ///     Current voucher in laundry.
        /// </summary>
        public const string VoucherInLaundry = "System.VoucherInLaundry";

        /// <summary>
        ///     Current cash in laundry.
        /// </summary>
        public const string CashInLaundry = "System.CashInLaundry";

        /// <summary>
        ///     Determines whether or not the current game round currency in should be checked before adding credits via the
        ///     note or coin acceptor
        /// </summary>
        public const string CheckLaundryLimit = "System.CheckLaundryLimit";

        /// <summary>
        ///     Determines whether or not to allow insertion of credit if the bank balance is under the max credit limit.
        /// </summary>
        public const string AllowCreditUnderLimit = "System.AllowCreditUnderLimit";

        /// <summary>
        ///     Determines whether to allow modification in MaxCreditsIn or not.
        /// </summary>
        public const string EditableMaxCreditsIn = "System.EditableMaxCreditsIn";

        /// <summary>
        ///     The maximum win that can be automatically paid by the EGM without generating a handpay request
        /// </summary>
        public const string LargeWinLimit = "Cabinet.LargeWinLimit";

        /// <summary>
        ///     The maximum values that can be set for the large win limit
        /// </summary>
        public const string LargeWinLimitMaxValue = "Cabinet.LargeWinLimitMaxValue";

        /// <summary>
        ///     The large win limit enabled value. Determines whether or not the large win limit is enabled.
        /// </summary>
        public const string LargeWinLimitEnabled = "Cabinet.LargeWinLimitEnabled";

        /// <summary>
        ///     Indicates whether large win limit is editable or not.
        /// </summary>
        public const string OverwriteLargeWinLimit = "Cabinet.OverwriteLargeWinLimit";

        /// <summary>
        ///     The ratio of win to wager that must also be satisfied for some jurisdictions (e.g. Kentucky) for a win to be
        ///     classed as "large". For example, in Kentucky, you must win at least 300 times your wagered amount AS WELL AS
        ///     winning at least $600 absolute value for a win to be "large". Note that the win amount means the amount OVER
        ///     the original wager. So for a wager of $10 and ratio of 300.0, a win of $3000 is not considered large, but a win
        ///     of $3010 is considered large.
        ///
        ///     Note that this setting is stored as a long, and presented with two decimal places. For example, the value of
        ///     300x for Kentucky is stored as 30000 and would be configured on the GUI as "300.00x Wager".
        /// </summary>
        public const string LargeWinRatio = "Cabinet.LargeWinRatio";

        /// <summary>
        ///     The large win ratio enabled value. Determines whether or not the large win ratio is enabled.
        /// </summary>
        public const string LargeWinRatioEnabled = "Cabinet.LargeWinRatioEnabled";

        /// <summary>
        ///     Indicates whether large win ratio is editable or not
        /// </summary>
        public const string OverwriteLargeWinRatio = "Cabinet.OverwriteLargeWinRatio";

        /// <summary>
        ///     Indicates whether large win ratio is shown on limits config wizard or not
        /// </summary>
        public const string DisplayLargeWinRatio = "Cabinet.DisplayLargeWinRatio";

        /// <summary>
        ///     The threshold absolute value at which the large win ratio kicks in. For example, the $600 amount mentioned
        ///     for <see cref="LargeWinRatio"/>
        /// </summary>
        public const string LargeWinRatioThreshold = "Cabinet.LargeWinRatioThreshold";

        /// <summary>
        ///     The large win ratio threshold enabled value. Determines whether or not the large win ratio threshold is enabled.
        /// </summary>
        public const string LargeWinRatioThresholdEnabled = "Cabinet.LargeWinRatioThresholdEnabled";

        /// <summary>
        ///     Indicates whether large win ratio threshold is editable or not
        /// </summary>
        public const string OverwriteLargeWinRatioThreshold = "Cabinet.OverwriteLargeWinRatioThreshold";

        /// <summary>
        ///     Indicates whether large win ratio threshold is shown on limits config wizard or not
        /// </summary>
        public const string DisplayLargeWinRatioThreshold = "Cabinet.DisplayLargeWinRatioThreshold";

        /// <summary>
        ///     Indicates whether handpay reset options are shown on limits config wizard or not
        /// </summary>
        public const string DisplayHandpayResetOptions = "Cabinet.DisplayHandpayResetOptions";

        /// <summary>
        ///     The default amount permitted on the credit meter
        /// </summary>
        public const string MaxCreditMeter = "Cabinet.MaxCreditMeter.Default";

        /// <summary>
        ///     The  maximum amount permitted on the credit meter
        /// </summary>
        public const string MaxCreditMeterMaxAllowed = "Cabinet.MaxCreditMeter.MaxAllowed";

        /// <summary>
        ///     Indicates whether the Credit Limit checkbox is checked or not
        /// </summary>
        public const string CreditLimitEnabled = "Cabinet.MaxCreditMeter.Enabled";

        /// <summary>
        ///     Indicates whether the message needs to be displayed when Max Credit Limit reached
        /// </summary>
        public const string ShowMessageWhenCreditLimitReached = "Cabinet.MaxCreditMeter.ShowMessageWhenCreditLimitReached";

        /// <summary>
        ///     Indicates whether the Bank Note Acceptor should be disabled on reaching Max Credit Limit or not
        /// </summary>
        public const string DisableBankNoteAcceptorWhenCreditLimitReached = "Cabinet.MaxCreditMeter.DisableBankNoteAcceptorWhenCreditLimitReached";

        /// <summary>
        ///     The maximum bet amount permitted
        /// </summary>
        public const string MaxBetLimit = "Cabinet.MaxBetLimit";

        /// <summary>
        ///     Indicate if maximum bet is editable or not
        /// </summary>
        public const string OverwriteMaxBetLimit = "Cabinet.OverwriteMaxBetLimit";

        /// <summary>
        ///     Indicates whether the Max Bet Limit checkbox is checked or not
        /// </summary>
        public const string MaxBetLimitEnabled= "Cabinet.MaxBetLimitEnabled";

        /// <summary>
        ///     The Highest Limit allowed on MaxBetLimit
        /// </summary>
        public const string HighestMaxBetLimitAllowed = "Cabinet.HighestMaxBetLimitAllowed";

        /// <summary>
        ///     Does jurisdiction allow cash win tickets
        /// </summary>
        public const string AllowCashWinTicket = "System.AllowCashWinTicket";

        /// <summary>
        ///     Determines whether to allow insertion of credits after max credit limit or not.
        /// </summary>
        public const string AllowCreditsInAboveMaxCredit = "Cabinet.AllowCreditsInAboveMaxCredit";

        /// <summary>
        ///     Indicates whether the EGM is to can accept money in
        /// </summary>
        public const string MoneyInEnabled = "Cabinet.EnableMoneyIn";

        /// <summary>
        ///     Determines whether or not the current bank balance should be checked before adding credits via the note or coin
        ///     acceptor
        /// </summary>
        public const string CheckCreditsIn = "System.CheckCreditsIn";

        /// <summary>
        ///     The maximum value that can be added to the credit meter via the voucher acceptor
        /// </summary>
        public const string VoucherInLimit = "Cabinet.VoucherInLimit";

        /// <summary>
        ///     Indicates whether or not voucher out is enabled.
        /// </summary>
        public const string VoucherOut = "System.VoucherOut";

        /// <summary>
        ///     Indicates whether or not voucher out box is checked or not.
        /// </summary>
        public const string VoucherOutLimitEnabled = "System.VoucherOutLimitEnabled";

        /// <summary>
        ///     Indicates whether or not voucher out limit is editable.
        /// </summary>
        public const string VoucherOutLimitEditable = "System.VoucherOutLimitEditable";

        /// <summary>
        ///     Indicates whether or not voucher in box is checked or not.
        /// </summary>
        public const string VoucherInLimitEnabled = "System.VoucherInLimitEnabled";

        /// <summary>
        ///     Indicates whether or not voucher in limit is editable.
        /// </summary>
        public const string VoucherInLimitEditable = "System.VoucherInLimitEditable";

        /// <summary>
        ///     The maximum amount permitted on the cash-out voucher
        /// </summary>
        public const string VoucherOutLimit = "Cabinet.VoucherOutLimit";

        /// <summary>
        ///     Indicates whether or not voucher out for non cash is enabled
        /// </summary>
        public const string VoucherOutNonCash = "System.VoucherOutNonCash";

        /// <summary>
        ///     Indicates Max sequence number for generating VoucherSequence
        /// </summary>
        public const string VoucherOutMaxSequence = "System.VoucherOutMaxSequence";

        /// <summary>
        ///     Indicates whether or not offline need to print in voucher title.
        /// </summary>
        public const string VoucherOfflineNotify = "System.VoucherOfflineNotify";

        /// <summary>
        ///     The voucher out expiration days
        /// </summary>
        public const string VoucherOutExpirationDays = "Cabinet.VoucherOutExpirationDays";

        /// <summary>
        ///     Allow expiration days overwrite
        /// </summary>
        public const string EditableExpiration = "Cabinet.EditableExpiration";

        /// <summary>
        ///     The voucher out non cash expiration days
        /// </summary>
        public const string VoucherOutNonCashExpirationDays = "Cabinet.VoucherOutNonCashExpirationDays";

        /// <summary>
        ///     The max allowed voucher out limit
        /// </summary>
        public const string VoucherOutMaxAllowed = "Cabinet.VoucherOutMaxAllowed";

        /// <summary>
        ///     The max allowed voucher in limit
        /// </summary>
        public const string VoucherInMaxAllowed = "Cabinet.VoucherInMaxAllowed";

        /// <summary>
        ///     Default voucher expiration days
        /// </summary>
        public const int DefaultVoucherExpirationDays = 30;

        /// <summary>
        ///     Default voucher-out limit
        /// </summary>
        public const long DefaultVoucherOutLimit = Int64.MaxValue;

        /// <summary>
        ///     Default voucher-in limit
        /// </summary>
        public const long DefaultVoucherInLimit = Int64.MaxValue;

        /// <summary>
        ///     Default Max sequence number for generating VoucherSequence
        /// </summary>
        public const int DefaultVoucherOutMaxSequence = 9999;

        /// <summary>
        ///     Default large win limit in millicents. Represents No Limit
        /// </summary>
        public const long DefaultLargeWinLimit = 9_999_999_999_00_000L;

        /// <summary>
        ///     Default large win ratio in terms of amount won divided by wagered amount. The default value of zero is so that
        ///     existing jurisdictions with no such requirement are always triggered by the large win limit rather than the ratio.
        /// </summary>
        public const long DefaultLargeWinRatio = 0L;

        /// <summary>
        ///     Maximum allowable value for the large win ratio. This is a ratio value with two decimal places so a common value
        ///     for a ratio of 300x will be held internally as 30000. This maximum then is 999.99x, and is only here to enforce
        ///     input rules on the GUI.
        /// </summary>
        public const long MaximumLargeWinRatio = 99999L;

        /// <summary>
        ///     Default large win ratio threshold amount. The default value of zero is so that existing jurisdictions with
        ///     no such requirement are always triggered by the large win limit rather than the ratio.
        /// </summary>
        public const long DefaultLargeWinRatioThreshold = 0L;

        /// <summary>
        ///     Default handpay limit in millicents, this value should not be lower than LargeWinLimit
        /// </summary>
        public const long DefaultHandpayLimit = 9_999_999_999_00_000L;

        /// <summary>
        ///     The default maximum value that can be added to the credit meter via the note or coin acceptor in the current period
        /// </summary>
        public const long DefaultMaxTenderInLimit = 100_000_00_000L;

        /// <summary>
        ///     The default maximum denomination limit in millicents when using "standard" production smart card
        /// </summary>
        public const long DefaultDenominationLimit = 100_00_000L;

        /// <summary>
        ///     The default maximum maximum bet limit in millicents
        /// </summary>
        public const long DefaultMaxBetLimit = 100_000_00_000L;

        /// <summary>
        ///     The default ticket line
        /// </summary>
        public const string DefaultTicketLine = @"data not available";

        /// <summary>
        ///     The default cashout ticket title
        /// </summary>
        public const string DefaultCashoutTicketTitle = @"CASHOUT TICKET";

        /// <summary>
        ///     The default non cash ticket title
        /// </summary>
        public const string DefaultNonCashTicketTitle = @"PLAYABLE ONLY";

        /// <summary>
        ///     The default large win ticket title
        /// </summary>
        public const string DefaultLargeWinTicketTitle = @"JACKPOT TICKET";

        /// <summary>
        ///     The default ticket barcode length
        /// </summary>
        public const int DefaultTicketBarcodeLength = 18;

        /// <summary>
        ///     The handpay limit.  Any win exceeding this amount will result in a handpay.
        /// </summary>
        public const string HandpayLimit = "Handpay.HandpayLimit";

        /// <summary>
        ///     The handpay limit enabled value. Determines whether or not the handpay limit is enabled.
        /// </summary>
        public const string HandpayLimitEnabled = "Handpay.HandpayLimitEnabled";

        /// <summary>
        ///     The method used for a large win handpay reset.
        /// </summary>
        public const string LargeWinHandpayResetMethod = "Handpay.LargeWinHandpayResetMethod";

        /// <summary>
        ///     The method used for a large win SubstantialWin Transaction name .
        /// </summary>
        public const string LargeWinTransactionName = "Handpay.OverwriteTransactionName";

        /// <summary>
        ///     Local handpays permitted when EGM is enabled
        /// </summary>
        public const string EnabledLocalHandpay = "Handpay.EnabledLocalHandpay";

        /// <summary>
        ///     Local key-off to credit meter permitted when EGM is enabled
        /// </summary>
        public const string EnabledLocalCredit = "Handpay.EnabledLocalCredit";

        /// <summary>
        ///     Local key-off to voucher permitted when EGM is enabled
        /// </summary>
        public const string EnabledLocalVoucher = "Handpay.EnabledLocalVoucher";

        /// <summary>
        ///     Local key-off to WAT permitted when EGM is enabled
        /// </summary>
        public const string EnabledLocalWat = "Handpay.EnabledLocalWat";

        /// <summary>
        ///     Remote handpays permitted when EGM is enabled
        /// </summary>
        public const string EnabledRemoteHandpay = "Handpay.EnabledRemoteHandpay";

        /// <summary>
        ///     Remote key-off to credit meter permitted when EGM is enabled
        /// </summary>
        public const string EnabledRemoteCredit = "Handpay.EnabledRemoteCredit";

        /// <summary>
        ///     Remote key-off to voucher permitted when EGM is enabled
        /// </summary>
        public const string EnabledRemoteVoucher = "Handpay.EnabledRemoteVoucher";

        /// <summary>
        ///     Remote key-off to WAT permitted when EGM is enabled
        /// </summary>
        public const string EnabledRemoteWat = "Handpay.EnabledRemoteWat";

        /// <summary>
        ///     Local handpays permitted when EGM is disabled
        /// </summary>
        public const string DisabledLocalHandpay = "Handpay.DisabledLocalHandpay";

        /// <summary>
        ///     Local key-off to credit meter permitted when EGM is disabled
        /// </summary>
        public const string DisabledLocalCredit = "Handpay.DisabledLocalCredit";

        /// <summary>
        ///     Local key-off to voucher permitted when EGM is disabled
        /// </summary>
        public const string DisabledLocalVoucher = "Handpay.DisabledLocalVoucher";

        /// <summary>
        ///     Local key-off to WAT permitted when EGM is disabled
        /// </summary>
        public const string DisabledLocalWat = "Handpay.DisabledLocalWat";

        /// <summary>
        ///     Remote handpays permitted when EGM is disabled
        /// </summary>
        public const string DisabledRemoteHandpay = "Handpay.DisabledRemoteHandpay";

        /// <summary>
        ///     Remote key-off to credit meter permitted when EGM is disabled
        /// </summary>
        public const string DisabledRemoteCredit = "Handpay.DisabledRemoteCredit";

        /// <summary>
        ///     Remote key-off to voucher permitted when EGM is disabled
        /// </summary>
        public const string DisabledRemoteVoucher = "Handpay.DisabledRemoteVoucher";

        /// <summary>
        ///     Remote key-off to WAT permitted when EGM is disabled
        /// </summary>
        public const string DisabledRemoteWat = "Handpay.DisabledRemoteWat";

        /// <summary>
        ///     Mix multiple credit types in a single handpay
        /// </summary>
        public const string MixCreditTypes = "Handpay.MixCreditTypes";

        /// <summary>
        ///     Include non-cashable credits in handpay requests
        /// </summary>
        public const string RequestNonCash = "Handpay.RequestNonCash";

        /// <summary>
        ///     Combine cashable and promo cashable on one handpay
        /// </summary>
        public const string CombineCashableOut = "Handpay.CombineCashableOut";

        /// <summary>
        ///     Local key-off methods prior to host authorization
        /// </summary>
        public const string LocalKeyOff = "Handpay.LocalKeyOff";

        /// <summary>
        ///     Indicates whether requests for partial handpays should be accepted for cashable and promotional credits.
        /// </summary>
        public const string PartialHandpays = "Handpay.PartialHandpays";

        /// <summary>
        ///     Indicates whether non-validated handpay receipts are enabled.
        /// </summary>
        public const string EnableReceipts = "Handpay.EnableReceipts";

        /// <summary>
        ///     Indicates whether game win receipts are enabled.
        /// </summary>
        public const string AllowGameWinReceipts = "Handpay.AllowGameWinReceipts";

        /// <summary>
        ///     Indicates whether or not print handpay receipts setting is editable
        /// </summary>
        public const string EditableReceipts = "Handpay.EditableReceipts";

        /// <summary>
        ///     Indicates whether or not handpays should be validated
        /// </summary>
        public const string ValidateHandpays = "Handpay.ValidateHandpays";

        /// <summary>
        ///     Indicates whether it is enabled to exit the handpay pending lockup as a player
        /// </summary>
        public const string HandpayPendingExitEnabled = "Handpay.HandpayPendingExitEnabled";

        /// <summary>
        ///     Indicates whether or not handpay receipts are required to be printed before marking a handpay completed
        /// </summary>
        public const string HandpayReceiptsRequired = "Handpay.HandpayReceiptsRequired";

        /// <summary>
        ///     Title printed on game win and bonus pay receipts.
        /// </summary>
        public const string TitleJackpotReceipt = "Handpay.TitleJackpotReceipt";

        /// <summary>
        ///     Title printed on cancel credit receipts.
        /// </summary>
        public const string TitleCancelReceipt = "Handpay.TitleCancelReceipt";

        /// <summary>
        ///     Parameters associated with Use-Player-ID-Reader option.
        /// </summary>
        public const string UsePlayerIdReader = "Handpay.UsePlayerIdReader";

        /// <summary>
        ///     Parameters associated with ID-Reader-ID option.
        /// </summary>
        public const string IdReaderId = "Handpay.IdReaderId";

        /// <summary>
        ///     The key to access the ticket title cash property.
        ///     Title printed on vouchers for cashable credits. EX. "CASHOUT VOUCHER"
        /// </summary>
        public const string TicketTitleCash = "TicketProperty.TicketTitleCash";

        /// <summary>
        ///     Specified whether the user can keyoff a handpayTransaction while another Lockup is occuring.
        /// </summary>
        public const string CanKeyOffWhileInLockUp = "Handpay.CanKeyOffWhileInLockUp";

        /// <summary>
        ///     The key to access the property value for title of cashout reprint ticket.
        ///     Ex. "RÉIMPRESSION - COUPON DE REMBOURSEMENT"
        /// </summary>
        public const string TicketTitleCashReprint = "TicketProperty.TicketTitleCashReprint";

        /// <summary>
        ///     The key to access the property value for title of noncash reprint ticket.
        ///     Ex. "PLAYABLE ONLY REPRINT"
        /// </summary>
        public const string TicketTitleNonCashReprint = "TicketProperty.TicketTitleNonCashReprint";

        /// <summary>
        ///     The key to access the ticket title promo property.
        ///     Title printed on vouchers for promotional cashable creditsEX. "CASHOUT VOUCHER"
        /// </summary>
        public const string TicketTitlePromo = "TicketProperty.TicketTitlePromo";

        /// <summary>
        ///     The key to access the ticket title non cash property.
        ///     Title printed on vouchers for noncashable credits. EX. "PLAYABLE ONLY"
        /// </summary>
        public const string TicketTitleNonCash = "TicketProperty.TicketTitleNonCash";

        /// <summary>
        ///     The key to access the ticket title large win property.
        ///     Title printed on vouchers for wins greater than cabinetProfile.largeWinLimit. EX. " JACKPOT VOUCHER"
        /// </summary>
        public const string TicketTitleLargeWin = "TicketProperty.TicketTitleLargeWin";

        /// <summary>
        ///     The key to access the ticket title bonus cash property.
        ///     Title printed on bonus award vouchers for cashable credits. EX. "CASHOUT VOUCHER"
        /// </summary>
        public const string TicketTitleBonusCash = "TicketProperty.TicketTitleBonusCash";

        /// <summary>
        ///     The key to access the ticket title bonus promo property.
        ///     Title printed on bonus award vouchers for promotional cashable creditsEX. "CASHOUT VOUCHER"
        /// </summary>
        public const string TicketTitleBonusPromo = "TicketProperty.TicketTitleBonusPromo";

        /// <summary>
        ///     The key to access the ticket title bonus non cash property.
        ///     Title printed on bonus award vouchers for non-cashable credits. EX. "PLAYABLE ONLY"
        /// </summary>
        public const string TicketTitleBonusNonCash = "TicketProperty.TicketTitleBonusNonCash";

        /// <summary>
        ///     The key to access the ticket title wat cash property.
        ///     Title printed on WAT transfer vouchers for cashable credits EX. "CASHOUT VOUCHER"
        /// </summary>
        public const string TicketTitleWatCash = "TicketProperty.TicketTitleWatCash";

        /// <summary>
        ///     The key to access the ticket title wat promo property.
        ///     Title printed on WAT transfer vouchers for promotional cashable credits. EX. "CASHOUT VOUCHER"
        /// </summary>
        public const string TicketTitleWatPromo = "TicketProperty.TicketTitleWatPromo";

        /// <summary>
        ///     The key to access the ticket title wat non cash property.
        ///     Title printed on WAT transfer vouchers for non-cashable credits EX. "PLAYABLE ONLY"
        /// </summary>
        public const string TicketTitleWatNonCash = "TicketProperty.TicketTitleWatNonCash";

        /// <summary> The key to access the ticket redemtion text property. </summary>
        public const string RedeemText = "TicketProperty.TicketRedeemText";

        /// <summary> The key to access the property indicating whether to ignore a voucher stacked during a reboot. </summary>
        public const string IgnoreVoucherStackedDuringReboot = "TicketProperty.IgnoreVoucherStackedDuringReboot";

        /// <summary> The key to access the property indicating whether a ticket redemption is timed out. </summary>
        public const string IsVoucherRedemptionTimedOut = "TicketProperty.IsVoucherRedemptionTimedOut";

        /// <summary>
        ///     Indicates the behavior to use for reprinting a logged voucher from the All - Logs screen.
        /// </summary>
        public const string ReprintLoggedVoucherBehavior = "Cabinet.ReprintLoggedVoucherBehavior";

        /// <summary>
        ///     Indicates the ticket title behavior to use for reprinting a logged voucher from the All - Logs screen.
        /// </summary>
        public const string ReprintLoggedVoucherTitleOverride = "Cabinet.ReprintLoggedVoucherTitleOverride";

        /// <summary>
        ///     Indicates the door requirement behavior to use for reprinting a logged voucher from the All - Logs screen.
        /// </summary>
        public const string ReprintLoggedVoucherDoorOpenRequirement = "Cabinet.ReprintLoggedVoucherDoorOpenRequirement";

        /// <summary>
        ///     Metadata used during the transfer out process.
        /// </summary>
        public const string TransferOutContext = "TransferOut.TransferContext";

        /// <summary>
        ///     Type used when forcibly keying off a handpay for a large win.  Typically None
        /// </summary>
        public const string HandpayLargeWinForcedKeyOff = "Handpay.LargeWinForcedKeyOff";

        /// <summary>
        ///     Type used to decide Large Win Key Off Strategy Typically LocalHandPay
        /// </summary>
        public const string HandpayLargeWinKeyOffStrategy = "Handpay.LargeWinKeyOffStrategy";

        /// <summary>
        ///     The maximum win amount automatically paid to the credit meter; wins exceeding this limit will cause a lockup that requires attendant reset key for the win to be paid to Credit Meter.
        /// </summary>
        public const string CelebrationLockupLimit = "Cabinet.CelebrationLockupLimit";

        /// <summary>
        ///     The length of barcode to generate.  Validation numbers shorter than this length will be left padded with 0s.
        /// </summary>
        public const string TicketBarcodeLength = "TicketProperty.BarcodeLength";

        /// <summary>
        ///     Meter Mystery Progressive pays as External bonusing (meter in machine/attendant paid external bonusing meters).
        /// </summary>
        public const string MysteryWinAsExternalBonus = "MysteryProgressive.WinAsExternalBonus";

        /// <summary> Property manager key for ConfigWizardLimitsPageEnabled. </summary>
        public const string ConfigWizardLimitsPageEnabled = "ConfigWizard.LimitsPage.Enabled";

       /// <summary> Property manager key for HandpayRemoteHandpayResetAllowed </summary>
        public const string RemoteHandpayResetAllowed = "Handpay.RemoteHandpayReset.Allowed";

        /// <summary> Property manager key for RemoteHandpayResetConfigurable  </summary>
        public const string RemoteHandpayResetConfigurable = "Handpay.RemoteHandpayReset.Configurable";

        /// <summary>
        ///    Flag that will force cashout if carrier board or CFast was removed/changed during power-down and if there is credit
        /// </summary>
        public const string CashoutOnCarrierBoardRemovalEnabled = @"CashoutOnCarrierBoardRemovalEnabled.Enabled";

        /// <summary>Property manager key for NoteAcceptorTimeLimit Enable.</summary>
        public const string NoteAcceptorTimeLimitEnabled = "Accounting.NoteAcceptorTimeLimit.Enable";

        /// <summary>Property manager key for NoteAcceptorTimeLimit Value.</summary>
        public const string NoteAcceptorTimeLimitValue = "Accounting.NoteAcceptorTimeLimit.Value";

        /// <summary>
        ///    Flag that will tell that if the menu to select the handPay method via menu is in progress.
        /// </summary>
        public const string MenuSelectionHandpayInProgress = "MenuSelectionHandpayInProgress";

        /// <summary>
        ///    Flag that determines whether BillClearance functionality is enabled or not
        /// </summary>
        public const string BillClearanceEnabled = "BillClearanceEnabled";

        /// <summary>
        ///    Name of the test ticket type which maps to ticket template
        /// </summary>
        public const string TestTicketType = "TestTicketType";

        /// <summary>
        /// Flag for Excessive Document Reject limit Lockup to persist on powercycle.
        /// </summary>
        public const string ExcessiveDocumentRejectLockupEnabled = "ExcessiveDocumentRejectLockupEnabled";

        /// <summary>System disable guid for when carrier board is removed/replaced with credits available.</summary>
        public static Guid DisabledDueToCarrierBoardRemovalKey = new Guid("{716969F6-174A-456A-A116-CF8E1DE3C791}");

        /// <summary> Property manager key for SeparateMeteringCashableAndPromoOutAmounts  </summary>
        public const string SeparateMeteringCashableAndPromoOutAmounts = "System.SeparateMeteringCashableAndPromoOutAmounts";

        /// <summary>The guid for the alternative cancel credit ticket message.</summary>
        public static Guid AlternativeCancelCreditTickerMessageGuid = new Guid("{1AA2ADE3-B978-4D5A-A477-E6E36C500366}");

        /// <summary>
        /// The System Disabled the Handpay payoff if BNA connection is required
        /// </summary>
        public const string HandpayNoteAcceptorConnectedRequired = "Handpay.NoteAcceptorConnectedRequired";

        /// <summary>    If player inserts a total amount above this threshold and without playing any game requests cash-out the machine will lock-up </summary>
        public const string IncrementThreshold = "IncrementThreshold";

        /// <summary>    Determines whether or not the meter increment threshold is checked via operator menu. </summary>
        public const string IncrementThresholdIsChecked = "IncrementThresholdIsChecked";

        /// <summary>    Determines whether or not the meter increment threshold applies to current jurisdiction. </summary>
        public const string LaunderingMonitorVisible = "LaunderingMonitorVisible";

        /// <summary>   The accumulated amount of money the user has inserted </summary>
        public const string ExcessiveMeterValue = "ExcessiveMeterValue";

        /// <summary>   The disabled status due to excessive meter </summary>
        public const string DisabledDueToExcessiveMeter = "DisabledDueToExcessiveMeter";

        /// <summary>   The properties manager key for sound file path </summary>
        public const string ExcessiveMeterSound = "ExcessiveMeterSound";

        /// <summary>   Default Increment Threshold in millicents </summary>
        public const long DefaultIncrementThreshold = 1_00_000L;

        /// <summary>   Minimum Increment Threshold in millicents </summary>
        public const long MinimumIncrementThreshold = 1_00_000L;

        /// <summary>   Hand count service enabled </summary>
        public const string HandCountServiceEnabled = "HandCountServiceEnabled";
    }
}
