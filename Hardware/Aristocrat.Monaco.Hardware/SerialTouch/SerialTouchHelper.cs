namespace Aristocrat.Monaco.Hardware.SerialTouch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using log4net;
    using NativeTouch;

    /// <summary>
    ///     Serial touch helper class
    /// </summary>
    public static class SerialTouchHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public static (int x, int y) GetRawFormatTabletPoint(byte[] touchPacket)
        {
            if (!IsValidTouchDataPacket(touchPacket))
            {
                Logger.Error($"ConvertFormatTabletCoordinate - Invalid data packet [{touchPacket.ToHexString()}]");
                return (0, 0);
            }

            var x = touchPacket[M3SerialTouchConstants.TouchDataPacketLowXByte] |
                    (touchPacket[M3SerialTouchConstants.TouchDataPacketHighXByte] << M3SerialTouchConstants.TouchDataPacketHighByteShift);
            var y = touchPacket[M3SerialTouchConstants.TouchDataPacketLowYByte] |
                    (touchPacket[M3SerialTouchConstants.TouchDataPacketHighYByte] << M3SerialTouchConstants.TouchDataPacketHighByteShift);

            return (x, y);
        }

        public static (int x, int y) GetScaledFormatTabletPoint(byte[] touchPacket)
        {
            var point = GetRawFormatTabletPoint(touchPacket);
            var adjustX = M3SerialTouchConstants.MaxCoordinateRange / SystemParameters.PrimaryScreenWidth;
            var xCoord = Convert.ToInt32(Math.Round(point.x > 0 ? point.x / adjustX : 0));
            var adjustY = M3SerialTouchConstants.MaxCoordinateRange / SystemParameters.PrimaryScreenHeight;
            var flipY = M3SerialTouchConstants.MaxCoordinateRange - point.y;
            var yCoord = Convert.ToInt32(Math.Round(flipY > 0 ? flipY / adjustY : 0));

            return (xCoord, yCoord);
        }

        public static string ToHexString(this byte b)
        {
            return b.ToString("X2");
        }

        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            return string.Join(":", bytes.Select(x => x.ToString("X2")));
        }

        public static bool IsSyncBitSet(byte b)
        {
            return (b & M3SerialTouchConstants.SyncBit) == M3SerialTouchConstants.SyncBit;
        }

        public static bool IsValidTouchDataPacket(byte[] bytes)
        {
            return (bytes?.Any() ?? false) &&
                   IsSyncBitSet(bytes[0]) &&
                   bytes.Length == M3SerialTouchConstants.TouchDataLength;
        }

        public static bool IsDownTouchDataPacket(byte[] bytes)
        {
            return IsValidTouchDataPacket(bytes) && (bytes[0] & M3SerialTouchConstants.ProximityBit) == M3SerialTouchConstants.ProximityBit;
        }

        public static bool IsUpTouchDataPacket(byte[] bytes)
        {
            return IsValidTouchDataPacket(bytes) && (bytes[0] & M3SerialTouchConstants.ProximityBit) == 0;
        }

        public static bool IsValidCommandResponsePacket(byte[] bytes)
        {
            return (bytes?.Any() ?? false) &&
                   bytes.Length >= M3SerialTouchConstants.MinimumResponseLength &&
                   (bytes[0] & M3SerialTouchConstants.Header) == M3SerialTouchConstants.Header &&
                   (bytes[bytes.Length - 1] & M3SerialTouchConstants.Terminator) == M3SerialTouchConstants.Terminator;
        }

        public static byte[] StripHeaderAndTerminator(byte[] bytes)
        {
            return !IsValidCommandResponsePacket(bytes) ? bytes : bytes.Skip(1).Take(bytes.Length - 2).ToArray();
        }

        public static byte[] ToggleProximityBit(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return bytes;
            }

            var newBytes = bytes.ToArray();
            newBytes[0] = (byte)(newBytes[0] ^ M3SerialTouchConstants.ProximityBit);
            return newBytes;
        }

        internal static TouchInput GetPointerTouchInfo(byte[] touchPacket, TouchAction action, uint id)
        {
            var (x, y) = GetScaledFormatTabletPoint(touchPacket);
            return new TouchInput
            {
                Action = action,
                ContactArea =
                    new ContactArea(
                        x - M3SerialTouchConstants.TouchRadius,
                        x + M3SerialTouchConstants.TouchRadius,
                        y - M3SerialTouchConstants.TouchRadius,
                        y + M3SerialTouchConstants.TouchRadius),
                TouchPressure = M3SerialTouchConstants.TouchPressure,
                TouchOrientation = M3SerialTouchConstants.TouchOrientation,
                PointerId = id,
                PointerType = PointerType.Touch,
                Point = new TouchPoint(x, y)
            };
        }
    }
}
