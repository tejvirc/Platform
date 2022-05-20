namespace Aristocrat.Monaco.Gaming.Contracts.Configuration
{
    /// <summary>
    ///     Defines a configuration restriction for a game
    /// </summary>
    public interface IConfigurationRestriction
    {
        /// <summary>
        ///     Gets the name of the configuration
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the associated game configuration for a specific game Theme. 
        /// </summary>
        IGameConfiguration Game { get; }
    }
}