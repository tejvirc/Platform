namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using Client;

    public class ReelServiceEndpointProvider : IClientEndpointProvider<IReelService>
    {
        private readonly object _sync = new object();

        private IReelService _client;

        public IReelService Client
        {
            get
            {
                lock (_sync)
                {
                    return _client;
                }
            }
            private set
            {
                lock (_sync)
                {
                    _client = value;
                }
            }
        }

        public void AddOrUpdate(IReelService client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            lock (_sync)
            {
                Clear();

                Client = client;
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                (Client as IDisposable)?.Dispose();

                Client = null;
            }
        }
    }
}