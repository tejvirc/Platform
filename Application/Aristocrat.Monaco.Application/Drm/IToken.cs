namespace Aristocrat.Monaco.Application.Drm
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Provides details about a software token
    /// </summary>
    [CLSCompliant(false)]
    internal interface IToken
    {
        /// <summary>
        ///     Gets the unique id of the license
        /// </summary>
        ushort Id { get; }

        /// <summary>
        ///     Gets the name of the license
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the extended details of the license such as supported features, etc.
        /// </summary>
        TokenData Data { get; }

        /// <summary>
        ///     Gets the lock flags
        /// </summary>
        ulong Locks { get; }

        /// <summary>
        ///     Gets the associated counters
        /// </summary>
        IReadOnlyDictionary<Counter, int> Counters { get; }
    }
}