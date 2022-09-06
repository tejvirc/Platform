namespace Aristocrat.Monaco.Hardware.SerialTouch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static SerialTouchHelper;

    public class TouchPacketBuilder
    {
        private readonly object _lock = new();
        private readonly List<byte[]> _availablePackets = new();
        private readonly List<byte> _packetUnderConstruction = new();
        private readonly List<InvalidPacketException> _exceptions = new();

        public void Append(byte[] bytes)
        {
            lock (_lock)
            {
                _exceptions.Clear();

                foreach (var currentByte in bytes)
                {
                    if (TryHandleByteWithNewPacket(currentByte))
                    {
                        continue;
                    }
                    
                    if (TryHandleByteWithExistingPacket(currentByte))
                    {
                        continue;
                    }

                    _packetUnderConstruction.Add(currentByte);
                    MakeFullPacketAvailable(currentByte);
                }

                if (_exceptions.Count > 0)
                {
                    throw new AggregateException(_exceptions);
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _packetUnderConstruction.Clear();
                _availablePackets.Clear();
            }
        }

        public bool TryTakePackets(out IEnumerable<byte[]> packets)
        {
            lock (_lock)
            {
                if (_availablePackets.Count > 0)
                {
                    packets = _availablePackets.ToArray();
                    _availablePackets.Clear();
                    return true;
                }
            }

            packets = null;
            return false;
        }

        private bool TryHandleByteWithNewPacket(byte b)
        {
            if (_packetUnderConstruction.Any())
            {
                return false;
            }

            // Partial packet is empty so this byte should be the header or have the sync bit set
            if (b == M3SerialTouchConstants.Header || IsSyncBitSet(b))
            {
                _packetUnderConstruction.Add(b);
                return true;
            }

            // There is no current packet and this is not a valid byte to start a new packet with
            DropBadData(b);
            return true;
        }

        private bool TryHandleByteWithExistingPacket(byte b)
        {
            // We have a packet under construction
            // Did we get a header or a sync bit before we were expecting it?
            // If so, drop the current packet and start a new one.

            if ((!IsTouchMessage() || !IsSyncBitSet(b)) &&
                (!IsCommandResponse() || b != M3SerialTouchConstants.Header) &&
                (!IsCommandResponse() || !IsSyncBitSet(b)))
            {
                return false;
            }

            DropBadData(b);
            _packetUnderConstruction.Add(b);
            return true;
        }

        private void MakeFullPacketAvailable(byte b)
        {
            // Is it a complete command response or complete message?
            if ((!IsCommandResponse() || b != M3SerialTouchConstants.Terminator) &&
                (!IsTouchMessage() || _packetUnderConstruction.Count != M3SerialTouchConstants.TouchDataLength))
            {
                return;
            }

            _availablePackets.Add(_packetUnderConstruction.ToArray());
            _packetUnderConstruction.Clear();
        }

        private void DropBadData(byte b)
        {
            var packet = _packetUnderConstruction.ToArray();
            _packetUnderConstruction.Clear();

            _exceptions.Add(new InvalidPacketException(b, packet));
        }

        private bool IsCommandResponse()
        {
            return _packetUnderConstruction.Any() && _packetUnderConstruction[0] == M3SerialTouchConstants.Header;
        }

        private bool IsTouchMessage()
        {
            return _packetUnderConstruction.Any() && IsSyncBitSet(_packetUnderConstruction[0]);
        }
    }
}
