namespace Aristocrat.Monaco.Gaming.Contracts.Constraints;

/// <summary>
///     An interface for validating a constriant
/// </summary>
public interface IConstraint
{
    /// <summary>
    ///     Gets the name of the constraint.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parameter"></param>
    /// <returns></returns>
    bool Validate<T>(T parameter = default) where T : ConstraintParameters;
}
