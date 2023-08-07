namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    public class NetworkService : INetworkService, IService, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Static;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private IPersistentStorageAccessor _accessor;
        private CancellationTokenSource _commitCancellationToken;
        private bool _disposed;

        private string BlockName => GetType().ToString();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public NetworkInfo GetNetworkInfo()
        {
            return GetNetworkInfoInternal();
        }

        public bool SetNetworkInfo(NetworkInfo networkInfo)
        {
            var networkInterface = NetworkInterfaceInfo.DefaultNetworkInterface;

            Log.Debug($"Updating network info - DHCP Enabled = {networkInfo.DhcpEnabled}");

            NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
            try
            {
                if (networkInfo.DhcpEnabled)
                {
                    if (!ExecuteCommand("interface ip set address \"" + networkInterface.Name + "\" dhcp"))
                    {
                        return false;
                    }

                    if (!ExecuteCommand("interface ip set dns \"" + networkInterface.Name + "\" dhcp"))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!ExecuteCommand("interface ip delete dnsserver \"" + networkInterface.Name + "\" all"))
                    {
                        return false;
                    }

                    if (!ExecuteCommand(
                        "interface ip set address \"" + networkInterface.Name + "\" static " + networkInfo.IpAddress +
                        " " + networkInfo.SubnetMask + " " + networkInfo.DefaultGateway))
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(networkInfo.PreferredDnsServer))
                    {
                        if (!ExecuteCommand(
                            "interface ip add dns name= \"" + networkInterface.Name + "\"  addr=" +
                            networkInfo.PreferredDnsServer + " index=1"))
                        {
                            return false;
                        }
                    }

                    if (!string.IsNullOrEmpty(networkInfo.AlternateDnsServer))
                    {
                        if (!ExecuteCommand(
                            "interface ip add dns name= \"" + networkInterface.Name + "\"  addr=" +
                            networkInfo.AlternateDnsServer + " index=2"))
                        {
                            return false;
                        }
                    }
                }

                Log.Debug("Network changes executed - waiting for commit");

                var storage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
                if (!storage.BlockExists(BlockName))
                {
                    _accessor = storage.CreateBlock(Level, BlockName, 1);
                }

                _accessor["DhcpEnabled"] = networkInfo.DhcpEnabled;
                _accessor["IpAddress"] = networkInfo.IpAddress;
                _accessor["SubnetMask"] = networkInfo.SubnetMask;
                _accessor["DefaultGateway"] = networkInfo.DefaultGateway;
                _accessor["PreferredDnsServer"] = networkInfo.PreferredDnsServer;
                _accessor["AlternateDnsServer"] = networkInfo.AlternateDnsServer;

                WaitForCommit(networkInfo);

                Log.Debug($"Network info committed - DHCP Enabled = {networkInfo.DhcpEnabled}");

                ServiceManager.GetInstance().GetService<IEventBus>().Publish(new NetworkInfoChangedEvent(networkInfo));
            }
            finally
            {
                NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
            }

            return true;
        }

        public async Task<PingReply> PingAsync(string addr)
        {
            var pingSender = new Ping();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            var options = new PingOptions { DontFragment = true };

            // Create a buffer of 32 bytes of data to be transmitted.
            const string data = "This is just a test ping message";
            var buffer = Encoding.ASCII.GetBytes(data);
            const int timeout = 120;

            return await pingSender.SendPingAsync(addr, timeout, buffer, options);
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(INetworkService) };

        public void Initialize()
        {
            var storage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            if (storage.BlockExists(BlockName))
            {
                _accessor = storage.GetBlock(BlockName);

                var networkInfo = new NetworkInfo
                {
                    DhcpEnabled = (bool)_accessor["DhcpEnabled"],
                    IpAddress = (string)_accessor["IpAddress"],
                    SubnetMask = (string)_accessor["SubnetMask"],
                    DefaultGateway = (string)_accessor["DefaultGateway"],
                    PreferredDnsServer = (string)_accessor["PreferredDnsServer"],
                    AlternateDnsServer = (string)_accessor["AlternateDnsServer"]
                };

                if (!IsCurrent(networkInfo))
                {
                    SetNetworkInfo(networkInfo);
                }
            }

            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
                NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;

                if (_commitCancellationToken != null)
                {
                    _commitCancellationToken.Cancel(false);
                    _commitCancellationToken.Dispose();
                }
            }

            _commitCancellationToken = null;

            _disposed = true;
        }

        private static bool ExecuteCommand(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("netsh.exe", command)
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas",
                    RedirectStandardOutput = true
                }
            };

            process.Start();
            process.WaitForExit(30000);

            return true;
        }

        private static NetworkInfo GetNetworkInfoInternal()
        {
            var networkInfo = new NetworkInfo();

            var networkInterface = NetworkInterfaceInfo.DefaultNetworkInterface;

            if (networkInterface != null && networkInterface.Supports(NetworkInterfaceComponent.IPv4))
            {
                var adapterProperties = networkInterface.GetIPProperties();
                var iPv4Properties = adapterProperties.GetIPv4Properties();

                networkInfo.DhcpEnabled = iPv4Properties.IsDhcpEnabled;

                var ip = adapterProperties.UnicastAddresses.FirstOrDefault(
                    i => i.Address.AddressFamily == AddressFamily.InterNetwork);
                if (ip != null)
                {
                    networkInfo.IpAddress = ip.Address.ToString();
                    networkInfo.SubnetMask = ip.IPv4Mask.ToString();
                }

                var index = 0;
                foreach (var dnsAddress in adapterProperties.DnsAddresses)
                {
                    if (dnsAddress.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    if (index == 0)
                    {
                        networkInfo.PreferredDnsServer = dnsAddress.ToString();
                    }
                    else if (index == 1)
                    {
                        networkInfo.AlternateDnsServer = dnsAddress.ToString();
                    }

                    index++;
                }

                var gatewayAddresses = adapterProperties.GatewayAddresses;
                foreach (var gatewayAddress in gatewayAddresses)
                {
                    if (gatewayAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        networkInfo.DefaultGateway = gatewayAddress.Address.ToString();
                    }
                }
            }

            return networkInfo;
        }

        private static bool IsCurrent(NetworkInfo info)
        {
            var current = GetNetworkInfoInternal();

            return info.DhcpEnabled == current.DhcpEnabled &&
                   info.IpAddress == current.IpAddress &&
                   info.SubnetMask == current.SubnetMask &&
                   info.DefaultGateway == current.DefaultGateway &&
                   info.PreferredDnsServer == current.PreferredDnsServer &&
                   info.AlternateDnsServer == current.AlternateDnsServer;
        }

        private void WaitForCommit(NetworkInfo networkInfo)
        {
            _commitCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                if (!_commitCancellationToken.IsCancellationRequested)
                {
                    // There is an arbitrary delay, but the it appears that the switch between static to dhcp with a different IP may report correctly without actually taking affect
                    Retry(
                            TimeSpan.FromSeconds(3),
                            TimeSpan.FromSeconds(1),
                            () => IsCurrent(networkInfo),
                            _commitCancellationToken.Token)
                        .Wait(_commitCancellationToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Debug("Wait for commit cancelled due to timeout or success");
            }
            catch (Exception e)
            {
                Log.Error("Exception while checking network change commit", e);
            }
            finally
            {
                _commitCancellationToken?.Cancel();
                _commitCancellationToken?.Dispose();
                _commitCancellationToken = null;
            }
        }

        private static Task Retry(TimeSpan delay, TimeSpan pollInterval, Func<bool> action, CancellationToken token)
        {
            token.WaitHandle.WaitOne(delay);
            if (token.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            return Task.Factory.StartNew(
                () =>
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested || action() || token.WaitHandle.WaitOne(pollInterval))
                        {
                            break;
                        }
                    }
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private static void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs args)
        {
            ServiceManager.GetInstance().GetService<IEventBus>()
                .Publish(new NetworkAvailabilityChangedEvent(args.IsAvailable));
        }

        private static void OnNetworkAddressChanged(object sender, EventArgs eventArgs)
        {
            if (NetworkInterfaceInfo.DefaultIsUp)
            {
                ServiceManager.GetInstance().GetService<IEventBus>()
                    .Publish(new NetworkInfoChangedEvent(GetNetworkInfoInternal()));
            }
        }
    }
}
