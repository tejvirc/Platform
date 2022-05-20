namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Provides meters to calculate acceptance rate documentMeterProvider
    /// </summary>
    public class DocumentMeterProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(AccountingMeters.DocumentsAcceptedCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VouchersRejectedCount, new OccurrenceMeterClassification()),
        };

        public DocumentMeterProvider()
            : this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>())
        {
        }

        public DocumentMeterProvider(IPersistentStorageManager storage)
            : base(typeof(DocumentMeterProvider).ToString())
        {
            AddMeters(storage.GetAccessor(Level, Name));
        }

        private void AddMeters(IPersistentStorageAccessor accessor)
        {
            foreach (var meter in _meters)
            {
                AddMeter(new AtomicMeter(meter.Item1, accessor, meter.Item2, this));
            }
        }
    }
}