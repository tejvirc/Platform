namespace Aristocrat.Monaco.Gaming.Presentation.Constraints;

using Gaming.Contracts;
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
        var allowGameInCharge = _properties.GetValue(GamingConstants.AllowGameInCharge, false);

        return allowGameInCharge;
    }
}
