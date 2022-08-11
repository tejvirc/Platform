namespace Aristocrat.G2S.Client
{
    using System;
    using Communications;

    /// <summary>
    ///     Defines a G2S host to be used by a G2S client
    /// </summary>
    public class RegisteredHost : IHostControl
    {
        private readonly ISendEndpointProvider _endpointProvider;

        private ISendEndpoint _sendEndpoint;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisteredHost" /> class.
        /// </summary>
        /// <param name="hostId">The host Id</param>
        /// <param name="address">The address</param>
        /// <param name="requiredForPlay">Is the host required for play</param>
        /// <param name="index">The host index</param>
        /// <param name="endpointProvider">The endpoint provider</param>
        /// <param name="queue">The host queue</param>
        public RegisteredHost(
            int hostId,
            Uri address,
            bool requiredForPlay,
            int index,
            ISendEndpointProvider endpointProvider,
            IHostQueue queue)
        {
            Id = hostId;
            Address = address;
            RequiredForPlay = requiredForPlay;
            Index = index;

            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            Queue.DisableSend();

            _endpointProvider = endpointProvider ?? throw new ArgumentNullException(nameof(endpointProvider));

            if (address != null)
            {
                _sendEndpoint = _endpointProvider.GetOrAddEndpoint(hostId, address);
            }
        }

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public bool Registered => true;

        /// <inheritdoc />
        public Uri Address { get; private set; }

        /// <inheritdoc />
        public IHostQueue Queue { get; }

        /// <inheritdoc />
        public int Index { get; }

        /// <inheritdoc />
        public bool RequiredForPlay { get; private set; }

        /*
        JLK add properties here for the multicast connection

        Add a string MulticastUri with a set and get

        Also add a IPEndPoint MulticastEndpoint with a get that uses IPEndPoint EndpointUtilities.CreateIPEndPoint(string endPoint) to convert MulticastUri into an IPEndPoint
        */

        /// <inheritdoc />
        public void Start()
        {
            Reset();
        }

        /// <inheritdoc />
        public void Stop()
        {
            Close();
        }

        /// <inheritdoc />
        public void SetAddress(Uri address)
        {
            if (address == Address)
            {
                return;
            }

            Address = address;

            Reset();
        }

        /// <inheritdoc />
        public void SetRequiredForPlay(bool requiredForPlay)
        {
            if (requiredForPlay == RequiredForPlay)
            {
                return;
            }

            RequiredForPlay = requiredForPlay;
        }

        /// <summary>
        ///     Disable and Close endpoint
        /// </summary>
        public void Close()
        {
            Queue.DisableSend();
            Queue.Clear(true);

            _sendEndpoint?.Close();
        }

        private void Reset()
        {
            if (Address != null)
            {
                _sendEndpoint = _endpointProvider.GetOrUpdateEndpoint(Id, Address);
            }

            _sendEndpoint?.Reset();
        }
    }
}
