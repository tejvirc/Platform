namespace Aristocrat.Monaco.Mgam.Services.Meters
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Mgam.Client;
    using Common.Events;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Monitor system meters for reporting to MGAM host via SetAttribute command.
    /// </summary>
    public class MeterMonitor : IMeterMonitor, IService, IDisposable
    {
        private readonly IEventBus _events;
        private readonly IMeterManager _meters;

        private readonly Dictionary<string, MeterData> _meterMap = new Dictionary<string, MeterData>
        {
            { GamingMeters.WageredAmount, new MeterData { AttributeName = AttributeNames.CashIn, IsMoney = true, IsCashBox = false } },
            { GamingMeters.TotalPaidAmt, new MeterData { AttributeName = AttributeNames.CashOut, IsMoney = true, IsCashBox = false } },
            { ApplicationMeters.MainDoorOpenTotalCount, new MeterData { AttributeName = AttributeNames.CabinetDoor, IsMoney = false, IsCashBox = false } },
            { GamingMeters.PlayedCount, new MeterData { AttributeName = AttributeNames.Games, IsMoney = false, IsCashBox = false } },
            { GamingMeters.FailedCount, new MeterData { AttributeName = AttributeNames.GameFailures, IsMoney = false, IsCashBox = false } },
            { ApplicationMeters.CashDoorOpenTotalCount, new MeterData { AttributeName = AttributeNames.DropDoor, IsMoney = false, IsCashBox = false } },
            { GamingMeters.TotalProgWonCount, new MeterData { AttributeName = AttributeNames.ProgressiveOccurence, IsMoney = false, IsCashBox = false } },

            // the CashBox meters only get sent when requested by the 'UpdateMeters' or 'ClearMeters' command from host.
            { ApplicationMeters.TotalIn, new MeterData { AttributeName = AttributeNames.CashBox, IsMoney = true, IsCashBox = true } },
            { AccountingMeters.BillCount1s, new MeterData { AttributeName = AttributeNames.CashBoxOnes, IsMoney = false, IsCashBox = true } },
            { AccountingMeters.BillCount2s, new MeterData { AttributeName = AttributeNames.CashBoxTwos, IsMoney = false, IsCashBox = true } },
            { AccountingMeters.BillCount5s, new MeterData { AttributeName = AttributeNames.CashBoxFives, IsMoney = false, IsCashBox = true } },
            { AccountingMeters.BillCount10s, new MeterData { AttributeName = AttributeNames.CashBoxTens, IsMoney = false, IsCashBox = true } },
            { AccountingMeters.BillCount20s, new MeterData { AttributeName = AttributeNames.CashBoxTwenties, IsMoney = false, IsCashBox = true } },
            { AccountingMeters.BillCount50s, new MeterData { AttributeName = AttributeNames.CashBoxFifties, IsMoney = false, IsCashBox = true } },
            { AccountingMeters.BillCount100s, new MeterData { AttributeName = AttributeNames.CashBoxHundreds, IsMoney = false, IsCashBox = true } },
            { AccountingMeters.TotalVouchersInCount, new MeterData { AttributeName = AttributeNames.CashBoxVouchers, IsMoney = false, IsCashBox = true } },
            { AccountingMeters.TotalVouchersIn, new MeterData { AttributeName = AttributeNames.CashBoxVoucherValueTotal, IsMoney = true, IsCashBox = true } }
        };

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterMonitor"/> class.
        /// </summary>
        /// <param name="events"><see cref="IEventBus"/></param>
        /// <param name="meters"><see cref="IMeterManager"/></param>
        public MeterMonitor(
            IEventBus events,
            IMeterManager meters)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));

            foreach (var meterName in _meterMap.Keys)
            {
                if (!_meterMap[meterName].IsCashBox && _meters.IsMeterProvided(meterName))
                {
                    var meter = _meters.GetMeter(meterName);
                    meter.MeterChangedEvent += OnMeterChanged;
                }
            }
        }

        /// <inheritdoc />
        public string Name => GetType().FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IMeterMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public void SendAllAttributes()
        {
            foreach (var meterMappingKey in _meterMap.Keys)
            {
                if (_meters.IsMeterProvided(meterMappingKey))
                {
                    var meter = _meters.GetMeter(meterMappingKey);
                    SendMeterUpdate(meter);
                }
                else
                {
                    _events.Publish(new MeterAttributeChangingEvent(_meterMap[meterMappingKey].AttributeName, 0));
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var meterName in _meterMap.Keys)
                {
                    if (!_meterMap[meterName].IsCashBox && _meters.IsMeterProvided(meterName))
                    {
                        var meter = _meters.GetMeter(meterName);
                        meter.MeterChangedEvent -= OnMeterChanged;
                    }
                }
            }

            _disposed = true;
        }

        private void OnMeterChanged(object sender, MeterChangedEventArgs args)
        {
            if (sender is IMeter meter)
            {
                SendMeterUpdate(meter);
            }
        }

        private void SendMeterUpdate(IMeter meter)
        {
            var amount = meter.Period;

            if (_meterMap[meter.Name].IsMoney)
            {
                amount = amount.MillicentsToCents();
            }

            _events.Publish(new MeterAttributeChangingEvent(_meterMap[meter.Name].AttributeName, amount));
        }

        private struct MeterData
        {
            public string AttributeName { get; set; }

            public bool IsMoney { get; set; }

            public bool IsCashBox { get; set; }
        }
    }
}
