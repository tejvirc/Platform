namespace Aristocrat.Monaco.Application.Authentication;

using System;
using System.Collections.Generic;
using System.IO;

public class HashingInfoComparer : IComparer<string>, IEqualityComparer<string>
{
    public int Compare(string x, string y)
    {
        var leftWithout = Path.ChangeExtension(x, null);
        var rightWithout = Path.ChangeExtension(y, null);
        if (string.Equals(leftWithout, rightWithout, StringComparison.Ordinal))
        {
            return StringComparer.InvariantCulture.Compare(x, y);
        }

        if (leftWithout.Contains(rightWithout, StringComparison.InvariantCulture))
        {
            return 1;
        }

        if (rightWithout.Contains(leftWithout, StringComparison.InvariantCulture))
        {
            return -1;
        }

        return StringComparer.InvariantCulture.Compare(x, y);
    }

    public bool Equals(string x, string y)
    {
        return string.Equals(x, y, StringComparison.Ordinal);
    }

    public int GetHashCode(string obj)
    {
        return obj?.GetHashCode(StringComparison.InvariantCulture) ?? throw new ArgumentNullException(obj);
    }
}