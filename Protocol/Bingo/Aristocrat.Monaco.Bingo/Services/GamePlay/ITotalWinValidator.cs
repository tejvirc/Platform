namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using Gaming.Contracts.Central;

    public interface ITotalWinValidator
    {
        void ValidateTotalWin(long totalWin, CentralTransaction transaction);
    }
}