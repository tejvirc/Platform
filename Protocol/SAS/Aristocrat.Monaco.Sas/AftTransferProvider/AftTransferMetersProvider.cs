namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using Application.Contracts;
    using Contracts.Metering;
    using log4net;

    /// <summary>
    /// Definition of the SasAftTransferMetersProvider class.
    /// </summary>
    public class AftTransferMetersProvider : SasBaseMeterProvider
    {
        /// <summary>
        /// Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        /// Aft meter info for each meter.
        /// </summary>
        private static readonly MeterInfo[] SasMeterInfoList =
        {
            new MeterInfo(SasMeterNames.AftCashableIn, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftCashableInQuantity, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftRestrictedIn, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftRestrictedInQuantity, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftNonRestrictedIn, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftNonRestrictedInQuantity, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftCashableBonusIn, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftCashableBonusInQuantity, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftNonRestrictedBonusIn, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftNonRestrictedBonusInQuantity, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftCashableOut, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftCashableOutQuantity, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftRestrictedOut, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftRestrictedOutQuantity, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftNonRestrictedOut, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.AftNonRestrictedOutQuantity, new CurrencyMeterClassification()),
            new MeterInfo(SasMeterNames.TotalElectronicDebitTransfers, new CurrencyMeterClassification())
        };

        /// <summary>
        /// Initializes a new instance of the SasAftTransferMetersProvider class.
        /// </summary>
        public AftTransferMetersProvider()
            : base(typeof(AftTransferMetersProvider).ToString(), SasMeterInfoList)
        {
            Logger.Debug("Initialized");
        }
    }
}
