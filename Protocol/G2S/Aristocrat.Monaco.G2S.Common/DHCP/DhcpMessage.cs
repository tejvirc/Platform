namespace Aristocrat.Monaco.G2S.Common.DHCP
{
    using System;
    using System.Collections.Generic;

    internal class DhcpMessage
    {
        private const uint DhcpOptionsMagicNumber = 1669485411;
        private const uint WinDhcpOptionsMagicNumber = 1666417251;
        private const int DhcpMinimumMessageSize = 236;

        private readonly byte[] _mAssignedAddress = new byte[4];
        private readonly byte[] _mClientAddress = new byte[4];
        private readonly byte[] _mClientHardwareAddress = new byte[16];
        private readonly byte[] _mNextServerAddress = new byte[4];
        private readonly Dictionary<DhcpOption, byte[]> _mOptions = new Dictionary<DhcpOption, byte[]>();
        private readonly byte[] _mRelayAgentAddress = new byte[4];

        private int _mOptionDataSize;

        public DhcpMessage()
        {
        }

        internal DhcpMessage(DhcpData data)
            : this(data.MessageBuffer)
        {
        }

        public DhcpMessage(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length == 0)
            {
                throw new ArgumentException(@"Value cannot be an empty collection.", nameof(data));
            }

            var offset = 0;
            Operation = (DhcpOperation)data[offset++];
            Hardware = (HardwareType)data[offset++];
            HardwareAddressLength = data[offset++];
            Hops = data[offset++];

            SessionId = BitConverter.ToInt32(data, offset);
            offset += 4;

            var secondsElapsed = new byte[2];
            Array.Copy(data, offset, secondsElapsed, 0, 2);
            SecondsElapsed = BitConverter.ToUInt16(ReverseByteOrder(secondsElapsed), 0);
            offset += 2;

            Flags = BitConverter.ToUInt16(data, offset);
            offset += 2;

            Array.Copy(data, offset, _mClientAddress, 0, 4);
            offset += 4;
            Array.Copy(data, offset, _mAssignedAddress, 0, 4);
            offset += 4;
            Array.Copy(data, offset, _mNextServerAddress, 0, 4);
            offset += 4;
            Array.Copy(data, offset, _mRelayAgentAddress, 0, 4);
            offset += 4;
            Array.Copy(data, offset, _mClientHardwareAddress, 0, 16);
            offset += 16;

            offset += 192; // Skip server host name and boot file

            if (offset + 4 < data.Length &&
                (BitConverter.ToUInt32(data, offset) == DhcpOptionsMagicNumber ||
                 BitConverter.ToUInt32(data, offset) == WinDhcpOptionsMagicNumber))
            {
                offset += 4;
                var end = false;

                while (!end && offset < data.Length)
                {
                    var option = (DhcpOption)data[offset];
                    offset++;

                    int optionLen;

                    switch (option)
                    {
                        case DhcpOption.Pad:
                            continue;
                        case DhcpOption.End:
                            end = true;
                            continue;
                        default:
                            optionLen = data[offset];
                            offset++;
                            break;
                    }

                    var optionData = new byte[optionLen];

                    Array.Copy(data, offset, optionData, 0, optionLen);
                    offset += optionLen;

                    _mOptions.Add(option, optionData);
                    _mOptionDataSize += optionLen;
                }
            }
        }

        public DhcpOperation Operation { get; set; } = DhcpOperation.BootRequest;

        public HardwareType Hardware { get; set; } = HardwareType.Ethernet;

        public byte HardwareAddressLength { get; set; }

        public byte Hops { get; set; }

        public int SessionId { get; set; }

        public ushort SecondsElapsed { get; set; }

        public ushort Flags { get; set; }

        public byte[] ClientAddress
        {
            get => _mClientAddress;
            set => CopyData(value, _mClientAddress);
        }

        public byte[] AssignedAddress
        {
            get => _mAssignedAddress;
            set => CopyData(value, _mAssignedAddress);
        }

        public byte[] NextServerAddress
        {
            get => _mNextServerAddress;
            set => CopyData(value, _mNextServerAddress);
        }

        public byte[] RelayAgentAddress
        {
            get => _mRelayAgentAddress;
            set => CopyData(value, _mRelayAgentAddress);
        }

        public byte[] ClientHardwareAddress
        {
            get
            {
                var hardwareAddress = new byte[HardwareAddressLength];
                Array.Copy(_mClientHardwareAddress, hardwareAddress, HardwareAddressLength);
                return hardwareAddress;
            }

            set
            {
                CopyData(value, _mClientHardwareAddress);
                HardwareAddressLength = (byte)value.Length;
                for (var i = value.Length; i < 16; i++)
                {
                    _mClientHardwareAddress[i] = 0;
                }
            }
        }

        public byte[] OptionOrdering { get; set; } = { };

        public byte[] GetOptionData(DhcpOption option)
        {
            if (_mOptions.ContainsKey(option))
            {
                return _mOptions[option];
            }

            return null;
        }

        public void AddOption(DhcpOption option, params byte[] data)
        {
            if (option == DhcpOption.Pad || option == DhcpOption.End)
            {
                throw new ArgumentException(@"The Pad and End DhcpOptions cannot be added explicitly.", nameof(option));
            }

            _mOptions.Add(option, data);
            _mOptionDataSize += data.Length;
        }

        public bool RemoveOption(DhcpOption option)
        {
            if (_mOptions.ContainsKey(option))
            {
                _mOptionDataSize -= _mOptions[option].Length;
            }

            return _mOptions.Remove(option);
        }

        public void ClearOptions()
        {
            _mOptionDataSize = 0;
            _mOptions.Clear();
        }

        public byte[] ToArray()
        {
            var data = new byte[DhcpMinimumMessageSize +
                                (_mOptions.Count > 0 ? 4 + _mOptions.Count * 2 + _mOptionDataSize + 1 : 0)];

            var offset = 0;

            data[offset++] = (byte)Operation;
            data[offset++] = (byte)Hardware;
            data[offset++] = HardwareAddressLength;
            data[offset++] = Hops;

            BitConverter.GetBytes(SessionId).CopyTo(data, offset);
            offset += 4;

            ReverseByteOrder(BitConverter.GetBytes(SecondsElapsed)).CopyTo(data, offset);
            offset += 2;

            BitConverter.GetBytes(Flags).CopyTo(data, offset);
            offset += 2;

            _mClientAddress.CopyTo(data, offset);
            offset += 4;

            _mAssignedAddress.CopyTo(data, offset);
            offset += 4;

            _mNextServerAddress.CopyTo(data, offset);
            offset += 4;

            _mRelayAgentAddress.CopyTo(data, offset);
            offset += 4;

            _mClientHardwareAddress.CopyTo(data, offset);
            offset += 16;

            offset += 192;

            if (_mOptions.Count > 0)
            {
                BitConverter.GetBytes(WinDhcpOptionsMagicNumber).CopyTo(data, offset);
                offset += 4;

                foreach (var optionId in OptionOrdering)
                {
                    var option = (DhcpOption)optionId;
                    if (_mOptions.ContainsKey(option))
                    {
                        data[offset++] = optionId;
                        if (_mOptions[option] != null && _mOptions[option].Length > 0)
                        {
                            data[offset++] = (byte)_mOptions[option].Length;
                            Array.Copy(_mOptions[option], 0, data, offset, _mOptions[option].Length);
                            offset += _mOptions[option].Length;
                        }
                    }
                }

                foreach (var option in _mOptions.Keys)
                {
                    if (Array.IndexOf(OptionOrdering, (byte)option) == -1)
                    {
                        data[offset++] = (byte)option;
                        if (_mOptions[option] != null && _mOptions[option].Length > 0)
                        {
                            data[offset++] = (byte)_mOptions[option].Length;
                            Array.Copy(_mOptions[option], 0, data, offset, _mOptions[option].Length);
                            offset += _mOptions[option].Length;
                        }
                    }
                }

                data[offset] = (byte)DhcpOption.End;
            }

            return data;
        }

        private void CopyData(byte[] source, byte[] dest)
        {
            if (source.Length > dest.Length)
            {
                throw new ArgumentException($@"Values must be no more than {dest.Length} bytes.", nameof(dest));
            }

            source.CopyTo(dest, 0);
        }

        public static byte[] ReverseByteOrder(byte[] source)
        {
            var reverse = new byte[source.Length];

            var j = 0;
            for (var i = source.Length - 1; i >= 0; i--)
            {
                reverse[j++] = source[i];
            }

            return reverse;
        }
    }
}