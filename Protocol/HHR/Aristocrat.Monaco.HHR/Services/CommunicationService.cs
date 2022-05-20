namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Application.Contracts;
    using Client.Communication;
    using Client.Data;
    using Client.Messages;
    using Client.WorkFlow;
    using Events;
    using Kernel;
    using log4net;
    using Timer = System.Timers.Timer;

    /// <summary>
    ///     Implementation of ICommunicationService
    /// </summary>
    public class CommunicationService : ICommunicationService, IDisposable
    {
        private const string FirewallUdpRuleName = "Platform.HHR.Multicast";
        private const string FirewallIgmpRuleName = "Platform.HHR.Igmp";
        private const int NetFwIgmpProtocolId = 2;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool _firewallRuleCreated;

        private readonly ITcpConnection _tcpConnection;
        private readonly IUdpConnection _udpConnection;
        private readonly ICentralManager _centralManager;
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;

        private readonly Timer _timer;
        private readonly List<IDisposable> _listOfSubscribers = new List<IDisposable>();

        private bool _disposed;
        private bool _connecting;

        /// <summary>
        ///     Implement the ICommunicationService interface, so we may control network connections to the
        ///     HHR server and keep them alive while the machine is running.
        /// </summary>
        public CommunicationService(
            ITcpConnection tcpConnection,
            IUdpConnection udpConnection,
            ICentralManager centralManager,
            IPropertiesManager properties,
            IEventBus eventBus)
        {
            _centralManager = centralManager;
            _eventBus = eventBus;
            _tcpConnection = tcpConnection;
            _udpConnection = udpConnection;
            _properties = properties;

            _timer = new Timer(
                _properties.GetValue(HHRPropertyNames.HeartbeatInterval, HhrConstants.HeartbeatInterval))
            {
                AutoReset = true
            };
            _timer.Elapsed += HeartBeat;

            _listOfSubscribers.Add(_tcpConnection.ConnectionStatus.Subscribe(OnTcpStatusChanged));

            _eventBus.Subscribe<NetworkAvailabilityChangedEvent>(this, HandleNetworkAvailabilityEvent);
        }

        /// <inheritdoc />
        public async Task<bool> ConnectTcp(CancellationToken token = default)
        {

            if (_tcpConnection.CurrentStatus.ConnectionState == ConnectionState.Connected || _connecting)
            {
                return true;
            }

            _connecting = true;

            try
            {
                var tcpHost = _properties.GetValue(HHRPropertyNames.ServerTcpIp, "");
                var tcpPort = _properties.GetValue(HHRPropertyNames.ServerTcpPort, 0);
                var tcpEndpoint = new IPEndPoint(IPAddress.Parse(tcpHost), tcpPort);

                var success = await _tcpConnection.Open(tcpEndpoint, token);
                if (!success)
                {
                    Logger.Error("Unable to connect to TCP server");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while connecting to TCP server", ex);
                return false;
            }
            finally
            {
                _connecting = false;
            }

            Logger.Debug("Connected to TCP server");

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> ConnectUdp(CancellationToken token = default)
        {
            try
            {
                var udpHost = ClientProperties.ProgressiveUdpIp;
                var udpPort = _properties.GetValue(HHRPropertyNames.ServerUdpPort, 0);
                var udpEndpoint = new IPEndPoint(IPAddress.Parse(udpHost), udpPort);

                if (!_firewallRuleCreated)
                {
                    _firewallRuleCreated = Firewall.AddUdpRule(FirewallUdpRuleName, (ushort) udpPort);
                    _firewallRuleCreated &= Firewall.AddProtocolRule(FirewallIgmpRuleName, NetFwIgmpProtocolId);
                }

                var success = await _udpConnection.Open(udpEndpoint, token);
                if (!success)
                {
                    Logger.Error("Unable to connect to UDP server");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while connecting to UDP server", ex);
                return false;
            }

            Logger.Debug("Connected to UDP server");

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> Disconnect(CancellationToken token = default)
        {
            try
            {
                // Close the connections. We don't care if they don't cooperate, because
                // when we call Connect again, a new socket will be created.
                await _tcpConnection.Close(token);
                await _udpConnection.Close(token);

                Logger.Debug("Disconnected from server");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while disconnecting from server", ex);
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _ = Disconnect().Result;
                _timer.Dispose();
                _listOfSubscribers.ForEach(x => x.Dispose());
                _eventBus.Unsubscribe<NetworkAvailabilityChangedEvent>(this);
            }

            _disposed = true;
        }

        private void OnTcpStatusChanged(ConnectionStatus connectionStatus)
        {
            switch (connectionStatus.ConnectionState)
            {
                case ConnectionState.Connected:
                    _timer.Start();
                    _eventBus.Publish(new CentralServerOnline());
                    break;
                case ConnectionState.Disconnected:
                    _timer.Stop();
                    _eventBus.Publish(new CentralServerOffline());
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        private async void HeartBeat(object sender, ElapsedEventArgs e)
        {
            try
            {
                _ = await _centralManager.Send<HeartBeatRequest, HeartBeatResponse>(new HeartBeatRequest());
            }
            catch (UnexpectedResponseException ex)
            {
                Logger.Warn(
                    $"Got incorrect response to heartbeat of type {ex.Response.GetType()} with status {ex.Response.MessageStatus}",
                    ex);
            }
            catch (Exception)
            {
                // Swallow any other exceptions that occur. We must release the lock below without fail, or this will all go
                // very wrong.
            }
        }

        private async void HandleNetworkAvailabilityEvent(NetworkAvailabilityChangedEvent evt)
        {
            if (!evt.Available)
            {
                await _tcpConnection.Close();
            }
        }
    }
}