namespace Aristocrat.Monaco.Gaming.Contracts.Progressives;

using System;
using System.Collections.Generic;

/// <summary>
///     Event args for added progressive levels
/// </summary>
public sealed class ProgressivesAddedEventArgs : EventArgs
{
    /// <summary>
    ///     Creates an instance of <see cref="ProgressivesAddedEventArgs"/>
    /// </summary>
    /// <param name="addedLevels">A collection of the added progressive levels</param>
    public ProgressivesAddedEventArgs(IReadOnlyCollection<ProgressiveLevel> addedLevels)
    {
        AddedLevels = addedLevels;
    }

    /// <summary>
    ///     The list of added progressive levels
    /// </summary>
    public IReadOnlyCollection<ProgressiveLevel> AddedLevels { get; }
}