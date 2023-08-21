namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Newtonsoft.Json;

    public class ReportableEventsManager : IReportableEventsManager
    {
        private static string FieldName => "AspActiveEvents";

        private readonly HashSet<string> _setEvents = new HashSet<string>();

        private IPersistentStorageAccessor _persistentStorageAccessor;

        public ReportableEventsManager(IPersistentStorageManager persistentStorageManager)
        {
            if (persistentStorageManager == null) throw new ArgumentNullException(nameof(persistentStorageManager));

            InitialiseStorage(persistentStorageManager);
            LoadFromDatabase();
        }

        public ImmutableList<(byte @class, byte type, byte parameter)> GetAll() => _setEvents.Select(ParseKey).ToImmutableList();

        /// <inheritdoc />
        public void SetBatch(List<(byte @class, byte type, byte parameter)> setEvents)
        {
            var keys = setEvents.Select(s => GenerateKey(s.@class, s.type, s.parameter)).ToList();
            if (!keys.Except(_setEvents).Any()) return;

            var payload = keys.ToHashSet();
            payload.UnionWith(_setEvents);
            WriteToStorage(payload);

            _setEvents.UnionWith(keys);
        }

        /// <inheritdoc />
        public void Set(byte @class, byte type, byte parameter)
        {
            var key = GenerateKey(@class, type, parameter);
            if (_setEvents.Contains(key)) return;

            var updated = new HashSet<string> { key };
            updated.UnionWith(_setEvents);
            WriteToStorage(updated);

            _setEvents.Add(key);
        }

        /// <inheritdoc />
        public void ClearBatch(List<(byte @class, byte type, byte parameter)> setEvents)
        {
            var keys = setEvents.Select(s => GenerateKey(s.@class, s.type, s.parameter)).ToList();
            if (!keys.Any(a => _setEvents.Contains(a))) return;

            var payload = _setEvents.Except(keys).ToHashSet();
            WriteToStorage(payload);

            _setEvents.RemoveWhere(w => keys.Contains(w));
        }

        /// <inheritdoc />
        public void Clear(byte @class, byte type, byte parameter)
        {
            var key = GenerateKey(@class, type, parameter);
            if (!_setEvents.Contains(key)) return;

            WriteToStorage(_setEvents.Except(new[] { key }));

            _setEvents.Remove(key);
        }

        private static string GenerateKey(byte @class, byte type, byte parameter) => $"{@class}_{type}_{parameter}";

        private static (byte @class, byte type, byte parameter) ParseKey(string key)
        {
            var parts = key.Split('_').Select(byte.Parse).ToList();
            return (parts[0], parts[1], parts[2]);
        }

        private void InitialiseStorage(IPersistentStorageManager persistentStorageManager)
        {
            var blockName = typeof(ReportableEventsManager).FullName;
            var blockExists = persistentStorageManager.BlockExists(blockName);
            _persistentStorageAccessor = blockExists ? persistentStorageManager.GetBlock(blockName) : persistentStorageManager.CreateBlock(PersistenceLevel.Static, blockName, 0);
        }

        private void LoadFromDatabase()
        {
            var result = _persistentStorageAccessor.GetAll()?.FirstOrDefault();
            var json = result?.Value?[FieldName]?.ToString();
            if (string.IsNullOrWhiteSpace(json)) return;

            var setEvents = JsonConvert.DeserializeObject<HashSet<string>>(json);
            _setEvents.UnionWith(setEvents);
        }

        private void WriteToStorage(IEnumerable<string> payload)
        {
            using (var persistentStorageTransaction = _persistentStorageAccessor.StartTransaction())
            {
                persistentStorageTransaction[FieldName] = JsonConvert.SerializeObject(payload);
                persistentStorageTransaction.Commit();
            }
        }
    }
}