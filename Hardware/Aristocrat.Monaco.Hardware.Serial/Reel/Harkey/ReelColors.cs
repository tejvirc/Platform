namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey
{
    using System;

    public readonly struct ReelColors : IEquatable<ReelColors>
    {
        public ReelColors(ushort top, ushort middle, ushort bottom)
        {
            TopColor = top;
            MiddleColor = middle;
            BottomColor = bottom;
        }

        public ushort TopColor { get; }

        public ushort MiddleColor { get; }

        public ushort BottomColor { get; }

        public bool Equals(ReelColors other)
        {
            return TopColor == other.TopColor && MiddleColor == other.MiddleColor && BottomColor == other.BottomColor;
        }

        public override bool Equals(object obj)
        {
            return obj is ReelColors other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TopColor.GetHashCode();
                hashCode = (hashCode * 397) ^ MiddleColor.GetHashCode();
                hashCode = (hashCode * 397) ^ BottomColor.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ReelColors left, ReelColors right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ReelColors left, ReelColors right)
        {
            return !left.Equals(right);
        }
    }
}
