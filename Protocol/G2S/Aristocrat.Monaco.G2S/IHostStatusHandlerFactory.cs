namespace Aristocrat.Monaco.G2S
{
    using Aristocrat.G2S.Client.Devices;

    public interface IHostStatusHandlerFactory
    {
        object Create<TDevice>(TDevice device)
            where TDevice : IDevice;
    }
}