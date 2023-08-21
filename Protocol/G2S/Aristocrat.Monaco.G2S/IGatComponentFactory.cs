namespace Aristocrat.Monaco.G2S
{
    /// <summary>
    ///     Defines a contract for a GAT component factory.
    /// </summary>
    public interface IGatComponentFactory
    {
        /// <summary>
        ///     Register all components
        /// </summary>
        void RegisterComponents();

        /// <summary>
        ///     Registers a game as a GAT component.
        /// </summary>
        /// <param name="packageId">Package identifier.</param>
        /// <param name="package">Game package.</param>
        void RegisterGame(string packageId, string package);
    }
}
