namespace Aristocrat.Monaco.Gaming.Contracts.Constraints;

/// <summary>
///     
/// </summary>
public interface IConstraintMapper
{
    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Get<T>() where T : IConstraint;
}
