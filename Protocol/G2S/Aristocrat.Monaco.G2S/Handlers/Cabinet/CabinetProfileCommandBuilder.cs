namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Kernel;
    using Monaco.Common;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class CabinetProfileCommandBuilder : ICommandBuilder<ICabinetDevice, cabinetProfile>
    {
        private readonly ILocalization _locale;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CabinetProfileCommandBuilder" /> class.
        /// </summary>
        /// <param name="properties">An instance of IPropertiesManager.</param>
        /// <param name="locale">An <see cref="ILocalization" /> instance.</param>
        public CabinetProfileCommandBuilder(IPropertiesManager properties, ILocalization locale)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _locale = locale ?? throw new ArgumentNullException(nameof(locale));
        }

        /// <inheritdoc />
        public async Task Build(ICabinetDevice device, cabinetProfile command)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.configurationId = device.ConfigurationId;
            command.restartStatus = device.RestartStatus;
            command.useDefaultConfig = device.UseDefaultConfig;
            command.requiredForPlay = device.RequiredForPlay;
            command.machineNum = (int)_properties.GetValue(ApplicationConstants.MachineId, (uint)0);
            command.machineId = _properties.GetValue<string>(ApplicationConstants.SerialNumber, null);

            var currencyId = _properties.GetValue<string>(ApplicationConstants.CurrencyId, null);
            command.currencyId =
                !string.IsNullOrEmpty(currencyId) ? currencyId : _locale.RegionInfo.ISOCurrencySymbol;
            command.reportDenomId = _properties.GetValue(G2S.Constants.ReportDenomId, G2S.Constants.DefaultReportDenomId);

            // ISO-639_ISO-3166
            command.localeId = device.LocaleId(_locale.CurrentCulture);
            command.areaId = _properties.GetValue<string>(ApplicationConstants.Area, null);
            command.zoneId = _properties.GetValue<string>(ApplicationConstants.Zone, null);
            command.bankId = _properties.GetValue<string>(ApplicationConstants.Bank, null);
            command.egmPosition = _properties.GetValue<string>(ApplicationConstants.Position, null);
            command.machineLoc = _properties.GetValue<string>(ApplicationConstants.Location, null);
            command.cabinetStyle =
                _properties.GetValue(G2S.Constants.CabinetStyle, G2S.Constants.DefaultCabinetStyle);

            command.largeWinLimit = _properties.GetValue(
                AccountingConstants.LargeWinLimit,
                AccountingConstants.DefaultLargeWinLimit);
            command.maxCreditMeter = _properties.GetValue(
                AccountingConstants.MaxCreditMeter,
                G2S.Constants.DefaultMaxCreditMeter);

            // TODO
            command.maxHopperPayOut = 0;
            command.splitPayOut = false;

            command.idleTimePeriod = _properties.GetValue(
                GamingConstants.IdleTimePeriod,
                (int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds);
            command.timeZoneOffset =
                _properties.GetValue(ApplicationConstants.TimeZoneOffsetKey, TimeSpan.Zero).GetFormattedOffset();

            command.acceptNonCashAmts = t_acceptNonCashAmts.G2S_acceptAlways;

            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;
            command.g2sResetSupported = false;

            command.configDelayPeriod = device.ConfigDelayPeriod;
            command.enhancedConfigMode = device.EnhancedConfigurationMode;
            command.restartStatusMode = device.RestartStatusMode;

            // command.faultsSupported
            // command.cashOutOnDisable
            command.timeZoneSupported = t_g2sBoolean.G2S_true;
            command.propertyId = _properties.GetValue<string>(ApplicationConstants.PropertyId, null);

            // command.maxEnabledThemes = -1;
            // command.maxActiveDenoms = -1;
            // command.occupancyTimeOut = 30000;
            command.masterResetAllowed = device.MasterResetAllowed;

            var idleText = _properties.GetValue(GamingConstants.IdleText, string.Empty);
            if (!string.IsNullOrEmpty(idleText))
            {
                command.idleText = idleText;
            }

            await Task.CompletedTask;
        }
    }
}