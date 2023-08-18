namespace Aristocrat.Monaco.Application.Authentication;

using System;
using System.Collections.Generic;
using System.IO;

public class FileInfoComparer : IComparer<FileInfo>, IEqualityComparer<FileInfo>
{
    private static readonly HashingInfoComparer InfoComparer = new();

    public int Compare(FileInfo x, FileInfo y)
    {
        if (Equals(x, y))
        {
            return 0;
        }

        if (x is null || y is null)
        {
            return StringComparer.InvariantCulture.Compare(x?.FullName, y?.FullName);
        }

        if (string.IsNullOrEmpty(x.Extension) || string.IsNullOrEmpty(y.Extension))
        {
            return StringComparer.InvariantCulture.Compare(x.FullName, y.FullName);
        }

        return InfoComparer.Compare(x.FullName, y.FullName);
    }

    public bool Equals(FileInfo x, FileInfo y)
    {
        return ReferenceEquals(x, y) ||
               x is not null && y is not null && string.Equals(x.FullName, y.FullName, StringComparison.Ordinal);
    }

    public int GetHashCode(FileInfo obj)
    {
        return obj?.FullName.GetHashCode(StringComparison.InvariantCulture) ??
               throw new ArgumentNullException(nameof(obj));
    }
}