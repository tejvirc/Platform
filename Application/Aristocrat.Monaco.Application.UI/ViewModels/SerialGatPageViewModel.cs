namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Contracts;
    using Contracts.SerialGat;
    using Kernel;
    using OperatorMenu;
    using System;
    using System.Collections.ObjectModel;

    [CLSCompliant(false)]
    public sealed class SerialGatPageViewModel : OperatorMenuPageViewModelBase
    {
        private readonly ISerialGat _serialGat;
        private bool _isGatConnected;
        private string _version;
        private string _status;

        public SerialGatPageViewModel()
        {
            _serialGat = ServiceManager.GetInstance().GetService<ISerialGat>();
        }

        public ObservableCollection<string> Versions => new ObservableCollection<string>
        {
            SerialGatVersionChangedEvent.Gat35, SerialGatVersionChangedEvent.LegacyGat3
        };

        public string GatVersion
        {
            get => _version;

            set
            {
                if (value != _version)
                {
                    Logger.Debug($"GAT version changed from {_version} to {value}");

                    _version = value;
                    OnPropertyChanged(nameof(GatVersion));

                    EventBus.Publish(new SerialGatVersionChangedEvent(value));
                }
            }
        }

        public bool IsGatConnected
        {
            get => _isGatConnected;
            set
            {
                if (value != _isGatConnected)
                {
                    _isGatConnected = value;
                    OnPropertyChanged(nameof(IsGatConnected));
                    OnPropertyChanged(nameof(IsGatIdle));
                }
            }
        }

        public bool IsGatIdle => !_isGatConnected;

        public string Status
        {
            get => _status;
            set
            {
                if (value != _status)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        ~SerialGatPageViewModel()
        {
            Dispose();
        }

        protected override void OnLoaded()
        {
            var config = new ApplicationConfigurationGatSerial();

            var configuration = ConfigurationUtilities.GetConfiguration(
                ApplicationConstants.JurisdictionConfigurationExtensionPath,
                () => new ApplicationConfiguration());

            if (configuration.GatSerial != null)
            {
                config = configuration.GatSerial;
            }

            GatVersion = config.Version;
            IsGatConnected = _serialGat?.IsConnected ?? false;
            Status = _serialGat?.GetStatus() ?? "";

            EventBus.Subscribe<SerialGatStatusEvent>(this, e => Status = e.StatusMessage);
        }
    }
}
