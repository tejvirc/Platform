namespace Aristocrat.Monaco.Application.Contracts
{
    /// <summary>
    ///     Interface for interacting with components being added/removed
    /// </summary>
    public interface IComponentMonitor
    {
        /// <summary>
        ///     Indicates if the current list of components matches the
        ///     persisted component list.
        /// </summary>
        /// <param name="identifier">Unique identifier of the calling class. Typically the name of the class</param>
        /// <returns>True if the component lists don't match. False otherwise</returns>
        bool HaveComponentsChangedWhilePoweredOff(string identifier);
    }
}