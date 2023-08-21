namespace Aristocrat.Monaco.Gaming.Presentation.Services.Replay;

using System.Threading.Tasks;

public interface IReplayService
{
    Task ContinueAsync();

    Task EndReplayAsync();

    Task<bool> GetReplayPauseActiveAsync();

    Task NotifyCompletedAsync(long endCredits);
}
