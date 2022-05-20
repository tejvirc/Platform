namespace Aristocrat.Monaco.Gaming.Contracts.Process
{
    /// <summary>
    ///     Defines the process args interface
    /// </summary>
    public interface IProcessArgs
    {
        /// <summary>
        ///     Gets a string that can be passed as a set of process args
        /// </summary>
        /// <returns>A string of arguments</returns>
        string Build();
    }
}
