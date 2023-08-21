namespace Aristocrat.Monaco.Protocol.Common.Installer
{
    using Kernel.Contracts;

    /// <summary>
    ///     Provides a mechanism to request an installer by product type
    /// </summary>
    public interface IInstallerFactory
    {
        /// <summary>
        ///     Creates a new installer from the specified type
        /// </summary>
        /// <param name="type">The product type</param>
        /// <returns>an <see cref="IInstaller" /> instance</returns>
        IInstaller CreateNew(string type);
    }
}
