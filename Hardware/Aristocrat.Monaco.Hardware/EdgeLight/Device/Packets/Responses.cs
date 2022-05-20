namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    public class LedConfigurationBase : ResponseBase
    {
        protected virtual int ChannelCountIndex => 11;

        public int ChannelCount => Data[ChannelCountIndex] +
                                   EdgeLightConstants.MaxChannelsPerBoard;

        public int BoardId => ByteHelper.ToUShort(Data, 9);
        public virtual byte NumberOfAdditionalReports => 0;

        public IList<(int StripId, int Count)> LedCounts =>
            Enumerable.Range(0, ChannelCount).Select(GetLedCount).ToList();

        protected virtual (int StripId, int Count) GetLedCount(int channel)
        {
            var offset = (channel < EdgeLightConstants.MaxChannelsPerBoard
                             ? 1
                             : 12) + channel * 2;
            return ((BoardId << 8) | channel,
                ByteHelper.ToUShort(Data, offset) /
                EdgeLightConstants.RgbBytesPerLed);
        }
    }

    [Accepts((int)ResponseType.LedConfiguration)]
    public class LedConfigurationResponse : LedConfigurationBase
    {
    }

    [Accepts((int)ResponseType.AlternateLedStripConfiguration)]
    public class AlternateLedConfigurationResponse : LedConfigurationBase
    {
        public override byte NumberOfAdditionalReports => Data[63];

        protected override (int StripId, int Count) GetLedCount(int channel)
        {
            return channel >= EdgeLightConstants.MaxChannelsPerBoard
                ? GetVirtualLedCount(channel)
                : base.GetLedCount(channel);
        }

        private (int StripId, int Count) GetVirtualLedCount(int index)
        {
            var offset = 12 +
                         (index - EdgeLightConstants.MaxChannelsPerBoard) * 3;
            return ((BoardId << 8) | Data[offset],
                ByteHelper.ToUShort(Data, offset + 1) /
                EdgeLightConstants.RgbBytesPerLed);
        }
    }

    [Accepts((int)ResponseType.AdditionalConfiguration)]
    public class AdditionalLedConfigurationResponse : ResponseBase
    {
        public int PacketIndex => Data[1];
        public int Size => Data[2];
        public int VirtualStripCount => Size / 3;

        public IList<(byte StripId, int Count)> VirtualLedCounts =>
            Enumerable.Range(0, VirtualStripCount).Select(GetVirtualLedCount).ToList();

        private (byte StripId, int Count) GetVirtualLedCount(int index)
        {
            var offset = 3 + index * 3;
            return (Data[offset],
                ByteHelper.ToUShort(Data, offset + 1) /
                EdgeLightConstants.RgbBytesPerLed);
        }
    }

    [Accepts((int)ResponseType.AlternateLedStripConfigurationWithLocation)]
    public class AlternateLedConfigurationLocationResponse : LedConfigurationBase
    {
        protected override int ChannelCountIndex => 12;
        public ushort BoardLocation => Data[11];

        public override byte NumberOfAdditionalReports => Data[63];

        protected override (int StripId, int Count) GetLedCount(int channel)
        {
            return channel >= EdgeLightConstants.MaxChannelsPerBoard
                ? GetVirtualLedCount(channel)
                : base.GetLedCount(channel);
        }

        private (int StripId, int Count) GetVirtualLedCount(int index)
        {
            var offset = ChannelCountIndex + 1 +
                         (index - EdgeLightConstants.MaxChannelsPerBoard) * 3;
            return ((BoardId << 8) | Data[offset],
                ByteHelper.ToUShort(Data, offset + 1) /
                EdgeLightConstants.RgbBytesPerLed);
        }
    }
}