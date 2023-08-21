namespace Aristocrat.Monaco.Hardware.Serial.Protocols
{
    using System;

    public class MessagedBuiltEventArgs : EventArgs
    {
        public byte[] Message { get; }

        public MessagedBuiltEventArgs(byte[] message)
        {
            Message = message;
        }
    }
}