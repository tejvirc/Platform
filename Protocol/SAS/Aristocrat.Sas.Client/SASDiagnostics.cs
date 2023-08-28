namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using log4net;

    public class SasDiagnostics : INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public readonly Dictionary<SasPollData.PacketType, SasPollData> LastPollSequence =
            new Dictionary<SasPollData.PacketType, SasPollData>();

        private readonly Dictionary<SasPollData.PollType, (byte pollName, decimal timeTaken, int exceedCount)>
            _maxResponseTimeTaken;

        private int _resendPacket;

        private int _generalPoll;

        private int _synchronizePoll;

        private int _longPoll;

        private int _globalBroadcast;

        private int _otherAddressedPoll;

        private int _addressedPoll;

        private int _crcError;

        private int _unknownCommand;

        private string _lastLinkDown;

        private string _lastLinkUp;

        private int _errorInterByteDelay;

        private int _chirp;

        private decimal _elapsedTimeToHandlePoll;

        private int _ack;

        private int _impliedNack;

        private int _badPacket;

        private decimal _elapsedTimeToGetPoll;

        private CommsStatus _commsStatus;

        private SasPollData _data;

        private decimal _totalTimeTakenToPopulateDiagnostics;

        public SasDiagnostics(SasClient sasClient)
        {
            PerformanceTiming = new SasPerformanceTiming(sasClient);

            _maxResponseTimeTaken =
                new Dictionary<SasPollData.PollType, (byte pollName, decimal timeTaken, int exceedCount)>
                {
                    { SasPollData.PollType.GeneralPoll, (0, 0, 0) },
                    { SasPollData.PollType.LongPoll, (0, 0, 0) },
                    { SasPollData.PollType.SyncPoll, (0, 0, 0) },
                    { SasPollData.PollType.NoActivity, (0, 0, 0) }
                };
            StartConsumingSasPollData(sasClient.SasPollDataBlockingCollection);
        }

        public string ProtocolVersion { get; } = "6.03";

        public decimal ElapsedTimeToHandlePoll
        {
            get => _elapsedTimeToHandlePoll;
            set
            {
                _elapsedTimeToHandlePoll = value;
                OnPropertyChanged(nameof(ElapsedTimeToHandlePoll));
            }
        }

        public SasPerformanceTiming PerformanceTiming { get; set; }

        public int ResendPacket
        {
            get => _resendPacket;
            set
            {
                _resendPacket = value;
                OnPropertyChanged(nameof(ResendPacket));
            }
        }

        public int Ack
        {
            get => _ack;
            set
            {
                _ack = value;
                OnPropertyChanged(nameof(Ack));
            }
        }

        public int ImpliedNack
        {
            get => _impliedNack;
            set
            {
                _impliedNack = value;
                OnPropertyChanged(nameof(ImpliedNack));
            }
        }

        public int BadPacket
        {
            get => _badPacket;
            set
            {
                _badPacket = value;
                OnPropertyChanged(nameof(BadPacket));
            }
        }

        public int GeneralPoll
        {
            get => _generalPoll;
            set
            {
                _generalPoll = value;
                OnPropertyChanged(nameof(GeneralPoll));
            }
        }

        public int SynchronizePoll
        {
            get => _synchronizePoll;
            set
            {
                _synchronizePoll = value;
                OnPropertyChanged(nameof(SynchronizePoll));
            }
        }

        public int LongPoll
        {
            get => _longPoll;
            set
            {
                _longPoll = value;
                OnPropertyChanged(nameof(LongPoll));
            }
        }

        public int GlobalBroadcast
        {
            get => _globalBroadcast;
            set
            {
                _globalBroadcast = value;
                OnPropertyChanged(nameof(GlobalBroadcast));
            }
        }

        public int OtherAddressedPoll
        {
            get => _otherAddressedPoll;
            set
            {
                _otherAddressedPoll = value;
                OnPropertyChanged(nameof(OtherAddressedPoll));
            }
        }

        public int AddressedPoll
        {
            get => _addressedPoll;
            set
            {
                _addressedPoll = value;
                OnPropertyChanged(nameof(AddressedPoll));
            }
        }

        public int CrcError
        {
            get => _crcError;
            set
            {
                _crcError = value;
                OnPropertyChanged(nameof(CrcError));
            }
        }

        public int UnknownCommand
        {
            get => _unknownCommand;
            set
            {
                _unknownCommand = value;
                OnPropertyChanged(nameof(UnknownCommand));
            }
        }

        public string LastLinkDown
        {
            get => _lastLinkDown;
            set
            {
                _lastLinkDown = value;
                OnPropertyChanged(nameof(LastLinkDown));
            }
        }

        public string LastLinkUp
        {
            get => _lastLinkUp;
            set
            {
                _lastLinkUp = value;
                OnPropertyChanged(nameof(LastLinkUp));
            }
        }

        public decimal ElapsedTimeToGetPoll
        {
            get => _elapsedTimeToGetPoll;
            set
            {
                _elapsedTimeToGetPoll = value;
                OnPropertyChanged(nameof(ElapsedTimeToGetPoll));
            }
        }

        public string TotalTimeTakenByLastPoll { get; set; }

        public CommsStatus CommsStatus
        {
            get => _commsStatus;
            set
            {
                _commsStatus = value;
                OnPropertyChanged(nameof(CommsStatus));
            }
        }

        public int ErrorInterByteDelay
        {
            get => _errorInterByteDelay;
            set
            {
                _errorInterByteDelay = value;
                OnPropertyChanged(nameof(ErrorInterByteDelay));
            }
        }

        public int Chirp
        {
            get => _chirp;
            set
            {
                _chirp = value;
                OnPropertyChanged(nameof(Chirp));
            }
        }

        public int TotalPollsExceedingReplyTimeThreshold =>
            TotalGpExceedingReplyTimeThreshold + TotalLpExceedingReplyTimeThreshold;

        public int TotalGpExceedingReplyTimeThreshold =>
            _maxResponseTimeTaken[SasPollData.PollType.GeneralPoll].exceedCount;

        public int TotalLpExceedingReplyTimeThreshold =>
            _maxResponseTimeTaken[SasPollData.PollType.LongPoll].exceedCount;

        public string MaxResponseTimeTakenByLongPoll => (_maxResponseTimeTaken[SasPollData.PollType.LongPoll].pollName,
            _maxResponseTimeTaken[SasPollData.PollType.LongPoll].timeTaken).SasPollTupleToString();

        public string MaxResponseTimeTakenByGeneralPoll => (
            _maxResponseTimeTaken[SasPollData.PollType.GeneralPoll].pollName,
            _maxResponseTimeTaken[SasPollData.PollType.GeneralPoll].timeTaken).SasPollTupleToString();

        public SasPollData Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

        public decimal TotalTimeTakenToPopulateDiagnostics
        {
            get => _totalTimeTakenToPopulateDiagnostics;
            set
            {
                if (value <= _totalTimeTakenToPopulateDiagnostics)
                {
                    return;
                }

                _totalTimeTakenToPopulateDiagnostics = Math.Round(value, 2);
                TotalTimeTakenToPopulateDiagnosticsAsString = $"{_totalTimeTakenToPopulateDiagnostics:0.00}ms";
                OnPropertyChanged(nameof(TotalTimeTakenToPopulateDiagnosticsAsString));
            }
        }

        public string TotalTimeTakenToPopulateDiagnosticsAsString { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<NewSasPollDataEventArgs> NewSasPollDataArgs;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnNewSasPoll(NewSasPollDataEventArgs e)
        {
            NewSasPollDataArgs?.Invoke(this, e);
        }

        private void StartConsumingSasPollData(BlockingCollection<SasPollData> sasClientSasPollDataBlockingCollection)
        {
            Task.Run(
                () =>
                {
                    foreach (var sasPollData in sasClientSasPollDataBlockingCollection.GetConsumingEnumerable())
                    {
                        AddSasPollData(sasPollData);
                    }

                }).ContinueWith(
                (task) =>
                {
                    Logger.Error($"{task.Exception}");
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void AddSasPollData(SasPollData sasPollData)
        {
            LastPollSequence[sasPollData.Type] = sasPollData;

            CheckResponseTime();

            OnNewSasPoll(new NewSasPollDataEventArgs(LastPollSequence[sasPollData.Type]));
            OnPropertyChanged(sasPollData.SasPollType);

            void CheckResponseTime()
            {
                if (sasPollData.Type == SasPollData.PacketType.Rx)
                {
                    return;
                }

                if (_maxResponseTimeTaken.TryGetValue(sasPollData.SasPollType, out var maxValue) &&
                    maxValue.timeTaken < sasPollData.ElapsedTime)
                {
                    var count = maxValue.exceedCount;
                    if (sasPollData.ElapsedTime > 20)
                    {
                        count++;
                    }

                    _maxResponseTimeTaken[sasPollData.SasPollType] = (GetLastPollName(sasPollData.SasPollType),
                        sasPollData.ElapsedTime, count);
                }

                if (sasPollData.SasPollType == SasPollData.PollType.GeneralPoll ||
                    sasPollData.SasPollType == SasPollData.PollType.LongPoll)
                {
                    TotalTimeTakenByLastPoll = (GetLastPollName(sasPollData.SasPollType), sasPollData.ElapsedTime)
                        .SasPollTupleToString();
                }
            }
        }

        private byte GetLastPollName(SasPollData.PollType type)
        {
            switch (type)
            {
                case SasPollData.PollType.GeneralPoll:
                    return LastPollSequence[SasPollData.PacketType.Tx].PollName;
                case SasPollData.PollType.LongPoll:
                    return LastPollSequence[SasPollData.PacketType.Rx].PollName;
                default:
                    return 0;
            }
        }

        private void OnPropertyChanged(SasPollData.PollType pollType)
        {
            switch (pollType)
            {
                case SasPollData.PollType.GeneralPoll:
                    OnPropertyChanged(nameof(TotalGpExceedingReplyTimeThreshold));
                    OnPropertyChanged(nameof(TotalPollsExceedingReplyTimeThreshold));
                    OnPropertyChanged(nameof(TotalTimeTakenByLastPoll));
                    OnPropertyChanged(nameof(MaxResponseTimeTakenByGeneralPoll));
                    break;
                case SasPollData.PollType.LongPoll:
                    OnPropertyChanged(nameof(TotalLpExceedingReplyTimeThreshold));
                    OnPropertyChanged(nameof(TotalPollsExceedingReplyTimeThreshold));
                    OnPropertyChanged(nameof(TotalTimeTakenByLastPoll));
                    OnPropertyChanged(nameof(MaxResponseTimeTakenByLongPoll));
                    break;
            }
        }
    }
}