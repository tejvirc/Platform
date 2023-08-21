namespace Aristocrat.Monaco.Hhr.Services
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Service identifies if we can proceed with game-play based on pending transaction
    /// </summary>
    public interface IRequestTimeoutBehaviorService
    {
        Task<bool> CanPlay();
    }
}