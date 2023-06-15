namespace Aristocrat.Monaco.Asp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Asp.Client.Comms;

    public class CommPortMock : ICommPort
    {
        public CommPortMock()
        {
            GetDataLinkDataFunc = LinkUpGetDataFunction;
            GetTransportDataFunc = TransportGetDataFunction;
            GetAppDataFunc = () => new List<byte>();
        }

        private List<byte> DataBytesToSend { get; set; } = new List<byte>();

        public byte Sequence { get; set; }
        public bool UpdateSequence { get; set; } = true;
        public byte Mac { get; set; } = 0x40;
        public Func<List<byte>> GetDataLinkDataFunc { get; set; }
        public Func<List<byte>> GetTransportDataFunc { get; set; }
        public Func<List<byte>> GetAppDataFunc { get; set; }
        public Func<List<byte>, int> DataReceivedAction { get; set; }

        public void Dispose()
        {
        }

        public bool IsOpen { get; private set; }
        public string PortName { get; set; }

        public void Open()
        {
            IsOpen = true;
        }

        public void Purge()
        {
        }

        public void Close()
        {
        }

        public int Read(byte[] buffer, int offset, uint numberOfBytesToRead)
        {
            if (DataBytesToSend.Count == 0)
            {
                if (GetDataLinkDataFunc != null)
                {
                    DataBytesToSend = GetDataLinkDataFunc();
                }

                if (DataBytesToSend.Count == 0)
                {
                    return 0;
                }
            }

            var bytesCanRead = (int)Math.Min(DataBytesToSend.Count, numberOfBytesToRead);
            Array.Copy(DataBytesToSend.ToArray(), 0, buffer, offset, bytesCanRead);
            DataBytesToSend.RemoveRange(0, bytesCanRead);
            return bytesCanRead;
        }

        public int Write(byte[] bytesToWrite, int offset, uint numberOfBytesToWrite)
        {
            if (DataReceivedAction != null)
            {
                return DataReceivedAction(
                    new ArraySegment<byte>(bytesToWrite, offset, (int)numberOfBytesToWrite).ToList());
            }

            return (int)numberOfBytesToWrite;
        }

        public List<byte> TransportGetDataFunction()
        {
            var data = new List<byte>();
            List<byte> appData;
            if (GetAppDataFunc == null || (appData = GetAppDataFunc()).Count <= 0)
            {
                return data;
            }

            data.Add((byte)(appData.Count + 1));
            data.Add(0x30);
            data.AddRange(appData);

            return data;
        }

        public List<byte> LinkUpGetDataFunction()
        {
            var data = new List<byte> { (byte)(Mac | ((Sequence & 0x7) << 3)), 0 };
            if (GetTransportDataFunc != null)
            {
                var tpData = GetTransportDataFunc();
                if (tpData.Count > 0)
                {
                    var pad = tpData.Count % 2 != 0;
                    data[1] = (byte)(tpData.Count + (pad ? 1 : 0));
                    data.AddRange(tpData);
                    if (pad)
                    {
                        data.Add(0);
                    }
                }
            }

            var dataArray = data.ToArray();
            int crc = AspCrc.CalcCrc(dataArray, 0, data.Count);
            data.Add((byte)((crc >> 8) & 0xFF));
            data.Add((byte)(crc & 0xFF));
            if (UpdateSequence)
            {
                Sequence = (byte)((Sequence + 1) & 0x7);
            }

            return data;
        }
    }
}