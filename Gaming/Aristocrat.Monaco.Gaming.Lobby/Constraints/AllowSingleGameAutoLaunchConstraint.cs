namespace Aristocrat.Monaco.Gaming.Lobby.Constraints;

using System;
using Contracts;
using Contracts.Constraints;
using Kernel;

/// <summary>
///     
/// </summary>
public sealed class AllowSingleGameAutoLaunchConstraint : IConstraint
{
    private readonly IPropertiesManager _properties;

    /// <summary>
    ///     Initializes an instance of the <see cref="AllowSingleGameAutoLaunchConstraint"/> class.
    /// </summary>
    /// <param name="properties"><see cref="IPropertiesManager"/></param>
    public AllowSingleGameAutoLaunchConstraint(IPropertiesManager properties)
    {
        _properties = properties;
    }

    /// <inheritdoc />
    public string Name => ConstraintNames.AllowSingleGameAutoLaunch;

    /// <inheritdoc />
    public bool Validate<T>(T? parameter = default) where T : ConstraintParameters
    {
        var allowGameInCharge = _properties.GetValue(GamingConstants.AllowGameInCharge, GetDefaultValue());
    }

    public bool GetDefaultValue()
    {
        var marketTpe = _properties.GetValue(GamingConstants.MarketType, MarketType.Unknown);

        if (marketTpe == MarketType.Unknown)
        {
            throw new InvalidOperationException($"Market type was not set");
        }

        return marketTpe == MarketType.Class2;
    }
}
