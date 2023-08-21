namespace Aristocrat.Monaco.G2S.Meters
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Gaming.Contracts;

    /// <summary>
    ///     An implementation of <see cref="IMeterAggregator{ICabinetDevice}" />
    /// </summary>
    public class CabinetMeterAggregator : MeterAggregatorBase<ICabinetDevice>
    {
        private readonly IBank _bank;

        private readonly IMeterManager _meterManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CabinetMeterAggregator" /> class.
        /// </summary>
        /// <param name="meterManager">An instance of an IMeterManager.</param>
        /// <param name="bank">An <see cref="IBank" /> instance</param>
        public CabinetMeterAggregator(IMeterManager meterManager, IBank bank)
            : base(meterManager, MeterMap.CabinetMeters)
        {
            _meterManager = meterManager;
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        /// <inheritdoc />
        protected override long GetValue(string meter)
        {
            switch (meter)
            {
                case CabinetMeterName.PlayerCashableAmount:
                    return _bank.QueryBalance(AccountType.Cashable);
                case CabinetMeterName.PlayerPromoAmount:
                    return _bank.QueryBalance(AccountType.Promo);
                case CabinetMeterName.PlayerNonCashableAmount:
                    return _bank.QueryBalance(AccountType.NonCash);
                case CabinetMeterName.GamesSincePowerResetCount:
                    return _meterManager.GetMeter(GamingMeters.PlayedCount).Session;
            }

            return 0;
        }
    }
}
