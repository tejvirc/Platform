namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;

    /// <summary>The Aft transfer code for an Aft game lock and status request.</summary>
    public enum AftLockCode
    {
        /// <summary>Request lock.</summary>
        RequestLock = 0x00,

        /// <summary>Cancel lock or pending lock request.</summary>
        CancelLockOrPendingLockRequest = 0x80,

        /// <summary>Interrogate current status only.</summary>
        InterrogateCurrentStatusOnly = 0xFF,
    }

    /// <summary>The Aft transfer conditions for an Aft game lock and status request.</summary>
    [Flags]
    public enum AftTransferConditions
    {
        None = 0,
        /// <summary>Transfer to gaming machine OK</summary>
        TransferToGamingMachineOk = 0x01,

        /// <summary>Transfer from gaming machine OK</summary>
        TransferFromGamingMachineOk = 0x02,

        /// <summary>Transfer to printer OK</summary>
        TransferToPrinterOk = 0x04,

        /// <summary>Bonus award to gaming machine OK</summary>
        BonusAwardToGamingMachineOk = 0x08,
    }

    /// <summary>The Aft lock code for an Aft game lock and status request.</summary>
    public enum AftGameLockStatus
    {
        /// <summary>Game Locked.</summary>
        GameLocked = 0x00,

        /// <summary>Game lock pending.</summary>
        GameLockPending = 0x40,

        /// <summary>Game not locked.</summary>
        GameNotLocked = 0xFF,
    }

    /// <summary>The available transfer conditions for an Aft game lock and status request.</summary>
    [Flags]
    public enum AftAvailableTransfers
    {
        /// <summary>No available aft transfers</summary>
        None = 0,

        /// <summary>Transfer to gaming machine OK</summary>
        TransferToGamingMachineOk = 0x01,

        /// <summary>Transfer from gaming machine OK</summary>
        TransferFromGamingMachineOk = 0x02,

        /// <summary>Transfer to printer OK</summary>
        TransferToPrinterOk = 0x04,

        /// <summary>Win amount pending cashout to host</summary>
        WinAmountPendingCashoutToHost = 0x08,

        /// <summary>Bonus award to gaming machine OK</summary>
        BonusAwardToGamingMachineOk = 0x10,
    }

    /// <summary>The Aft status for an Aft game lock and status request.</summary>
    [Flags]
    public enum AftStatus
    {
        /// <summary>Printer available for transaction receipts</summary>
        PrinterAvailableForTransactionReceipts = 0x01,

        /// <summary>Transfer to host of less than full available amount allowed</summary>
        TransferToHostOfLessThanFullAvailableAmountAllowed = 0x02,

        /// <summary>Custom ticket data supported</summary>
        CustomTicketDataSupported = 0x04,

        /// <summary>AFT registered</summary>
        AftRegistered = 0x08,

        /// <summary>In-house transfers enabled</summary>
        InHouseTransfersEnabled = 0x10,

        /// <summary>Bonus transfers enabled</summary>
        BonusTransfersEnabled = 0x20,

        /// <summary>Debit transfers enabled</summary>
        DebitTransfersEnabled = 0x40,

        /// <summary>Any AFT enabled</summary>
        AnyAftEnabled = 0x80,
    }

    /// <summary>Holds the data for an AFT game lock and status request</summary>
    public class AftGameLockAndStatusData : LongPollData
    {
        /// <summary>Gets or sets the Lock code.</summary>
        public AftLockCode LockCode { get; set; }

        /// <summary>Gets or sets the Transfer condition.</summary>
        public AftTransferConditions TransferConditions { get; set; }

        /// <summary>Gets or sets the Lock timeout in hundredths of a second.</summary>
        public int LockTimeout { get; set; }
    }

    /// <summary>Holds the response data for an AFT game lock and status request</summary>
    public class AftGameLockAndStatusResponseData : LongPollResponse
    {
        /// <summary>Gets the length of the response.</summary>
        public byte Length { get; } = 0x23;

        /// <summary>Gets or sets the Asset number.</summary>
        public long AssetNumber { get; set; }

        /// <summary>Gets or sets the Game lock status.</summary>
        public AftGameLockStatus GameLockStatus { get; set; }

        /// <summary>Gets or sets the Available transfers.</summary>
        public AftAvailableTransfers AvailableTransfers { get; set; }

        /// <summary>Gets or sets the Host cashout status.</summary>
        public AftTransferFlags HostCashoutStatus { get; set; }

        /// <summary>Gets or sets the AFT status.</summary>
        public AftStatus AftStatus { get; set; }

        /// <summary>Gets or sets the Maximum transactions in history buffer.</summary>
        public byte MaxBufferIndex { get; set; }

        /// <summary>Gets or sets the Current cashable amount.</summary>
        public ulong CurrentCashableAmount { get; set; }

        /// <summary>Gets or sets the Current restricted amount.</summary>
        public ulong CurrentRestrictedAmount { get; set; }

        /// <summary>Gets or sets the Current nonrestricted amount.</summary>
        public ulong CurrentNonRestrictedAmount { get; set; }

        /// <summary>Gets or sets the Gaming machine transfer limit.</summary>
        public ulong CurrentGamingMachineTransferLimit { get; set; }

        /// <summary>Gets or sets the Restricted expiration.</summary>
        public uint RestrictedExpiration { get; set; }

        /// <summary>Gets or sets the Restricted pool ID.</summary>
        public ushort RestrictedPoolId { get; set; }
    }
}