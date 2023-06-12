namespace Aristocrat.Monaco.Gaming.Contracts.Constraints;

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
    /// <param name="properties"></param>
    public AllowSingleGameAutoLaunchConstraint(IPropertiesManager properties)
    {
        _properties = properties;
    }

    /// <inheritdoc />
    public string Name => ConstraintNames.AllowSingleGameAutoLaunch;

    /// <inheritdoc />
    public bool Validate<T>(T parameter = default) where T : ConstraintParameters
    {
        var allowGameInCharge = _properties.
    }
}
