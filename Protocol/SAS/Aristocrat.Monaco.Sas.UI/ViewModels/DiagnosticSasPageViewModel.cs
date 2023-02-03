namespace Aristocrat.Monaco.Sas.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Data;
    using System.Windows.Input;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Aristocrat.Sas.Client;
    using Base;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.Client;
    using Localization.Properties;

    /// <summary>
    ///     ViewModel for Sas diagnostics
    /// </summary>
    public class DiagnosticSasPageViewModel : OperatorMenuPageViewModelBase
    {
        private const int SasPollDataCapacity = 300;
        private readonly object _sasPollDataLock = new object();

        private readonly ISasHost _sasHost = SasBase.Container.GetInstance<ISasHost>();
        private readonly Dictionary<SasPollData.PollType, Func<bool>> _canMonitorPollType;
        private readonly Dictionary<SasPollData.PacketType, Func<bool>> _canMonitorPacketType;
        private bool _monitoringSasHost;
        private readonly Dictionary<SasPollData.PacketType, string> _packetDictionary = new Dictionary<SasPollData.PacketType, string>
        {
            { SasPollData.PacketType.Tx, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasTx) }, { SasPollData.PacketType.Rx, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasRx) }
        };

        private bool _monitor = true;

        /// <inheritdoc />
        public DiagnosticSasPageViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(SasPollDatas, _sasPollDataLock);
            SelectedSasDiagnostics = _sasHost.GetSasClientDiagnostics(0);
            ToggleMonitoringCommand = new RelayCommand<object>(_ => ToggleMonitoring());
            ClearSasDataCommand = new RelayCommand<object>(_ => ClearSasData());
            _canMonitorPollType = new Dictionary<SasPollData.PollType, Func<bool>>
            {
                { SasPollData.PollType.GeneralPoll, () => MonitoringGeneralPoll && _monitor },
                { SasPollData.PollType.LongPoll, () => MonitoringLongPoll && _monitor },
                { SasPollData.PollType.SyncPoll, () => MonitoringSyncPoll && _monitor },
                { SasPollData.PollType.NoActivity, () => MonitoringNoActivity && _monitor }
            };

            _canMonitorPacketType = new Dictionary<SasPollData.PacketType, Func<bool>>
            {
                {SasPollData.PacketType.Rx, () => MonitoringIncomingPackets && _monitor},
                {SasPollData.PacketType.Tx, () => MonitoringOutgoingPackets && _monitor},
            };
            SelectedSasDiagnostics.NewSasPollDataArgs += NewPollDataArgsDataAvailable;
        }

        /// <summary>
        ///     Represents currently selected Sas Client for diagnostics
        /// </summary>
        public SasDiagnostics SelectedSasDiagnostics { get; set; }

        /// <summary>
        ///     Command to execute On Start/Stop button click
        /// </summary>
        public ICommand ToggleMonitoringCommand { get; set; }

        /// <summary>
        ///     Monitoring General Poll
        /// </summary>
        public bool MonitoringGeneralPoll { get; set; } = true;

        /// <summary>
        ///     Monitoring Sync Poll
        /// </summary>
        public bool MonitoringSyncPoll { get; set; } = true;

        /// <summary>
        ///     Monitoring Long Poll
        /// </summary>
        public bool MonitoringLongPoll { get; set; } = true;

        /// <summary>
        ///     Monitoring No Activity
        /// </summary>
        public bool MonitoringNoActivity { get; set; } = true;

        /// <summary>
        ///     Represents if sas configuration allows to select Second host for monitoring
        /// </summary>
        public bool CanMonitorHost1 => _sasHost.GetSasClientDiagnostics(1) != null;

        /// <summary>
        ///     Monitoring Sas host 0
        /// </summary>
        public bool MonitoringSasHost
        {
            get => _monitoringSasHost;
            set
            {
                _monitoringSasHost = value;
                SelectedSasDiagnostics.NewSasPollDataArgs -= NewPollDataArgsDataAvailable;
                SelectedSasDiagnostics = _sasHost.GetSasClientDiagnostics(_monitoringSasHost ? 1 : 0);
                ClearSasData();
                SelectedSasDiagnostics.NewSasPollDataArgs += NewPollDataArgsDataAvailable;
                OnPropertyChanged(nameof(SelectedSasDiagnostics));
                OnPropertyChanged(nameof(SasPollDatas));
            }
        }

        /// <summary>
        ///     Collection of Sas poll Data
        /// </summary>
        public ObservableCollection<SasPollData> SasPollDatas { get; set; } = new ObservableCollection<SasPollData>();

        /// <summary>
        ///     Command to be executed on Clear button press
        /// </summary>
        public ICommand ClearSasDataCommand { get; set; }

        /// <summary>
        ///     Monitoring button name
        /// </summary>
        public string MonitorButtonName
        {
            get
            {
                if (_monitor)
                {
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasStopMonitoring);
                }

                return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasStartMonitoring);
            }
        }

        /// <summary>
        ///     Monitor Outgoing data
        /// </summary>
        public bool MonitoringOutgoingPackets { get; set; } = true;

        /// <summary>
        ///     Monitor Incoming data
        /// </summary>
        public bool MonitoringIncomingPackets { get; set; } = true;

        private void NewPollDataArgsDataAvailable(object sender, NewSasPollDataEventArgs e)
        {
            if (_canMonitorPollType[e.SasPollData.SasPollType]() && _canMonitorPacketType[e.SasPollData.Type]())
            {
                lock (_sasPollDataLock)
                {
                    e.SasPollData.UpdatePollDescription(SelectedSasDiagnostics.LastPollSequence[SasPollData.PacketType.Rx]);
                    e.SasPollData.TypeDescription = _packetDictionary[e.SasPollData.Type];
                    SasPollDatas.Insert(0, e.SasPollData);
                    while (SasPollDatas.Count > SasPollDataCapacity)
                    {
                        SasPollDatas.RemoveAt(SasPollDataCapacity);
                    }
                }
            }
        }

        private void ClearSasData()
        {
            lock (_sasPollDataLock)
            {
                SasPollDatas.Clear();
            }
        }

        private void ToggleMonitoring()
        {
            _monitor = !_monitor;
            if (_monitor)
            {
                SelectedSasDiagnostics.NewSasPollDataArgs += NewPollDataArgsDataAvailable;
            }
            else
            {
                SelectedSasDiagnostics.NewSasPollDataArgs -= NewPollDataArgsDataAvailable;
            }

            OnPropertyChanged(nameof(MonitorButtonName));
        }
    }
}