namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Common.Storage;

    /// <summary>
    ///     Model for the GamePlay messages.
    /// </summary>
    public class GamePlayEntity : BaseEntity
    {
        public string GamePlayRequest { get; set; }

        public string GamePlayResponse { get; set; }

        public string RaceStartRequest { get; set; }

        public bool PrizeCalculationError { get; set; }

        public bool GamePlayRequestFailed { get; set; }

        public bool HorseAnimationFinished { get; set; }

        public bool ManualHandicapWin { get; set; }
    }
}