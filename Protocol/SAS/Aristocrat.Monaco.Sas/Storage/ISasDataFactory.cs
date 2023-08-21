namespace Aristocrat.Monaco.Sas.Storage
{
    /// <summary>
    ///     The data factory for SAS
    /// </summary>
    public interface ISasDataFactory
    {
        /// <summary>
        ///     Gets the configuration service for SAS
        /// </summary>
        /// <returns>The configuration service</returns>
        ISasConfigurationService GetConfigurationService();
    }
}