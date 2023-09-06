namespace Aristocrat.Monaco.Gaming.Presentation.Constraints;

/// <summary>
///     An interface for validating a constraint
/// </summary>
public interface IConstraint
{
    /// <summary>
    ///     Gets the name of the constraint.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Validates whether a constraint is satisfied.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parameter"></param>
    /// <returns>True if constraint is satisfied. Otherwise, false.</returns>
    bool Validate<T>(T parameter) where T : notnull, ConstraintParameters;
}
