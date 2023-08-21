namespace Aristocrat.Monaco.G2S.Common.DHCP
{
    using System;
    using System.Net;

    internal class DhcpData
    {
        private int _mBufferSize;

        public DhcpData(byte[] messageBuffer)
        {
            MessageBuffer = messageBuffer;
            _mBufferSize = messageBuffer.Length;
        }

        public DhcpData(IPEndPoint source, byte[] messageBuffer)
        {
            Source = source;
            MessageBuffer = messageBuffer;
            _mBufferSize = messageBuffer.Length;
        }

        public IPEndPoint Source { get; set; }

        public byte[] MessageBuffer { get; private set; }

        public int BufferSize
        {
            get => _mBufferSize;

            set
            {
                _mBufferSize = value;

                var oldBuffer = MessageBuffer;
                MessageBuffer = new byte[_mBufferSize];

                var copyLen = Math.Min(oldBuffer.Length, _mBufferSize);
                Array.Copy(oldBuffer, MessageBuffer, copyLen);
            }
        }

        public IAsyncResult Result { get; set; }
    }
}