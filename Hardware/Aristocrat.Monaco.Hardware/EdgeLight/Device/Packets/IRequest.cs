namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    public interface IRequest
    {
        byte[] Data { get; set; }
        byte Type { get; set; }
        void Wrap(byte[] data);
    }
}