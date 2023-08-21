namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using Client;

    public class RuntimeEndpointProvider : IClientEndpointProvider<IRuntime>
    {
        private readonly object _sync = new object();

        private IRuntime _client;

        public IRuntime Client
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

        public void AddOrUpdate(IRuntime client)
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