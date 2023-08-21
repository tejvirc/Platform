namespace Aristocrat.Monaco.G2S.Data.Hosts
{
    using System.Collections.Generic;
    using Kernel;
    using Model;

    /// <summary>
    ///     Provides a mechanism to interact with a host service
    /// </summary>
    public interface IHostService : IService
    {
        /// <summary>
        ///     Gets the current list of hosts.
        /// </summary>
        /// <returns>Returns a collection of hosts.</returns>
        IEnumerable<Host> GetAll();

        /// <summary>
        ///     Saves a host list.
        /// </summary>
        /// <param name="hosts">A collection of hosts. Existing hosts will be updated and undefined hosts will be removed.</param>
        void Save(IEnumerable<Host> hosts);
    }
}