namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System.Collections.Generic;

    /// <summary> Interface for key accessor. </summary>
    public interface IKeyAccessor : IKeyValueAccessor, IKeyLevelAccessor
    {
        /// <summary> Commits the given values. </summary>
        /// <param name="values"> The values. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool Commit(IEnumerable<((string prefix, int? index) key, object value)> values);

        /// <summary> Removes all the data stored at the given persistence level. </summary>
        /// <param name="level"> The persistence level. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool Clear(PersistenceLevel level);

        /// <summary> Verifies all the data. </summary>
        /// <param name="full"> Tells whether to run full or quick verification. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool Verify(bool full);
    }
}