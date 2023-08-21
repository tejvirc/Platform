namespace Aristocrat.Monaco.Application.Meters
{
    using System.Collections.Generic;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Provide meters for the note acceptor
    /// </summary>
    public class NoteAcceptorMeterProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<(string name, MeterClassification classification)> _meters = new List<(string, MeterClassification)>
        {
            (ApplicationMeters.StackerRemovedCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.NoteAcceptorDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.NoteAcceptorErrorCount, new OccurrenceMeterClassification())
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorMeterProvider" /> class.
        /// </summary>
        public NoteAcceptorMeterProvider()
            : base(typeof(NoteAcceptorMeterProvider).ToString())
        {
            var persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var blockName = GetType().ToString();

            var accessor = persistentStorage.BlockExists(blockName)
                ? persistentStorage.GetBlock(blockName)
                : persistentStorage.CreateBlock(Level, blockName, 1);

            AddMeters(accessor);
        }

        private void AddMeters(IPersistentStorageAccessor accessor)
        {
            foreach (var (name, classification) in _meters)
            {
                AddMeter(new AtomicMeter(name, accessor, classification, this));
            }
        }
    }
}