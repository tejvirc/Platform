namespace Aristocrat.Monaco.G2S.Data.Hosts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     An implementation of <see cref="IHostService" />
    /// </summary>
    public class HostService : IHostService
    {
        private readonly IMonacoContextFactory _contextFactory;

        private readonly IHostRepository _hostRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostService" /> class.
        /// </summary>
        /// <param name="contextFactory">The db context factory</param>
        /// <param name="hostRepository">An <see cref="IRepository&lt;Host&gt;" /> instance.</param>
        public HostService(IMonacoContextFactory contextFactory, IHostRepository hostRepository)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _hostRepository = hostRepository ?? throw new ArgumentNullException(nameof(hostRepository));
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHostService) };

        /// <inheritdoc />
        public IEnumerable<Host> GetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _hostRepository.GetAll(context).ToList();
            }
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public void Save(IEnumerable<Host> hosts)
        {
            if (hosts == null)
            {
                throw new ArgumentNullException(nameof(hosts));
            }

            var hostList = hosts as IList<Host> ?? hosts.ToList();

            using (var context = _contextFactory.CreateDbContext())
            {
                var currentList = _hostRepository.GetAll(context).ToList();

                var deleted = currentList.Where(c => hostList.All(h => h.Id != c.Id));
                foreach (var host in deleted)
                {
                    _hostRepository.Delete(context, host);
                }

                var updated = currentList.Where(c => hostList.Any(h => h.Id == c.Id));
                foreach (var host in updated)
                {
                    var update = hostList.First(h => h.Id == host.Id);

                    host.HostId = update.HostId;
                    host.Address = update.Address;
                    host.Registered = update.Registered;
                    host.RequiredForPlay = update.RequiredForPlay;
                    host.IsProgressiveHost = update.IsProgressiveHost;
                    host.OfflineTimerInterval = update.OfflineTimerInterval;

                    _hostRepository.Update(context, host);
                }

                var added = hostList.Where(c => currentList.All(h => h.Id != c.Id));
                foreach (var host in added)
                {
                    _hostRepository.Add(context, host);
                }
            }
        }
    }
}