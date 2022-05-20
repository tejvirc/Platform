namespace Aristocrat.Monaco.Gaming
{
    using System;
    using Runtime.Client;

    public class RpcClientEndpointProvider<T> : IClientEndpointProvider<T> where T : class, IClientEndpoint
    {
        private readonly object _sync = new object();
        private T _client;

        public T Client
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

        public void AddOrUpdate(T client)
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