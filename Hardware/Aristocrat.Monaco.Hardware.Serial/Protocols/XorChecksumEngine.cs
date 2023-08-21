namespace Aristocrat.Monaco.Hardware.Serial.Protocols
{
    using System;

    public class XorChecksumEngine : ICrcEngine
    {
        public ushort Crc { get; private set; }

        public void Initialize(ushort seed)
        {
            Crc = seed;
        }

        public void Hash(byte[] bytes, uint start, uint count)
        {
            if (bytes.Length < count + start)
            {
                throw new ArgumentException();
            }

            for (var i = start; i < (count + start); ++i)
            {
                Hash(bytes[i]);
            }
        }

        private void Hash(byte b)
        {
            Crc ^= b;
        }
    }
}