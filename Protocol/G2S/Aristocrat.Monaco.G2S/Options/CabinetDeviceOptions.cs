namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using System.Globalization;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts;

    /// <inheritdoc />
    public class CabinetDeviceOptions : BaseDeviceOptions
    {
        private const string MachineNumParameterName = "G2S_machineNum";
        private const string MachineIdParameterName = "G2S_machineId";
        private const string CurrencyIdParameterName = "G2S_currencyId";
        private const string ReportDenomIdParameterName = "G2S_reportDenomId";
        private const string LocaleIdParameterName = "G2S_localeId";
        private const string AreaIdParameterName = "G2S_areaId";
        private const string ZoneIdParameterName = "G2S_zoneId";
        private const string BankIdParameterName = "G2S_bankId";
        private const string EgmPositionParameterName = "G2S_egmPosition";
        private const string MachineLocParameterName = "G2S_machineLoc";
        private const string CabinetStyleParameterName = "G2S_cabinetStyle";
        private const string IdleTimePeriodParameterName = "G2S_idleTimePeriod";
        private const string TimeZoneOffsetParameterName = "G2S_timeZoneOffset";
        private const string LargeWinLimitParameterName = "G2S_largeWinLimit";
        private const string MaxCreditMeterParameterName = "G2S_maxCreditMeter";
        private const string MaxHopperPayOutParameterName = "G2S_maxHopperPayOut";
        private const string SplitPayOutParameterName = "G2S_splitPayOut";
        private const string AcceptNonCashAmtsParameterName = "G2S_acceptNonCashAmts";
        private const string G2SResetSupportedParameterName = "G2S_g2sResetSupported";
        private const string EnhancedConfigModeParameterName = "G2S_enhancedConfigMode";
        private const string CashOutOnDisableParameterName = "G2S_cashOutOnDisable";
        private const string FaultsSupportedParameterName = "G2S_faultsSupported";
        private const string TimeZoneSupportedParameterName = "G2S_timeZoneSupported";
        private const string PropertyIdParameterName = "G2S_propertyId";
        private const string MaxEnabledThemesParameterName = "G2S_maxEnabledThemes";
        private const string MaxActiveDenomsParameterName = "G2S_maxActiveDenoms";
        private const string OccupancyTimeOutParameterName = "G2S_occupancyTimeOut";
        private const string MasterResetAllowedParameterName = "GTK_masterResetAllowed";
        private const string ReelStopParameterName = "G2S_reelStop";
        private const string ReelDurationOptionParameterName = "G2S_reelDuration";
        private const string CashoutBehaviorParameterName = "IGT_autoCashoutBehaviorForWinsExceedingCreditLimit";
        private const string CashInLimitParameterName = "IGT_cashInLimit";
        private const string NonCashInLimitParameterName = "IGT_nonCashInLimit";
        private const string IdleTextParameterName = "G2S_idleText";

        private static ILocalization LocaleProvider => ServiceManager.GetInstance().GetService<ILocalization>();

        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.Cabinet;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            SetMachineNum(optionConfigValues);
            SetMachineId(optionConfigValues);
            SetCurrencyId(optionConfigValues);
            SetReportDenomId(optionConfigValues);
            SetLocaleId(optionConfigValues);
            SetAreaId(optionConfigValues);
            SetZoneId(optionConfigValues);
            SetBankId(optionConfigValues);
            SetEgmPosition(optionConfigValues);
            SetMachineLoc(optionConfigValues);
            SetCabinetStyle(optionConfigValues);
            SetIdleTimePeriod(optionConfigValues);
            SetTimeZoneOffset(optionConfigValues);
            SetLargeWinLimit(optionConfigValues);
            SetMaxCreditMeter(optionConfigValues);
            SetMaxHopperPayOut(optionConfigValues);
            SetSplitPayOut(optionConfigValues);
            SetAcceptNonCashAmts(optionConfigValues);
            SetG2SResetSupported(optionConfigValues);
            SetEnhancedConfigMode(optionConfigValues);
            SetCashOutOnDisable(optionConfigValues);
            SetFaultsSupported(optionConfigValues);
            SetTimeZoneSupported(optionConfigValues);
            SetPropertyId(optionConfigValues);
            SetMaxEnabledThemes(optionConfigValues);
            SetMaxActiveDenoms(optionConfigValues);
            SetOccupancyTimeOut(optionConfigValues);
            SetMasterResetAllowed(optionConfigValues);
            SetReelDuration(optionConfigValues);
            SetReelStop(optionConfigValues);
            SetCashoutBehavior(optionConfigValues);
            SetCashinLimit(optionConfigValues);
            SetNonCashinLimit(optionConfigValues);
            SetIdleText(optionConfigValues);
        }

        private void SetMachineNum(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(MachineNumParameterName))
            {
                PropertiesManager.SetProperty(
                    ApplicationConstants.MachineId,
                    (uint)optionConfigValues.Int32Value(MachineNumParameterName));
            }
        }

        private void SetMachineId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(MachineIdParameterName))
            {
                PropertiesManager.SetProperty(
                    ApplicationConstants.SerialNumber,
                    optionConfigValues.StringValue(MachineIdParameterName));
            }
        }

        private void SetCurrencyId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(CurrencyIdParameterName))
            {
                PropertiesManager.SetProperty(
                    ApplicationConstants.CurrencyId,
                    optionConfigValues.StringValue(CurrencyIdParameterName));
            }
        }

        private void SetReportDenomId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(ReportDenomIdParameterName))
            {
                PropertiesManager.SetProperty(
                    Constants.ReportDenomId,
                    optionConfigValues.Int64Value(ReportDenomIdParameterName));
            }
        }

        private void SetLocaleId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(LocaleIdParameterName))
            {
                var localeId = optionConfigValues.StringValue(LocaleIdParameterName);
                if (!string.IsNullOrEmpty(localeId))
                {
                    LocaleProvider.CurrentCulture = CultureInfo.GetCultureInfo(localeId.Replace("_", "-"));
                }
            }
        }

        private void SetAreaId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(AreaIdParameterName))
            {
                PropertiesManager.SetProperty(
                    ApplicationConstants.Area,
                    optionConfigValues.StringValue(AreaIdParameterName));
            }
        }

        private void SetZoneId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(ZoneIdParameterName))
            {
                PropertiesManager.SetProperty(
                    ApplicationConstants.Zone,
                    optionConfigValues.StringValue(ZoneIdParameterName));
            }
        }

        private void SetBankId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(BankIdParameterName))
            {
                PropertiesManager.SetProperty(
                    ApplicationConstants.Bank,
                    optionConfigValues.StringValue(BankIdParameterName));
            }
        }

        private void SetEgmPosition(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(EgmPositionParameterName))
            {
                PropertiesManager.SetProperty(
                    ApplicationConstants.Position,
                    optionConfigValues.StringValue(EgmPositionParameterName));
            }
        }

        private void SetMachineLoc(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(MachineLocParameterName))
            {
                PropertiesManager.SetProperty(
                    ApplicationConstants.Location,
                    optionConfigValues.StringValue(MachineLocParameterName));
            }
        }

        private void SetCabinetStyle(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(CabinetStyleParameterName))
            {
                var cabinetStyle = optionConfigValues.StringValue(CabinetStyleParameterName);
                if (MatchProtocolConventionsString(cabinetStyle))
                {
                    PropertiesManager.SetProperty(
                        Constants.CabinetStyle,
                        optionConfigValues.StringValue(CabinetStyleParameterName));
                }
            }
        }

        private void SetIdleTimePeriod(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(IdleTimePeriodParameterName))
            {
                PropertiesManager.SetProperty(
                    GamingConstants.IdleTimePeriod,
                    optionConfigValues.Int32Value(IdleTimePeriodParameterName));
            }
        }

        private void SetTimeZoneOffset(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(TimeZoneOffsetParameterName))
            {
                var rawText = optionConfigValues.StringValue(TimeZoneOffsetParameterName);

                rawText = rawText.Replace("+", string.Empty); // For RGS

                var offset = TimeSpan.Parse(rawText);

                PropertiesManager.SetProperty(ApplicationConstants.TimeZoneOffsetKey, offset);
            }
        }

        private void SetLargeWinLimit(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(LargeWinLimitParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.LargeWinLimit,
                    optionConfigValues.Int64Value(LargeWinLimitParameterName));
            }
        }

        private void SetMaxCreditMeter(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(MaxCreditMeterParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.MaxCreditMeter,
                    optionConfigValues.Int64Value(MaxCreditMeterParameterName));
            }
        }

        private void SetMaxHopperPayOut(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(MaxHopperPayOutParameterName))
            {
                // ignore?
            }
        }

        private void SetSplitPayOut(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(SplitPayOutParameterName))
            {
                // ignore, hardcoded to false?
            }
        }

        private void SetAcceptNonCashAmts(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(AcceptNonCashAmtsParameterName))
            {
                // ignore, hardcoded to t_acceptNonCashAmts.G2S_acceptAlways?
            }
        }

        private void SetG2SResetSupported(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(G2SResetSupportedParameterName))
            {
                // ignore, hardcoded to false?
            }
        }

        private void SetEnhancedConfigMode(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(EnhancedConfigModeParameterName))
            {
                // ignore?
            }
        }

        private void SetCashOutOnDisable(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(CashOutOnDisableParameterName))
            {
                // ignore?
            }
        }

        private void SetFaultsSupported(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(FaultsSupportedParameterName))
            {
                // ignore?
            }
        }

        private void SetTimeZoneSupported(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(TimeZoneSupportedParameterName))
            {
                // ignore, hardcoded to t_g2sBoolean.G2S_true?
            }
        }

        private void SetPropertyId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(PropertyIdParameterName))
            {
                PropertiesManager.SetProperty(
                    ApplicationConstants.PropertyId,
                    optionConfigValues.StringValue(PropertyIdParameterName));
            }
        }

        private void SetMaxEnabledThemes(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(MaxEnabledThemesParameterName))
            {
            }
        }

        private void SetMaxActiveDenoms(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(MaxActiveDenomsParameterName))
            {
            }
        }

        private void SetOccupancyTimeOut(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(OccupancyTimeOutParameterName))
            {
            }
        }

        private void SetMasterResetAllowed(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(MasterResetAllowedParameterName))
            {
            }
        }

        private void SetReelStop(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(ReelStopParameterName))
            {
                PropertiesManager.SetProperty(
                    GamingConstants.ReelStopEnabled,
                    optionConfigValues.BooleanValue(ReelStopParameterName));
            }
        }

        private void SetReelDuration(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(ReelDurationOptionParameterName))
            {
                // If ReelDuration value is less than jurisdictional mandated value, don't set.
                if (PropertiesManager.GetValue(GamingConstants.MinimumGameRoundDuration, 0) >=
                    optionConfigValues.Int32Value(ReelDurationOptionParameterName))
                {
                    return;
                }

                PropertiesManager.SetProperty(
                     GamingConstants.GameRoundDurationMs,
                     optionConfigValues.Int32Value(ReelDurationOptionParameterName));
            }
        }

        private void SetCashoutBehavior(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(CashoutBehaviorParameterName))
            {
                // It's not clear how this option should be used
                var value = optionConfigValues.StringValue(CashoutBehaviorParameterName);
                switch (value)
                {
                    case "CASHOUTFULLBANKAMOUNT":
                        break;
                    case "CASHOUTCREDITLIMIT":
                        break;
                    case "CASHOUTWINAMOUNT":
                        break;
                }
            }
        }

        private void SetCashinLimit(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(CashInLimitParameterName))
            {
                PropertiesManager.SetProperty(
                    PropertyKey.MaxCreditsIn,
                    optionConfigValues.Int64Value(CashInLimitParameterName));
            }
        }

        private void SetNonCashinLimit(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(NonCashInLimitParameterName))
            {
            }
        }

        private void SetIdleText(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasKey(IdleTextParameterName))
            {
                PropertiesManager.SetProperty(
                    GamingConstants.IdleText,
                    optionConfigValues.StringValue(IdleTextParameterName) ?? string.Empty);
            }
        }
    }
}
