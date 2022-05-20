namespace Aristocrat.Monaco.Hhr.Storage.Helpers
{
    using Client.Messages;

    public interface IGamePlayEntityHelper
    {
        GamePlayRequest GamePlayRequest { get; set; }

        GamePlayResponse GamePlayResponse { get; set; }

        RaceStartRequest RaceStartRequest { get; set; }

        bool PrizeCalculationError { get; set; }

        bool GamePlayRequestFailed { get; set; }

        bool HorseAnimationFinished { get; set; }

        bool ManualHandicapWin { get; set; }
    }
}