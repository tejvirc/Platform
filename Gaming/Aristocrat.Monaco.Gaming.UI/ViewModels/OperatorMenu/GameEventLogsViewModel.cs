namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Models;
    using Kernel;

    public class GameEventLogsViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IGameHistoryLog _gameHistoryLog;
        private readonly ITime _timeService;

        private readonly bool _lastGame;

        private ObservableCollection<GameEventLogEntry> _logEvents;

        public ObservableCollection<GameEventLogEntry> LogEvents
        {
            get => _logEvents;
            set
            {
                _logEvents = value;
                RaisePropertyChanged(nameof(LogEvents));
                RaisePropertyChanged(nameof(DataEmpty));
            }
        }

        public override bool DataEmpty => (LogEvents?.Count ?? 0) == 0;

        public GameEventLogsViewModel(long logSequence)
        {
            var gameHistory = ServiceManager.GetInstance().TryGetService<IGameHistory>();

            var games = gameHistory.GetGameHistory().ToList();
            _gameHistoryLog = games.SingleOrDefault(t => t.LogSequence == logSequence);

            if (_gameHistoryLog == null)
            {
                return;
            }

            _timeService = ServiceManager.GetInstance().TryGetService<ITime>();

            if (_timeService == null)
            {
                return;
            }

            _lastGame = games.Count == _gameHistoryLog?.LogSequence;
        }

        protected override void InitializeData()
        {
            if (_gameHistoryLog == null || _timeService == null)
            {
                return;
            }
            
            var logs = new List<GameEventLogEntry>(_gameHistoryLog.Events);

            if (_lastGame)
            {
                var container = ServiceManager.GetInstance().GetService<IContainerService>();

                var loggedEventContainer = container.Container.GetInstance<ILoggedEventContainer>();
                logs.AddRange(loggedEventContainer.Events);
            }

            LogEvents = new ObservableCollection<GameEventLogEntry>(
                logs.OrderByDescending(entry => entry.TransactionId).ThenByDescending(entry => entry.EntryDate));
            base.InitializeData();
        }
    }
}
