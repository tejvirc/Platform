namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using System;

    public interface ICommPort : IDisposable
    {
        bool IsOpen { get; }
        string PortName { get; set; }
        void Open();
        void Purge();
        void Close();
        int Read(byte[] buffer, int offset, uint numberOfBytesToRead);
        int Write(byte[] bytesToWrite, int offset, uint numberOfBytesToWrite);
    }
}