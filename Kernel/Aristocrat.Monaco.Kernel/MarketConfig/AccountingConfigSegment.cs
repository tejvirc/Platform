//------------------------------------------------------------------------------
// <auto-generated>
// This file was automatically generated by a tool.
// Schema Format Version: 0.9
// Generator Version: 1.1.0.0
//
// DO NOT MODIFY THIS FILE MANUALLY.
// Changes to this file may cause incorrect behavior and will be overwritten.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Text.Json.Serialization;

namespace Aristocrat.Monaco.Kernel.MarketConfig.Accounting
{
    /// <summary>
    /// Tender-in Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class TenderInFieldset
    {
        /// <summary>
        /// Check credits-in
        /// - `None` Disable checking the bank balance against max credits in.
        /// - `Balance` Enable checking the bank balance against max credits in.
        /// - `Session` Enable checking the added note up to the max credits in. This is session based and is reset when the balance hits zero.
        /// </summary>
        [JsonPropertyName("check_credits_in")]
        public String CheckCreditsIn { get; set; }

        /// <summary>
        /// Allow credit under limit
        /// - `Off/false` Dis-allow the insertion of any credit amount if the bank balance would be over the max credit limit after amount is deposited to the bank.
        /// - `On/true` Allow insertion of any credit amount if the bank balance is below the max credit limit.
        /// </summary>
        [JsonPropertyName("allow_credit_under_limit")]
        public Boolean AllowCreditUnderLimit { get; set; }

        /// <summary>
        /// Max credits-in default
        /// The default max credits in value (in millicents) to check against the bank balance if check credits-in is enabled.
        /// </summary>
        [JsonPropertyName("max_credits_in_default")]
        public Int64 MaxCreditsInDefault { get; set; }

        /// <summary>
        /// Max credits-in editable
        /// The max credits-in default value may be overridden by the technician in the audit menu settings if set to true.
        /// </summary>
        [JsonPropertyName("max_credits_in_editable")]
        public Boolean MaxCreditsInEditable { get; set; }

        /// <summary>
        /// Max tender-in limit
        /// </summary>
        [JsonPropertyName("max_tender_in_limit")]
        public Int64 MaxTenderInLimit { get; set; }

        /// <summary>
        /// Check laundry limit
        /// </summary>
        [JsonPropertyName("check_laundry_limit")]
        public Boolean CheckLaundryLimit { get; set; }
    }

    /// <summary>
    /// Max credit meter Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class MaxCreditMeterFieldset
    {
        /// <summary>
        /// Default
        /// </summary>
        [JsonPropertyName("default")]
        public Int64 Default { get; set; }

        /// <summary>
        /// Max allowed
        /// </summary>
        [JsonPropertyName("max_allowed")]
        public Int64 MaxAllowed { get; set; }

        /// <summary>
        /// Show message when credit limit reached
        /// </summary>
        [JsonPropertyName("show_message_when_credit_limit_reached")]
        public Boolean ShowMessageWhenCreditLimitReached { get; set; }

        /// <summary>
        /// Disable bank note acceptor when credit limit reached
        /// </summary>
        [JsonPropertyName("disable_bank_note_acceptor_when_credit_limit_reached")]
        public Boolean DisableBankNoteAcceptorWhenCreditLimitReached { get; set; }
    }

    /// <summary>
    /// Max bet limit Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class MaxBetLimitFieldset
    {
        /// <summary>
        /// Default
        /// </summary>
        [JsonPropertyName("default")]
        public Int64 Default { get; set; }

        /// <summary>
        /// Editable
        /// </summary>
        [JsonPropertyName("editable")]
        public Boolean Editable { get; set; }

        /// <summary>
        /// Limit max
        /// </summary>
        [JsonPropertyName("limit_max")]
        public Int64 LimitMax { get; set; }
    }

    /// <summary>
    /// Large win limit Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class LargeWinLimitFieldset
    {
        /// <summary>
        /// Default
        /// </summary>
        [JsonPropertyName("default")]
        public Int64 Default { get; set; }

        /// <summary>
        /// Max allowed
        /// </summary>
        [JsonPropertyName("max_allowed")]
        public Int64 MaxAllowed { get; set; }

        /// <summary>
        /// Editable
        /// </summary>
        [JsonPropertyName("editable")]
        public Boolean Editable { get; set; }

        /// <summary>
        /// Override transaction name
        /// </summary>
        [JsonPropertyName("override_transaction_name")]
        public Boolean OverrideTransactionName { get; set; }
    }

    /// <summary>
    /// Large win ratio Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class LargeWinRatioFieldset
    {
        /// <summary>
        /// Default
        /// </summary>
        [JsonPropertyName("default")]
        public Int64 Default { get; set; }

        /// <summary>
        /// Editable
        /// </summary>
        [JsonPropertyName("editable")]
        public Boolean Editable { get; set; }

        /// <summary>
        /// Visible
        /// </summary>
        [JsonPropertyName("visible")]
        public Boolean Visible { get; set; }
    }

    /// <summary>
    /// Large win ratio threshold Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class LargeWinRatioThresholdFieldset
    {
        /// <summary>
        /// Default
        /// </summary>
        [JsonPropertyName("default")]
        public Int64 Default { get; set; }

        /// <summary>
        /// Editable
        /// </summary>
        [JsonPropertyName("editable")]
        public Boolean Editable { get; set; }

        /// <summary>
        /// Visible
        /// </summary>
        [JsonPropertyName("visible")]
        public Boolean Visible { get; set; }
    }

    /// <summary>
    /// Voucher-in Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class VoucherInFieldset
    {
        /// <summary>
        /// Enabled
        /// </summary>
        [JsonPropertyName("enabled")]
        public Boolean Enabled { get; set; }

        /// <summary>
        /// Enable limit
        /// </summary>
        [JsonPropertyName("enable_limit")]
        public Boolean EnableLimit { get; set; }

        /// <summary>
        /// Limit default
        /// </summary>
        [JsonPropertyName("limit_default")]
        public Int64 LimitDefault { get; set; }

        /// <summary>
        /// Limit max
        /// </summary>
        [JsonPropertyName("limit_max")]
        public Int64 LimitMax { get; set; }

        /// <summary>
        /// Allow limit edit
        /// </summary>
        [JsonPropertyName("allow_limit_edit")]
        public Boolean AllowLimitEdit { get; set; }
    }

    /// <summary>
    /// Voucher-out Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class VoucherOutFieldset
    {
        /// <summary>
        /// Enabled
        /// Voucher-out is allowed when on/true and disabled if off/false.
        /// </summary>
        [JsonPropertyName("enabled")]
        public Boolean Enabled { get; set; }

        /// <summary>
        /// Enable limit
        /// </summary>
        [JsonPropertyName("enable_limit")]
        public Boolean EnableLimit { get; set; }

        /// <summary>
        /// Limit default
        /// Max allowed limit
        /// </summary>
        [JsonPropertyName("limit_default")]
        public Int64 LimitDefault { get; set; }

        /// <summary>
        /// Limit max
        /// </summary>
        [JsonPropertyName("limit_max")]
        public Int64 LimitMax { get; set; }

        /// <summary>
        /// Allow limit edit
        /// </summary>
        [JsonPropertyName("allow_limit_edit")]
        public Boolean AllowLimitEdit { get; set; }

        /// <summary>
        /// Expiration days
        /// The number of days by which the ticket will expire. "0" means Never Expire.
        /// </summary>
        [JsonPropertyName("expiration_days")]
        public Int32 ExpirationDays { get; set; }

        /// <summary>
        /// Expiration editable
        /// The expiration days value may be overridden by the technician in the audit menu settings if set to on/true.
        /// </summary>
        [JsonPropertyName("expiration_editable")]
        public Boolean ExpirationEditable { get; set; }

        /// <summary>
        /// Allow cash win ticket
        /// </summary>
        [JsonPropertyName("allow_cash_win_ticket")]
        public Boolean AllowCashWinTicket { get; set; }

        /// <summary>
        /// Allow non-cashable ticket
        /// </summary>
        [JsonPropertyName("allow_non_cashable_ticket")]
        public Boolean AllowNonCashableTicket { get; set; }

        /// <summary>
        /// Offline
        /// </summary>
        [JsonPropertyName("offline")]
        public Boolean Offline { get; set; }

        /// <summary>
        /// Max sequence
        /// </summary>
        [JsonPropertyName("max_sequence")]
        public Int64 MaxSequence { get; set; }

        /// <summary>
        /// Separate metering cashable and promo amounts
        /// </summary>
        [JsonPropertyName("separate_metering_cashable_and_promo_amounts")]
        public Boolean SeparateMeteringCashableAndPromoAmounts { get; set; }
    }

    /// <summary>
    /// Reprint logged voucher Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class ReprintLoggedVoucherFieldset
    {
        /// <summary>
        /// Behavior
        /// - `None` No vouchers can be reprinted from the Logs - All screen (Reprint button is hidden).
        /// - `Any`  Any voucher can be reprinted from the Logs - All screen.
        /// - `Last` Only the last voucher can be reprinted from the Logs - All screen (Reprint button is disabled for all other logged vouchers).
        /// </summary>
        [JsonPropertyName("behavior")]
        public String Behavior { get; set; }

        /// <summary>
        /// Title override
        /// - `Off/false` Use value in property "TicketProperty.TicketTitleCash" as title
        /// - `On/true` Use value in property "TicketProperty.TicketTitleCashReprint" as title
        /// </summary>
        [JsonPropertyName("title_override")]
        public Boolean TitleOverride { get; set; }

        /// <summary>
        /// Door open requirement
        /// - `None` No doors are required to be open
        /// - `Main` Main door must be open for reprint to be enabled
        /// - `Logic` Logic door must be open for reprint to be enabled
        /// - `n` Where n is any logical door id or Aristocrat.Monaco.Hardware.Contracts.Door.DoorLogicalId, door is required for reprint to be enabled
        /// - `|` Requiring multiple doors can be accomplished by combining, for example "Main|Logic" will require both Main and Logic door to be open
        /// </summary>
        [JsonPropertyName("door_open_requirement")]
        public String DoorOpenRequirement { get; set; }
    }

    /// <summary>
    /// Handpay Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class HandpayFieldset
    {
        /// <summary>
        /// Note acceptor connected required
        /// </summary>
        [JsonPropertyName("note_acceptor_connected_required")]
        public Boolean NoteAcceptorConnectedRequired { get; set; }

        /// <summary>
        /// Large win forced key-off
        /// </summary>
        [JsonPropertyName("large_win_forced_key_off")]
        public Boolean LargeWinForcedKeyOff { get; set; }

        /// <summary>
        /// Large win key-off strategy
        /// </summary>
        [JsonPropertyName("large_win_key_off_strategy")]
        public String LargeWinKeyOffStrategy { get; set; }

        /// <summary>
        /// Handpay pending exit enabled
        /// </summary>
        [JsonPropertyName("handpay_pending_exit_enabled")]
        public Boolean HandpayPendingExitEnabled { get; set; }

        /// <summary>
        /// Can key-off while in lock-up
        /// </summary>
        [JsonPropertyName("can_key_off_while_in_lock_up")]
        public Boolean CanKeyOffWhileInLockUp { get; set; }

        /// <summary>
        /// Handpay receipts required
        /// </summary>
        [JsonPropertyName("handpay_receipts_required")]
        public Boolean HandpayReceiptsRequired { get; set; }

        /// <summary>
        /// Remote handpay reset allowed
        /// </summary>
        [JsonPropertyName("remote_handpay_reset_allowed")]
        public Boolean RemoteHandpayResetAllowed { get; set; }

        /// <summary>
        /// Remote handpay reset configurable
        /// </summary>
        [JsonPropertyName("remote_handpay_reset_configurable")]
        public Boolean RemoteHandpayResetConfigurable { get; set; }

        /// <summary>
        /// Print handpay receipt enabled
        /// </summary>
        [JsonPropertyName("print_handpay_receipt_enabled")]
        public Boolean PrintHandpayReceiptEnabled { get; set; }

        /// <summary>
        /// Print handpay receipt editable
        /// </summary>
        [JsonPropertyName("print_handpay_receipt_editable")]
        public Boolean PrintHandpayReceiptEditable { get; set; }

        /// <summary>
        /// Allow game win receipt enabled
        /// </summary>
        [JsonPropertyName("allow_game_win_receipt_enabled")]
        public Boolean AllowGameWinReceiptEnabled { get; set; }
    }

    /// <summary>
    /// Mystery progressive Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class MysteryProgressiveFieldset
    {
        /// <summary>
        /// Win as external bonus
        /// </summary>
        [JsonPropertyName("win_as_external_bonus")]
        public Boolean WinAsExternalBonus { get; set; }
    }

    /// <summary>
    /// Note acceptor time limit Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class NoteAcceptorTimeLimitFieldset
    {
        /// <summary>
        /// Enabled
        /// </summary>
        [JsonPropertyName("enabled")]
        public Boolean Enabled { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [JsonPropertyName("value")]
        public Int32 Value { get; set; }
    }

    /// <summary>
    /// Money laundering monitor Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class MoneyLaunderingMonitorFieldset
    {
        /// <summary>
        /// Enabled
        /// </summary>
        [JsonPropertyName("enabled")]
        public Boolean Enabled { get; set; }

        /// <summary>
        /// Visible
        /// </summary>
        [JsonPropertyName("visible")]
        public Boolean Visible { get; set; }

        /// <summary>
        /// Sound file path
        /// </summary>
        [JsonPropertyName("sound_file_path")]
        public String SoundFilePath { get; set; }
    }

    /// <summary>
    /// Market Jurisdiction Configuration for the Accounting segment
    /// </summary>
    [MarketConfigSegment("accounting")]
    public sealed partial class AccountingConfigSegment
    {
        /// <summary>
        /// Tender-in
        /// </summary>
        [JsonPropertyName("tender_in")]
        public TenderInFieldset TenderIn { get; set; }

        /// <summary>
        /// Max credit meter
        /// </summary>
        [JsonPropertyName("max_credit_meter")]
        public MaxCreditMeterFieldset MaxCreditMeter { get; set; }

        /// <summary>
        /// Max bet limit
        /// </summary>
        [JsonPropertyName("max_bet_limit")]
        public MaxBetLimitFieldset MaxBetLimit { get; set; }

        /// <summary>
        /// Allow credits-in above max credit
        /// </summary>
        [JsonPropertyName("allow_credits_in_above_max_credit")]
        public Boolean AllowCreditsInAboveMaxCredit { get; set; }

        /// <summary>
        /// Large win limit
        /// </summary>
        [JsonPropertyName("large_win_limit")]
        public LargeWinLimitFieldset LargeWinLimit { get; set; }

        /// <summary>
        /// Large win ratio
        /// </summary>
        [JsonPropertyName("large_win_ratio")]
        public LargeWinRatioFieldset LargeWinRatio { get; set; }

        /// <summary>
        /// Large win ratio threshold
        /// </summary>
        [JsonPropertyName("large_win_ratio_threshold")]
        public LargeWinRatioThresholdFieldset LargeWinRatioThreshold { get; set; }

        /// <summary>
        /// Handpay reset visible
        /// </summary>
        [JsonPropertyName("handpay_reset_visible")]
        public Boolean HandpayResetVisible { get; set; }

        /// <summary>
        /// Handpay limit
        /// </summary>
        [JsonPropertyName("handpay_limit")]
        public Int64 HandpayLimit { get; set; }

        /// <summary>
        /// Celebration lockup limit
        /// </summary>
        [JsonPropertyName("celebration_lockup_limit")]
        public Int64 CelebrationLockupLimit { get; set; }

        /// <summary>
        /// Note-in Enabled
        /// </summary>
        [JsonPropertyName("note_in_enabled")]
        public Boolean NoteInEnabled { get; set; }

        /// <summary>
        /// Voucher-in
        /// </summary>
        [JsonPropertyName("voucher_in")]
        public VoucherInFieldset VoucherIn { get; set; }

        /// <summary>
        /// Voucher-out
        /// </summary>
        [JsonPropertyName("voucher_out")]
        public VoucherOutFieldset VoucherOut { get; set; }

        /// <summary>
        /// Reboot while printing behavior
        /// - `Reprint` After operator key-off of reboot while printing error, remove remaining credits and automatically reprint a cashout ticket.
        /// - `Clear` After operator key-off of reboot while printing error, will remove remaining credits.
        /// - `Prompt` After operator key-off of reboot while printing error, will prompt with a pop-up selection to reprint the voucher or cancel.  Either option will remove the remaining credits.  If cancel is selected, the voucher will not be allowed for reprint.
        /// </summary>
        [JsonPropertyName("reboot_while_printing_behavior")]
        public String RebootWhilePrintingBehavior { get; set; }

        /// <summary>
        /// Reprint logged voucher
        /// </summary>
        [JsonPropertyName("reprint_logged_voucher")]
        public ReprintLoggedVoucherFieldset ReprintLoggedVoucher { get; set; }

        /// <summary>
        /// Handpay
        /// </summary>
        [JsonPropertyName("handpay")]
        public HandpayFieldset Handpay { get; set; }

        /// <summary>
        /// Mystery progressive
        /// </summary>
        [JsonPropertyName("mystery_progressive")]
        public MysteryProgressiveFieldset MysteryProgressive { get; set; }

        /// <summary>
        /// Cashout on carrier board removal enabled
        /// </summary>
        [JsonPropertyName("cashout_on_carrier_board_removal_enabled")]
        public Boolean CashoutOnCarrierBoardRemovalEnabled { get; set; }

        /// <summary>
        /// Note acceptor time limit
        /// </summary>
        [JsonPropertyName("note_acceptor_time_limit")]
        public NoteAcceptorTimeLimitFieldset NoteAcceptorTimeLimit { get; set; }

        /// <summary>
        /// Money laundering monitor
        /// </summary>
        [JsonPropertyName("money_laundering_monitor")]
        public MoneyLaunderingMonitorFieldset MoneyLaunderingMonitor { get; set; }

        /// <summary>
        /// Bill clearance enabled
        /// </summary>
        [JsonPropertyName("bill_clearance_enabled")]
        public Boolean BillClearanceEnabled { get; set; }

        /// <summary>
        /// Test ticket type
        /// Name of the test ticket type which maps to the ticket template
        /// </summary>
        [JsonPropertyName("test_ticket_type")]
        public String TestTicketType { get; set; }
    }
}