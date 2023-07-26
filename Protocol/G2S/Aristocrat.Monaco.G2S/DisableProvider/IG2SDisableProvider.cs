namespace Aristocrat.Monaco.G2S.DisableProvider
{
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.Monaco.Protocol.Common.DisableProvider;
    using Kernel;

    public interface IG2SDisableProvider : IProtocolDisableProvider<G2SDisableStates>
    {
        /// <summary>
        ///     Called when G2S get re-configured from the audit menu
        /// </summary>
        /// <returns>The task for when the G2S client is being reconfigured</returns>
        Task OnG2SReconfigured();
    }
}
