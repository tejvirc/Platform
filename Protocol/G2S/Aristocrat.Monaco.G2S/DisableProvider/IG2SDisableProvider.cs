namespace Aristocrat.Monaco.G2S.DisableProvider
{
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.Monaco.Protocol.Common.DisableProvider;
    using Kernel;

    public interface IG2SDisableProvider : IProtocolDisableProvider<G2SDisableStates>
    {
    }
}
