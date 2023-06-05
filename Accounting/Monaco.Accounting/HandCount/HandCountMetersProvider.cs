namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Metering;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Definition of the HandCountMetersProvider class.
    ///     Provides Hand Count meters for the EGM.
    /// </summary>
    public class HandCountMetersProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandCount, new OccurrenceMeterClassification())
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandCountMetersProvider" /> class.
        /// </summary>
        public HandCountMetersProvider()
            : base(typeof(HandCountMetersProvider).ToString())
        {
            var persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var blockName = GetType().ToString();

            AddMeters(persistentStorage.GetAccessor(Level, blockName));
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