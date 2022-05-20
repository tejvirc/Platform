namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    public static class ByteHelper
    {
        public static ushort ToUShort(byte[] data, int offset)
        {
            return (ushort)((data[offset + 1] << 8) | data[offset]);
        }
    }

    public class ResponseBase : IResponse
    {
        public byte[] Data { get; set; } = new byte[64];
        public byte Type => Data[0];

        public void Wrap(byte[] data)
        {
            Data = data;
        }
    }

    public class RequestBase : IRequest
    {
        public byte[] Data { get; set; } = new byte[64];

        public byte Type
        {
            get => Data[0];
            set => Data[0] = value;
        }

        public void Wrap(byte[] data)
        {
            Data = data;
        }
    }
}