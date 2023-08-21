namespace Aristocrat.Mgam.Client
{
    /// <summary>
    ///     Attribute names.
    /// </summary>
    public static class AttributeNames
    {
        /// <summary>sysMGAMConnectionTimeout</summary>
        public const string ConnectionTimeout = "sysMGAMConnectionTimeout";

        /// <summary>sysMGAMKeepAliveInterval</summary>
        public const string KeepAliveInterval = "sysMGAMKeepAliveInterval";

        /// <summary>sitMGAMSessionBalanceLimit</summary>
        public const string SessionBalanceLimit = "sitMGAMSessionBalanceLimit";

        /// <summary>sitMGAMLocationName</summary>
        public const string LocationName = "sitMGAMLocationName";

        /// <summary>sitMGAMLocationAddress</summary>
        public const string LocationAddress = "sitMGAMLocationAddress";

        /// <summary>sitMGAMVoucherExpiration</summary>
        public const string VoucherExpiration = "sitMGAMVoucherExpiration";

        /// <summary>sitMGAMVoucherSecurityLimit</summary>
        public const string VoucherSecurityLimit = "sitMGAMVoucherSecurityLimit";

        /// <summary>sitMGAMMessageCompressionThreshold</summary>
        public const string MessageCompressionThreshold = "sitMGAMMessageCompressionThreshold";

        /// <summary>devMGAMCashIn</summary>
        public const string CashIn = "devMGAMCashIn";

        /// <summary>devMGAMCashOut</summary>
        public const string CashOut = "devMGAMCashOut";

        /// <summary>devMGAMCabinetDoor</summary>
        public const string CabinetDoor = "devMGAMCabinetDoor";

        /// <summary>devMGAMGames</summary>
        public const string Games = "devMGAMGames";

        /// <summary>devMGAMGameFailures</summary>
        public const string GameFailures = "devMGAMGameFailures";

        /// <summary>devMGAMDropDoor</summary>
        public const string DropDoor = "devMGAMDropDoor";

        /// <summary>devMGAMProgressiveOccurence</summary>
        /// <remarks>NOTE: Occurrence is intentionally misspelled since that's how it's defined in the protocol doc</remarks>
        public const string ProgressiveOccurence = "devMGAMProgressiveOccurance";

        /// <summary>devMGAMCashBox</summary>
        public const string CashBox = "devMGAMCashBox";

        /// <summary>devMGAMCashBoxHundreds</summary>
        public const string CashBoxHundreds = "devMGAMCashBoxHundreds";

        /// <summary>devMGAMCashBoxFifties</summary>
        public const string CashBoxFifties = "devMGAMCashBoxFifties";

        /// <summary>devMGAMCashBoxTwenties</summary>
        public const string CashBoxTwenties = "devMGAMCashBoxTwenties";

        /// <summary>devMGAMCashBoxTens</summary>
        public const string CashBoxTens = "devMGAMCashBoxTens";

        /// <summary>devMGAMCashBoxFives</summary>
        public const string CashBoxFives = "devMGAMCashBoxFives";

        /// <summary>devMGAMCashBoxTwos</summary>
        public const string CashBoxTwos = "devMGAMCashBoxTwos";

        /// <summary>devMGAMCashBoxOnes</summary>
        public const string CashBoxOnes = "devMGAMCashBoxOnes";

        /// <summary>devMGAMCashBoxVouchers</summary>
        public const string CashBoxVouchers = "devMGAMCashBoxVouchers";

        /// <summary>devMGAMCashBoxVoucherValueTotal</summary>
        public const string CashBoxVoucherValueTotal = "devMGAMCashBoxVoucherValueTotal";

        /// <summary>devMGAMBillAcceptorEnabled</summary>
        public const string BillAcceptorEnabled = "devMGAMBillAcceptorEnabled";

        /// <summary>devMGAMDropMode</summary>
        public const string DropMode = "devMGAMDropMode";

        /// <summary>appMGAMVersionNumber</summary>
        public const string VersionNumber = "appMGAMVersionNumber";

        /// <summary>appMGAMVersionName</summary>
        public const string VersionName = "appMGAMVersionName";

        /// <summary>inlMGAMGameDescription</summary>
        public const string GameDescription = "inlMGAMGameDescription";

        /// <summary>inlMGAMAutoPlay</summary>
        public const string AutoPlay = "inlMGAMAutoPlay";

        /// <summary>insMGAMPlayerTrackingPoints</summary>
        public const string PlayerTrackingPoints = "insMGAMPlayerTrackingPoints";

        /// <summary>insMGAMPenniesPerPoint</summary>
        public const string PenniesPerPoint = "insMGAMPenniesPerPoint";

        /// <summary>insMGAMPointsPerEntry</summary>
        public const string PointsPerEntry = "insMGAMPointsPerEntry";

        /// <summary>insMGAMSweepstakesEntries</summary>
        public const string SweepstakesEntries = "insMGAMSweepstakesEntries";

        /// <summary>insMGAMPromotionalInfo</summary>
        public const string PromotionalInfo = "insMGAMPromotionalInfo";
    }
}
