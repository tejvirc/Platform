namespace Aristocrat.Monaco.Sas.Contracts.SASProperties
{
    /// <summary>
    /// Gets Static class defining keys for Sas related properties in the SasPropertiesProvider component.
    /// </summary>
    public static class SasProperties
    {
        /// <summary> The key for port assignments </summary>
        public const string SasPortAssignments = "Sas.PortAssignments";

        /// <summary> The key for feature settings </summary>
        public const string SasFeatureSettings = "Sas.FeatureSettings";

        /// <summary> The key for host settings </summary>
        public const string SasHosts = "Sas.Hosts";

        /// <summary>
        /// The key for accessing the no validation seeds received property via the IPropertiesManager.
        /// </summary>
        public const string SasNoValidationSeedsReceivedKey = "Sas.NoValidationSeedsReceived";

        /// <summary>
        /// The key for accessing the validation buffer full property via the IPropertiesManager.
        /// </summary>
        public const string SasValidationBufferFullKey = "Sas.ValidationBufferFull";

        /// <summary>
        /// The key for accessing the shutdown command received property via the IPropertiesManager.
        /// </summary>
        public const string SasShutdownCommandReceivedKey = "Sas.ShutdownCommandReceived";

        /// <summary>
        /// The key for accessing the Sas communications offline property via the IPropertiesManager.
        /// </summary>
        public const string SasCommunicationsOfflineKey = "Sas.SasCommmunicationsOffline";

        /// <summary>The key for returning whether or not Aft custom tickets are supported.</summary>
        public const string AftCustomTicketsSupportedKey = "Sas.AftCustomTicketsSupported";

        /// <summary>
        /// The key for returning whether or not Aft transaction receipts are supported.
        /// </summary>
        public const string AftTransactionReceiptsSupportedKey = "Sas.AftTransactionReceiptsSupported";

        /// <summary>The key for returning whether or not Aft lock after transfer is supported. </summary>
        public const string AftLockAfterTransferSupportedKey = "Sas.AftLockAfterTransferSupported";

        /// <summary>
        /// The key for returning whether or not the client can choose the Aft cashout device.
        /// </summary>
        public const string AftClientChoosesCashOutDeviceKey = "Sas.AftClientChoosesCashOutDevice";

        /// <summary>The key for returning whether or not Aft relaxed lock check is enabled.</summary>
        public const string RelaxedAftLockCheckKey = "Sas.RelaxedAftLockCheck";

        /// <summary>
        /// The key for returning whether or not lock for final Aft interrogate is enabled.
        /// </summary>
        public const string LockForFinalAftInterrogateKey = "Sas.LockForFinalAftInterrogate";

        /// <summary>The key for returning whether or not a hopper is supported.</summary>
        public const string HopperSupportedKey = "Sas.HopperSupported";

        /// <summary>
        /// The key for returning the value indicating the token denomination supported.
        /// </summary>
        public const string TokenDenominationKey = "Sas.TokenDenomination";

        /// <summary>The key for returning whether or not a coin acceptor is supported.</summary>
        public const string CoinAcceptorSupportedKey = "Sas.CoinAcceptorSupported";

        /// <summary>The key for returning whether or not tracking hopper percentage is enabled.</summary>
        public const string TrackHopperPercentageSupportedKey = "Sas.TrackHopperPercentageSupported";

        /// <summary>The key for returning whether or not tracking hopper level is enabled.</summary>
        public const string TrackHopperLevelSupportedKey = "Sas.TrackHopperLevelSupported";

        /// <summary>The key for returning whether or not remote hand pay reset is supported.</summary>
        public const string RemoteHandPayResetSupportedKey = "Sas.RemoteHandPayResetSupported";

        /// <summary>The key for returning whether or not multiple games are allowed.</summary>
        public const string MultipleGameSupportedKey = "Sas.MultipleGameSupported";

        /// <summary>The key for returning whether or not multiple denominations are allowed.</summary>
        public const string MultipleDenominationSupportedKey = "Sas.MultipleDenominationSupported";

        /// <summary>The key for returning whether or not tournaments are supported.</summary>
        public const string TournamentSupportedKey = "Sas.TournamentSupported";

        /// <summary>The key for returning whether or not progressives are supported.</summary>
        public const string ProgressivesSupportedKey = "Sas.ProgressivesSupported";

        /// <summary>The key for returning whether or not component authentication is supported.</summary>
        public const string ComponentAuthenticationSupportedKey = "Sas.ComponentAuthenticationSupported";

        /// <summary>The key for returning whether or not jackpot multiplier is supported.</summary>
        public const string JackpotMultiplierSupportedKey = "Sas.JackpotMultiplierSupported";

        /// <summary>
        /// The key for returning whether or not packets should be compared entirely
        /// for implied acknowledgements of messages received.
        /// </summary>
        public const string CompareEntirePacketForImpliedAcknowledgementKey = "Sas.CompareEntirePacketForImpliedAcknowledgement";

        /// <summary>The key for returning whether or not Sas logging is supported.</summary>
        public const string LoggingSupportedKey = "Sas.LoggingSupported";

        /// <summary>The key for returning whether or not ticketing is enabled.</summary>
        public const string TicketingSupportedKey = "Sas.TicketingSupported";

        /// <summary>The key for returning whether or not ticket redemption is supported.</summary>
        public const string TicketRedemptionSupportedKey = "Sas.TicketRedemptionSupported";

        /// <summary>The key for returning whether or not restricted tickets are allowed.</summary>
        public const string RestrictedTicketsSupportedKey = "Sas.RestrictedTicketsSupported";

        /// <summary>The key for returning whether or not foreign restricted tickets are allowed.</summary>
        public const string ForeignRestrictedTicketsSupportedKey = "Sas.ForeignRestrictedTicketsSupported";

        /// <summary>The key for returning whether or not hand pay validation is supported.</summary>
        public const string HandPayValidationSupportedKey = "Sas.HandPayValidationSupported";

        /// <summary>
        /// The key for returning whether or not the printer can be used to print hand pay receipts.
        /// </summary>
        public const string PrinterAsHandPayDeviceSupportedKey = "Sas.PrinterAsHandPayDeviceSupported";

        /// <summary>
        /// The key for returning whether or not the printer can be used as the cashout device.
        /// </summary>
        public const string PrinterAsCashOutDeviceSupportedKey = "Sas.PrinterAsCashOutDeviceSupported";

        /// <summary>The key for returning the default location to appear on a ticket.</summary>
        public const string DefaultLocationKey = "Sas.DefaultLocation";

        /// <summary>
        /// The key for returning the default first line of an address to appear on a ticket.
        /// </summary>
        public const string DefaultAddressLine1Key = "Sas.DefaultAddressLine1";

        /// <summary>
        /// The key for returning the default second line of an address to appear on a ticket.
        /// </summary>
        public const string DefaultAddressLine2Key = "Sas.DefaultAddressLine2";

        /// <summary>The key for returning the default restricted title to appear on a ticket.</summary>
        public const string DefaultRestrictedTitleKey = "Sas.DefaultRestrictedTitle";

        /// <summary>The key for returning the default debit title to appear on a ticket.</summary>
        public const string DefaultDebitTitleKey = "Sas.DefaultDebitTitle";

        /// <summary>The key for returning the string that is on the Aft transfer receipt location line.</summary>
        public const string AftTransferReceiptLocationLine = "Sas.AftTransferReceiptLocationLine";

        /// <summary>The key for returning the string that is on the Aft transfer receipt address1 line.</summary>
        public const string AftTransferReceiptAddressLine1 = "Sas.AftTransferReceiptAddressLine1";

        /// <summary>The key for returning the string that is on the Aft transfer receipt address2 line.</summary>
        public const string AftTransferReceiptAddressLine2 = "Sas.AftTransferReceiptAddressLine2";

        /// <summary>The key for returning the string that is on the Aft transfer receipt in-house1 line.</summary>
        public const string AftTransferReceiptInHouseLine1 = "Sas.AftTransferReceiptInHouseLine1";

        /// <summary>The key for returning the string that is on the Aft transfer receipt in-house2 line.</summary>
        public const string AftTransferReceiptInHouseLine2 = "Sas.AftTransferReceiptInHouseLine2";

        /// <summary>The key for returning the string that is on the Aft transfer receipt in-house3 line.</summary>
        public const string AftTransferReceiptInHouseLine3 = "Sas.AftTransferReceiptInHouseLine3";

        /// <summary>The key for returning the string that is on the Aft transfer receipt in-house4 line.</summary>
        public const string AftTransferReceiptInHouseLine4 = "Sas.AftTransferReceiptInHouseLine4";

        /// <summary>The key for returning the string that is on the Aft transfer receipt debit1 line.</summary>
        public const string AftTransferReceiptDebitLine1 = "Sas.AftTransferReceiptDebitLine1";

        /// <summary>The key for returning the string that is on the Aft transfer receipt debit2 line.</summary>
        public const string AftTransferReceiptDebitLine2 = "Sas.AftTransferReceiptDebitLine2";

        /// <summary>The key for returning the string that is on the Aft transfer receipt debit3 line.</summary>
        public const string AftTransferReceiptDebitLine3 = "Sas.AftTransferReceiptDebitLine3";

        /// <summary>The key for returning the string that is on the Aft transfer receipt debit4 line.</summary>
        public const string AftTransferReceiptDebitLine4 = "Sas.AftTransferReceiptDebitLine4";

        /// <summary>If there are any pending ticket data statuses to be cleared</summary>
        public const string ExtendedTicketDataStatusClearPending = "Sas.ExtendedTicketDataStatusClearPending";

        /// <summary>Whether an AFT transfer needs to be cleared</summary>
        public const string AftTransferInterrogatePending = "Sas.AftTransferInterrogatePending";

        /// <summary>Sas Version property.</summary>
        public const string SasVersion = "Sas.Version";

        /// <summary>Previously selected GameId to compare with current selection.</summary>
        public const string PreviousSelectedGameId = "Sas.PreviousSelectedGameId";

        /// <summary>Whether tickets are added to drop meters or not</summary>
        public const string TicketsToDropMetersKey = "Sas.TicketsToDropMeters";

        /// <summary>How credits are metered</summary>
        public const string MeterModelKey = "Sas.MeterModel";

        /// <summary>Whether extended meters are supported</summary>
        public const string ExtendedMetersSupportedKey = "Sas.ExtendedMetersSupported";

        /// <summary>Whether the jackpot keyoff exception is supported</summary>
        public const string JackpotKeyoffExceptionSupportedKey = "Sas.JackpotKeyoffExceptionSupported";

        /// <summary>Whether multi-denomination extensions are supported</summary>
        public const string MultiDenomExtensionsSupportedKey = "Sas.MultiDenomExtensionsSupported";

        /// <summary>Whether the 40ms max polling rate is supported</summary>
        public const string MaxPollingRateSupportedKey = "Sas.MaxPollingRateSupported";

        /// <summary>Whether multiple SAS progressive win reporting is supported</summary>
        public const string MultipleSasProgressiveWinReportingSupportedKey = "Sas.MultipleSasProgressiveWinReportingSupported";

        /// <summary>Whether meter change notifications are supported in SAS</summary>
        public const string MeterChangeNotificationSupportedKey = "Sas.MeterChangeNotificationSupported";

        /// <summary>Key to get the meter collect status in SAS</summary>
        public const string MeterCollectStatusKey = "Sas.MeterCollectStatus";

        /// <summary>Whether session play (purchasing multiple games at once) is supported</summary>
        public const string SessionPlaySupportedKey = "Sas.SessionPlaySupported";

        /// <summary>Whether we can accept foreign currency</summary>
        public const string ForeignCurrencyRedemptionSupportedKey = "Sas.ForeignCurrencyRedemptionSupported";

        /// <summary>Whether enhanced progressive data reporting is supported</summary>
        public const string EnhancedProgressiveDataReportingKey = "Sas.EnhancedProgressiveDataReporting";

        /// <summary>Whether a maximum progressive payback is supported</summary>
        public const string MaxProgressivePaybackSupportedKey = "Sas.MaxProgressivePaybackSupported";

        /// <summary>Whether SAS can alter the asset number</summary>
        public const string ChangeAssetNumberSupportedKey = "Sas.ChangeAssetNumberSupportedKey";

        /// <summary>Whether SAS can alter the floor location</summary>
        public const string ChangeFloorLocationSupportedKey = "Sas.ChangeFloorLocationSupportedKey";
    }
}
