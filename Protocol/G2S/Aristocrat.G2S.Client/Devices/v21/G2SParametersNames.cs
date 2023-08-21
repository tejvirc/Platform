namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;

    /// <summary>
    ///     Stores all constants for g2s params names.
    /// </summary>
    public static class G2SParametersNames
    {
        /// <summary>
        ///     The configuration identifier parameter name
        /// </summary>
        public const string ConfigurationIdParameterName = "G2S_configurationId";

        /// <summary>
        ///     The use default configuration parameter name
        /// </summary>
        public const string UseDefaultConfigParameterName = "G2S_useDefaultConfig";

        /// <summary>
        ///     The required for play parameter name
        /// </summary>
        public const string RequiredForPlayParameterName = "G2S_requiredForPlay";

        /// <summary>
        ///     The restart status parameter name.
        /// </summary>
        public const string RestartStatusParameterName = "G2S_restartStatus";

        /// <summary>
        ///     The configuration date time parameter name
        /// </summary>
        public const string ConfigDateTimeParameterName = "G2S_configDateTime";

        /// <summary>
        ///     The configuration complete parameter name
        /// </summary>
        public const string ConfigCompleteParameterName = "G2S_configComplete";

        /// <summary>
        ///     The no response timer parameter name.
        /// </summary>
        public const string NoResponseTimerParameterName = "G2S_noResponseTimer";

        /// <summary>
        ///     The device class parameter name
        /// </summary>
        public const string DeviceClassParameterName = "G2S_deviceClass";

        /// <summary>
        ///     The device identifier parameter name
        /// </summary>
        public const string DeviceIdParameterName = "G2S_deviceId";

        /// <summary>
        ///     The currency id parameter name.
        /// </summary>
        public const string CurrencyIdParameterName = "G2S_currencyId";

        /// <summary>
        ///     The no response timer parameter name.
        /// </summary>
        public const string TimeToLiveParameterName = "G2S_timeToLive";

        /// <summary>
        ///     Constants specific for Event handler device.
        /// </summary>
        public static class EventHandlerDevice
        {
            /// <summary>
            ///     The minimum log entries parameter name.
            /// </summary>
            public const string MinLogEntriesParameterName = "G2S_minLogEntries";

            /// <summary>
            ///     The queue behavior parameter name.
            /// </summary>
            public const string QueueBehaviorParameterName = "G2S_queueBehavior";

            /// <summary>
            ///     The disable behavior parameter name.
            /// </summary>
            public const string DisableBehaviorParameterName = "G2S_disableBehavior";

            /// <summary>
            ///     The event code parameter name.
            /// </summary>
            public const string EventCodeParameterName = "G2S_eventCode";

            /// <summary>
            ///     The force device status parameter name.
            /// </summary>
            public const string ForceDeviceStatusParameterName = "G2S_forceDeviceStatus";

            /// <summary>
            ///     The force transaction parameter name.
            /// </summary>
            public const string ForceTransactionParameterName = "G2S_forceTransaction";

            /// <summary>
            ///     The force class meters parameter name.
            /// </summary>
            public const string ForceClassMetersParameterName = "G2S_forceClassMeters";

            /// <summary>
            ///     The force device meters parameter name.
            /// </summary>
            public const string ForceDeviceMetersParameterName = "G2S_forceDeviceMeters";

            /// <summary>
            ///     The force updatable meters parameter name.
            /// </summary>
            public const string ForceUpdatableMetersParameterName = "G2S_forceUpdatableMeters";

            /// <summary>
            ///     The persist event parameter name.
            /// </summary>
            public const string ForcePersistsParameterName = "G2S_forcePersist";
        }

        /// <summary>
        ///     Constants specific for Printer device.
        /// </summary>
        public static class PrinterDevice
        {
            /// <summary>
            ///     The template index parameter name.
            /// </summary>
            public const string TemplateIndexParameterName = "G2S_templateIndex";

            /// <summary>
            ///     The template config parameter name.
            /// </summary>
            public const string TemplateConfigParameterName = "G2S_templateConfig";

            /// <summary>
            ///     The region index parameter name.
            /// </summary>
            public const string RegionIndexParameterName = "G2S_regionIndex";

            /// <summary>
            ///     The region config parameter name.
            /// </summary>
            public const string RegionConfigParameterName = "G2S_regionConfig";

            /// <summary>
            ///     The host-initiated print requests parameter name.
            /// </summary>
            public const string HostInitiatedParameterName = "G2S_hostInitiated";

            /// <summary>
            ///     The custom templates parameter name.
            /// </summary>
            public const string CustomTemplatesParameterName = "G2S_customTemplates";

            /// <summary>
            ///     The ID reader to use.
            /// </summary>
            public const string IgtIdReaderIdParameterName = "IGT_idReaderId";
        }

        /// <summary>
        ///     Constants specific for CoinAcceptor device.
        /// </summary>
        public static class CoinAcceptorDevice
        {
            /// <summary>
            ///     The denomination identifier parameter name.
            /// </summary>
            public const string DenomIdParameterName = "G2S_denomId";

            /// <summary>
            ///     The coin is a token parameter name.
            /// </summary>
            public const string TokenParameterName = "G2S_token";

            /// <summary>
            ///     The cashable exchange value per unit parameter name.
            /// </summary>
            public const string BaseCashableAmtParameterName = "G2S_baseCashableAmt";

            /// <summary>
            ///     The coin activated parameter name.
            /// </summary>
            public const string CoinActiveParameterName = "G2S_coinActive";

            /// <summary>
            ///     The promotional exchange value per init parameter name.
            /// </summary>
            public const string BasePromoAmtParameterName = "G2S_basePromoAmt";

            /// <summary>
            ///     The non-cashable exchange value per unit parameter name.
            /// </summary>
            public const string BaseNonCashAmtParameterName = "G2S_baseNonCashAmt";
        }

        /// <summary>
        ///     Constants specific for Cabinet device.
        /// </summary>
        public static class CabinetDevice
        {
            /// <summary>
            ///     The machine number parameter name.
            /// </summary>
            public const string MachineNumberParameterName = "G2S_machineNum";

            /// <summary>
            ///     The machine id parameter name.
            /// </summary>
            public const string MachineIdParameterName = "G2S_machineId";

            /// <summary>
            ///     The report denom id parameter name.
            /// </summary>
            public const string ReportDenomIdParameterName = "G2S_reportDenomId";

            /// <summary>
            ///     The locale id parameter name.
            /// </summary>
            public const string LocaleIdParameterName = "G2S_localeId";

            /// <summary>
            ///     The area id parameter name.
            /// </summary>
            public const string AreaIdParameterName = "G2S_areaId";

            /// <summary>
            ///     The zone id parameter name.
            /// </summary>
            public const string ZoneIdParameterName = "G2S_zoneId";

            /// <summary>
            ///     The bank id parameter name.
            /// </summary>
            public const string BankIdParameterName = "G2S_bankId";

            /// <summary>
            ///     The egm position parameter name.
            /// </summary>
            public const string EgmPositionParameterName = "G2S_egmPosition";

            /// <summary>
            ///     The machine location parameter name.
            /// </summary>
            public const string MachineLocationParameterName = "G2S_machineLoc";

            /// <summary>
            ///     The cabinet style parameter name.
            /// </summary>
            public const string CabinetStyleParameterName = "G2S_cabinetStyle";

            /// <summary>
            ///     The idle time period parameter name.
            /// </summary>
            public const string IdleTimePeriodParameterName = "G2S_idleTimePeriod";

            /// <summary>
            ///     The time zone offset parameter name.
            /// </summary>
            public const string TimeZoneOffsetParameterName = "G2S_timeZoneOffset";

            /// <summary>
            ///     The large win limit parameter name.
            /// </summary>
            public const string LargeWinLimitParameterName = "G2S_largeWinLimit";

            /// <summary>
            ///     The maximum credit meter parameter name.
            /// </summary>
            public const string MaxCreditMeterParameterName = "G2S_maxCreditMeter";

            /// <summary>
            ///     The maximum hopper pay out parameter name.
            /// </summary>
            public const string MaxHopperPayOutParameterName = "G2S_maxHopperPayOut";

            /// <summary>
            ///     The split pay out parameter name.
            /// </summary>
            public const string SplitPayOutParameterName = "G2S_splitPayOut";

            /// <summary>
            ///     The accept non cash amounts parameter name.
            /// </summary>
            public const string AcceptNonCashableMoneyParameterName = "G2S_acceptNonCashAmts";

            /// <summary>
            ///     The G2S reset supported parameter name.
            /// </summary>
            public const string G2SResetSupportedParameterName = "G2S_g2sResetSupported";

            /// <summary>
            ///     The configuration delay period parameter name.
            /// </summary>
            public const string ConfigDelayPeriodParameterName = "G2S_configDelayPeriod";

            /// <summary>
            ///     The enhanced configuration mode parameter name.
            /// </summary>
            public const string EnhancedConfigModeParameterName = "G2S_enhancedConfigMode";

            /// <summary>
            ///     The cash out on disable parameter name.
            /// </summary>
            public const string CashOutOnDisableParameterName = "G2S_cashOutOnDisable";

            /// <summary>
            ///     The faults supported parameter name.
            /// </summary>
            public const string FaultsSupportedParameterName = "G2S_faultsSupported";

            /// <summary>
            ///     The time zone supported parameter name.
            /// </summary>
            public const string TimeZoneSupportedParameterName = "G2S_timeZoneSupported";

            /// <summary>
            ///     The property identifier parameter name.
            /// </summary>
            public const string PropertyIdParameterName = "G2S_propertyId";

            /// <summary>
            ///     The maximum enabled themes parameter name.
            /// </summary>
            public const string MaxEnabledThemesParameterName = "G2S_maxEnabledThemes";

            /// <summary>
            ///     The maximum active denoms parameter name.
            /// </summary>
            public const string MaxActiveDenominationsParameterName = "G2S_maxActiveDenoms";

            /// <summary>
            ///     The occupancy time out parameter name.
            /// </summary>
            public const string OccupancyTimeOutParameterName = "G2S_occupancyTimeOut";

            /// <summary>
            ///     The master reset allowed parameter name
            /// </summary>
            public const string MasterResetAllowedParameterName = "GTK_masterResetAllowed";

            /// <summary>
            ///     The reel stop parameter name
            /// </summary>
            public const string ReelStopParameterName = "G2S_reelStop";

            /// <summary>
            ///     The reel duration parameter name
            /// </summary>
            public const string ReelDurationParameterName = "G2S_reelDuration";

            /// <summary>
            ///     The cash out behavior parameter name
            /// </summary>
            public const string CashoutBehaviorParameterName = "IGT_autoCashoutBehaviorForWinsExceedingCreditLimit";

            /// <summary>
            ///     The cash in limit parameter name
            /// </summary>
            public const string CashInLimitParameterName = "IGT_cashInLimit";

            /// <summary>
            ///     The non-cash in limit parameter name
            /// </summary>
            public const string NonCashInLimitParameterName = "IGT_nonCashInLimit";

            /// <summary>
            ///     The idle text parameter name
            /// </summary>
            public const string IdleTextParameterName = "G2S_idleText";
        }

        /// <summary>
        ///     Constants specific for Download device.
        /// </summary>
        public static class DownloadDevice
        {
            /// <summary>
            ///     The no message timer parameter name
            /// </summary>
            public const string NoMessageTimerParameterName = "G2S_noMessageTimer";

            /// <summary>
            ///     The minimum package log entries parameter name
            /// </summary>
            public const string MinPackageLogEntriesParameterName = "G2S_minPackageLogEntries";

            /// <summary>
            ///     The minimum script log entries parameter name
            /// </summary>
            public const string MinScriptLogEntriesParameterName = "G2S_minScriptLogEntries";

            /// <summary>
            ///     The minimum package list entries parameter name
            /// </summary>
            public const string MinPackageListEntriesParameterName = "G2S_minPackageListEntries";

            /// <summary>
            ///     The minimum script list entries parameter name
            /// </summary>
            public const string MinScriptListEntriesParameterName = "G2S_minScriptListEntries";

            /// <summary>
            ///     The minimum authorization wait timeout parameter name
            /// </summary>
            public const string AuthorizationWaitTimeOutParameterName = "G2S_authWaitTimeOut";

            /// <summary>
            ///     The minimum authorization wait retries parameter name
            /// </summary>
            public const string AuthorizationWaitRetryParameterName = "G2S_authWaitRetry";

            /// <summary>
            ///     The download enabled parameter name
            /// </summary>
            public const string DownloadEnabledParameterName = "G2S_downloadEnabled";

            /// <summary>
            ///     The upload enabled parameter name
            /// </summary>
            public const string UploadEnabledParameterName = "G2S_uploadEnabled";

            /// <summary>
            ///     The scripting enabled parameter name
            /// </summary>
            public const string ScriptingEnabledParameterName = "G2S_scriptingEnabled";

            /// <summary>
            ///     The protocol list support parameter name
            /// </summary>
            public const string ProtocolListSupportParameterName = "G2S_protocolListSupport";

            /// <summary>
            ///     The transfer progress frequency parameter name
            /// </summary>
            public const string TransferProgressFrequencyParameterName = "GTK_transferProgressFreq";

            /// <summary>
            ///     The pause supported parameter name
            /// </summary>
            public const string PauseSupportedParameterName = "GTK_pauseSupported";

            /// <summary>
            ///     The transfer supported parameter name
            /// </summary>
            public const string AbortTransferSupportedParameterName = "GTK_abortTransferSupported";
        }

        /// <summary>
        ///     Constants specific for GAT device.
        /// </summary>
        public static class GatDevice
        {
            /// <summary>
            ///     The id reader to use parameter name.
            /// </summary>
            public const string IdReaderIdParameterName = "G2S_idReaderId";

            /// <summary>
            ///     The minimum queued component requests parameter name.
            /// </summary>
            public const string MinQueuedCompsParameterName = "G2S_minQueuedComps";

            /// <summary>
            ///     The special function parameter name.
            /// </summary>
            public const string SpecialFunctionsParameterName = "G2S_specialFunctions";
        }

        /// <summary>
        ///     Constants specific for Note Acceptor device.
        /// </summary>
        public static class NoteAcceptor
        {
            /// <summary>
            ///     The note acceptor data parameter name.
            /// </summary>
            public const string NoteAcceptorDataParameterName = "G2S_noteAcceptorData";

            /// <summary>
            ///     The note enabled parameter name.
            /// </summary>
            public const string NoteEnabledParameterName = "G2S_noteEnabled";

            /// <summary>
            ///     The voucher enabled parameter name.
            /// </summary>
            public const string VoucherEnabledParameterName = "G2S_voucherEnabled";

            /// <summary>
            ///     The denom identifier parameter name.
            /// </summary>
            public const string DenomIdParameterName = "G2S_denomId";

            /// <summary>
            ///     The base cashable amt parameter name.
            /// </summary>
            public const string BaseCashableAmountParameterName = "G2S_baseCashableAmt";

            /// <summary>
            ///     The note active parameter name.
            /// </summary>
            public const string NoteActiveParameterName = "G2S_noteActive";

            /// <summary>
            ///     The token parameter name.
            /// </summary>
            public const string TokenParameterName = "G2S_token";

            /// <summary>
            ///     The base promo amt parameter name.
            /// </summary>
            public const string BasePromoAmountParameterName = "G2S_basePromoAmt";

            /// <summary>
            ///     The base non cash amt parameter name.
            /// </summary>
            public const string BaseNonCashAmountParameterName = "G2S_baseNonCashAmt";
        }

        /// <summary>
        ///     Constants specific for an Id Reader device.
        /// </summary>
        public static class IdReaderDevice
        {
            /*
             * How to detail a parameter name.
             * Search for "paramId" to find a list of these in the G2S spec.
            /// <summary>
            ///     <ParamHelp> parameter name.
            /// </summary>
            public const string <latter-half paramId>ParameterName = "G2S_<paramId>";
            */

            // ID Reader Option Sub-Parameter Definitions

            /// <summary>
            ///     The EGM controls the ID reader parameter name.
            /// </summary>
            public const string EgmPhysicallyControlsParameterName = "G2S_egmPhysicallyControls";

            /// <summary>
            ///     The ID reader type parameter name.
            /// </summary>
            public const string IdReaderTypeParameterName = "G2S_idReaderType";

            /// <summary>
            ///     Read from track parameter name.
            /// </summary>
            public const string IdReaderTrackParameterName = "G2S_idReaderTrack";

            /// <summary>
            ///     ID validation method parameter name.
            /// </summary>
            public const string IdValidMethodParameterName = "G2S_idValidMethod";

            /// <summary>
            ///     Wait timeout in milliseconds parameter name.
            /// </summary>
            public const string WaitTimeOutParameterName = "G2S_waitTimeOut";

            /// <summary>
            ///     Do offline validation parameter name.
            /// </summary>
            public const string OffLineValidParameterName = "G2S_offLineValid";

            /// <summary>
            ///     Abandoned card timer in milliseconds parameter name.
            /// </summary>
            public const string ValidTimeOutParameterName = "G2S_validTimeOut";

            /// <summary>
            ///     ID removal delay in milliseconds parameter name.
            /// </summary>
            public const string RemovalDelayParameterName = "G2S_removalDelay";

            /// <summary>
            ///     Message duration in milliseconds parameter name.
            /// </summary>
            public const string MsgDurationParameterName = "G2S_msgDuration";

            // ID Reader Messages Sub-Parameter Definitions

            /// <summary>
            ///     Message to display when no ID is present parameter name.
            /// </summary>
            public const string AttractMsgParameterName = "G2S_attractMsg";

            /// <summary>
            ///     Message to display while waiting for validation parameter name.
            /// </summary>
            public const string WaitMsgParameterName = "G2S_waitMsg";

            /// <summary>
            ///     Message to display while a valid ID is present parameter name.
            /// </summary>
            public const string ValidMsgParameterName = "G2S_validMsg";

            /// <summary>
            ///     Message to display while an invalid ID is present parameter name.
            /// </summary>
            public const string InvalidMsgParameterName = "G2S_invalidMsg";

            /// <summary>
            ///     Message to display while a lost ID is present parameter name.
            /// </summary>
            public const string LostMsgParameterName = "G2S_lostMsg";

            /// <summary>
            ///     Message to display if an ID cannot be validated parameter name.
            /// </summary>
            public const string OffLineMsgParameterName = "G2S_offLineMsg";

            /// <summary>
            ///     Message to display if an ID is abandoned parameter name.
            /// </summary>
            public const string AbandonMsgParameterName = "G2S_abandonMsg";

            // ID Pattern Table Sub-Parameter Definitions

            /// <summary>
            ///     ID type parameter name.
            /// </summary>
            public const string IdTypeParameterName = "G2S_idType";

            /// <summary>
            ///     Expected pattern for this ID type parameter name.
            /// </summary>
            public const string OffLinePatternParameterName = "G2S_offLinePattern";

            // G2S Protocol Options 3 Sub-Parameter Definitions
            // Depreciated

            // Multi-Lingual Option Sub-Parameter Definitions
            // Messages here are the same as the 'ID Reader Messages Sub-Parameter Definitions' section

            /// <summary>
            ///     Locale identifier.
            /// </summary>
            public const string LocaleIdParameterName = "G2S_localeId";

            /// <summary>
            ///     Name of the locale.
            /// </summary>
            public const string LocaleNameParameterName = "G2S_localeName";

            // G2S No Player Messages Sub-Parameter Definitions

            /// <summary>
            ///     Indicates whether a host can disable player tracking
            ///     messages for a specific player.
            /// </summary>
            public const string NoPlayerMessagesParameterName = "G2S_noPlayerMessages";
        }

        /// <summary>
        ///     Constants specific for Voucher device.
        /// </summary>
        public static class VoucherDevice
        {
            /// <summary>
            ///     The ID reader to use parameter name.
            /// </summary>
            public const string IdReaderIdParameterName = "G2S_idReaderId"; // Voucher and ID Reader are associated

            /// <summary>
            ///     The combine cashable credit types parameter name.
            /// </summary>
            public const string CombineCashableOutParameterName = "G2S_combineCashableOut";

            /// <summary>
            ///     The allow non-cashable out parameter name.
            /// </summary>
            public const string AllowNonCashableOutParameterName = "G2S_allowNonCashOut";

            /// <summary>
            ///     The maximum validation ids parameter name.
            /// </summary>
            public const string MaxValidationIdsParameterName = "G2S_maxValIds";

            /// <summary>
            ///     The minimum level for validation ids parameter name.
            /// </summary>
            public const string MinLevelValidationIdsParameterName = "G2S_minLevelValIds";

            /// <summary>
            ///     The validation ID refresh time parameter name.
            /// </summary>
            public const string ValidationIdListRefreshParameterName = "G2S_valIdListRefresh";

            /// <summary>
            ///     The validation ID list life parameter name.
            /// </summary>
            public const string ValidationIdListLifeParameterName = "G2S_valIdListLife";

            /// <summary>
            ///     The maximum voucher hold time parameter name.
            /// </summary>
            public const string VoucherHoldTimeParameterName = "G2S_voucherHoldTime";

            /// <summary>
            ///     The print offline parameter name.
            /// </summary>
            public const string PrintOfflineParameterName = "G2S_printOffLine";

            /// <summary>
            ///     The expire days cash promo parameter name.
            /// </summary>
            public const string ExpireCashPromoParameterName = "G2S_expireCashPromo";

            /// <summary>
            ///     The print cash promo expiration parameter name.
            /// </summary>
            public const string PrintExpirationCashPromoParameterName = "G2S_printExpCashPromo";

            /// <summary>
            ///     The expire non-cashable parameter name.
            /// </summary>
            public const string ExpireNonCashParameterName = "G2S_expireNonCash";

            /// <summary>
            ///     The Print Non-Cashable Expiration parameter name.
            /// </summary>
            public const string PrintExpireNonCashParameterName = "G2S_printExpNonCash";

            /// <summary>
            ///     The property name parameter name.
            /// </summary>
            public const string PropertyNameParameterName = "G2S_propName";

            /// <summary>
            ///     The property line 1 parameter name.
            /// </summary>
            public const string PropertyLine1ParameterName = "G2S_propLine1";

            /// <summary>
            ///     The property line 2 parameter name.
            /// </summary>
            public const string PropertyLine2ParameterName = "G2S_propLine2";

            /// <summary>
            ///     The cashable title parameter name.
            /// </summary>
            public const string TitleCashParameterName = "G2S_titleCash";

            /// <summary>
            ///     The promotional title parameter name.
            /// </summary>
            public const string TitlePromoParameterName = "G2S_titlePromo";

            /// <summary>
            ///     The non-cashable title parameter name.
            /// </summary>
            public const string TitleNonCashParameterName = "G2S_titleNonCash";

            /// <summary>
            ///     The large win title parameter name.
            /// </summary>
            public const string TitleLargeWinParameterName = "G2S_titleLargeWin";

            /// <summary>
            ///     The bonus cashable title parameter name.
            /// </summary>
            public const string TitleBonusCashParameterName = "G2S_titleBonusCash";

            /// <summary>
            ///     The bonus promotional title parameter name.
            /// </summary>
            public const string TitleBonusPromoParameterName = "G2S_titleBonusPromo";

            /// <summary>
            ///     The bonus non-cashable title parameter name.
            /// </summary>
            public const string TitleBonusNonCashParameterName = "G2S_titleBonusNonCash";

            /// <summary>
            ///     The WAT cashable title parameter name.
            /// </summary>
            public const string TitleWatCashParameterName = "G2S_titleWatCash";

            /// <summary>
            ///     The WAT promotional title parameter name.
            /// </summary>
            public const string TitleWatPromoParameterName = "G2S_titleWatPromo";

            /// <summary>
            ///     The WAT non-cash title parameter name.
            /// </summary>
            public const string TitleWatNonCashParameterName = "G2S_titleWatNonCash";

            /// <summary>
            ///     The allow validation data parameter name.
            /// </summary>
            public const string AllowVoucherIssueParameterName = "G2S_allowVoucherIssue";

            /// <summary>
            ///     The voucher redemption allowed parameter name.
            /// </summary>
            public const string AllowVoucherRedeemParameterName = "G2S_allowVoucherRedeem";

            /// <summary>
            ///     The print non-cashable vouchers when offline parameter name.
            /// </summary>
            public const string PrintNonCashOfflineParameterName = "G2S_printNonCashOffLine";

            /// <summary>
            ///     The maximum online voucher parameter name.
            /// </summary>
            public const string MaxOnLinePayOutParameterName = "G2S_maxOnLinePayOut";

            /// <summary>
            ///     The maximum offline voucher parameter name.
            /// </summary>
            public const string MaxOffLinePayOutParameterName = "G2S_maxOffLinePayOut";

            /// <summary>
            ///     The cash out to voucher parameter name.
            /// </summary>
            public const string CashOutToVoucherParameterName = "G2S_cashOutToVoucher";

            /// <summary>
            ///     The redeem pattern parameter name.
            /// </summary>
            public const string RedeemPrefixParameterName = "G2S_redeemPrefix";
        }

        /// <summary>
        ///     Constants specific for GamePlay Device.
        /// </summary>
        public static class GamePlayDevice
        {
            /// <summary>
            ///     The theme Id parameter name.
            /// </summary>
            public const string ThemeIdParameterName = "G2S_themeId";

            /// <summary>
            ///     The paytable Id parameter name.
            /// </summary>
            public const string PaytableIdParameterName = "G2S_paytableId";

            /// <summary>
            ///     Max wager credits parameter name.
            /// </summary>
            public const string MaxWagerCreditsParameterName = "G2S_maxWagerCredits";

            /// <summary>
            ///     Progressive allowed parameter name.
            /// </summary>
            public const string ProgAllowedParameterName = "G2S_progAllowed";

            /// <summary>
            ///     Secondary game allowed parameter name.
            /// </summary>
            public const string SecondaryAllowedParameterName = "G2S_secondaryAllowed";

            /// <summary>
            ///     Central determination parameter name.
            /// </summary>
            public const string CentralAllowedParameterName = "G2S_centralAllowed";

            /// <summary>
            ///     Standard play parameter name.
            /// </summary>
            public const string StandardPlayParameterName = "G2S_standardPlay";

            /// <summary>
            ///     Tournament play parameter name.
            /// </summary>
            public const string TournamentPlayParameterName = "G2S_tournamentPlay";

            /// <summary>
            ///     Denomination list parameter name.
            /// </summary>
            public const string DenomListParameterName = "G2S_denomList";

            /// <summary>
            ///     Denomination value parameter name.
            /// </summary>
            public const string DenomIdParameterName = "G2S_denomId";

            /// <summary>
            ///     Denom active flag parameter name.
            /// </summary>
            public const string DenomActiveParameterName = "G2S_denomActive";
        }

        /// <summary>
        ///     Chooser device options
        /// </summary>
        public static class ChooserDevice
        {
            /// <summary>
            ///     The position parameter data table parameter name.
            /// </summary>
            public const string GameComboPositionParameterName = "G2S_gameComboPosPriorityData";

            /// <summary>
            ///     The game tag data table parameter name.
            /// </summary>
            public const string GameTagDataParameterName = "G2S_gameComboTagData";

            /// <summary>
            ///     The game play id parameter name.
            /// </summary>
            public const string GamePlayIdParameterName = "G2S_gamePlayId";

            /// <summary>
            ///     The theme id parameter name.
            /// </summary>
            public const string ThemeIdParameterName = "G2S_themeId";

            /// <summary>
            ///     The pay table id parameter name.
            /// </summary>
            public const string PaytableIdParameterName = "G2S_payTableId";

            /// <summary>
            ///     The denom id parameter name.
            /// </summary>
            public const string DenomIdParameterName = "G2S_denomId";

            /// <summary>
            ///     The game position parameter name.
            /// </summary>
            public const string PositionParameterName = "G2S_gamePosPriority";

            /// <summary>
            ///     The game tag name.
            /// </summary>
            public const string GameTagParameterName = "G2S_gameTag";
        }

        /// <summary>
        ///     Player device options
        /// </summary>
        public static class PlayerDevice
        {
            /// <summary>
            ///     EGM can display messages
            /// </summary>
            public const string DisplayPresentParameterName = "G2S_displayPresent";

            /// <summary>
            ///     ID reader device to use for player tracking
            /// </summary>
            public const string IdReaderParameterName = "G2S_idReaderId";

            /// <summary>
            ///     The game play id parameter name.
            /// </summary>
            public const string MinimumTheoHoldPercentageParameterName = "G2S_minTheoHoldPct";

            /// <summary>
            ///     Number of implied decimal places for point display
            /// </summary>
            public const string DecimalPointsParameterName = "G2S_decimalPoints";

            /// <summary>
            ///     Terminate session on inactivity
            /// </summary>
            public const string EndSessionOnInactivityParameterName = "G2S_inactiveSessionEnd";

            /// <summary>
            ///     How often to send interval ratings
            /// </summary>
            public const string IntervalPeriodParameterName = "G2S_intervalPeriod";

            /// <summary>
            ///     Generate interval ratings when selected game combo changes
            /// </summary>
            public const string GamePlayIntervalParameterName = "G2S_gamePlayInterval";

            /// <summary>
            ///     The Message duration (in milliseconds)
            /// </summary>
            public const string MessageDurationParameterName = "G2S_msgDuration";

            /// <summary>
            ///     Meter basis used for countdown calculation
            /// </summary>
            public const string CountBasisParameterName = "G2S_countBasis";

            /// <summary>
            ///     Count direction (up or down)
            /// </summary>
            public const string CountDirectionParameterName = "G2S_countDirection";

            /// <summary>
            ///     The Countdown value needed to earn a point
            /// </summary>
            public const string BaseTargetParameterName = "G2S_baseTarget";

            /// <summary>
            ///     The Value of basis change (in units of countBasis) needed to change the countdown
            /// </summary>
            public const string BaseIncrementParameterName = "G2S_baseIncrement";

            /// <summary>
            ///     How many points are awarded when countdown target is achieved
            /// </summary>
            public const string BaseAwardParameterName = "G2S_baseAward";

            /// <summary>
            ///     The Meter basis used for hot player calculation
            /// </summary>
            public const string HotPlayerBasisParameterName = "G2S_hotPlayerBasis";

            /// <summary>
            ///     The Length of hot player qualification period
            /// </summary>
            public const string HotPlayerPeriodParameterName = "G2S_hotPlayerPeriod";

            /// <summary>
            ///     Value of play during period to achieve Hot Player Limit 1 (0 = inactive)
            /// </summary>
            public const string HotPlayerLimit1ParameterName = "G2S_hotPlayerLimit1";

            /// <summary>
            ///     Value of play during period to achieve Hot Player Limit 2 (0 = inactive)
            /// </summary>
            public const string HotPlayerLimit2ParameterName = "G2S_hotPlayerLimit2";

            /// <summary>
            ///     Value of play during period to achieve Hot Player Limit 3 (0 = inactive)
            /// </summary>
            public const string HotPlayerLimit3ParameterName = "G2S_hotPlayerLimit3";

            /// <summary>
            ///     Value of play during period to achieve Hot Player Limit 4 (0 = inactive)
            /// </summary>
            public const string HotPlayerLimit4ParameterName = "G2S_hotPlayerLimit4";

            /// <summary>
            ///     Value of play during period to achieve Hot Player Limit 5 (0 = inactive)
            /// </summary>
            public const string HotPlayerLimit5ParameterName = "G2S_hotPlayerLimit5";

            /// <summary>
            ///     Indicates whether the EGM must send the meter delta information when a player session ends
            /// </summary>
            public const string SendMeterDeltaParameterName = "G2S_sendMeterDelta";

            /// <summary>
            ///     Indicates whether the EGM must send the meter delta information when a player session ends
            /// </summary>
            public const string MultipleValidationDevicesParameterName = "G2S_useMultipleIdDevices";

            /// <summary>
            ///     Additional validation devices for players
            /// </summary>
            public const string PlayerValidationDeviceParameterName = "G2S_playerValDeviceParams";

            /// <summary>
            ///     Indicates whether the EGM must send the meter delta information when a player session ends
            /// </summary>
            public const string IdReaderTypeParameterName = "G2S_idReaderType";

            /// <summary>
            ///     Indicates whether the EGM must send the meter delta information when a player session ends
            /// </summary>
            public const string IdReaderIdParameterName = "G2S_idReaderId";

            /// <summary>
            ///     Indicates whether the EGM must send the meter delta information when a player session ends
            /// </summary>
            public const string IdReaderLinkedParameterName = "G2S_idReaderLinked";
        }

        /// <summary>
        ///     Informed player device options
        /// </summary>
        public static class InformedPlayerDevice
        {
            /// <summary>
            ///     The no message timer parameter name
            /// </summary>
            public const string NoMessageTimerParameterName = "G2S_noMessageTimer";

            /// <summary>
            ///     The no host text parameter name
            /// </summary>
            public const string NoHostTextParameterName = "G2S_noHostText";

            /// <summary>
            ///     The uncarded money in parameter name
            /// </summary>
            public const string UncardedMoneyInParameterName = "G2S_uncardedMoneyIn";

            /// <summary>
            ///     The uncarded game play parameter name
            /// </summary>
            public const string UncardedGamePlayParameterName = "G2S_uncardedGamePlay";

            /// <summary>
            ///     The session start money in parameter name
            /// </summary>
            public const string SessionStartMoneyInParameterName = "G2S_sessionStartMoneyIn";

            /// <summary>
            ///     The session start game play parameter name
            /// </summary>
            public const string SessionStartGamePlayParameterName = "G2S_sessionStartGamePlay";

            /// <summary>
            ///     The session start cash out parameter name
            /// </summary>
            public const string SessionStartCashOutParameterName = "G2S_sessionStartCashOut";

            /// <summary>
            ///     The session end cash out parameter name
            /// </summary>
            public const string SessionEndCashOutParameterName = "G2S_sessionEndCashOut";

            /// <summary>
            ///     The session start PIN entry parameter name
            /// </summary>
            public const string SessionStartPinEntryParameterName = "G2S_sessionStartPinEntry";

            /// <summary>
            ///     The session start limit parameter name
            /// </summary>
            public const string SessionStartLimitParameterName = "G2S_sessionStartLimit";
        }

        /// <summary>
        ///     Informed player device options
        /// </summary>
        public static class MediaDisplayDevice
        {
            /// <summary>
            ///     Device priority compared to other devices
            /// </summary>
            public const string MediaDisplayPriorityParameterName = "IGT_mediaDisplayPriority";

            /// <summary>
            ///     Screen type
            /// </summary>
            public const string ScreenTypeParameterName = "IGT_screenType";

            /// <summary>
            ///     Screen description
            /// </summary>
            public const string ScreenDescriptionParameterName = "IGT_screenDescription";

            /// <summary>
            ///     Display type
            /// </summary>
            public const string MediaDisplayTypeParameterName = "IGT_mediaDisplayType";

            /// <summary>
            ///     Display position on screen
            /// </summary>
            public const string MediaDisplayPositionParameterName = "IGT_mediaDisplayPosition";

            /// <summary>
            ///     Display description
            /// </summary>
            public const string MediaDisplayDescriptionParameterName = "IGT_mediaDisplayDescription";

            /// <summary>
            ///     Display's X position on screen
            /// </summary>
            public const string XPositionParameterName = "IGT_xPosition";

            /// <summary>
            ///     Display's Y position on screen
            /// </summary>
            public const string YPositionParameterName = "IGT_yPosition";

            /// <summary>
            ///     Recommended content height
            /// </summary>
            public const string ContentHeightParameterName = "IGT_contentHeight";

            /// <summary>
            ///     Recommended content width
            /// </summary>
            public const string ContentWidthParameterName = "IGT_contentWidth";

            /// <summary>
            ///     Display height on screen
            /// </summary>
            public const string MediaDisplayHeightParameterName = "IGT_mediaDisplayHeight";

            /// <summary>
            ///     Display width on screen
            /// </summary>
            public const string MediaDisplayWidthParameterName = "IGT_mediaDisplayWidth";

            /// <summary>
            ///     Whether screen is touch capable
            /// </summary>
            public const string TouchscreenCapableParameterName = "IGT_touchscreenCapable";

            /// <summary>
            ///     Local connection port
            /// </summary>
            public const string LocalConnectionPortParameterName = "IGT_localConnectionPort";

            /// <summary>
            ///     Whether device is audio capable
            /// </summary>
            public const string AudioCapableParameterName = "IGT_audioCapable";

        }

        /// <summary>
        ///     HandPay device options
        /// </summary>
        public static class HandpayDevice
        {
            /// <summary>
            ///     Configuration settings for this handpay device.
            /// </summary>
            public const string HandpayParams = "G2S_handpayParams";

            /// <summary>
            ///     Local handpays permitted when EGM is enabled
            /// </summary>
            public const string EnabledLocalHandpay = "G2S_enabledLocalHandpay";

            /// <summary>
            ///     Local key-off to credit meter permitted when EGM is enabled
            /// </summary>
            public const string EnabledLocalCredit = "G2S_enabledLocalCredit";

            /// <summary>
            ///     Local key-off to voucher permitted when EGM is enabled
            /// </summary>
            public const string EnabledLocalVoucher = "G2S_enabledLocalVoucher";

            /// <summary>
            ///     Local key-off to WAT permitted when EGM is enabled
            /// </summary>
            public const string EnabledLocalWat = "G2S_enabledLocalWat";

            /// <summary>
            ///     Remote handpays permitted when EGM is enabled
            /// </summary>
            public const string EnabledRemoteHandpay = "G2S_enabledRemoteHandpay";

            /// <summary>
            ///     Remote key-off to credit meter permitted when EGM is enabled
            /// </summary>
            public const string EnabledRemoteCredit = "G2S_enabledRemoteCredit";

            /// <summary>
            ///     Remote key-off to voucher permitted when EGM is enabled
            /// </summary>
            public const string EnabledRemoteVoucher = "G2S_enabledRemoteVoucher";

            /// <summary>
            ///     Remote key-off to WAT permitted when EGM is enabled
            /// </summary>
            public const string EnabledRemoteWat = "G2S_enabledRemoteWat";

            /// <summary>
            ///     Local handpays permitted when EGM is disabled
            /// </summary>
            public const string DisabledLocalHandpay = "G2S_disabledLocalHandpay";

            /// <summary>
            ///     Local key-off to credit meter permitted when EGM is disabled
            /// </summary>
            public const string DisabledLocalCredit = "G2S_disabledLocalCredit";

            /// <summary>
            ///     Local key-off to voucher permitted when EGM is disabled
            /// </summary>
            public const string DisabledLocalVoucher = "G2S_disabledLocalVoucher";

            /// <summary>
            ///     Local key-off to WAT permitted when EGM is disabled
            /// </summary>
            public const string DisabledLocalWat = "G2S_disabledLocalWat";

            /// <summary>
            ///     Remote handpays permitted when EGM is disabled
            /// </summary>
            public const string DisabledRemoteHandpay = "G2S_disabledRemoteHandpay";

            /// <summary>
            ///     Remote key-off to credit meter permitted when EGM is disabled
            /// </summary>
            public const string DisabledRemoteCredit = "G2S_disabledRemoteCredit";

            /// <summary>
            ///     Remote key-off to voucher permitted when EGM is disabled
            /// </summary>
            public const string DisabledRemoteVoucher = "G2S_disabledRemoteVoucher";

            /// <summary>
            ///     Remote key-off to WAT permitted when EGM is disabled
            /// </summary>
            public const string DisabledRemoteWat = "G2S_disabledRemoteWat";

            /// <summary>
            ///     Mix multiple credit types in a single handpay
            /// </summary>
            public const string MixCreditTypes = "G2S_mixCreditTypes";

            /// <summary>
            ///     Include non-cashable credits in handpay requests
            /// </summary>
            public const string RequestNonCash = "G2S_requestNonCash";

            /// <summary>
            ///     Combine cashable and promo cashable on one handpay
            /// </summary>
            public const string CombineCashableOut = "G2S_combineCashableOut";

            /// <summary>
            ///     Parameters for enabling partial handpays.
            /// </summary>
            public const string PartialHandpayParams = "G2S_partialHandpayParams";

            /// <summary>
            ///     Indicates whether requests for partial handpays should be accepted for cashable and promotional credits.
            /// </summary>
            public const string LocalKeyOff = "G2S_localKeyOff";

            /// <summary>
            ///     Indicates whether requests for partial handpays should be accepted for cashable and promotional credits.
            /// </summary>
            public const string PartialHandpays = "G2S_partialHandpays";

            /// <summary>
            ///     Parameters for enabling non-validated handpay receipts.
            /// </summary>
            public const string HandpayReceiptParams = "G2S_handpayReceiptParams";

            /// <summary>
            ///     Indicates whether non-validated handpay receipts are enabled.
            /// </summary>
            public const string EnableReceipts = "G2S_enableReceipts";

            /// <summary>
            ///     Title printed on game win and bonus pay receipts.
            /// </summary>
            public const string TitleJackpotReceipt = "G2S_titleJackpotReceipt";

            /// <summary>
            ///     Title printed on cancel credit receipts.
            /// </summary>
            public const string TitleCancelReceipt = "G2S_titleCancelReceipt";

            /// <summary>
            ///     Parameters associated with Use-Player-ID-Reader option.
            /// </summary>
            public const string UsePlayerIdReaderParams = "G2S_usePlayerIdReaderParams";

            /// <summary>
            ///     Indicates whether the ID reader associated with the currently active player session should be used.
            /// </summary>
            public const string UsePlayerIdReader = "G2S_usePlayerIdReader";
        }

        /// <summary>
        ///     Progressive device options
        /// </summary>
        public static class ProgressiveDevice
        {
            /// <summary>
            ///     Progressive Id 
            /// </summary>
            public const string ProgIdParameterName = "G2S_progId";

            /// <summary>
            ///     Progressive Level Info
            /// </summary>
            public const string NoProgInfoParameterName = "G2S_noProgInfo";
        }

        /// <summary>
        /// Bonus device options
        /// </summary>
        public static class BonusDevice
        {
            /// <summary>
            ///     The no message timer parameter name
            /// </summary>
            public const string NoMessageTimerParameterName = "G2S_noMessageTimer";

            /// <summary>
            ///     The no host text parameter name
            /// </summary>
            public const string NoHostTextParameterName = "G2S_noHostText";

            /// <summary>
            ///     The id Reader Id parameter name
            /// </summary>
            public const string IdReaderIdParameterName = "G2S_idReaderId";

            /// <summary>
            ///     The id Reader Id parameter name
            /// </summary>
            public const string MaxPendingBonusParameterName = "G2S_maxPendingBonus";

            /// <summary>
            ///     Use player id reader
            /// </summary>
            public const string UsePlayerIdReaderParameterName = "G2S_usePlayerIdReader";

            /// <summary>
            ///     Bonus Eligibility Timer
            /// </summary>
            public const string BonusEligibilityTimer = "IGT_eligibleTimer";

            /// <summary>
            ///     Maximum bonus limit
            /// </summary>
            public const string MaximumBonusLimit = "IGT_displayLimit";

            /// <summary>
            ///     Maximum bonus message
            /// </summary>
            public const string MaximumBonusLimitText = "IGT_displayLimitText";

            /// <summary>
            ///     Maximum bonus text duration
            /// </summary>
            public const string DisplayLimitDuration = "IGT_displayLimitDuration";



            /// <summary>
            ///     Card Required For Wager Match
            /// </summary>
            public const string WagerMatchCardRequired = "IGT_wmCardRequired";

            /// <summary>
            ///     Wager Match limit
            /// </summary>
            public const string WagerMatchLimit = "IGT_wmLimit";

            /// <summary>
            ///     Wager Match limit message
            /// </summary>
            public const string WagerMatchLimitText = "IGT_wmLimitText";

            /// <summary>
            ///     Wager Match limit message duration
            /// </summary>
            public const string WagerMatchLimitDuration = "IGT_wmLimitDuration";

            /// <summary>
            ///     Wager Match Exit message
            /// </summary>
            public const string WagerMatchExitText = "IGT_wmExitText";

            /// <summary>
            ///     Wager Match Exit message duration
            /// </summary>
            public const string WagerMatchExitDuration = "IGT_wmExitDuration";

            /// <summary>
            ///     The Eligibility Timer default
            /// </summary>
            public static readonly TimeSpan EligibilityTimerDefault = TimeSpan.FromSeconds(30);

            /// <summary>
            ///     The Display Limit Duration default
            /// </summary>
            public static readonly TimeSpan DisplayLimitDurationDefault = TimeSpan.FromSeconds(30);

            /// <summary>
            ///     The Wager Match Limit Duration default
            /// </summary>
            public static readonly TimeSpan WagerMatchLimitDurationDefault = TimeSpan.FromSeconds(30);

            /// <summary>
            ///     The Wager Match Exit Duration default
            /// </summary>
            public static readonly TimeSpan WagerMatchExitDurationDefault = TimeSpan.FromSeconds(5);

            /// <summary>
            ///     The bonus limit default
            /// </summary>
            public const long LimitDefault = 1000000000;
        }

        /// <summary>
        ///     SPC device options.
        /// </summary>
        public static class SpcDevice
        {
            /// <summary>
            ///     The level data parameter name.
            /// </summary>
            public const string LevelDataParameterName = "G2S_spcLevelData";

            /// <summary>
            ///     The controller type parameter name.
            /// </summary>
            public const string ControllerTypeParameterName = "G2S_standaloneProg";

            /// <summary>
            ///     The level ID parameter name.
            /// </summary>
            public const string LevelIdParameterName = "G2S_levelId";

            /// <summary>
            ///     The reset amount parameter name.
            /// </summary>
            public const string ResetAmountParameterName = "G2S_resetAmt";

            /// <summary>
            ///     The max level amount parameter name.
            /// </summary>
            public const string MaxLevelAmountParameterName = "G2S_maxLevelAmt";

            /// <summary>
            ///     The contribution percentage parameter name.
            /// </summary>
            public const string ContribPercentParameterName = "G2S_contribPercent";

            /// <summary>
            ///     The rounding enabled parameter name.
            /// </summary>
            public const string RoundingEnabledParameterName = "G2S_roundingEnabled";

            /// <summary>
            ///     The mystery minimum parameter name.
            /// </summary>
            public const string MystMinParameterName = "G2S_mystMinAmt";

            /// <summary>
            ///     The mystery maximum parameter name.
            /// </summary>
            public const string MystMaxParameterName = "G2S_mystMaxAmt";

            /// <summary>
            ///     The game configuration data parameter name.
            /// </summary>
            public const string GameConfigDataParameterName = "G2S_spcGameConfigData";

            /// <summary>
            ///     The game play ID parameter name.
            /// </summary>
            public const string GamePlayIdParameterName = "G2S_gamePlayId";

            /// <summary>
            ///     The win level index parameter name.
            /// </summary>
            public const string WinLevelIndexParameterName = "G2S_winLevelIndex";

            /// <summary>
            ///     The paytable ID parameter name.
            /// </summary>
            public const string PaytableIdParameterName = "G2S_paytableId";

            /// <summary>
            ///     The paytable ID parameter name.
            /// </summary>
            public const string ThemeIdParameterName = "G2S_themeId";

            /// <summary>
            ///     The denomination ID parameter name.
            /// </summary>
            public const string DenomIdParameterName = "G2S_denomId";
        }
    }
}
