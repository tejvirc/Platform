namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;
    using PackageManifest.Models;

    /// <summary>
    ///     Interface for restricting game math per jurisdiction
    /// </summary>
    public interface ICustomGameMathFilterService : IService
    {
        /// <summary>
        ///     Filters given game.
        /// </summary>
        /// <param name="game">The game that needs to be filtered.</param>
        void Filter(ref GameContent game);
    }
}