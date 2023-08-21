namespace Aristocrat.Monaco.G2S.Handlers
{
    using Aristocrat.G2S.Client.Devices;

    public interface IStatusChangedHandler<in TDevice>
        where TDevice : IDevice
    {
        void Handle(TDevice device);
    }
}