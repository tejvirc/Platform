namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Contracts.Authentication;

    public readonly struct AlgorithmInfo : IEquatable<AlgorithmInfo>
    {
        public AlgorithmInfo(string name, AlgorithmType type, int hashSize)
        {
            const int hashLengthDivider = 4;
            Name = name;
            Type = type;
            HexHashLength = hashSize / hashLengthDivider; // We want the size of the hash in Hex
            AllZerosKey = new string('0', HexHashLength); // Default Zero value for this Hash
        }

        public string Name { get; }

        public AlgorithmType Type { get; }

        public int HexHashLength { get; }

        public string AllZerosKey { get; }

        public bool CanUseHMacKey =>
            Type is AlgorithmType.HmacSha1 or AlgorithmType.HmacSha256 or AlgorithmType.HmacSha512;

        public static bool operator ==(AlgorithmInfo left, AlgorithmInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AlgorithmInfo left, AlgorithmInfo right)
        {
            return !left.Equals(right);
        }

        public bool Equals(AlgorithmInfo other)
        {
            return Name == other.Name && Type == other.Type && HexHashLength == other.HexHashLength && AllZerosKey == other.AllZerosKey;
        }

        public override bool Equals(object obj)
        {
            return obj is AlgorithmInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Type;
                hashCode = (hashCode * 397) ^ HexHashLength;
                hashCode = (hashCode * 397) ^ (AllZerosKey != null ? AllZerosKey.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString() => Name;
    }
}