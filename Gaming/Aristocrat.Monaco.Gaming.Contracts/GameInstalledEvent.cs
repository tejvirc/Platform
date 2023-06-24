namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A Game Installed Event is posted whenever a game is installed.  This typically happens when the host instructs the
    ///     EGM to add a package
    /// </summary>
    [Serializable]
    public class GameInstalledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameInstalledEvent" /> class.
        /// </summary>
        /// <param name="packageId">The package identifier</param>
        /// <param name="gamePackage">The game package</param>
        /// <param name="gameDetail">The game detail</param>
        public GameInstalledEvent(string packageId, string gamePackage, IGameDetail gameDetail)
        {
            PackageId = packageId;
            GamePackage = gamePackage;
            GameDetail = gameDetail;
        }

        /// <summary>
        ///     Gets the package identifier
        /// </summary>
        public string PackageId { get; }

        /// <summary>
        ///     Gets the game package
        /// </summary>
        public string GamePackage { get; }

        /// <summary>
        ///     Gets the game's details
        /// </summary>
        public IGameDetail GameDetail { get; }
    }
}