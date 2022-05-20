namespace Aristocrat.Monaco.Application.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Contracts.Protocol;
    using Hardware.Contracts.Persistence;
    using Kernel;

    public class MultiProtocolConfigurationProvider : IMultiProtocolConfigurationProvider, IService
    {
        private const string InternalRecord = @"RequiredFlags";
        private const string ProtocolIdField = @"MultiProtocol.ProtocolId";
        private const string ProtocolValidationField = @"MultiProtocol.Validation";
        private const string ProtocolFundTransferField = @"MultiProtocol.FundTransfer";
        private const string ProtocolProgressiveField = @"MultiProtocol.Progressive";
        private const string ProtocolCentralDeterminationField = @"MultiProtocol.CentralDetermination";
        private const int BlockStartIndex = 0;

        private readonly IPersistentStorageManager _storageManager;

        private readonly object _sync = new object();

        public MultiProtocolConfigurationProvider() : this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>()) { }

        public MultiProtocolConfigurationProvider(IPersistentStorageManager storageManager)
        {
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
        }

        public IEnumerable<ProtocolConfiguration> MultiProtocolConfiguration
        {
            get => GetMultiProtocolConfiguration();

            set => SaveMultiProtocolConfiguration(value);
        }

        public bool IsValidationRequired { get; set; }

        public bool IsFundsTransferRequired { get; set; }

        public bool IsProgressiveRequired { get; set; }

        public bool IsCentralDeterminationSystemRequired { get; set; }

        public string Name => typeof(MultiProtocolConfigurationProvider).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IMultiProtocolConfigurationProvider) };

        public void Initialize() { }

        private IPersistentStorageAccessor GetStorageAccessor()
        {
            return _storageManager.BlockExists(Name)
                ? _storageManager.GetBlock(Name)
                : _storageManager.CreateBlock(PersistenceLevel.Critical, Name, 1);
        }

        private IEnumerable<ProtocolConfiguration> GetMultiProtocolConfiguration()
        {
            lock (_sync)
            {
                var block = GetStorageAccessor();
                var blockDictionary = block.GetAll();

                foreach (var item in blockDictionary)
                {
                    var protocol = item.Value.ContainsKey(ProtocolIdField) && !string.IsNullOrWhiteSpace(item.Value[ProtocolIdField]?.ToString()) ? item.Value[ProtocolIdField] : null;
                    if (protocol?.ToString() != InternalRecord)
                    {
                        continue;
                    }

                    IsValidationRequired = (bool)item.Value[ProtocolValidationField];
                    IsFundsTransferRequired = (bool)item.Value[ProtocolFundTransferField];
                    IsProgressiveRequired = (bool)item.Value[ProtocolProgressiveField];
                    IsCentralDeterminationSystemRequired = (bool)item.Value[ProtocolCentralDeterminationField];

                    blockDictionary.Remove(item.Key);
                    break;
                }

                return blockDictionary.Values.Select(
                        row =>
                        {
                            var (isValid, result) = EnumParser.Parse<CommsProtocol>(row[ProtocolIdField]);

                            return new ProtocolConfiguration(
                                isValid && result.HasValue
                                    ? result.Value
                                    : CommsProtocol.None,
                                (bool)row[ProtocolValidationField],
                                (bool)row[ProtocolFundTransferField],
                                (bool)row[ProtocolProgressiveField],
                                (bool)row[ProtocolCentralDeterminationField]);
                        })
                    .ToList();
            }
        }

        private void SaveMultiProtocolConfiguration(IEnumerable<ProtocolConfiguration> multiProtocolConfiguration)
        {
            if (multiProtocolConfiguration == null)
            {
                return;
            }

            var selectedConfiguration = multiProtocolConfiguration.ToList();
            selectedConfiguration.Add(new ProtocolConfiguration(CommsProtocol.None, IsValidationRequired, IsFundsTransferRequired, IsProgressiveRequired, IsCentralDeterminationSystemRequired));

            lock (_sync)
            {
                var block = GetStorageAccessor();

                var newCount = selectedConfiguration.Count;

                if (block.Count != newCount)
                {
                    _storageManager.ResizeBlock(Name, newCount);
                }

                var blockIndex = BlockStartIndex;

                using (var transaction = block.StartTransaction())
                {
                    foreach (var configuration in selectedConfiguration)
                    {
                        PopulateTransaction(transaction, blockIndex, configuration);
                        blockIndex++;
                    }

                    transaction.Commit();
                }
            }
        }

        private static void PopulateTransaction(IPersistentStorageTransaction transaction, int blockIndex, ProtocolConfiguration configuration)
        {
            transaction[blockIndex, ProtocolIdField] = configuration.Protocol == CommsProtocol.None ? InternalRecord : EnumParser.ToName(configuration.Protocol);
            transaction[blockIndex, ProtocolValidationField] = configuration.IsValidationHandled;
            transaction[blockIndex, ProtocolFundTransferField] = configuration.IsFundTransferHandled;
            transaction[blockIndex, ProtocolProgressiveField] = configuration.IsProgressiveHandled;
            transaction[blockIndex, ProtocolCentralDeterminationField] = configuration.IsCentralDeterminationHandled;
        }
    }
}