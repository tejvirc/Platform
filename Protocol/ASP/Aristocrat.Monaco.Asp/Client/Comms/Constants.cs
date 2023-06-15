namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using System;
    using System.IO.Ports;
    using System.Threading;
    using NativeSerial;

    public static class Constants
    {
        public static readonly SerialConfiguration DefaultConfiguration = new()
        {
            ReadIntervalTimeout = Timeout.InfiniteTimeSpan,
            ReadTotalTimeout = TimeSpan.FromMilliseconds(20),
            BaudRate = 19200,
            Parity = Parity.None,
            StopBits = NativeStopBits.One,
            BitsPerByte = 8,
            WriteTotalTimeout = TimeSpan.FromMilliseconds(100)
        };
    }
}