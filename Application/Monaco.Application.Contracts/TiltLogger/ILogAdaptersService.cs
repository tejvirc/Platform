namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     Interface for Log adapter service
    /// </summary>
    public interface ILogAdaptersService : IService
    {
        /// <summary>
        ///     Registers the log adapter
        /// </summary>
        /// <param name="logAdapter">log adapter to be registered</param>
        void RegisterLogAdapter(IEventLogAdapter logAdapter);

        /// <summary>
        ///     Unregisters the log adapter
        /// </summary>
        /// <param name="logAdapterType">log type for the adapter to be unregistered</param>
        void UnRegisterLogAdapter(string logAdapterType);

        /// <summary>
        ///     Gets a list of available log adapters
        /// </summary>
        /// <returns> List of log adapters</returns>
        IEnumerable<IEventLogAdapter> GetLogAdapters();
    }
}
