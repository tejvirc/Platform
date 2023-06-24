namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Application.Contracts;
    using Application.UI.OperatorMenu;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Application.Contracts.Localization;
    using Kernel;
    using Models;
    using Monaco.UI.Common;
    using MVVM;
    using MVVM.Command;
    using ICommand = System.Windows.Input.ICommand;

    /// <summary>
    ///     ViewModel for Host HostTranscriptsViewModel.
    /// </summary>
    public class HostTranscriptsViewModel : OperatorMenuPageViewModelBase, IObserver<ClassCommand>
    {
        private const int MaxMessageCount = 500;

        private readonly ITimer _pollConnectionTimer;
        private readonly ITime _time;

        private IG2SEgm _egm;
        private IDisposable _observer;

        private ObservableCollection<CommsInfo> _commInfoData = new ObservableCollection<CommsInfo>();
        private ObservableCollection<HostMessage> _messages = new ObservableCollection<HostMessage>();

        private HostMessage _selectedItem;

        private bool _commsConnected;
        private bool _enableViewHostTranscripts;
        private bool _showHostTranscripts;

        private string _selectedText;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTranscriptsViewModel" /> class.
        /// </summary>
        public HostTranscriptsViewModel()
        {
            _time = ServiceManager.GetInstance().GetService<ITime>();

            ViewHostTranscriptsCommand = new ActionCommand<object>(ViewHostTranscript, _ => CanViewDetail());
            CloseDetailCommand = new ActionCommand<object>(CloseHostTranscriptDetail);

            _enableViewHostTranscripts = false;

            _pollConnectionTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(1.0) };
            _pollConnectionTimer.Tick += PollConnectionTimerOnTick;

            EventBus.Subscribe<OperatorCultureChangedEvent>(this, Handle);
        }

        /// <summary>
        ///     Gets or sets the host Transcript.
        /// </summary>
        public ObservableCollection<HostMessage> HostTranscriptsData
        {
            get => _messages;
            set
            {
                if (_messages != value)
                {
                    _messages = value;
                    RaisePropertyChanged(nameof(HostTranscriptsData));
                }
            }
        }

        /// <summary>
        ///     Gets the command that fires when page unloaded.
        /// </summary>
        public ActionCommand<object> ViewHostTranscriptsCommand { get; }

        /// <summary>
        ///     Gets the command that fires when page unloaded.
        /// </summary>
        public ICommand CloseDetailCommand { get; }

        /// <summary>
        ///     Gets the selected transcription text.
        /// </summary>
        public string SelectedHostTranscriptText
        {
            get => _selectedText;
            private set
            {
                if (_selectedText != value)
                {
                    _selectedText = value;
                    RaisePropertyChanged(nameof(SelectedHostTranscriptText));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the selected game item.
        /// </summary>
        public HostMessage SelectedHostTranscript
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;

                    SelectedHostTranscriptText = string.Empty;
                    var text = _selectedItem?.Summary;
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (!text.StartsWith("\0", StringComparison.InvariantCulture))
                        {
                            SelectedHostTranscriptText = _selectedItem.Summary;
                        }
                    }

                    EnableViewHostTranscripts = SelectedHostTranscript != null;
                    RaisePropertyChanged(nameof(SelectedHostTranscript));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the thumbprint popup should be shown
        /// </summary>
        public bool ShowHostTranscripts
        {
            get => _showHostTranscripts;
            set
            {
                _showHostTranscripts = value;
                RaisePropertyChanged(nameof(ShowHostTranscripts));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the host transcripts popup should be shown
        /// </summary>
        public bool EnableViewHostTranscripts
        {
            get => _enableViewHostTranscripts;
            set
            {
                if (_enableViewHostTranscripts != value)
                {
                    _enableViewHostTranscripts = value;
                    RaisePropertyChanged(nameof(EnableViewHostTranscripts));
                    ViewHostTranscriptsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<CommsInfo> CommsInfoData
        {
            get => _commInfoData;
            set
            {
                if (_commInfoData != value)
                {
                    _commInfoData = value;
                    RaisePropertyChanged(nameof(CommsInfoData));
                }
            }
        }

        /// <summary>
        ///     Gets the Egm Id.
        /// </summary>
        public string EgmId => _egm?.Id ?? string.Empty;

        /// <summary>
        ///     Gets or sets a value indicating whether we are connected to comms.
        /// </summary>
        public bool CommsConnected
        {
            get => _commsConnected;
            set
            {
                if (_commsConnected != value)
                {
                    _commsConnected = value;
                    RaisePropertyChanged(nameof(CommsConnected));
                }
            }
        }

        /// <summary>
        ///     Gets the Egm address.
        /// </summary>
        public string EgmAddress => _egm?.Address.ToString() ?? string.Empty;

        public void OnNext(ClassCommand value)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    var eventDesc = GetCommandSummary(value);

                    if (value.ClassName == nameof(eventHandler))
                    {
                        var command = value.IClass as eventHandler;

                        try
                        {
                            if (command?.Item is eventReport report)
                            {
                                eventDesc = report.eventText;
                            }
                        }
                        catch
                        {
                            // do nothing since I set eventDesc default above
                        }
                    }

                    var message = new HostMessage
                    {
                        DateReceived = _time.GetLocationTime(value.IClass.dateTime),
                        CommandId = value.CommandId,
                        Device = $"{value.ClassName}[{value.IClass.deviceId}]",
                        ToLocation = $"Host ID {value.HostId}",
                        SessionId = value.SessionId,
                        SessionType = value.IClass.sessionType,
                        Summary = eventDesc,
                        ErrorCode = value.Error.IsError ? value.Error.Code : string.Empty
                    };
                    message.RawData = ToXml(message);
                    HostTranscriptsData.Insert(0, message);

                    if (HostTranscriptsData.Count > MaxMessageCount)
                    {
                        HostTranscriptsData.RemoveAt(HostTranscriptsData.Count - 1);
                    }
                });
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        protected override void OnLoaded()
        {
            if (!SetEgm())
            {
                return;
            }

            RefreshData();

            EventBus.Subscribe<ProtocolLoadedEvent>(this, _ => SetEgm());

            _pollConnectionTimer.Start();
        }

        protected override void OnUnloaded()
        {
            EventBus.UnsubscribeAll(this);

            _pollConnectionTimer?.Stop();
            _observer?.Dispose();
            _observer = null;
        }

        private static string GetCommandSummary(ClassCommand command)
        {
            var args = command.GetType().GetGenericArguments();
            return args.Length < 2 ? null : $"{command.ClassName}.{args[1].Name}";
        }

        private bool CanViewDetail()
        {
            return EnableViewHostTranscripts;
        }

        private bool SetEgm()
        {
            var containerService = ServiceManager.GetInstance().TryGetService<IContainerService>();
            if (containerService == null)
            {
                _egm = null;
                return false;
            }

            var egm = containerService.Container.GetInstance<IG2SEgm>();
            _observer?.Dispose();
            _observer = egm.Subscribe(this);

            _egm = containerService.Container.GetInstance<IG2SEgm>();

            return true;
        }

        private void ViewHostTranscript(object obj)
        {
            if (SelectedHostTranscript == null)
            {
                ShowHostTranscripts = false;
                EnableViewHostTranscripts = false;
            }
            else
            {
                ShowHostTranscripts = true;
            }
        }

        private void CloseHostTranscriptDetail(object obj)
        {
            ShowHostTranscripts = false;
        }

        private static string ToXml<T>(T @class) where T : class
        {
            if (@class == null)
            {
                return string.Empty;
            }

            using (var stringWriter = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stringWriter, @class);
                return stringWriter.ToString();
            }
        }

        private void PollConnectionTimerOnTick(object sender, EventArgs eventArgs)
        {
            MvvmHelper.ExecuteOnUI(RefreshData);
        }

        private void RefreshData()
        {
            RaisePropertyChanged(nameof(EgmId));
            RaisePropertyChanged(nameof(EgmAddress));

            CommsConnected = _egm.Running;

            var currentInfos = (from host in _egm.Hosts.Where(host => !host.IsEgm())
                let commDevice = _egm.GetDevice<ICommunicationsDevice>(host.Id)
                where commDevice != null
                select new CommsInfo
                {
                    Index = host.Index,
                    HostId = host.Id,
                    Address = host.Address,
                    OutboundOverflow = commDevice.OutboundOverflow,
                    InboundOverflow = commDevice.InboundOverflow,
                    TransportState = commDevice.TransportState,
                    State = commDevice.State
                }).ToList();

            if (CommsInfoData.Any(i => currentInfos.All(l => l.Index != i.Index))
                || currentInfos.Any(i => CommsInfoData.All(l => l.Index != i.Index)))
            {
                CommsInfoData = new ObservableCollection<CommsInfo>(currentInfos);

                return;
            }

            foreach (var host in _egm.Hosts.Where(host => !host.IsEgm()))
            {
                var data = CommsInfoData.FirstOrDefault(i => i.Index == host.Index);
                if (data == null)
                {
                    continue;
                }

                var commDevice = _egm.GetDevice<ICommunicationsDevice>(host.Id);
                if (commDevice == null)
                {
                    continue;
                }

                data.HostId = commDevice.Id;
                data.Address = host.Address;
                data.Registered = host.Registered;
                data.OutboundOverflow = commDevice.OutboundOverflow;
                data.InboundOverflow = commDevice.InboundOverflow;
                data.TransportState = commDevice.TransportState;
                data.State = commDevice.State;
            }
        }

        private void Handle(OperatorCultureChangedEvent obj)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                RaisePropertyChanged(nameof(CommsInfoData));
            });
        }
    }
}