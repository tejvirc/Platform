namespace Aristocrat.Monaco.Bingo.UI.ViewModels.TestTool
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Common;
    using Common.Events;
    using Gaming.Contracts;
    using Hardware.Contracts.Button;
    using Kernel;
    using log4net;
    using MVVM.Command;
    using MVVM.ViewModel;

    public class BingoTestTestToolViewModel : BaseEntityViewModel
    {
        private const string CsvFileFilter = "CSV files (*.csv)|*.csv";
        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly Stopwatch _stopWatch = new();

        private long _totalGameStartInterval;
        private long _totalClientSendInterval;
        private long _totalClientResponseInterval;
        private long _totalFakeDelayInterval;
        private long _totalCardDisplayInterval;
        private int _fakeDelayMs;
        private BingoTestEventType _state;
        private string _csvFileName = @"..\logs\Bingo_Timing.csv";

        public BingoTestTestToolViewModel(
            IEventBus eventBus,
            IGamePlayState gamePlayState,
            IPropertiesManager properties)
        {
            _properties = properties;

            eventBus.Subscribe<DownEvent>(this, _ => AdvanceState(BingoTestEventType.ButtonPressed), _ => gamePlayState.Idle || gamePlayState.InPresentationIdle );
            eventBus.Subscribe<GamePlayInitiatedEvent>(this, _ => AdvanceState(BingoTestEventType.GameStarted));
            eventBus.Subscribe<BingoTestTimingEvent>(this, evt => AdvanceState(evt.Type));
            eventBus.Subscribe<GamePresentationEndedEvent>(this, _ => ClearState());

            ResetCommand = new ActionCommand<object>(_ => Reset());
            CsvFileCommand = new ActionCommand<object>(_ => SelectCsvFile());
        }

        public string BuildType =>
#if DEBUG
            "Debug";
#else
            "Release";
#endif

        public ICommand ResetCommand { get; set; }

        public ICommand CsvFileCommand { get; set; }

        public string CsvFile { get; set; }

        public long LastGameStartInterval { get; set; }

        public long LastClientSendInterval { get; set; }

        public long LastClientResponseInterval { get; set; }

        public long LastFakeDelayInterval { get; set; }

        public long LastCardDisplayInterval { get; set; }

        public long LastFullTripInterval => LastGameStartInterval + LastClientSendInterval + LastClientResponseInterval +
                                            LastFakeDelayInterval + LastCardDisplayInterval;

        public long AverageGameStartInterval { get; set; }

        public long AverageClientSendInterval { get; set; }

        public long AverageClientResponseInterval { get; set; }

        public long AverageFakeDelayInterval { get; set; }

        public long AverageCardDisplayInterval { get; set; }

        public long AverageFullTripInterval => AverageGameStartInterval + AverageClientSendInterval + AverageClientResponseInterval +
                                               AverageFakeDelayInterval + AverageCardDisplayInterval;

        public int CountGames { get; set; }

        public int FakeDelayMs
        {
            get => _fakeDelayMs;
            set
            {
                if (_fakeDelayMs == value)
                {
                    return;
                }

                _fakeDelayMs = value;
                _properties.SetProperty(BingoConstants.TestingFakeDelayMs, _fakeDelayMs);
            }
        }

        public string CurrentState => Enum.GetName(typeof(BingoTestEventType), _state);

        private void ClearState()
        {
            _state = BingoTestEventType.Idle;
            RaisePropertyChanged(nameof(CurrentState));
        }

        private void AdvanceState(BingoTestEventType newState)
        {
            if ((int)newState != (int)_state + 1)
            {
                Logger.Debug($"Fail to Advance state from {_state} to {newState}");
                return;
            }

            var elapsed = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Restart();
            Logger.Debug($"Advance state from {_state} to {newState} after {elapsed} ms");

            switch (newState)
            {
                case BingoTestEventType.ButtonPressed:
                    CountGames++;
                    RaisePropertyChanged(nameof(CountGames));
                    break;
                case BingoTestEventType.GameStarted:
                    LastGameStartInterval = elapsed;
                    _totalGameStartInterval += LastGameStartInterval;
                    AverageGameStartInterval = _totalGameStartInterval / CountGames;
                    RaisePropertyChanged(nameof(LastGameStartInterval), nameof(AverageGameStartInterval));
                    break;
                case BingoTestEventType.SentToClient:
                    LastClientSendInterval = elapsed;
                    _totalClientSendInterval += LastClientSendInterval;
                    AverageClientSendInterval = _totalClientSendInterval / CountGames;
                    RaisePropertyChanged(nameof(LastClientSendInterval), nameof(AverageClientSendInterval));
                    break;
                case BingoTestEventType.ResponseFromClient:
                    LastClientResponseInterval = elapsed;
                    _totalClientResponseInterval += LastClientResponseInterval;
                    AverageClientResponseInterval = _totalClientResponseInterval / CountGames;

                    _properties.SetProperty(BingoConstants.TestingIntervalBeforeCardDisplayMs,
                        AverageGameStartInterval + AverageClientSendInterval + AverageClientResponseInterval + AverageCardDisplayInterval);

                    RaisePropertyChanged(nameof(LastClientResponseInterval), nameof(AverageClientResponseInterval));
                    break;
                case BingoTestEventType.FakeDelay:
                    LastFakeDelayInterval = elapsed;
                    _totalFakeDelayInterval += LastFakeDelayInterval;
                    AverageFakeDelayInterval = _totalFakeDelayInterval / CountGames;
                    RaisePropertyChanged(nameof(LastFakeDelayInterval), nameof(AverageFakeDelayInterval));
                    break;
                case BingoTestEventType.CardDisplayed:
                    LastCardDisplayInterval = elapsed;
                    _totalCardDisplayInterval += LastCardDisplayInterval;
                    AverageCardDisplayInterval = _totalCardDisplayInterval / CountGames;
                    RaisePropertyChanged(
                        nameof(LastCardDisplayInterval),
                        nameof(LastFullTripInterval),
                        nameof(AverageCardDisplayInterval),
                        nameof(AverageFullTripInterval));

                    LogLine(new []{$"{CountGames}", $"{LastGameStartInterval}", $"{LastClientSendInterval}", $"{LastFakeDelayInterval}",
                        $"{LastClientResponseInterval}", $"{LastCardDisplayInterval}", $"{LastFullTripInterval}"});

                    break;
            }

            _state = newState;
            RaisePropertyChanged(nameof(CurrentState));
        }

        private void Reset()
        {
            LastGameStartInterval = 0;
            LastClientSendInterval = 0;
            LastFakeDelayInterval = 0;
            LastClientResponseInterval = 0;
            LastCardDisplayInterval = 0;
            AverageGameStartInterval = 0;
            AverageClientSendInterval = 0;
            AverageFakeDelayInterval = 0;
            AverageClientResponseInterval = 0;
            AverageCardDisplayInterval = 0;
            CountGames = 0;
            _totalGameStartInterval = 0;
            _totalClientSendInterval = 0;
            _totalFakeDelayInterval = 0;
            _totalClientResponseInterval = 0;
            _totalCardDisplayInterval = 0;
            ClearState();

            RaisePropertyChanged(
                nameof(CountGames),
                nameof(LastGameStartInterval),
                nameof(AverageGameStartInterval),
                nameof(LastClientSendInterval),
                nameof(AverageClientSendInterval),
                nameof(LastFakeDelayInterval),
                nameof(AverageFakeDelayInterval),
                nameof(LastClientResponseInterval),
                nameof(AverageClientResponseInterval),
                nameof(LastCardDisplayInterval),
                nameof(LastFullTripInterval),
                nameof(AverageCardDisplayInterval),
                nameof(AverageFullTripInterval));

            LogLine(new [] {$"Build = {BuildType},,,({FakeDelayMs} ms)"}, false);
            LogLine(new [] { "", "GameStart", "ClientSend", "FakeDelay", "ClientRespond", "CardDisplay", "Total" });
        }

        private void SelectCsvFile()
        {
            var sfd = new SaveFileDialog
            {
                FileName = _csvFileName,
                InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(_csvFileName)),
                Filter = CsvFileFilter
            };

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _csvFileName = sfd.FileName;
            RaisePropertyChanged(nameof(CsvFile));

            Reset();
        }

        private async void LogLine(string[] cells, bool append = true)
        {
            var file = new StreamWriter(_csvFileName, append);
            await file.WriteLineAsync(string.Join(",", cells));
            file.Close();
        }
    }
}
