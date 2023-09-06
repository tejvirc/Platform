namespace Aristocrat.Monaco.Gaming.Presentation.Constraints;

/// <summary>
///     Maps constraints to constraint names.
/// </summary>
public interface IConstraintMapper
{
    /// <summary>
    ///     Gets a constraint by data type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>An instance that derives from <see cref="IConstraint"/>.</returns>
    T Get<T>() where T : IConstraint;
}
