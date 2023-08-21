namespace Aristocrat.Monaco.Hhr.Services.Progressive
{
    using System.Threading.Tasks;
    using Client.Messages;
    using System.Collections.Generic;
    using Gaming.Contracts.Progressives;

    public interface IProgressiveAssociation
    {
        /// <summary>
        ///     This function tries to match/associate progressive information received from server to levels defined by game.
        /// </summary>
        Task<bool> AssociateServerLevelsToGame(ProgressiveInfoResponse serverDefinedLevel,
            GameInfoResponse gameInfo,
            IList<ProgressiveLevelAssignment> levelAssignments);
    }
}
