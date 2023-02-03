namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;
    using OperatorMenu;
    using Toolkit.Mvvm.Extensions;

    /// <summary>
    ///     View model for diagnostic network panel
    /// </summary>
    [CLSCompliant(false)]
    public sealed class DiagnosticNetworkPageViewModel : OperatorMenuPageViewModelBase
    {
        private static readonly Regex ValidIpFormat = new Regex(
            @"^(?:(?:2[0-4]\d|25[0-5]|1\d{2}|[1-9]?\d)\.){3}(?:2[0-4]\d|25[0-5]|1\d{2}|[1-9]?\d)$");

        private static readonly TimeSpan NetStatRefreshInterval = TimeSpan.FromMinutes(5);

        private readonly INetworkService _network;

        private ITimer _monitorNetworkStatusTimer;
        private Timer _netStatTimer;
        private Process _process;

        private string _interfaceType;
        private string _ipAddress;
        private string _operationalStatus;
        private string _physicalAddress;
        private string _pingIpAddress = "127.0.0.1";
        private string _pingResult;
        private string _receivedBytes;
        private string _sentBytes;
        private bool _showStatus;
        private bool _canOnPing;
        private bool _staticIp;

        public DiagnosticNetworkPageViewModel()
        {
            _network = ServiceManager.GetInstance().GetService<INetworkService>();

            PingCommand = new RelayCommand<object>(OnPing, _ => CanOnPing);

            _monitorNetworkStatusTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(1) };
            _monitorNetworkStatusTimer.Tick += OnMonitorNetworkStatus;

            _netStatTimer = new Timer(OnNetStatUpdate);
        }

        public RelayCommand<object> PingCommand { get; set; }

        public string PingIpAddress
        {
            get => _pingIpAddress;

            set
            {
                if (_pingIpAddress != value)
                {
                    if (value.Length > 15)
                    {
                        return;
                    }

                    ValidateIpAddress(value);
                    _pingIpAddress = value;
                    OnPropertyChanged(nameof(PingIpAddress));
                }
            }
        }

        public string ReceivedBytes
        {
            get => _receivedBytes;

            set
            {
                if (value != _receivedBytes)
                {
                    _receivedBytes = value;
                    OnPropertyChanged(nameof(ReceivedBytes));
                }
            }
        }

        public string SentBytes
        {
            get => _sentBytes;

            set
            {
                if (value != _sentBytes)
                {
                    _sentBytes = value;
                    OnPropertyChanged(nameof(SentBytes));
                }
            }
        }

        public ObservableCollection<NetworkState> NetstatResultSet { get; } = new ObservableCollection<NetworkState>();

        public string PingResult
        {
            get => _pingResult;

            set
            {
                if (value != _pingResult)
                {
                    _pingResult = value;
                    OnPropertyChanged(nameof(PingResult));
                }
            }
        }

        public string IpAddress
        {
            get => _ipAddress;

            set
            {
                if (value != _ipAddress)
                {
                    _ipAddress = value;
                    OnPropertyChanged(nameof(IpAddress));
                }
            }
        }

        public string OperationalStatus
        {
            get => _operationalStatus;

            set
            {
                if (value != _operationalStatus)
                {
                    _operationalStatus = value;
                    OnPropertyChanged(nameof(OperationalStatus));
                }
            }
        }

        public string InterfaceType
        {
            get => _interfaceType;

            set
            {
                if (value != _interfaceType)
                {
                    _interfaceType = value;
                    OnPropertyChanged(nameof(InterfaceType));
                }
            }
        }

        public bool StaticIp
        {
            get => _staticIp;

            set
            {
                if (_staticIp != value)
                {
                    _staticIp = value;
                    OnPropertyChanged(nameof(StaticIp));
                }
            }
        }

        public string PhysicalAddress
        {
            get => _physicalAddress;

            set
            {
                if (_physicalAddress == value)
                {
                    return;
                }

                _physicalAddress = value;
                OnPropertyChanged(nameof(PhysicalAddress));
            }
        }

        public bool ShowStatus
        {
            get => _showStatus;

            set
            {
                if (_showStatus != value)
                {
                    _showStatus = value;
                    OnPropertyChanged(nameof(ShowStatus));
                }
            }
        }

        public bool CanOnPing
        {
            get => _canOnPing;

            set
            {
                if (_canOnPing != value)
                {
                    _canOnPing = value;
                    OnPropertyChanged("CanOnPing");
                }

                Execute.OnUIThread(() => PingCommand?.NotifyCanExecuteChanged());
            }
        }

        public static bool IsIpV4AddressValid(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            var addressTrim = address.Trim();
            return ValidIpFormat.IsMatch(addressTrim);
        }

        protected override void OnLoaded()
        {
            _netStatTimer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);

            RefreshAsync();
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            _netStatTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _monitorNetworkStatusTimer?.Stop();

            KillNetStatLookup();
        }

        protected override void DisposeInternal()
        {
            if (_monitorNetworkStatusTimer != null)
            {
                _monitorNetworkStatusTimer.Tick -= OnMonitorNetworkStatus;
                _monitorNetworkStatusTimer.Stop();
                _monitorNetworkStatusTimer = null;
            }

            if (_netStatTimer != null)
            {
                _netStatTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

                using (var handle = new ManualResetEvent(false))
                {
                    if (_netStatTimer.Dispose(handle))
                    {
                        KillNetStatLookup();

                        handle.WaitOne(TimeSpan.FromSeconds(5));
                    }
                }

                _netStatTimer = null;
            }

            if (_process != null)
            {
                _process.Dispose();
                _process = null;
            }

            base.DisposeInternal();
        }

        private static NetworkState ToNetworkState(string result)
        {
            if (string.IsNullOrWhiteSpace(result) || result.Length < 4)
            {
                return null;
            }

            if (result.Contains("Active Connections") || result.Contains("Local Address"))
            {
                return null;
            }

            var words = result.Split(new[] { ' ', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length != 4)
            {
                return null;
            }

            return new NetworkState
            {
                Protocol = words[0],
                LocalAddress = words[1],
                ForeignAddress = words[2],
                State = words[3]
            };
        }

        private void ValidateIpAddress(string address)
        {
            CanOnPing = true;

            if (!IsIpV4AddressValid(address))
            {
                CanOnPing = false;
            }
        }

        private async void OnPing(object o)
        {
            ShowStatus = true;
            PingResult = string.Empty;

            var result = new StringBuilder();

            try
            {
                var reply = await _network.PingAsync(_pingIpAddress);
                if (reply.Status == IPStatus.Success)
                {
                    result.Append($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PingSuccessfullText)}\n");
                    result.Append($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RoundTripTimeText)}: {reply.RoundtripTime}ms\n");
                }
                else
                {
                    result.Append(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PingTimeoutText));
                }
            }
            catch (AggregateException ex)
            {
                foreach (var innerEx in ex.InnerExceptions)
                {
                    if (innerEx.InnerException != null && innerEx.InnerException.Message.Contains("No such host is known"))
                    {
                        result.Append(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoSuchHostText));
                    }
                    else
                    {
                        result.Append($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Failed)}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("No such host is known"))
                {
                    result.Append(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoSuchHostText));
                }
                else
                {
                    result.Append($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Failed)}: {ex.Message}");
                }
            }

            ShowStatus = false;
            PingResult = result.ToString();
        }

        private void OnMonitorNetworkStatus(object sender, EventArgs args)
        {
            RefreshScreen();
        }

        private void RefreshScreen()
        {
            var info = _network.GetNetworkInfo();
            var networkInterface = NetworkInterfaceInfo.DefaultNetworkInterface;

            PhysicalAddress = NetworkInterfaceInfo.DefaultPhysicalAddress;
            StaticIp = !info.DhcpEnabled;
            IpAddress = NetworkInterfaceInfo.DefaultIpAddress?.ToString();
            InterfaceType = networkInterface.NetworkInterfaceType.ToString();
            OperationalStatus = networkInterface.OperationalStatus.ToString();
            ReceivedBytes = networkInterface.GetIPStatistics().BytesReceived.ToString();
            SentBytes = networkInterface.GetIPStatistics().BytesSent.ToString();
        }

        private async void RefreshAsync()
        {
            await Task.Run(
                () =>
                {
                    IsLoadingData = true;

                    RefreshScreen();
                    _monitorNetworkStatusTimer?.Start();
                    ValidateIpAddress(PingIpAddress);

                    IsLoadingData = false;
                });

            OnPropertyChanged(nameof(DataEmpty));
        }

        private void GetNetStat()
        {
            _process?.Dispose();

            Execute.OnUIThread(() => NetstatResultSet.Clear());

            _process = new Process
            {
                StartInfo = new ProcessStartInfo(@"netstat.exe")
                {
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas",
                    UseShellExecute = false
                }
            };

            _process.OutputDataReceived += (_, args) =>
            {
                var networkState = ToNetworkState(args.Data);

                if (networkState != null)
                {
                    Execute.OnUIThread(() => NetstatResultSet.Add(networkState));
                }
            };

            _process.Start();

            _process.BeginOutputReadLine();

            _process.WaitForExit((int)NetStatRefreshInterval.TotalMilliseconds - 1);
        }

        private void OnNetStatUpdate(object state)
        {
            GetNetStat();

            try
            {
                _netStatTimer.Change(Timeout.InfiniteTimeSpan, NetStatRefreshInterval);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void KillNetStatLookup()
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    _process.Kill();
                }
                catch (Exception)
                {
                }
            }
        }
    }

    public class NetworkState
    {
        public string Protocol { get; set; }

        public string LocalAddress { get; set; }

        public string ForeignAddress { get; set; }

        public string State { get; set; }
    }
}