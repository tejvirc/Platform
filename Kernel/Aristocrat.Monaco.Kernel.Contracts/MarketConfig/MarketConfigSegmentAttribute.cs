namespace Aristocrat.Monaco.Kernel.MarketConfig;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Attribute to mark a class as a market config segment model object and map that class to a segment id.
///     <seealso cref="IMarketConfigManager"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MarketConfigSegmentAttribute : Attribute
{
    /// <summary>
    ///     The configuration tool segment ID that maps to this model class.
    /// </summary>
    [DisallowNull]
    public string SegmentId { get; }

    /// <summary>
    ///     Default constructor used by applying the attribute to a class.
    /// </summary>
    /// <param name="segmentId">
    ///     The configuration tool segment ID that maps to this model class.
    /// </param>
    public MarketConfigSegmentAttribute(string segmentId) => SegmentId = segmentId;
}
