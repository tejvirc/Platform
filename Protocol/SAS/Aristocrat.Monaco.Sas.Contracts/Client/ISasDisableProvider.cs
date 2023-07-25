namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Protocol.Common.DisableProvider;

    /// <summary>
    ///     The SAS disable provider
    /// </summary>
    public interface ISasDisableProvider : IProtocolDisableProvider<DisableState>
    {
        /// <summary>
        ///     Called when SAS get re-configured from the audit menu
        /// </summary>
        /// <returns>The task for when the SAS client is being reconfigured</returns>
        Task OnSasReconfigured();
    }
}