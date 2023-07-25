namespace Aristocrat.Monaco.Mgam.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reactive;
    using Application.UI.OperatorMenu;
    using Aristocrat.Mgam.Client.Common;
    using Aristocrat.Mgam.Client.Routing;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Common;
    using CommunityToolkit.Mvvm.Input;
    using Kernel;
    using Services.Communications;

    /// <summary>
    ///     The view model for displaying communication with the NYL host.
    /// </summary>
    public partial class HostTranscriptsViewModel : OperatorMenuPageViewModelBase
    {
        private const int MaxMessageCount = 500;

        private readonly IHostTranscripts _transcripts;

        private SubscriptionList _subscriptions = new SubscriptionList();

        private HostTranscript _selectedHostTranscript;

        private bool _initialized;

        private string _egmId;
        private bool _showHostTranscripts;
        private string _selectedText;
        private bool _enableViewHostTranscripts;
        private bool _online;
        private bool _includeKeepAliveMessages;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTranscriptsViewModel"/> class.
        /// </summary>
        public HostTranscriptsViewModel()
        {
            _transcripts = ServiceManager.GetInstance().GetService<IHostTranscripts>();

            ViewHostTranscriptsCommand = new RelayCommand<object>(ViewHostTranscript, _ => CanViewDetail());
            CloseDetailCommand = new RelayCommand<object>(CloseHostTranscriptDetail);
        }

        /// <summary>
        ///     Gets the command that fires when page unloaded.
        /// </summary>
        public RelayCommand<object> ViewHostTranscriptsCommand { get; }

        /// <summary>
        ///     Gets the command that fires when page unloaded.
        /// </summary>
        public RelayCommand<object> CloseDetailCommand { get; }

        /// <summary>
        ///     Gets messages sent to and from the server.
        /// </summary>
        public ObservableCollection<HostTranscript> Messages { get; } = new ObservableCollection<HostTranscript>();

        /// <summary>
        ///     Gets messages sent to and from the server.
        /// </summary>
        public ObservableCollection<RegisteredInstance> RegisteredInstances { get; } = new ObservableCollection<RegisteredInstance>();

        /// <summary>
        ///     Gets or sets the selected message.
        /// </summary>
        public HostTranscript SelectedHostTranscript
        {
            get => _selectedHostTranscript;
            set
            {
                if (_selectedHostTranscript != value)
                {
                    _selectedHostTranscript = value;

                    SelectedHostTranscriptText = string.Empty;
                    var text = _selectedHostTranscript?.Summary;
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (!text.StartsWith("\0", StringComparison.InvariantCulture))
                        {
                            SelectedHostTranscriptText = _selectedHostTranscript.Summary;
                        }
                    }

                    EnableViewHostTranscripts = SelectedHostTranscript != null;
                    OnPropertyChanged(nameof(SelectedHostTranscript));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the keep-alive messages should be shown.
        /// </summary>
        public bool IncludeKeepAliveMessages
        {
            get => _includeKeepAliveMessages;

            set => SetProperty(ref _includeKeepAliveMessages, value, nameof(IncludeKeepAliveMessages));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the host transcripts popup should be shown
        /// </summary>
        public bool EnableViewHostTranscripts
        {
            get => _enableViewHostTranscripts;

            set => SetProperty(ref _enableViewHostTranscripts, value, nameof(EnableViewHostTranscripts));
        }

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
                    OnPropertyChanged(nameof(SelectedHostTranscriptText));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the VLT registration date/time.
        /// </summary>
        public string EgmId
        {
            get => _egmId;

            set => SetProperty(ref _egmId, value, nameof(EgmId));
        }

        /// <summary>
        ///     Gets or sets a value that indicates if the communications is online.
        /// </summary>
        public bool Online
        {
            get => _online;

            set => SetProperty(ref _online, value, nameof(Online));
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to show the host transcripts.
        /// </summary>
        public bool ShowHostTranscripts
        {
            get => _showHostTranscripts;

            set => SetProperty(ref _showHostTranscripts, value, nameof(ShowHostTranscripts));
        }

        /// <inheritdoc />
        protected override void DisposeInternal()
        {
            if (_subscriptions != null)
            {
                _subscriptions.Dispose();
            }

            _subscriptions = null;
        }

        /// <inheritdoc />
        protected override void OnLoaded()
        {
            if (!_initialized)
            {
                try
                {
                    EgmId = PropertiesManager.GetValue(PropertyNames.EgmId, string.Empty);

                    _subscriptions.Add(
                        _transcripts.Subscribe(
                            Observer.Create<RegisteredInstance>(
                                instance =>
                                {
                                    Execute.OnUIThread(
                                        () =>
                                        {
                                            RegisteredInstances.Clear();
                                            RegisteredInstances.Add(instance);
                                        });
                                },
                                error =>
                                {
                                    Execute.OnUIThread(() => CommitCommand.NotifyCanExecuteChanged());
                                })));

                    _subscriptions.Add(
                        _transcripts.Subscribe(
                            Observer.Create<ConnectionState>(
                                state => Online = state == ConnectionState.Connected)));

                    _subscriptions.Add(
                        _transcripts.Subscribe(
                            Observer.Create<RoutedMessage>(
                                messages => { Execute.OnUIThread(() => Populate(messages)); },
                                error =>
                                {
                                    Execute.OnUIThread(() => CommitCommand.NotifyCanExecuteChanged());
                                })));
                }
                finally
                {
                    _initialized = true;
                }
            }
        }

        private void Populate(RoutedMessage message)
        {
            if (!IncludeKeepAliveMessages && message.IsHeartbeat)
            {
                return;
            }

            Messages.Insert(0, new HostTranscript(message));

            while (Messages.Count > MaxMessageCount)
            {
                Messages.RemoveAt(Messages.Count - 1);
            }
        }

        private bool CanViewDetail()
        {
            return EnableViewHostTranscripts;
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
    }
}
