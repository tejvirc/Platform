namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A Game Upgraded Event is posted whenever a game is upgraded.  This typically happens when the host instructs the
    ///     EGM to add a package
    /// </summary>
    [Serializable]
    public class GameUpgradedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameUpgradedEvent" /> class.
        /// </summary>
        /// <param name="packageId">The package identifier</param>
        /// <param name="gamePackage">The game package</param>
        public GameUpgradedEvent(string packageId, string gamePackage)
        {
            PackageId = packageId;
            GamePackage = gamePackage;
        }

        /// <summary>
        ///     Gets the package identifier
        /// </summary>
        public string PackageId { get; }

        /// <summary>
        ///     Gets the game package
        /// </summary>
        public string GamePackage { get; }
    }
}