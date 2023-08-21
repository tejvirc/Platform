using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;

namespace Aristocrat.Monaco.Hhr.Services.Progressive
{
    public interface IProgressiveUpdateService
    {
        /// <summary>
        /// Return true if progressive update for a given level is locked, and
        /// stores the update which will be applied when it's unlocked
        /// </summary>
        /// <param name="linkedLevel">The linked level to check</param>
        /// <returns>True, if the level update is stored,
        ///          false otherwise
        /// </returns>
        bool IsProgressiveLevelUpdateLocked(LinkedProgressiveLevel linkedLevel);
    }
}
