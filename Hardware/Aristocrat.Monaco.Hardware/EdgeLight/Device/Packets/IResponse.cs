namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    public interface IResponse
    {
        byte[] Data { get; }
        byte Type { get; }
        void Wrap(byte[] data);
    }
}