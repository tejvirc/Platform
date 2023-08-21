namespace Aristocrat.Monaco.Gaming.Contracts.Constraints;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///     
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
    public T Get<T>() where T : IConstraint
    {
        if (_constraints.TryGetValue(typeof(T), out var constraint))
        {
            throw new InvalidOperationException(@$"Constraint with name {typeof(T).Name} not found");
        }

        return (T)constraint;
    }
}
