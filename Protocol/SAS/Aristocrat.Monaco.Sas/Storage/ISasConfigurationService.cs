namespace Aristocrat.Monaco.Sas.Storage
{
    using System.Collections.Generic;
    using Contracts.SASProperties;
    using Models;

    /// <summary>
    ///     The configuration service for SAS
    /// </summary>
    public interface ISasConfigurationService
    {
        /// <summary>
        ///     Gets all the hosts for SAS
        /// </summary>
        IEnumerable<Host> GetHosts();

        /// <summary>
        ///     Saves the hosts for SAS
        /// </summary>
        /// <param name="hosts">The hosts to save</param>
        void SasHosts(IEnumerable<Host> hosts);

        /// <summary>
        ///     Gets the port assignment for SAS
        /// </summary>
        PortAssignment GetPortAssignment();

        /// <summary>
        ///     Saves the port assignment for SAS
        /// </summary>
        /// <param name="portAssignment">The port assignment to save</param>
        void SavePortAssignment(PortAssignment portAssignment);

        /// <summary>
        ///     Gets the SAS features
        /// </summary>
        SasFeatures GetSasFeatures();

        /// <summary>
        ///     Saves the SAS features
        /// </summary>
        /// <param name="features">The features to save</param>
        void SaveSasFeatures(SasFeatures features);
    }
}