namespace Aristocrat.Monaco.G2S.Meters
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Gaming.Contracts.Meters;
    using Kernel;

    /// <summary>
    ///     Definition of the G2SMeterProvider class.
    /// </summary>
    public class G2SMeterProvider : BaseMeterProvider, IG2SMeterProvider
    {
        private readonly IG2SEgm _egm;
        private readonly IGameMeterManager _meters;
        private readonly IMetersSubscriptionManager _metersSubscriptionManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SMeterProvider" /> class.
        /// </summary>
        /// <param name="meters">Meter manager</param>
        /// <param name="subscriptionManager">Meter subscription manager.</param>
        /// <param name="egm">Egm</param>
        public G2SMeterProvider(IGameMeterManager meters, IMetersSubscriptionManager subscriptionManager, IG2SEgm egm)
            : base(typeof(G2SMeterProvider).ToString())
        {
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _metersSubscriptionManager =
                subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public void Start()
        {
            Initialize();
        }

        private void SetAtomicMeters()
        {
            // Add device meters
            foreach (var device in _egm.Devices)
            {
                SetDeviceMeters(device, _metersSubscriptionManager.WageMeters);
                SetDeviceMeters(device, _metersSubscriptionManager.CurrencyMeters);
                SetDeviceMeters(device, _metersSubscriptionManager.GameMeters);
                SetDeviceMeters(device, _metersSubscriptionManager.DeviceMeters);
            }
        }

        private void SetDeviceMeters(IDevice device, IReadOnlyDictionary<string, List<string>> meterMap)
        {
            if (!meterMap.ContainsKey(device.PrefixedDeviceClass()))
            {
                return;
            }

            foreach (var meter in meterMap[device.PrefixedDeviceClass()])
            {
                var mappedMeter = G2SMeterCollection.GetG2SMeter(meter, false);
                if (mappedMeter != null && device is IGamePlayDevice)
                {
                    var meterName = _meters.GetMeterName(device.Id, mappedMeter.MeterId);
                    if (_meters.IsMeterProvided(meterName))
                    {
                        var newMeter = meter + device.Id;
                        G2SMeterCollection.AddG2SMeter(newMeter, meterName);
                    }
                }
            }
        }

        private void Initialize()
        {
            // add G2S meters that map to the bank
            G2SMeterCollection.AddG2SMeter(
                "cabinet." + CabinetMeterName.PlayerCashableAmount,
                () => ServiceManager.GetInstance().TryGetService<IBank>()?.QueryBalance(AccountType.Cashable) ?? 0);

            G2SMeterCollection.AddG2SMeter(
                "cabinet." + CabinetMeterName.PlayerPromoAmount,
                () => ServiceManager.GetInstance().TryGetService<IBank>()?.QueryBalance(AccountType.Promo) ?? 0);

            G2SMeterCollection.AddG2SMeter(
                "cabinet." + CabinetMeterName.PlayerNonCashableAmount,
                () => ServiceManager.GetInstance().TryGetService<IBank>()?.QueryBalance(AccountType.NonCash) ?? 0);

            SetAtomicMeters();
        }
    }
}