namespace Aristocrat.Monaco.G2S
{
    using Aristocrat.G2S.Client;

    /// <summary>
    ///     Provides a mechanism to create and remove a host
    /// </summary>
    public interface IHostFactory
    {
        /// <summary>
        ///     Registers a host and creates the required devices.
        /// </summary>
        /// <param name="host">The host</param>
        /// <returns>an instance of an IHostControl if successfully registered.</returns>
        IHostControl Create(IHost host);

        /// <summary>
        ///     Updates a host and modifies the required devices.
        /// </summary>
        /// <param name="host">The host</param>
        /// <returns>an instance of an IHostControl if successfully registered.</returns>
        IHostControl Update(IHost host);

        /// <summary>
        ///     Deletes a host and removes associated devices.
        /// </summary>
        /// <param name="host">The host.</param>
        void Delete(IHost host);
    }
}