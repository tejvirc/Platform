namespace Aristocrat.Mgam.Client.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    /// <summary>
    ///     
    /// </summary>
    internal class HostServiceCollection : IHostServiceCollection
    {
        private readonly ConcurrentDictionary<Type, IHostService> _services = new();

        /// <inheritdoc />
        public TService GetService<TService>()
            where TService : IHostService
        {
            return (TService)_services.Values.Single(service => service is TService);
        }

        /// <inheritdoc />
        public void Add(IHostService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            _services.AddOrUpdate(service.GetType(), service, (_, _) => service);
        }

        /// <inheritdoc />
        public void Remove(IHostService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            _services.TryRemove(service.GetType(), out _);
        }
    }
}
