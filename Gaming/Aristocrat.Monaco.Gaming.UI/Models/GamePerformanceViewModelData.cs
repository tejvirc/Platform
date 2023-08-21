namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Contracts.Models;

    /// <summary>
    ///     Data to be cached for GamePerformanceViewModel.
    /// </summary>
    public class GamePerformanceViewModelData
    {
        public List<GamePerformanceData> AllGamePerformanceItems { get; set; }

        public List<GamePerformanceGameTheme> AllGameThemes { get; set; }

        public Dictionary<string, List<GamePerformanceGameTheme>> GameTypeGameThemes { get; set; }

        public List<GamePerformanceData> GamePerformanceItems { get; set; }

        public ObservableCollection<GamePerformanceGameTheme> GameThemes { get; set; }

        public ObservableCollection<string> GameTypes { get; set; }

        public decimal MachineWeightedPayback { get; set; }

        public decimal MachineActualPayback { get; set; }
    }
}