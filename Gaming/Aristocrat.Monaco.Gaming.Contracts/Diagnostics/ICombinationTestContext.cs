namespace Aristocrat.Monaco.Gaming.Contracts.Diagnostics
{
    using System.Collections.Generic;

    /// <summary>
    ///      ICombinationTestContext interface
    /// </summary>
    public interface ICombinationTestContext
    {
        /// <summary>
        ///     return parameters as readonly dictionary.
        /// </summary>
        IReadOnlyDictionary<string, string> GetParameters();
    }
}