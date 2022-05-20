namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    /// <summary>Interface for addin factory.</summary>
    public interface IAddinFactory
    {
        /// <summary>Creates an addin.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="extensionPath">Full pathname of the extension file.</param>
        /// <param name="protocolName">Name of the protocol.</param>
        /// <returns>The new addin.</returns>
        T CreateAddin<T>(string extensionPath, string protocolName);

        /// <summary>Query if 'extensionPath' does addin exist.</summary>
        /// <param name="extensionPath">Full pathname of the extension file.</param>
        /// <param name="protocolName">Name of the protocol.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        bool DoesAddinExist(string extensionPath, string protocolName);

        /// <summary>Searches for the first file path.</summary>
        /// <param name="extensionPath">Full pathname of the extension file.</param>
        /// <returns>The found file path.</returns>
        string FindFirstFilePath(string extensionPath);
    }
}