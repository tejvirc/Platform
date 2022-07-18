namespace Aristocrat.Monaco.Sas.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Storage;
    using Contracts.SASProperties;
    using Kernel;
    using Models;

    /// <summary>
    ///     The configuration provider for SAS
    /// </summary>
    public class SasConfigurationProvider : ISasConfigurationService, IService
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IRepository<Host> _hostRepository;
        private readonly IRepository<PortAssignment> _portRepository;
        private readonly IRepository<SasFeatures> _featuresRepository;

        /// <summary>
        ///     Creates an instance of <see cref="SasConfigurationProvider" />
        /// </summary>
        /// <param name="contextFactory">An instance of <see cref="IMonacoContextFactory" /></param>
        /// <param name="hostRepository">An instance of <see cref="IRepository{Host}" /></param>
        /// <param name="portRepository">An instance of <see cref="IRepository{PortAssignment}" /></param>
        /// <param name="featuresRepository">An instance of <see cref="IRepository{SasFeatures}" /></param>
        public SasConfigurationProvider(
            IMonacoContextFactory contextFactory,
            IRepository<Host> hostRepository,
            IRepository<PortAssignment> portRepository,
            IRepository<SasFeatures> featuresRepository)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _hostRepository = hostRepository ?? throw new ArgumentNullException(nameof(hostRepository));
            _portRepository = portRepository ?? throw new ArgumentNullException(nameof(portRepository));
            _featuresRepository = featuresRepository ?? throw new ArgumentNullException(nameof(featuresRepository));
        }

        /// <inheritdoc />
        public string Name => nameof(SasConfigurationProvider);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new List<Type> { typeof(ISasConfigurationService) };

        /// <inheritdoc />
        public IEnumerable<Host> GetHosts()
        {
            using var context = _contextFactory.Create();
            return _hostRepository.GetAll(context).ToList();
        }

        /// <inheritdoc />
        public void SasHosts(IEnumerable<Host> hosts)
        {
            if (hosts == null)
            {
                throw new ArgumentNullException(nameof(hosts));
            }

            var hostList = hosts.ToList();
            using var context = _contextFactory.Create();
            var currentList = _hostRepository.GetAll(context).ToList();
            var deleted = currentList.Where(c => hostList.All(h => h.ComPort != c.ComPort));
            foreach (var host in deleted)
            {
                _hostRepository.Delete(context, host);
            }

            var updated = currentList.Where(c => hostList.Any(h => h.ComPort == c.ComPort));
            foreach (var host in updated)
            {
                var update = hostList.First(h => h.ComPort == host.ComPort);
                host.AccountingDenom = update.AccountingDenom;
                host.ComPort = update.ComPort;
                host.SasAddress = update.SasAddress;
                _hostRepository.Update(context, host);
            }

            var added = hostList.Where(c => currentList.All(h => h.ComPort != c.ComPort));
            foreach (var host in added)
            {
                _hostRepository.Add(context, host);
            }
        }

        /// <inheritdoc />
        public PortAssignment GetPortAssignment()
        {
            using var context = _contextFactory.Create();
            return _portRepository.GetSingle(context) ?? new PortAssignment { ProgressivePort = HostId.None };
        }

        /// <inheritdoc />
        public void SavePortAssignment(PortAssignment portAssignment)
        {
            if (portAssignment == null)
            {
                throw new ArgumentNullException(nameof(portAssignment));
            }

            AddOrUpdate(
                _portRepository,
                portAssignment,
                x =>
                {
                    x.FundTransferPort = portAssignment.FundTransferPort;
                    x.FundTransferType = portAssignment.FundTransferType;
                    x.GameStartEndHosts = portAssignment.GameStartEndHosts;
                    x.GeneralControlPort = portAssignment.GeneralControlPort;
                    x.IsDualHost = portAssignment.IsDualHost;
                    x.LegacyBonusPort = portAssignment.LegacyBonusPort;
                    x.ProgressivePort = portAssignment.ProgressivePort;
                    x.ValidationPort = portAssignment.ValidationPort;
                    x.Host1NonSasProgressiveHitReporting = portAssignment.Host1NonSasProgressiveHitReporting;
                    x.Host2NonSasProgressiveHitReporting = portAssignment.Host2NonSasProgressiveHitReporting;
                });
        }

        /// <inheritdoc />
        public SasFeatures GetSasFeatures()
        {
            using var context = _contextFactory.Create();
            return _featuresRepository.GetSingle(context) ??
                   new SasFeatures { ValidationType = SasValidationType.SecureEnhanced };
        }

        /// <inheritdoc />
        public void SaveSasFeatures(SasFeatures features)
        {
            if (features == null)
            {
                throw new ArgumentNullException(nameof(features));
            }

            AddOrUpdate(
                _featuresRepository,
                features,
                x =>
                {
                    x.AddressConfigurableOnlyOnce = features.AddressConfigurableOnlyOnce;
                    x.OverflowBehavior = features.OverflowBehavior;
                    x.BonusTransferStatusEditable = features.BonusTransferStatusEditable;
                    x.TransferToTicketAllowed = features.TransferToTicketAllowed;
                    x.ConfigNotification = features.ConfigNotification;
                    x.DisableOnDisconnectConfigurable = features.DisableOnDisconnectConfigurable;
                    x.DisabledOnPowerUp = features.DisabledOnPowerUp;
                    x.GeneralControlEditable = features.GeneralControlEditable;
                    x.MaxAllowedTransferLimits = features.MaxAllowedTransferLimits;
                    x.AftBonusAllowed = features.AftBonusAllowed;
                    x.WinTransferAllowed = features.WinTransferAllowed;
                    x.LegacyBonusAllowed = features.LegacyBonusAllowed;
                    x.ProgressiveGroupId = features.ProgressiveGroupId;
                    x.DebitTransfersAllowed = features.DebitTransfersAllowed;
                    x.DisableOnDisconnect = features.DisableOnDisconnect;
                    x.NonSasProgressiveHitReporting = features.NonSasProgressiveHitReporting;
                    x.HandpayReportingType = features.HandpayReportingType;
                    x.ValidationType = features.ValidationType;
                    x.TransferLimit = features.TransferLimit;
                    x.PartialTransferAllowed = features.PartialTransferAllowed;
                    x.TransferInAllowed = features.TransferInAllowed;
                    x.TransferOutAllowed = features.TransferOutAllowed;
                    x.FundTransferType = features.FundTransferType;
                });
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private void AddOrUpdate<T>(IRepository<T> repository, T entity, Action<T> dataUpdater) where T : BaseEntity
        {
            using var context = _contextFactory.Create();
            var current = repository.GetSingle(context);

            var add = current is null || current.Id != entity.Id;
            if (current != null && add)
            {
                repository.Delete(context, current);
            }

            if (add)
            {
                repository.Add(context, entity);
            }
            else
            {
                dataUpdater.Invoke(current);
                repository.Update(context, current);
            }
        }
    }
}