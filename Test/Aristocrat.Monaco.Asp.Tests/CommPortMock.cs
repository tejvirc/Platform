namespace Aristocrat.Monaco.Asp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using Asp.Client.Comms;
    using NativeSerial;

    public class CommPortMock : INativeComPort
    {
        public CommPortMock()
        {
            GetDataLinkDataFunc = LinkUpGetDataFunction;
            GetTransportDataFunc = TransportGetDataFunction;
            GetAppDataFunc = () => new List<byte>();
        }

        private List<byte> DataBytesToSend { get; set; } = new();

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

        public bool FlushTx()
        {
            return true;
        }

        public bool IsOpen { get; private set; }

        public bool Open(string port, SerialConfiguration configuration)
        {
            IsOpen = true;
            return true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public int Write(byte[] data)
        {
            return DataReceivedAction?.Invoke(new ArraySegment<byte>(data, 0, data.Length).ToList()) ?? data.Length;
        }

        public int Write(byte[] data, int offset, int count)
        {
            return DataReceivedAction?.Invoke(new ArraySegment<byte>(data, offset, count).ToList()) ?? count;
        }

        public int Write(byte data)
        {
            return DataReceivedAction?.Invoke(new ArraySegment<byte>(new[] { data }, 0, 1).ToList()) ?? 1;
        }

        public int WriteWithModifyParity(byte[] data, Parity parity)
        {
            return DataReceivedAction?.Invoke(new ArraySegment<byte>(data, 0, data.Length).ToList()) ?? data.Length;
        }

        public int WriteWithModifyParity(byte[] data, int offset, int count, Parity parity)
        {
            return DataReceivedAction?.Invoke(new ArraySegment<byte>(data, offset, count).ToList()) ?? count;
        }

        public int WriteWithModifyParity(byte data, Parity parity)
        {
            return DataReceivedAction?.Invoke(new ArraySegment<byte>(new[] { data }, 0, 1).ToList()) ?? 1;
        }

        public ComPortByte Read()
        {
            if (DataBytesToSend.Count == 0)
            {
                if (GetDataLinkDataFunc != null)
                {
                    DataBytesToSend = GetDataLinkDataFunc();
                }

                if (DataBytesToSend.Count == 0)
                {
                    return ComPortByte.NoData;
                }
            }

            var read = DataBytesToSend[0];
            DataBytesToSend.RemoveAt(0);
            return new ComPortByte(read, ComPortErrors.None, (uint)DataBytesToSend.Count);
        }

        public ComPortByte Read(TimeSpan readTimeout)
        {
            if (DataBytesToSend.Count == 0)
            {
                if (GetDataLinkDataFunc != null)
                {
                    DataBytesToSend = GetDataLinkDataFunc();
                }

                if (DataBytesToSend.Count == 0)
                {
                    return ComPortByte.NoData;
                }
            }

            var read = DataBytesToSend[0];
            DataBytesToSend.RemoveAt(0);
            return new ComPortByte(read, ComPortErrors.None, (uint)DataBytesToSend.Count);
        }

        public IReadOnlyCollection<ComPortByte> Read(int length)
        {
            if (DataBytesToSend.Count == 0)
            {
                if (GetDataLinkDataFunc != null)
                {
                    DataBytesToSend = GetDataLinkDataFunc();
                }

                if (DataBytesToSend.Count == 0)
                {
                    return Array.Empty<ComPortByte>();
                }
            }

            var comPortBytes = DataBytesToSend.Take(length).Select(x => new ComPortByte(x, ComPortErrors.None, 1))
                .ToArray();
            DataBytesToSend.RemoveRange(0, length);
            return comPortBytes;
        }

        public IReadOnlyCollection<ComPortByte> Read(int length, TimeSpan readTimeout)
        {
            if (DataBytesToSend.Count == 0)
            {
                if (GetDataLinkDataFunc != null)
                {
                    DataBytesToSend = GetDataLinkDataFunc();
                }

                if (DataBytesToSend.Count == 0)
                {
                    return Array.Empty<ComPortByte>();
                }
            }

            var comPortBytes = DataBytesToSend.Take(length).Select(x => new ComPortByte(x, ComPortErrors.None, 1))
                .ToArray();
            DataBytesToSend.RemoveRange(0, length);
            return comPortBytes;
        }

        public bool FlushAll()
        {
            return true;
        }

        public bool FlushRx()
        {
            return true;
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