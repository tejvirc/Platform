namespace Aristocrat.Monaco.Gaming.Presentation.Constraints;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///     Maps constraints to constraint names.
/// </summary>
public class ConstraintMapper : IConstraintMapper
{
    private readonly ConcurrentDictionary<Type, IConstraint> _constraints;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConstraintMapper"/> class
    /// </summary>
    /// <param name="constraints"></param>
    public ConstraintMapper(IEnumerable<IConstraint> constraints)
    {
        _constraints = new ConcurrentDictionary<Type, IConstraint>(constraints.ToDictionary(x => x.GetType(), x => x));
    }

    /// <inheritdoc />
    public T Get<T>() where T : notnull, IConstraint
    {
        if (_constraints.TryGetValue(typeof(T), out var constraint) || constraint is null)
        {
            throw new InvalidOperationException(@$"Constraint with name {typeof(T).Name} not found or is null");
        }

        return (T)constraint;
    }
}
