namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;

public readonly struct RegionViewRegistration : IEquatable<RegionViewRegistration>
{
    public string RegionName { get; init; }

    public string ViewName { get; init; }

    public Func<object> ViewCreator { get; init; }

    public bool Equals(RegionViewRegistration other)
    {
        return RegionName == other.RegionName && ViewName == other.ViewName;
    }

    public override bool Equals(object obj)
    {
        if (obj is RegionViewRegistration regionView)
            return Equals(regionView);

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RegionName, ViewName);
    }

    public static bool operator ==(RegionViewRegistration left, RegionViewRegistration right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RegionViewRegistration left, RegionViewRegistration right)
    {
        return !(left == right);
    }
}
