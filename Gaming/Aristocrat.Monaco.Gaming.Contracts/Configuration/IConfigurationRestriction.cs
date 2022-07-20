namespace Aristocrat.Monaco.Gaming.Contracts.Configuration
{
    /// <summary>
    ///     Defines a configuration restriction for a game
    /// </summary>
    public interface IConfigurationRestriction
    {
        /// <summary>
        ///     Gets the name of the configuration restriction
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the associated game configuration restriction information.
        /// </summary>
        IRestrictionDetails RestrictionDetails { get; }
    }
}