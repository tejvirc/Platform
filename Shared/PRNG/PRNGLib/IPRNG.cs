// <copyright file="IPRNG.cs" company="Aristocrat, Inc.">
// Copyright (c) Aristocrat, Inc. All rights reserved.
// </copyright>

namespace PRNGLib
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     IPRNG _ Pseudorandom number generator interface.
    /// </summary>
    public interface IPRNG
    {
        /// <summary>
        ///     GetValue() - returns a random value in [0, range].
        /// </summary>
        /// <param name="range">Max value to return.</param>
        /// <returns>64-bit random values.</returns>
        UInt64 GetValue(UInt64 range);

        /// <summary>
        ///     GetValues() - gets multiple random values.
        /// </summary>
        /// <param name="ranges">Range for each value.</param>
        /// <param name="values">Generated values.</param>
        void GetValues(Collection<UInt64> ranges, Collection<UInt64> values);
    }
}