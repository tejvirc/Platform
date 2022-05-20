namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    [Accepts((int)RequestType.GetLedConfiguration)]
    public class GetLedConfiguration : RequestBase
    {
        public GetLedConfiguration()
        {
            Type = 0x60;
        }
    }

    public class ChannelFrameBase : RequestBase
    {
        public int PacketIndex
        {
            get => Data[1];
            set => Data[1] = (byte)value;
        }

        public int Size
        {
            get => Data[2];
            set => Data[2] = (byte)value;
        }

        private int ExtraBytesInSize => RgbDataIndex - 3;
        public virtual int StripId { get; set; }
        protected virtual int RgbDataIndex { get; } = 3;
        public int RgbDataByteCount => Math.Max(Size - ExtraBytesInSize, 0);

        public byte[] RgbData
        {
            set
            {
                Size = (byte)Math.Min(value.Length + ExtraBytesInSize, 61);
                Array.Copy(value, 0, Data, RgbDataIndex, Size - ExtraBytesInSize);
            }
        }

        public static List<ChannelFrameBase> PrepareChunks(int stripId, byte[] rgbBytes)
        {
            return stripId < 4
                ? PrepareChunks<ChannelFrame>(stripId, rgbBytes)
                : PrepareChunks<LedDataUpdate>(stripId + 1, rgbBytes);
        }

        public static List<ChannelFrameBase> PrepareChunks<T>(int stripId, byte[] rgbBytes)
            where T : ChannelFrameBase, new()
        {
            var bytesUsed = 0;
            var packets = new List<ChannelFrameBase>();
            var lastPacket = new T();
            while (rgbBytes.Length - bytesUsed > 0)
            {
                lastPacket = new T
                {
                    StripId = stripId, RgbData = rgbBytes.Skip(bytesUsed).ToArray(), PacketIndex = packets.Count + 1
                };
                bytesUsed += lastPacket.RgbDataByteCount;
                packets.Add(lastPacket);
            }

            if (lastPacket.Size == 61)
            {
                packets.Add(new T { StripId = stripId, PacketIndex = packets.Count + 1, Size = 1});
            }

            return packets;
        }
    }

    [Accepts(
        (int)RequestType.Channel1LedFrame,
        (int)RequestType.Channel2LedFrame,
        (int)RequestType.Channel3LedFrame,
        (int)RequestType.Channel4LedFrame)]
    public class ChannelFrame : ChannelFrameBase
    {
        public override int StripId
        {
            get => (int)(Type - RequestType.Channel1LedFrame);
            set => Type = (byte)((int)RequestType.Channel1LedFrame + value);
        }
    }

    [Accepts((int)RequestType.SetLowPowerMode)]
    public class LowPowerMode : RequestBase
    {
        public LowPowerMode()
        {
            Type = (byte)RequestType.SetLowPowerMode;
        }

        public bool Control
        {
            get => Data[1] != 0;
            set => Data[1] = (byte)(value ? 1 : 0);
        }
    }

    [Accepts((int)RequestType.SetDeviceLedBrightness)]
    public class BrightnessControl : RequestBase
    {
        public BrightnessControl()
        {
            Type = (byte)RequestType.SetDeviceLedBrightness;
        }

        public byte GetLevel(int channel)
        {
            return Data[channel + 1];
        }

        public void SetLevel(int channel, int level)
        {
            Data[channel + 1] = (byte)Math.Min(level, EdgeLightConstants.MaxChannelBrightness);
        }

        public void SetBrightnessLevel(int brightnessChannelStart, int numBrightnessChannels, int level)
        {
            foreach (int channel in Enumerable.Range(brightnessChannelStart + 1, numBrightnessChannels))
            {
                Data[channel] = (byte)Math.Min(level, EdgeLightConstants.MaxChannelBrightness);
            }
        }
    }

    [Accepts((int)RequestType.UpdateVirtualStrip)]
    public class LedDataUpdate : ChannelFrameBase
    {
        public LedDataUpdate()
        {
            Type = (byte)RequestType.UpdateVirtualStrip;
        }

        public override int StripId
        {
            get => Data[3];
            set => Data[3] = (byte)value;
        }

        protected override int RgbDataIndex { get; } = 4;
    }

    [Accepts((int)RequestType.SetBarkeeperLedBrightness)]
    public class StripBrightnessControl : RequestBase
    {
        public StripBrightnessControl()
        {
            Type = (byte)RequestType.SetBarkeeperLedBrightness;
        }

        public int StripId
        {
            get => Data[1];
            set => Data[1] = (byte)value;
        }

        public byte Level
        {
            get => Data[2];
            set => Data[2] = value;
        }
    }
}