namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;
    using System.Linq;

    /// <summary>The Aft transfer code for an Aft transfer.</summary>
    public enum AftTransferCode
    {
        /// <summary>Transfer request full transfer only.</summary>
        TransferRequestFullTransferOnly = 0x00,

        /// <summary>Transfer request partial transfer allowed.</summary>
        TransferRequestPartialTransferAllowed = 0x01,

        /// <summary>Cancel transfer request.</summary>
        CancelTransferRequest = 0x80,

        /// <summary>Interrogation request status only.</summary>
        InterrogationRequestStatusOnly = 0xfe,

        /// <summary>Interrogation request.</summary>
        InterrogationRequest = 0xff,
    }

    /// <summary>The transfer type of an Aft transfer.</summary>
    public enum AftTransferType
    {
        /// <summary>Transfer in-house amount from host to gaming machine.</summary>
        HostToGameInHouse = 0x00,

        /// <summary>Transfer bonus coin out win amount from host to gaming machine.</summary>
        HostToGameBonusCoinOut = 0x10,

        /// <summary>Transfer bonus jackpot win amount from host to gaming machine (force attendant pay lockup).</summary>
        HostToGameBonusJackpot = 0x11,

        /// <summary>Transfer in-house amount from host to ticket (only one amount type allowed per transfer).</summary>
        HostToGameInHouseTicket = 0x20,

        /// <summary>Transfer debit amount from host to gaming machine.</summary>
        HostToGameDebit = 0x40,

        /// <summary>Transfer debit amount from host to ticket.</summary>
        HostToGameDebitTicket = 0x60,

        /// <summary>Transfer in-house amount from gaming machine to host.</summary>
        GameToHostInHouse = 0x80,

        /// <summary>Transfer win amount (in-house) from gaming machine to host.</summary>
        GameToHostInHouseWin = 0x90,

        /// <summary>Unknown transfer type.</summary>
        TransferUnknown = 0xFF,
    }

    /// <summary>
    ///     Enum that represents the status code reported to Sas for Aft transfer responses.
    /// </summary>
    public enum AftTransferStatusCode
    {
        /// <summary>Full transfer successful.</summary>
        FullTransferSuccessful = 0x00,

        /// <summary>Partial transfer successful.</summary>
        PartialTransferSuccessful = 0x01,

        /// <summary>Transfer pending.</summary>
        TransferPending = 0x40,

        /// <summary>Transfer canceled by host.</summary>
        TransferCanceledByHost = 0x80,

        /// <summary>Transaction id not unique.</summary>
        TransactionIdNotUnique = 0x81,

        /// <summary>Not a valid transfer function.</summary>
        NotAValidTransferFunction = 0x82,

        /// <summary>Not a valid transfer amount or Expiration date.</summary>
        NotAValidTransferAmountOrExpirationDate = 0x83,

        /// <summary>Transfer amount exceeds game limit.</summary>
        TransferAmountExceedsGameLimit = 0x84,

        /// <summary>Transfer amount not even multiple.</summary>
        TransferAmountNotEvenMultiple = 0x85,

        /// <summary>Gaming machine unable to perform partial.</summary>
        GamingMachineUnableToPerformPartial = 0x86,

        /// <summary>Gaming machine unable to perform transfer.</summary>
        GamingMachineUnableToPerformTransfer = 0x87,

        /// <summary>Gaming machine not registered.</summary>
        GamingMachineNotRegistered = 0x88,

        /// <summary>Registration key does not match.</summary>
        RegistrationKeyDoesNotMatch = 0x89,

        /// <summary>No POS ID (required for debit machines).</summary>
        NoPosId = 0x8a,

        /// <summary>No won credits available for cashout.</summary>
        NoWonCreditsAvailableForCashOut = 0x8b,

        /// <summary>No gaming machine denomination set.</summary>
        NoGamingMachineDenominationSet = 0x8c,

        /// <summary>Expiration not valid for transfer to ticket.</summary>
        ExpirationNotValidForTransferToTicket = 0x8d,

        /// <summary>Transfer to ticket device not available.</summary>
        TransferToTicketDeviceNotAvailable = 0x8e,

        /// <summary>Unable to accept transfer due to existing restricted amounts.</summary>
        UnableToAcceptTransferDueToExistingRestrictedAmounts = 0x8f,

        /// <summary>Unable to print transaction receipt.</summary>
        UnableToPrintTransactionReceipt = 0x90,

        /// <summary>Insufficient data to print transaction receipt.</summary>
        InsufficientDataToPrintTransactionReceipt = 0x91,

        /// <summary>Transaction receipt not allowed for transfer type.</summary>
        TransactionReceiptNotAllowedForTransferType = 0x92,

        /// <summary>Asset number zero or does not match.</summary>
        AssetNumberZeroOrDoesNotMatch = 0x93,

        /// <summary>Gaming machine not locked.</summary>
        GamingMachineNotLocked = 0x94,

        /// <summary>Transaction id not valid.</summary>
        TransactionIdNotValid = 0x95,

        /// <summary>Unexpected error.</summary>
        UnexpectedError = 0x9f,

        /// <summary>Not compatible with current transfer.</summary>
        NotCompatibleWithCurrentTransfer = 0xc0,

        /// <summary>Unsupported transfer code.</summary>
        UnsupportedTransferCode = 0xc1,

        /// <summary>No transfer info available.</summary>
        NoTransferInfoAvailable = 0xff,
    }

    /// <summary>
    ///     The enum for the different receipt status codes.
    /// </summary>
    public enum ReceiptStatus
    {
        /// <summary>The receipt is finished printing.</summary>
        ReceiptPrinted = 0x00,

        /// <summary>The receipt is in the process of printing.</summary>
        ReceiptPrintingInProgress = 0x20,

        /// <summary>The receipt has not started to print.</summary>
        ReceiptPending = 0x40,

        /// <summary>There was no request to print or the receipt was not printed.</summary>
        NoReceiptRequested = 0xFF
    }

    /// <summary>
    ///     From table 8.3f of the SAS Spec
    /// </summary>
    public enum ReceiptField
    {
        /// <summary>The transfer source and direction.</summary>
        TransferSourceDestination = 0x00,

        /// <summary>The date/time of the transaction.</summary>
        DateAndTime = 0x01,

        /// <summary>The patron name.</summary>
        PatronName = 0x10,

        /// <summary>The patron account number.</summary>
        PatronAccountNumber = 0x11,

        /// <summary>The account balance before the transfer.</summary>
        AccountBalance = 0x13,

        /// <summary>The last 4 digits of the debit card number.</summary>
        DebitCardNumber = 0x41,

        /// <summary>The transaction fee applied to debit transfers.</summary>
        TransactionFee = 0x42,

        /// <summary>The debit amount of the transfer.</summary>
        TotalDebitAmount = 0x43
    }

    [Flags]
    public enum AftTransferFlags
    {
        /// <summary>No transfer flag sent.</summary>
        None = 0x00,

        /// <summary>Host cashout enable control. 1 = set enable to bit 1 state</summary>
        HostCashOutEnableControl = 0x01,

        /// <summary>Host cashout enable (ignore if bit 0 is 0)</summary>
        HostCashOutEnable = 0x02,

        /// <summary>Host cashout mode. 0=soft, 1=hard (ignore is bit 0 is 0)</summary>
        HostCashOutMode = 0x04,

        /// <summary>Cashout from gaming machine request</summary>
        CashOutFromGamingMachineRequest = 0x08,

        /// <summary>Use custom ticket data from long poll 76</summary>
        UseCustomTicketData = 0x20,

        /// <summary>Accept transfer only if locked</summary>
        AcceptTransferOnlyIfLocked = 0x40,

        /// <summary>Transaction Receipt request</summary>
        TransactionReceiptRequested = 0x80,

        /// <summary>These are all the flags used for host cash out</summary>
        HostCashOutOptions = HostCashOutEnable | HostCashOutEnableControl | HostCashOutMode,

        /// <summary>The default flags at power up</summary>
        Default = HostCashOutEnableControl
    }

    /// <summary>
    ///     Holds the data for an AFT transfer
    /// </summary>
    public class AftTransferData : LongPollData
    {
        /// <summary>Gets or sets the transfer code.</summary>
        public AftTransferCode TransferCode { get; set; }

        /// <summary>Gets or sets the transfer type of the Aft transfer.</summary>
        public AftTransferType TransferType { get; set; }

        /// <summary>Gets or sets the CashableAmount of the Aft transfer.</summary>
        public ulong CashableAmount { get; set; }

        /// <summary>Gets or sets the RestrictedAmount of the Aft transfer.</summary>
        public ulong RestrictedAmount { get; set; }

        /// <summary>Gets or sets the Non-restrictedAmount of the Aft transfer.</summary>
        public ulong NonRestrictedAmount { get; set; }

        /// <summary>Gets or sets the TransferFlags of the Aft transfer.</summary>
        public AftTransferFlags TransferFlags { get; set; }

        /// <summary>Gets or sets the asset number of the Aft transfer.</summary>
        public long AssetNumber { get; set; }

        /// <summary>Gets or sets the registration key of the Aft transfer.</summary>
        public byte[] RegistrationKey { get; set; } = new byte[20];

        /// <summary>Gets or sets the transaction id of the Aft transfer.</summary>
        public string TransactionId { get; set; }

        /// <summary>Gets or sets the expiration of the Aft transfer.</summary>
        public uint Expiration { get; set; }

        /// <summary>Gets or sets the pool id of the Aft transfer.</summary>
        public ushort PoolId { get; set; }

        /// <summary>Gets or sets the receipt data of the Aft transfer.</summary>
        public AftReceiptData ReceiptData { get; set; } = new AftReceiptData();

        /// <summary>Gets or sets the lock timeout of the Aft transfer.</summary>
        public int LockTimeout { get; set; }

        /// <summary>Gets or sets the transaction index for an Aft Interrogation only poll</summary>
        public byte TransactionIndex { get; set; }

        public AftResponseData ToAftResponseData()
        {
            return new AftResponseData
            {
                TransferCode = TransferCode,
                TransferType = TransferType,
                CashableAmount = CashableAmount,
                RestrictedAmount = RestrictedAmount,
                NonRestrictedAmount = NonRestrictedAmount,
                TransferFlags = TransferFlags,
                AssetNumber = (uint)AssetNumber,
                RegistrationKey = RegistrationKey,
                TransactionId = TransactionId,
                Expiration = Expiration,
                PoolId = PoolId,
                TransactionIndex = TransactionIndex,
                ReceiptStatus = ((byte)TransferFlags & (byte)AftTransferFlags.TransactionReceiptRequested) != 0 ? (byte)ReceiptStatus.ReceiptPending : (byte)ReceiptStatus.NoReceiptRequested,
                ReceiptData = ReceiptData
            };
        }
    }

    public class AftResponseData : LongPollResponse, IAftHistoryLog
    {
        /// <summary>Gets or sets the transfer code.</summary>
        public AftTransferCode TransferCode { get; set; }

        /// <summary>Gets or sets the TransferStatus of the Aft transfer.</summary>
        public AftTransferStatusCode TransferStatus { get; set; }

        /// <summary>Gets or sets the ReceiptStatus of the Aft transfer.</summary>
        public byte ReceiptStatus { get; set; }

        /// <summary>Gets or sets the transfer type of the Aft transfer.</summary>
        public AftTransferType TransferType { get; set; }

        /// <summary>Gets or sets the CashableAmount of the Aft transfer.</summary>
        public ulong CashableAmount { get; set; }

        /// <summary>Gets or sets the RestrictedAmount of the Aft transfer.</summary>
        public ulong RestrictedAmount { get; set; }

        /// <summary>Gets or sets the Non-restrictedAmount of the Aft transfer.</summary>
        public ulong NonRestrictedAmount { get; set; }

        /// <summary>Gets or sets the TransferFlags of the Aft transfer.</summary>
        public AftTransferFlags TransferFlags { get; set; }

        /// <summary>Gets or sets the asset number of the Aft transfer.</summary>
        public uint AssetNumber { get; set; }

        /// <summary>Gets or sets the registration key of the Aft transfer.</summary>
        public byte[] RegistrationKey { get; set; } = new byte[20];

        /// <summary>Gets or sets the transaction id of the Aft transfer.</summary>
        public string TransactionId { get; set; }

        /// <summary>Gets or sets the transaction date and time</summary>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>Gets or sets the expiration of the Aft transfer.</summary>
        public uint Expiration { get; set; }

        /// <summary>Gets or sets the pool id of the Aft transfer.</summary>
        public ushort PoolId { get; set; }

        /// <summary>Cumulative cashable amount meter for transfer type, in cents, 0-9 bytes</summary>
        public long CumulativeCashableAmount { get; set; }

        /// <summary>Cumulative restricted amount meter for transfer type, in cents, 0-9 bytes</summary>
        public long CumulativeRestrictedAmount { get; set; }

        /// <summary>Cumulative non-restricted amount meter for transfer type, in cents, 0-9 bytes</summary>
        public long CumulativeNonRestrictedAmount { get; set; }

        /// <summary>Gets or sets the transaction index for an Aft Interrogation only poll</summary>
        public byte TransactionIndex { get; set; }

        /// <summary>Gets or sets the receipt data of the Aft transfer.</summary>
        public AftReceiptData ReceiptData { get; set; } = new AftReceiptData();

        public static AftResponseData FromIAftHistoryLog(IAftHistoryLog log)
        {
            return new AftResponseData
            {
                TransferCode = log.TransferCode,
                TransferStatus = log.TransferStatus,
                ReceiptStatus = log.ReceiptStatus,
                TransferType = log.TransferType,
                CashableAmount = log.CashableAmount,
                RestrictedAmount = log.RestrictedAmount,
                NonRestrictedAmount = log.NonRestrictedAmount,
                TransferFlags = log.TransferFlags,
                AssetNumber = log.AssetNumber,
                RegistrationKey = log.RegistrationKey,
                TransactionId = log.TransactionId,
                TransactionDateTime = log.TransactionDateTime,
                Expiration = log.Expiration,
                PoolId = log.PoolId,
                CumulativeCashableAmount = log.CumulativeCashableAmount,
                CumulativeRestrictedAmount = log.CumulativeRestrictedAmount,
                CumulativeNonRestrictedAmount = log.CumulativeNonRestrictedAmount,
                TransactionIndex = log.TransactionIndex
            };
        }

        /// <summary>
        ///     Checks if a duplicate request has been sent
        /// </summary>
        /// <param name="data">The data to check for a duplicate</param>
        /// <returns>True if a duplicate, false otherwise</returns>
        public bool IsDuplicate(AftResponseData data)
        {
            return data.TransferCode == TransferCode &&
                   data.TransferType == TransferType &&
                   data.CashableAmount == CashableAmount &&
                   data.RestrictedAmount == RestrictedAmount &&
                   data.NonRestrictedAmount == NonRestrictedAmount &&
                   data.TransferFlags == TransferFlags &&
                   data.AssetNumber == AssetNumber &&
                   data.RegistrationKey.SequenceEqual(RegistrationKey) &&
                   data.TransactionId == TransactionId &&
                   data.Expiration == Expiration &&
                   data.PoolId == PoolId &&
                   data.TransactionIndex == TransactionIndex &&
                   data.ReceiptData.TransferSource == ReceiptData.TransferSource &&
                   data.ReceiptData.ReceiptTime == ReceiptData.ReceiptTime &&
                   data.ReceiptData.PatronName == ReceiptData.PatronName &&
                   data.ReceiptData.PatronAccount == ReceiptData.PatronAccount &&
                   data.ReceiptData.DebitCardNumber == ReceiptData.DebitCardNumber &&
                   data.ReceiptData.AccountBalance == ReceiptData.AccountBalance &&
                   data.ReceiptData.TransactionFee == ReceiptData.TransactionFee &&
                   data.ReceiptData.DebitAmount == ReceiptData.DebitAmount;
        }
    }
}