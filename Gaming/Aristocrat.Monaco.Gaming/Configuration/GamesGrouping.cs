namespace Aristocrat.Monaco.Gaming.Configuration
{
    using System;
    using Contracts.Models;

    public class GamesGrouping
    {
        public GamesGrouping(long denom, GameType gameType, string themeId, string themeName)
        {
            Denom = denom;
            GameType = gameType;
            ThemeId = themeId;
            ThemeName = themeName;
        }

        public long Denom { get; }

        public GameType GameType { get; }

        public string ThemeId { get; }

        public string ThemeName { get; }

        public static bool operator ==(GamesGrouping left, GamesGrouping right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GamesGrouping left, GamesGrouping right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Denom.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)GameType;
                hashCode = (hashCode * 397) ^
                           (ThemeName != null ? StringComparer.InvariantCulture.GetHashCode(ThemeName) : 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((GamesGrouping)obj);
        }

        private bool Equals(GamesGrouping other)
        {
            return Denom == other.Denom && GameType == other.GameType && string.Equals(
                ThemeName,
                other.ThemeName,
                StringComparison.InvariantCulture);
        }
    }
}